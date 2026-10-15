#if ENABLE_PLAYFABSERVER_API
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.MultiplayerAgent.Model;
using Shooter.Application;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabServerAllocationService : IServerAllocationService, IDisposable
    {
        private const string GamePortName = "game_port";
        private readonly TaskCompletionSource<NetworkSessionStartRequest> _allocationCompletion = new TaskCompletionSource<NetworkSessionStartRequest>(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _started;
        private bool _disposed;

        public PlayFabServerAllocationService()
        {
            RegisterCallbacks();
        }

        public event Action ShutdownRequested;

        public async Task<NetworkSessionStartRequest> WaitForAllocationAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            StartAgentIfNeeded();
            using (cancellationToken.Register(() => _allocationCompletion.TrySetCanceled(cancellationToken)))
            {
                return await _allocationCompletion.Task;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            UnregisterCallbacks();
        }

        private void StartAgentIfNeeded()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            PlayFabServerEnvironment.ValidateConfigurationFile();
            PlayFabMultiplayerAgentAPI.IsDebugging = Debug.isDebugBuild;
            PlayFabMultiplayerAgentAPI.Start();
            LogServerEnvironment();
            PlayFabMultiplayerAgentAPI.ReadyForPlayers();
            Debug.Log("[PlayFab][Server] GSDK is standing by for a server allocation.");
        }

        private void RegisterCallbacks()
        {
            PlayFabMultiplayerAgentAPI.OnServerActiveCallback += HandleServerActive;
            PlayFabMultiplayerAgentAPI.OnShutDownCallback += HandleShutdown;
            PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += HandleMaintenance;
            PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += HandleAgentError;
        }

        private void UnregisterCallbacks()
        {
            PlayFabMultiplayerAgentAPI.OnServerActiveCallback -= HandleServerActive;
            PlayFabMultiplayerAgentAPI.OnShutDownCallback -= HandleShutdown;
            PlayFabMultiplayerAgentAPI.OnMaintenanceCallback -= HandleMaintenance;
            PlayFabMultiplayerAgentAPI.OnAgentErrorCallback -= HandleAgentError;
        }

        private void HandleServerActive()
        {
            try
            {
                var request = CreateNetworkRequest();
                _allocationCompletion.TrySetResult(request);
            }
            catch (Exception exception)
            {
                _allocationCompletion.TrySetException(exception);
            }
        }

        private static NetworkSessionStartRequest CreateNetworkRequest()
        {
            var configuration = PlayFabMultiplayerAgentAPI.GetConfigSettings();
            var sessionId = GetRequiredConfiguration(configuration, PlayFabMultiplayerAgentAPI.SessionIdKey);
            var port = SelectGamePort(PlayFabMultiplayerAgentAPI.GetGameServerConnectionInfo());
            LogAllocation(configuration, sessionId, port);
            return NetworkSessionStartRequest.ForServer(sessionId, checked((ushort)port.ServerListeningPort));
        }

        private static GamePort SelectGamePort(GameServerConnectionInfo connectionInfo)
        {
            var ports = connectionInfo?.GamePortsConfiguration?.ToArray() ?? Array.Empty<GamePort>();
            var namedPort = ports.FirstOrDefault(port => string.Equals(port.Name, GamePortName, StringComparison.OrdinalIgnoreCase));
            if (namedPort != null)
            {
                ValidateListeningPort(namedPort);
                return namedPort;
            }

            if (ports.Length == 1)
            {
                Debug.LogWarning($"[PlayFab][Server] Port '{GamePortName}' is not configured; using the only assigned port '{ports[0].Name}'.");
                ValidateListeningPort(ports[0]);
                return ports[0];
            }

            throw new InvalidOperationException($"PlayFab allocation must expose a UDP game port named '{GamePortName}'.");
        }

        private static void ValidateListeningPort(GamePort port)
        {
            if (port.ServerListeningPort < 1 || port.ServerListeningPort > ushort.MaxValue)
            {
                throw new InvalidOperationException($"PlayFab assigned invalid server listening port {port.ServerListeningPort} for '{port.Name}'.");
            }
        }

        private static string GetRequiredConfiguration(IDictionary<string, string> configuration, string key)
        {
            if (configuration.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            throw new InvalidOperationException($"PlayFab GSDK configuration does not contain '{key}'.");
        }

        private static void LogServerEnvironment()
        {
            var configuration = PlayFabMultiplayerAgentAPI.GetConfigSettings();
            var buildId = GetOptionalConfiguration(configuration, PlayFabMultiplayerAgentAPI.BuildIdKey);
            var region = GetOptionalConfiguration(configuration, PlayFabMultiplayerAgentAPI.RegionKey);
            var serverId = GetOptionalConfiguration(configuration, PlayFabMultiplayerAgentAPI.ServerIdKey);
            Debug.Log($"[PlayFab][Server] GSDK started. Build: '{buildId}', region: '{region}', server: '{serverId}'.");
        }

        private static void LogAllocation(IDictionary<string, string> configuration, string sessionId, GamePort port)
        {
            var region = GetOptionalConfiguration(configuration, PlayFabMultiplayerAgentAPI.RegionKey);
            var publicAddress = GetOptionalConfiguration(configuration, PlayFabMultiplayerAgentAPI.PublicIpV4AddressKey);
            var initialPlayerCount = PlayFabMultiplayerAgentAPI.GetInitialPlayers().Count;
            Debug.Log($"[PlayFab][Server] Allocation active. Session: '{sessionId}', region: '{region}', bind: {port.ServerListeningPort}, public endpoint: {publicAddress}:{port.ClientConnectionPort}, initial players: {initialPlayerCount}.");
        }

        private static string GetOptionalConfiguration(IDictionary<string, string> configuration, string key)
        {
            return configuration.TryGetValue(key, out var value) ? value : "unknown";
        }

        private void HandleShutdown()
        {
            Debug.Log("[PlayFab][Server] GSDK requested termination.");
            ShutdownRequested?.Invoke();
        }

        private static void HandleMaintenance(DateTime? maintenanceTimeUtc)
        {
            var description = maintenanceTimeUtc.HasValue ? maintenanceTimeUtc.Value.ToString("O") : "unspecified";
            Debug.LogWarning($"[PlayFab][Server] Scheduled maintenance: {description}.");
        }

        private static void HandleAgentError(string error)
        {
            Debug.LogError($"[PlayFab][Server] GSDK agent communication error: {error}");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlayFabServerAllocationService));
            }
        }
    }
}
#endif
