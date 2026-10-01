using System;
using System.Threading;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;

namespace Shooter.Bootstrap
{
    public sealed class LocalNetworkSessionStarter : IAsyncStartable
    {
        private readonly INetworkSession _networkSession;
        private readonly NetworkSessionStartRequest _request;

        public LocalNetworkSessionStarter(
            INetworkSession networkSession,
            NetworkSessionStartRequest request)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            cancellation.ThrowIfCancellationRequested();

            var result = await _networkSession.StartAsync(_request);

            if (cancellation.IsCancellationRequested)
            {
                await _networkSession.StopAsync();
                cancellation.ThrowIfCancellationRequested();
            }

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to start local {_request.Role} network session: " +
                    $"{result.Error}. {result.Detail}");
            }
        }
    }
}
