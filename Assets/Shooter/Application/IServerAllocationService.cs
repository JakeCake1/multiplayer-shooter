using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Application
{
    public interface IServerAllocationService
    {
        event Action ShutdownRequested;

        Task<NetworkSessionStartRequest> WaitForAllocationAsync(CancellationToken cancellationToken);
    }
}
