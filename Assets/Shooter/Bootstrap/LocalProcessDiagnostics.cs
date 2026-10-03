using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Shooter.Bootstrap
{
    public sealed class LocalShutdownSignal
    {
        public LocalShutdownSignal(string path)
        {
            Path = string.IsNullOrWhiteSpace(path)
                ? throw new ArgumentException("A shutdown signal path is required.", nameof(path))
                : path;
        }

        public string Path { get; }
    }

    public sealed class LocalShutdownWatcher : ITickable
    {
        private const float PollIntervalSeconds = 0.25f;

        private readonly INetworkSession _networkSession;
        private readonly LocalShutdownSignal _signal;

        private float _nextPollTime;
        private bool _shutdownStarted;

        public LocalShutdownWatcher(
            INetworkSession networkSession,
            LocalShutdownSignal signal)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _signal = signal ?? throw new ArgumentNullException(nameof(signal));
        }

        public void Tick()
        {
            if (_shutdownStarted || Time.unscaledTime < _nextPollTime)
            {
                return;
            }

            _nextPollTime = Time.unscaledTime + PollIntervalSeconds;
            if (!File.Exists(_signal.Path))
            {
                return;
            }

            _shutdownStarted = true;
            Debug.Log($"[LocalProcess] Graceful shutdown requested through '{_signal.Path}'.");
            _ = ShutdownAsync();
        }

        private async Task ShutdownAsync()
        {
            try
            {
                await _networkSession.StopAsync();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                UnityEngine.Application.Quit();
            }
        }
    }

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
