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
        private readonly IServerMatchLifecycle _matchLifecycle;
        private readonly ServerProcessShutdownPolicy _shutdownPolicy;
        private CancellationTokenSource _scheduledShutdownCancellation;
        private bool _shutdownStarted;

        public ServerNetworkSessionHost(INetworkSession networkSession, IServerAllocationService allocationService, IServerMatchLifecycle matchLifecycle, ServerProcessShutdownPolicy shutdownPolicy)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
            _matchLifecycle = matchLifecycle ?? throw new ArgumentNullException(nameof(matchLifecycle));
            _shutdownPolicy = shutdownPolicy ?? throw new ArgumentNullException(nameof(shutdownPolicy));
            _allocationService.ShutdownRequested += HandleShutdownRequested;
            _matchLifecycle.MatchFinished += HandleMatchFinished;
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
            _matchLifecycle.MatchFinished -= HandleMatchFinished;
            CancelScheduledShutdown();
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
            BeginShutdown("the allocation service requested termination");
        }

        private void HandleMatchFinished()
        {
            if (!_shutdownPolicy.ShutdownAfterMatchFinished || _shutdownStarted || _scheduledShutdownCancellation != null)
            {
                return;
            }

            _scheduledShutdownCancellation = new CancellationTokenSource();
            Debug.Log($"[Server] Match finished. Process shutdown scheduled in {_shutdownPolicy.MatchFinishedGracePeriod.TotalSeconds:0.0}s.");
            _ = ShutdownAfterGracePeriodAsync(_scheduledShutdownCancellation.Token);
        }

        private async Task ShutdownAfterGracePeriodAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(_shutdownPolicy.MatchFinishedGracePeriod, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            BeginShutdown("the post-match grace period elapsed");
        }

        private void BeginShutdown(string reason)
        {
            if (_shutdownStarted)
            {
                return;
            }

            _shutdownStarted = true;
            CancelScheduledShutdown();
            Debug.Log($"[Server] Graceful shutdown started because {reason}.");
            _ = ShutdownAsync();
        }

        private void CancelScheduledShutdown()
        {
            if (_scheduledShutdownCancellation == null)
            {
                return;
            }

            _scheduledShutdownCancellation.Cancel();
            _scheduledShutdownCancellation.Dispose();
            _scheduledShutdownCancellation = null;
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
