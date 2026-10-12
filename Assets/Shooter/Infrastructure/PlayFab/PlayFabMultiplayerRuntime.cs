using System;
using PlayFab.Multiplayer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMultiplayerRuntime : IDisposable
    {
        private readonly PlayFabClientOptions _options;
        private GameObject _eventPumpObject;
        private PlayFabMatchmakingEventPump _eventPump;
        private bool _ownsInitialization;
        private bool _disposed;

        public PlayFabMultiplayerRuntime(PlayFabClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void EnsureInitialized()
        {
            ThrowIfDisposed();
            InitializeSdk();
            CreateEventPump();
        }

        public void SetMatchmakingProcessingActive(bool active)
        {
            if (_eventPump != null)
            {
                _eventPump.enabled = active;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DestroyEventPump();
            UninitializeSdk();
        }

        private void InitializeSdk()
        {
            if (PlayFabMultiplayer.IsInitialized)
            {
                return;
            }

            PlayFabMultiplayer.Initialize(_options.TitleId);
            _ownsInitialization = true;
        }

        private void CreateEventPump()
        {
            if (_eventPumpObject != null)
            {
                return;
            }

            _eventPumpObject = new GameObject("PlayFab Matchmaking Event Pump");
            Object.DontDestroyOnLoad(_eventPumpObject);
            _eventPump = _eventPumpObject.AddComponent<PlayFabMatchmakingEventPump>();
            _eventPump.enabled = false;
        }

        private void DestroyEventPump()
        {
            if (_eventPumpObject == null)
            {
                return;
            }

            Object.Destroy(_eventPumpObject);
            _eventPumpObject = null;
            _eventPump = null;
        }

        private void UninitializeSdk()
        {
            if (!_ownsInitialization || !PlayFabMultiplayer.IsInitialized)
            {
                return;
            }

            PlayFabMultiplayer.Uninitialize();
            _ownsInitialization = false;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlayFabMultiplayerRuntime));
            }
        }
    }
}
