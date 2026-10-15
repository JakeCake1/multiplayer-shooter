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
        private CancellationTokenSource _operation;
        private int _attempt;
        private bool _authenticated;
        private bool _expectingNetworkTransition;

        public ClientFlowCoordinator(IAuthenticationService authenticationService, IMatchmakingService matchmakingService, INetworkSession networkSession, IApplicationQuitter applicationQuitter)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _matchmakingService = matchmakingService ?? throw new ArgumentNullException(nameof(matchmakingService));
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _applicationQuitter = applicationQuitter ?? throw new ArgumentNullException(nameof(applicationQuitter));
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
            await StopCompletedSessionAsync();
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
            return _authenticated && (Current.State == ClientFlowState.Menu || Current.State == ClientFlowState.Results);
        }

        private async Task StopCompletedSessionAsync()
        {
            if (_networkSession.State != NetworkSessionState.Disconnected)
            {
                _expectingNetworkTransition = true;
                try
                {
                    await _networkSession.StopAsync();
                }
                finally
                {
                    _expectingNetworkTransition = false;
                }
            }
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
            return new ClientFlowSnapshot(ClientFlowState.Menu, problem, isAuthenticationReady: _authenticated);
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
