using Shooter.Application;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ConnectingClientFlowScreenProvider : ProgressClientFlowScreenProvider
    {
        public override ClientFlowState State => ClientFlowState.Connecting;

        protected override string Status => "Connecting to the allocated match server...";
    }
}
