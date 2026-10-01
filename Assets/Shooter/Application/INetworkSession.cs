using System;
using System.Threading.Tasks;

namespace Shooter.Application
{
    public interface INetworkSession
    {
        event Action<NetworkSessionState> StateChanged;

        NetworkSessionState State { get; }

        Task<NetworkSessionStartResult> StartAsync(NetworkSessionStartRequest request);

        Task StopAsync();
    }
}
