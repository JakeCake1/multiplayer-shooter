using System;
using System.Threading.Tasks;

namespace Shooter.Application
{
    public interface IClientFlowController
    {
        event Action<ClientFlowSnapshot> Changed;

        ClientFlowSnapshot Current { get; }

        Task InitializeAsync();

        Task FindGameAsync();

        void ObserveMatch(ClientMatchSnapshot snapshot);

        void Exit();
    }
}
