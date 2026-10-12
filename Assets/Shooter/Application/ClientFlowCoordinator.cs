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
        private bool _startingNetworkSession;

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

        public async Task InitializeAsync()
        {
            if (Current.State != ClientFlowState.Booting)
            {
                return;
            }

            var context = BeginOperation();
            _startingNetworkSession = true;
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
                FinishNetworkStart(context.Attempt);
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
            Publish(new ClientFlowSnapshot(ClientFlowState.Menu, result.Succeeded ? string.Empty : result.Problem));
        }

        private async Task FindAndConnectAsync(FlowOperationContext context)
        {
            await StopCompletedSessionAsync();
            if (!await EnsureAuthenticatedAsync(context))
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

        private async Task<bool> EnsureAuthenticatedAsync(FlowOperationContext context)
        {
            if (_authenticated)
            {
                return true;
            }

            Publish(new ClientFlowSnapshot(ClientFlowState.Booting));
            var result = await _authenticationService.AuthenticateAsync(context.Token);
            if (!IsCurrent(context.Attempt))
            {
                return false;
            }

            _authenticated = result.Succeeded;
            if (!result.Succeeded)
            {
                Publish(new ClientFlowSnapshot(ClientFlowState.Menu, result.Problem));
            }

            return result.Succeeded;
        }

        private bool ApplyMatchmakingResult(int attempt, MatchmakingResult result)
        {
            if (!IsCurrent(attempt))
            {
                return false;
            }

            if (!result.Succeeded)
            {
                Publish(new ClientFlowSnapshot(ClientFlowState.Menu, result.Problem));
                return false;
            }

            Publish(new ClientFlowSnapshot(ClientFlowState.Connecting));
            return true;
        }

        private async Task ConnectAsync(int attempt, NetworkSessionStartRequest request)
        {
            var result = await _networkSession.StartAsync(request);
            if (IsCurrent(attempt))
            {
                Publish(result.Succeeded ? new ClientFlowSnapshot(ClientFlowState.WaitingForPlayers) : new ClientFlowSnapshot(ClientFlowState.Menu, BuildProblem(result)));
            }
        }

        private bool CanFindGame()
        {
            return Current.State == ClientFlowState.Menu || Current.State == ClientFlowState.Results;
        }

        private async Task StopCompletedSessionAsync()
        {
            if (_networkSession.State != NetworkSessionState.Disconnected)
            {
                await _networkSession.StopAsync();
            }
        }

        private void HandleNetworkSessionStateChanged(NetworkSessionState state)
        {
            if (state == NetworkSessionState.Disconnected && !_startingNetworkSession && Current.State != ClientFlowState.Menu && Current.State != ClientFlowState.Booting)
            {
                Publish(new ClientFlowSnapshot(ClientFlowState.Menu, "Disconnected from the match server."));
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

        private void FinishNetworkStart(int attempt)
        {
            if (IsCurrent(attempt))
            {
                _startingNetworkSession = false;
            }
        }

        private void PublishUnexpectedFailure(int attempt, string stage, Exception exception)
        {
            if (IsCurrent(attempt))
            {
                Publish(new ClientFlowSnapshot(ClientFlowState.Menu, $"{stage} unexpected failure: {exception.Message}"));
            }
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
