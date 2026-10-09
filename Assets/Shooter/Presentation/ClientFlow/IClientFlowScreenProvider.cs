using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public interface IClientFlowScreenProvider
    {
        ClientFlowState State { get; }

        UiScreenRequest Create(ClientFlowSnapshot snapshot);
    }
}
