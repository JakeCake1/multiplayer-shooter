using System;
using System.Threading;
using System.Threading.Tasks;
using PlayFab;
using Shooter.Application;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabAuthenticationService : IAuthenticationService, IPlayFabPlayerSession, IDisposable
    {
        private readonly PlayFabClientOptions _options;
        private PFServiceConfig _serviceConfig;
        private PFPlayerEntity _playerEntity;
        private bool _xGameRuntimeInitialized;
        private bool _servicesInitialized;
        private bool _disposed;

        public PlayFabAuthenticationService(PlayFabClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public PFPlayerEntity PlayerEntity => _playerEntity;

        public async Task<AuthenticationResult> AuthenticateAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (_playerEntity != null)
            {
                return AuthenticationResult.Success(GetAuthenticatedPlayerId());
            }

            try
            {
                InitializeXGameRuntime();
                InitializeServices();
                CreateServiceConfiguration();
                return await LoginWithCustomIdAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                ReleaseAuthenticationHandles();
                return AuthenticationResult.Failure($"PlayFab authentication failed: {exception.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ReleaseAuthenticationHandles();
        }

        private void InitializeXGameRuntime()
        {
            if (_xGameRuntimeInitialized)
            {
                return;
            }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var result = XGameRuntime.Initialize();
            ThrowIfFailed(result, "Failed to initialize XGameRuntime");
            _xGameRuntimeInitialized = true;
#else
            throw new PlatformNotSupportedException("PlayFab Unified SDK currently requires a Windows client build.");
#endif
        }

        private void InitializeServices()
        {
            if (_servicesInitialized)
            {
                return;
            }

            var result = PFServices.Initialize();
            var alreadyInitialized = result.HResult == HRESULT.E_PF_CORE_ALREADY_INITIALIZED || result.HResult == HRESULT.E_PF_SERVICES_ALREADY_INITIALIZED;
            if (result.Failed() && !alreadyInitialized)
            {
                throw new InvalidOperationException(FormatFailure("Failed to initialize PlayFab services", result.HResult));
            }

            _servicesInitialized = true;
        }

        private void CreateServiceConfiguration()
        {
            if (_serviceConfig != null)
            {
                return;
            }

            var result = PFCore.CreateServiceConfig(_options.ApiEndpoint, _options.TitleId);
            if (result.Failed())
            {
                throw new InvalidOperationException(FormatFailure("Failed to create the PlayFab service configuration", result.HResult));
            }

            _serviceConfig = result.Result;
        }

        private async Task<AuthenticationResult> LoginWithCustomIdAsync(CancellationToken cancellationToken)
        {
            var request = new PFAuthenticationLoginWithCustomIDRequest { CustomId = _options.CustomId, CreateAccount = true };
            var result = await _serviceConfig.AuthenticationLoginWithCustomIDAsync(request);

            if (cancellationToken.IsCancellationRequested)
            {
                result.Result?.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (result.Failed())
            {
                return AuthenticationResult.Failure(FormatFailure("PlayFab Custom ID login failed", result.HResult));
            }

            _playerEntity = result.Result ?? throw new InvalidOperationException("PlayFab login succeeded without a player entity.");
            return AuthenticationResult.Success(GetAuthenticatedPlayerId());
        }

        private string GetAuthenticatedPlayerId()
        {
            var playFabId = _playerEntity.LoginResult.HasValue ? _playerEntity.LoginResult.Value.PlayFabId : string.Empty;
            return string.IsNullOrWhiteSpace(playFabId) ? _options.CustomId : playFabId;
        }

        private void ReleaseAuthenticationHandles()
        {
            _playerEntity?.Dispose();
            _playerEntity = null;
            _serviceConfig?.Dispose();
            _serviceConfig = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlayFabAuthenticationService));
            }
        }

        private static void ThrowIfFailed(int result, string operation)
        {
            if (HRESULT.Failed(result))
            {
                throw new InvalidOperationException(FormatFailure(operation, result));
            }
        }

        private static string FormatFailure(string operation, int result)
        {
            return $"{operation} (HRESULT 0x{result:X8}).";
        }
    }
}
