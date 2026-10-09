using Shooter.Application;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class BootingClientFlowScreenProvider : ProgressClientFlowScreenProvider
    {
        public override ClientFlowState State => ClientFlowState.Booting;

        protected override string Status => "Preparing player authentication...";
    }
}
