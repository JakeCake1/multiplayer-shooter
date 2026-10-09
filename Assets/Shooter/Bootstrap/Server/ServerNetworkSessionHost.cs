using System;
using System.Threading;
using System.Threading.Tasks;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;

namespace Shooter.Bootstrap.Server
{
    public sealed class ServerNetworkSessionHost : IAsyncStartable, IDisposable
    {
        private readonly INetworkSession _networkSession;
        private readonly IServerAllocationService _allocationService;
        private bool _shutdownStarted;

        public ServerNetworkSessionHost(INetworkSession networkSession, IServerAllocationService allocationService)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
            _allocationService.ShutdownRequested += HandleShutdownRequested;
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            var request = await _allocationService.WaitForAllocationAsync(cancellation);
            cancellation.ThrowIfCancellationRequested();
            var result = await _networkSession.StartAsync(request);
            await FinishStartAsync(request, result, cancellation);
        }

        public void Dispose()
        {
            _allocationService.ShutdownRequested -= HandleShutdownRequested;
        }

        private async Task FinishStartAsync(NetworkSessionStartRequest request, NetworkSessionStartResult result, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await _networkSession.StopAsync();
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to start {request.Role} network session: {result.Error}. {result.Detail}");
            }

            Debug.Log($"[Server] Network session '{request.SessionName}' is listening on port {request.Port}.");
        }

        private void HandleShutdownRequested()
        {
            if (_shutdownStarted)
            {
                return;
            }

            _shutdownStarted = true;
            Debug.Log("[Server] Graceful shutdown requested by the allocation service.");
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
}
