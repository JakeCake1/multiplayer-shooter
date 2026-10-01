using System;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Shooter.Application;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionNetworkSession : INetworkSession, IDisposable
    {
        private NetworkRunner _runner;

        public event Action<NetworkSessionState> StateChanged;

        public NetworkSessionState State { get; private set; } = NetworkSessionState.Disconnected;

        public async Task<NetworkSessionStartResult> StartAsync(NetworkSessionStartRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (State != NetworkSessionState.Disconnected)
            {
                throw new InvalidOperationException($"A network session cannot start while it is {State}.");
            }

            SetState(NetworkSessionState.Starting);

            try
            {
                _runner = CreateRunner(request.Role);

                var result = await _runner.StartGame(new StartGameArgs
                {
                    GameMode = request.Role == NetworkSessionRole.Server
                        ? GameMode.Server
                        : GameMode.Client,
                    SessionName = request.SessionName,
                    Address = request.Role == NetworkSessionRole.Server
                        ? NetAddress.Any(request.Port)
                        : NetAddress.Any(),
                    PlayerCount = request.PlayerCount,
                    EnableClientSessionCreation = false,
                    SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>(),
                    ObjectProvider = _runner.GetComponent<NetworkObjectProviderDefault>()
                });

                if (!result.Ok)
                {
                    await DestroyRunnerAsync();
                    SetState(NetworkSessionState.Disconnected);
                    return NetworkSessionStartResult.Failure(
                        NetworkSessionError.ConnectionFailed,
                        result.ShutdownReason.ToString());
                }

                SetState(NetworkSessionState.Connected);
                return NetworkSessionStartResult.Success();
            }
            catch (Exception exception)
            {
                await DestroyRunnerAsync();
                SetState(NetworkSessionState.Disconnected);
                return NetworkSessionStartResult.Failure(
                    NetworkSessionError.UnexpectedFailure,
                    exception.Message);
            }
        }

        public async Task StopAsync()
        {
            if (State == NetworkSessionState.Disconnected)
            {
                return;
            }

            SetState(NetworkSessionState.Stopping);
            await DestroyRunnerAsync();
            SetState(NetworkSessionState.Disconnected);
        }

        public void Dispose()
        {
            if (_runner != null)
            {
                Object.Destroy(_runner.gameObject);
                _runner = null;
            }

            SetState(NetworkSessionState.Disconnected);
        }

        private static NetworkRunner CreateRunner(NetworkSessionRole role)
        {
            var runnerObject = new GameObject($"Fusion NetworkRunner ({role})");
            Object.DontDestroyOnLoad(runnerObject);

            var runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = role == NetworkSessionRole.Client;
            runnerObject.AddComponent<NetworkSceneManagerDefault>();
            runnerObject.AddComponent<NetworkObjectProviderDefault>();
            return runner;
        }

        private async Task DestroyRunnerAsync()
        {
            var runner = _runner;
            _runner = null;

            if (runner == null)
            {
                return;
            }

            if (runner.IsRunning)
            {
                await runner.Shutdown(destroyGameObject: false);
            }

            if (runner != null)
            {
                Object.Destroy(runner.gameObject);
            }
        }

        private void SetState(NetworkSessionState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
