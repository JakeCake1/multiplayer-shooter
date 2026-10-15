using System;
using System.Threading;
using System.Threading.Tasks;
using Shooter.Application;

namespace Shooter.Bootstrap.Server
{
    public sealed class LocalServerAllocationService : IServerAllocationService
    {
        private readonly NetworkSessionStartRequest _request;

        public LocalServerAllocationService(NetworkSessionStartRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public event Action ShutdownRequested
        {
            add { }
            remove { }
        }

        public Task<NetworkSessionStartRequest> WaitForAllocationAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_request);
        }
    }
}
