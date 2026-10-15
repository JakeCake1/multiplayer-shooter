using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Application
{
    public sealed class ClientFlowCoordinator : IClientFlowController, IDisposable
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMatchmakingService _matchmakingService;
        private readonly INetworkSession _networkSession;
        private readonly IApplicationQuitter _applicationQuitter;
        private readonly ClientFlowTimeoutPolicy _timeoutPolicy;
        private CancellationTokenSource _operation;
        private int _attempt;
        private bool _authenticated;
        private bool _expectingNetworkTransition;
        private bool _networkCleanupPending;

        public ClientFlowCoordinator(IAuthenticationService authenticationService, IMatchmakingService matchmakingService, INetworkSession networkSession, IApplicationQuitter applicationQuitter, ClientFlowTimeoutPolicy timeoutPolicy)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _matchmakingService = matchmakingService ?? throw new ArgumentNullException(nameof(matchmakingService));
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _applicationQuitter = applicationQuitter ?? throw new ArgumentNullException(nameof(applicationQuitter));
            _timeoutPolicy = timeoutPolicy ?? throw new ArgumentNullException(nameof(timeoutPolicy));
            _networkSession.StateChanged += HandleNetworkSessionStateChanged;
        }

        public event Action<ClientFlowSnapshot> Changed;

        public ClientFlowSnapshot Current { get; private set; } = new ClientFlowSnapshot(ClientFlowState.Booting);

        public Task InitializeAsync()
        {
            if (Current.State != ClientFlowState.Booting)
            {
                return Task.CompletedTask;
            }

            return RunAuthenticationAsync(false);
        }

        public Task RetryAuthenticationAsync()
        {
            if (Current.State != ClientFlowState.Menu || _authenticated)
            {
                return Task.CompletedTask;
            }

            return RunAuthenticationAsync(true);
        }

        private async Task RunAuthenticationAsync(bool publishBooting)
        {
            var context = BeginOperation();
            if (publishBooting)
            {
                Publish(new ClientFlowSnapshot(ClientFlowState.Booting));
            }

            try
            {
                await AuthenticateForMenuAsync(context);
            }
            catch (OperationCanceledException) when (context.Token.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                PublishUnexpectedFailure(context.Attempt, "Authentication", exception);
            }
            finally
            {
                CompleteOperation(context.Source);
            }
        }

        public async Task FindGameAsync()
        {
            if (!CanFindGame())
            {
                return;
            }

            var context = BeginOperation();
            try
            {
                await FindAndConnectAsync(context);
            }
            catch (OperationCanceledException) when (context.Token.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                PublishUnexpectedFailure(context.Attempt, "Find Game", exception);
            }
            finally
            {
                CompleteOperation(context.Source);
            }
        }

        public void ObserveMatch(ClientMatchSnapshot snapshot)
        {
            if (snapshot == null || _networkSession.State != NetworkSessionState.Connected)
            {
                return;
            }

            Publish(new ClientFlowSnapshot(MapState(snapshot.Stage), match: snapshot));
        }

        public void CancelSearch()
        {
            if (Current.State != ClientFlowState.Searching)
            {
                return;
            }

            CancelOperation();
            Publish(CreateMenuSnapshot());
        }

        public void Exit()
        {
            CancelOperation();
            _applicationQuitter.Quit();
        }

        public void Dispose()
        {
            CancelOperation();
            _networkSession.StateChanged -= HandleNetworkSessionStateChanged;
        }

        private async Task AuthenticateForMenuAsync(FlowOperationContext context)
        {
            var result = await _authenticationService.AuthenticateAsync(context.Token);
            if (!IsCurrent(context.Attempt))
            {
                return;
            }

            _authenticated = result.Succeeded;
            Publish(CreateMenuSnapshot(result.Succeeded ? string.Empty : result.Problem));
        }

        private async Task FindAndConnectAsync(FlowOperationContext context)
        {
            if (!await StopCompletedSessionAsync(context))
            {
                return;
            }

            Publish(new ClientFlowSnapshot(ClientFlowState.Searching));
            var matchmakingResult = await _matchmakingService.FindMatchAsync(context.Token);
            if (!ApplyMatchmakingResult(context.Attempt, matchmakingResult))
            {
                return;
            }

            await ConnectAsync(context.Attempt, matchmakingResult.SessionRequest);
        }

        private bool ApplyMatchmakingResult(int attempt, MatchmakingResult result)
        {
            if (!IsCurrent(attempt))
            {
                return false;
            }

            if (!result.Succeeded)
            {
                Publish(CreateMenuSnapshot(result.Problem));
                return false;
            }

            Publish(new ClientFlowSnapshot(ClientFlowState.Connecting));
            return true;
        }

        private async Task ConnectAsync(int attempt, NetworkSessionStartRequest request)
        {
            _expectingNetworkTransition = true;
            try
            {
                var result = await _networkSession.StartAsync(request);
                if (IsCurrent(attempt))
                {
                    Publish(result.Succeeded ? new ClientFlowSnapshot(ClientFlowState.WaitingForPlayers) : CreateMenuSnapshot(BuildProblem(result)));
                }
            }
            finally
            {
                _expectingNetworkTransition = false;
            }
        }

        private bool CanFindGame()
        {
            return _authenticated && !_networkCleanupPending && (Current.State == ClientFlowState.Menu || Current.State == ClientFlowState.Results);
        }

        private async Task<bool> StopCompletedSessionAsync(FlowOperationContext context)
        {
            if (_networkSession.State == NetworkSessionState.Disconnected)
            {
                return true;
            }

            _expectingNetworkTransition = true;
            Task stopTask;
            try
            {
                stopTask = _networkSession.StopAsync();
            }
            catch (Exception exception)
            {
                return HandleNetworkCleanupFailure(exception);
            }

            var timeoutTask = Task.Delay(_timeoutPolicy.CompletedSessionStopTimeout, context.Token);
            var completedTask = await Task.WhenAny(stopTask, timeoutTask);
            if (completedTask == stopTask)
            {
                return await FinishNetworkCleanupAsync(stopTask);
            }

            context.Token.ThrowIfCancellationRequested();
            HandleNetworkCleanupTimeout(stopTask, context.Attempt);
            return false;
        }

        private async Task<bool> FinishNetworkCleanupAsync(Task stopTask)
        {
            try
            {
                await stopTask;
            }
            catch (Exception exception)
            {
                return HandleNetworkCleanupFailure(exception);
            }

            _expectingNetworkTransition = false;
            if (_networkSession.State != NetworkSessionState.Disconnected)
            {
                return HandleNetworkCleanupFailure("Network session cleanup completed without disconnecting.");
            }

            return true;
        }

        private void HandleNetworkCleanupTimeout(Task stopTask, int attempt)
        {
            _networkCleanupPending = true;
            Publish(CreateMenuSnapshot($"Network session cleanup timed out after {_timeoutPolicy.CompletedSessionStopTimeout.TotalSeconds:0.#} seconds."));
            _ = ObserveTimedOutNetworkCleanupAsync(stopTask, attempt);
        }

        private async Task ObserveTimedOutNetworkCleanupAsync(Task stopTask, int attempt)
        {
            var problem = string.Empty;
            try
            {
                await stopTask;
            }
            catch (Exception exception)
            {
                problem = $"Network session cleanup failed: {exception.Message}";
            }
            finally
            {
                _expectingNetworkTransition = false;
                _networkCleanupPending = _networkSession.State != NetworkSessionState.Disconnected;
            }

            if (_networkCleanupPending && string.IsNullOrWhiteSpace(problem))
            {
                problem = "Network session cleanup completed without disconnecting.";
            }

            if (IsCurrent(attempt) && Current.State == ClientFlowState.Menu)
            {
                Publish(CreateMenuSnapshot(problem));
            }
        }

        private bool HandleNetworkCleanupFailure(Exception exception)
        {
            return HandleNetworkCleanupFailure($"Network session cleanup failed: {exception.Message}");
        }

        private bool HandleNetworkCleanupFailure(string problem)
        {
            _expectingNetworkTransition = false;
            _networkCleanupPending = _networkSession.State != NetworkSessionState.Disconnected;
            Publish(CreateMenuSnapshot(problem));
            return false;
        }

        private void HandleNetworkSessionStateChanged(NetworkSessionState state)
        {
            if (state == NetworkSessionState.Disconnected && !_expectingNetworkTransition && Current.State != ClientFlowState.Menu && Current.State != ClientFlowState.Booting)
            {
                Publish(CreateMenuSnapshot("Disconnected from the match server."));
            }
        }

        private FlowOperationContext BeginOperation()
        {
            CancelOperation();
            _operation = new CancellationTokenSource();
            return new FlowOperationContext(++_attempt, _operation);
        }

        private void CompleteOperation(CancellationTokenSource source)
        {
            if (!ReferenceEquals(_operation, source))
            {
                return;
            }

            _operation.Dispose();
            _operation = null;
        }

        private void CancelOperation()
        {
            _attempt++;
            _operation?.Cancel();
            _operation?.Dispose();
            _operation = null;
        }

        private bool IsCurrent(int attempt)
        {
            return attempt == _attempt;
        }

        private void PublishUnexpectedFailure(int attempt, string stage, Exception exception)
        {
            if (IsCurrent(attempt))
            {
                Publish(CreateMenuSnapshot($"{stage} unexpected failure: {exception.Message}"));
            }
        }

        private ClientFlowSnapshot CreateMenuSnapshot(string problem = "")
        {
            return new ClientFlowSnapshot(ClientFlowState.Menu, problem, isAuthenticationReady: _authenticated, canFindGame: _authenticated && !_networkCleanupPending);
        }

        private void Publish(ClientFlowSnapshot snapshot)
        {
            Current = snapshot;
            Changed?.Invoke(snapshot);
        }

        private static ClientFlowState MapState(ClientMatchStage stage)
        {
            return stage == ClientMatchStage.Results ? ClientFlowState.Results : stage == ClientMatchStage.Playing ? ClientFlowState.Playing : ClientFlowState.WaitingForPlayers;
        }

        private static string BuildProblem(NetworkSessionStartResult result)
        {
            return string.IsNullOrWhiteSpace(result.Detail) ? result.Error.ToString() : $"{result.Error}: {result.Detail}";
        }
    }
}
