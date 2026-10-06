using System;
using System.Threading.Tasks;

namespace Shooter.Application
{
    public sealed class ClientFlowCoordinator : IClientFlowController, IDisposable
    {
        private readonly INetworkSession _networkSession;
        private readonly NetworkSessionStartRequest _request;
        private readonly IApplicationQuitter _applicationQuitter;
        private int _attempt;
        private bool _starting;

        public ClientFlowCoordinator(INetworkSession networkSession, NetworkSessionStartRequest request, IApplicationQuitter applicationQuitter)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _applicationQuitter = applicationQuitter ?? throw new ArgumentNullException(nameof(applicationQuitter));
            _networkSession.StateChanged += HandleNetworkSessionStateChanged;
        }

        public event Action<ClientFlowSnapshot> Changed;

        public ClientFlowSnapshot Current { get; private set; } = new ClientFlowSnapshot(ClientFlowState.Menu);

        public async Task FindGameAsync()
        {
            if (Current.State != ClientFlowState.Menu && Current.State != ClientFlowState.Results)
            {
                return;
            }

            var attempt = ++_attempt;
            _starting = true;
            try
            {
                await StopCompletedSessionAsync();
                Publish(new ClientFlowSnapshot(ClientFlowState.Connecting));
                var result = await _networkSession.StartAsync(_request);
                if (attempt == _attempt)
                {
                    Publish(result.Succeeded ? new ClientFlowSnapshot(ClientFlowState.WaitingForPlayers) : new ClientFlowSnapshot(ClientFlowState.Menu, BuildProblem(result)));
                }
            }
            catch (Exception exception)
            {
                if (attempt == _attempt)
                {
                    Publish(new ClientFlowSnapshot(ClientFlowState.Menu, $"UnexpectedFailure: {exception.Message}"));
                }
            }
            finally
            {
                _starting = false;
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
            _applicationQuitter.Quit();
        }

        public void Dispose()
        {
            _attempt++;
            _networkSession.StateChanged -= HandleNetworkSessionStateChanged;
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
            if (state == NetworkSessionState.Disconnected && !_starting && Current.State != ClientFlowState.Menu)
            {
                Publish(new ClientFlowSnapshot(ClientFlowState.Menu, "Disconnected from the match server."));
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
