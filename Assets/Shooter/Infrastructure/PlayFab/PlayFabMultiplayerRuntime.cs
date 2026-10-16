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
            _eventPumpObject.AddComponent<PlayFabMatchmakingEventPump>();
        }

        private void DestroyEventPump()
        {
            if (_eventPumpObject == null)
            {
                return;
            }

            Object.Destroy(_eventPumpObject);
            _eventPumpObject = null;
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
