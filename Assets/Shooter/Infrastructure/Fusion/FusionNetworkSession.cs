using System;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Shooter.Application;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionNetworkSession : INetworkSession, IDisposable
    {
        private readonly FusionNetworkSessionOptions _options;
        private NetworkRunner _runner;

        public FusionNetworkSession(FusionNetworkSessionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

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
                if (!TryValidateConfiguration(out var configurationError))
                {
                    Debug.LogError($"[Fusion] {configurationError}");
                    SetState(NetworkSessionState.Disconnected);
                    return NetworkSessionStartResult.Failure(
                        NetworkSessionError.InvalidConfiguration,
                        configurationError);
                }

                Debug.Log(
                    $"[Fusion] Starting {request.Role} session '{request.SessionName}'. " +
                    $"Fusion AppId configured: true; region: {GetConfiguredRegion()}.");

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
                    var error = result.ShutdownReason == ShutdownReason.InvalidAuthentication
                        ? NetworkSessionError.AuthenticationFailed
                        : NetworkSessionError.ConnectionFailed;
                    var detail = BuildFailureDetail(result);

                    Debug.LogError(
                        $"[Fusion] Failed to start {request.Role} session '{request.SessionName}': " +
                        $"{error}. {detail}");

                    await DestroyRunnerAsync();
                    SetState(NetworkSessionState.Disconnected);
                    return NetworkSessionStartResult.Failure(
                        error,
                        detail);
                }

                SetState(NetworkSessionState.Connected);
                return NetworkSessionStartResult.Success();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                await DestroyRunnerAsync();
                SetState(NetworkSessionState.Disconnected);
                return NetworkSessionStartResult.Failure(
                    NetworkSessionError.UnexpectedFailure,
                    $"{exception.GetType().Name}: {exception.Message}");
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

        private NetworkRunner CreateRunner(NetworkSessionRole role)
        {
            var runnerObject = new GameObject($"Fusion NetworkRunner ({role})");
            Object.DontDestroyOnLoad(runnerObject);

            var runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = role == NetworkSessionRole.Client;
            runnerObject.AddComponent<NetworkSceneManagerDefault>();
            runnerObject.AddComponent<NetworkObjectProviderDefault>();

            if (role == NetworkSessionRole.Server)
            {
                runnerObject.AddComponent<FusionPlayerSpawner>().Configure(_options.PlayerPrefab);
            }
            else
            {
                var events = runnerObject.AddComponent<NetworkEvents>();
                events.OnInput ??= new NetworkEvents.InputEvent();
                events.OnInput.AddListener(CollectInput);
            }

            return runner;
        }

        private static void CollectInput(NetworkRunner runner, NetworkInput networkInput)
        {
            var keyboard = Keyboard.current;
            var movement = Vector2.zero;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed)
                {
                    movement += Vector2.up;
                }

                if (keyboard.sKey.isPressed)
                {
                    movement += Vector2.down;
                }

                if (keyboard.aKey.isPressed)
                {
                    movement += Vector2.left;
                }

                if (keyboard.dKey.isPressed)
                {
                    movement += Vector2.right;
                }
            }

            networkInput.Set(new FusionPlayerInput
            {
                MoveDirection = movement.normalized
            });
        }

        private static bool TryValidateConfiguration(out string error)
        {
            if (!PhotonAppSettings.TryGetGlobal(out var settings) || settings.AppSettings == null)
            {
                error = "PhotonAppSettings could not be loaded. Ensure the Fusion settings asset exists in a Resources folder.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(settings.AppSettings.AppIdFusion))
            {
                error =
                    "Fusion AppId is missing. Set App Id Fusion in " +
                    "Assets/Photon/Fusion/Resources/PhotonAppSettings.asset and rebuild the local players.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static string GetConfiguredRegion()
        {
            var region = PhotonAppSettings.Global.AppSettings.FixedRegion;
            return string.IsNullOrWhiteSpace(region) ? "automatic" : region;
        }

        private static string BuildFailureDetail(StartGameResult result)
        {
            var detail = result.ShutdownReason.ToString();
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                detail += $". {result.ErrorMessage}";
            }

            if (result.ShutdownReason == ShutdownReason.InvalidAuthentication)
            {
                detail += ". Verify that App Id Fusion belongs to an active Photon Fusion application.";
            }

            return detail;
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
