using System;
using System.Diagnostics;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Shooter.Bootstrap
{
    public sealed class LocalProcessDiagnostics : IStartable, IDisposable
    {
        private readonly INetworkSession _networkSession;
        private readonly NetworkSessionStartRequest _request;

        private LocalProcessDebugOverlay _overlay;
        private int _processId;

        public LocalProcessDiagnostics(
            INetworkSession networkSession,
            NetworkSessionStartRequest request)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Start()
        {
            using (var process = Process.GetCurrentProcess())
            {
                _processId = process.Id;
            }

            _networkSession.StateChanged += OnNetworkStateChanged;
            Publish(_networkSession.State);

            if (_request.Role == NetworkSessionRole.Client && Debug.isDebugBuild)
            {
                var overlayObject = new GameObject("Local Process Diagnostics");
                Object.DontDestroyOnLoad(overlayObject);
                _overlay = overlayObject.AddComponent<LocalProcessDebugOverlay>();
                _overlay.SetText(BuildText(_networkSession.State));
            }
        }

        public void Dispose()
        {
            _networkSession.StateChanged -= OnNetworkStateChanged;

            if (_overlay != null)
            {
                Object.Destroy(_overlay.gameObject);
                _overlay = null;
            }
        }

        private void OnNetworkStateChanged(NetworkSessionState state)
        {
            Publish(state);
            _overlay?.SetText(BuildText(state));
        }

        private void Publish(NetworkSessionState state)
        {
            Debug.Log($"[LocalProcess] {BuildText(state).Replace('\n', ' ')}");
        }

        private string BuildText(NetworkSessionState state)
        {
            var port = _request.Role == NetworkSessionRole.Server
                ? _request.Port.ToString()
                : "n/a (client joins by session)";

            return
                $"Role: {_request.Role}    PID: {_processId}\n" +
                $"Session: {_request.SessionName}    Port: {port}\n" +
                $"Network: {state}    Development: {Debug.isDebugBuild}";
        }
    }
}
