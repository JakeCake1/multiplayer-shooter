using System;
using System.Threading.Tasks;
using Shooter.Application;

namespace Shooter.Presentation.Tests
{
    public sealed class StubClientFlowController : IClientFlowController
    {
        public event Action<ClientFlowSnapshot> Changed;

        public ClientFlowSnapshot Current { get; private set; } = new ClientFlowSnapshot(ClientFlowState.Menu);

        public int FindGameRequestCount { get; private set; }

        public bool ExitRequested { get; private set; }

        public Task FindGameAsync()
        {
            FindGameRequestCount++;
            return Task.CompletedTask;
        }

        public void ObserveMatch(ClientMatchSnapshot snapshot)
        {
        }

        public void Exit()
        {
            ExitRequested = true;
        }

        public void Publish(ClientFlowSnapshot snapshot)
        {
            Current = snapshot;
            Changed?.Invoke(snapshot);
        }
    }
}
