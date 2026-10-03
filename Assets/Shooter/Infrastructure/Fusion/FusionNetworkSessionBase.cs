using System;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Shooter.Application;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shooter.Infrastructure.Fusion
{
    public abstract class FusionNetworkSessionBase : INetworkSession, IDisposable
    {
        private NetworkRunner _runner;

        public event Action<NetworkSessionState> StateChanged;

        public NetworkSessionState State { get; private set; } = NetworkSessionState.Disconnected;

        protected abstract NetworkSessionRole Role { get; }

        public async Task<NetworkSessionStartResult> StartAsync(NetworkSessionStartRequest request)
        {
            ValidateStartRequest(request);
            SetState(NetworkSessionState.Starting);

            try
            {
                return await StartValidatedAsync(request);
            }
            catch (Exception exception)
            {
                return await HandleUnexpectedFailureAsync(exception);
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
            DestroyRunnerImmediately();
            SetState(NetworkSessionState.Disconnected);
        }

        protected abstract NetworkRunner CreateRunner();

        protected abstract StartGameArgs CreateStartGameArgs(NetworkSessionStartRequest request, NetworkRunner runner);

        protected virtual Task PrepareAsync()
        {
            return Task.CompletedTask;
        }

        protected NetworkRunner CreateRunnerBase()
        {
            var runnerObject = new GameObject($"Fusion NetworkRunner ({Role})");
            Object.DontDestroyOnLoad(runnerObject);
            var runner = runnerObject.AddComponent<NetworkRunner>();
            runnerObject.AddComponent<NetworkSceneManagerDefault>();
            runnerObject.AddComponent<NetworkObjectProviderDefault>();
            return runner;
        }

        private async Task<NetworkSessionStartResult> StartValidatedAsync(NetworkSessionStartRequest request)
        {
            if (!TryValidatePhotonConfiguration(out var configurationError))
            {
                return HandleConfigurationFailure(configurationError);
            }

            LogStart(request);
            await PrepareAsync();
            _runner = CreateRunner();
            var result = await _runner.StartGame(CreateStartGameArgs(request, _runner));
            return result.Ok ? HandleStartSuccess() : await HandleStartFailureAsync(request, result);
        }

        private void ValidateStartRequest(NetworkSessionStartRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Role != Role)
            {
                throw new ArgumentException($"A {Role} network session cannot start a {request.Role} request.", nameof(request));
            }

            if (State != NetworkSessionState.Disconnected)
            {
                throw new InvalidOperationException($"A network session cannot start while it is {State}.");
            }
        }

        private NetworkSessionStartResult HandleConfigurationFailure(string error)
        {
            Debug.LogError($"[Fusion] {error}");
            SetState(NetworkSessionState.Disconnected);
            return NetworkSessionStartResult.Failure(NetworkSessionError.InvalidConfiguration, error);
        }

        private NetworkSessionStartResult HandleStartSuccess()
        {
            SetState(NetworkSessionState.Connected);
            return NetworkSessionStartResult.Success();
        }

        private async Task<NetworkSessionStartResult> HandleStartFailureAsync(NetworkSessionStartRequest request, StartGameResult result)
        {
            var error = GetStartError(result);
            var detail = BuildFailureDetail(result);
            Debug.LogError($"[Fusion] Failed to start {Role} session '{request.SessionName}': {error}. {detail}");
            await DestroyRunnerAsync();
            SetState(NetworkSessionState.Disconnected);
            return NetworkSessionStartResult.Failure(error, detail);
        }

        private async Task<NetworkSessionStartResult> HandleUnexpectedFailureAsync(Exception exception)
        {
            Debug.LogException(exception);
            await DestroyRunnerAsync();
            SetState(NetworkSessionState.Disconnected);
            return NetworkSessionStartResult.Failure(NetworkSessionError.UnexpectedFailure, $"{exception.GetType().Name}: {exception.Message}");
        }

        private void LogStart(NetworkSessionStartRequest request)
        {
            Debug.Log($"[Fusion] Starting {Role} session '{request.SessionName}'. Fusion AppId configured: true; region: {GetConfiguredRegion()}.");
        }

        private static NetworkSessionError GetStartError(StartGameResult result)
        {
            return result.ShutdownReason == ShutdownReason.InvalidAuthentication ? NetworkSessionError.AuthenticationFailed : NetworkSessionError.ConnectionFailed;
        }

        private static bool TryValidatePhotonConfiguration(out string error)
        {
            if (!PhotonAppSettings.TryGetGlobal(out var settings) || settings.AppSettings == null)
            {
                error = "PhotonAppSettings could not be loaded. Ensure the Fusion settings asset exists in a Resources folder.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(settings.AppSettings.AppIdFusion))
            {
                error = "Fusion AppId is missing. Set App Id Fusion in Assets/Photon/Fusion/Resources/PhotonAppSettings.asset and rebuild the local players.";
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
            var detail = AppendErrorMessage(result.ShutdownReason.ToString(), result.ErrorMessage);
            return result.ShutdownReason == ShutdownReason.InvalidAuthentication ? $"{detail}. Verify that App Id Fusion belongs to an active Photon Fusion application." : detail;
        }

        private static string AppendErrorMessage(string detail, string errorMessage)
        {
            return string.IsNullOrWhiteSpace(errorMessage) ? detail : $"{detail}. {errorMessage}";
        }

        private async Task DestroyRunnerAsync()
        {
            var runner = TakeRunner();
            if (runner == null)
            {
                return;
            }

            if (runner.IsRunning)
            {
                await runner.Shutdown(destroyGameObject: false);
            }

            Object.Destroy(runner.gameObject);
        }

        private void DestroyRunnerImmediately()
        {
            var runner = TakeRunner();
            if (runner != null)
            {
                Object.Destroy(runner.gameObject);
            }
        }

        private NetworkRunner TakeRunner()
        {
            var runner = _runner;
            _runner = null;
            return runner;
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
