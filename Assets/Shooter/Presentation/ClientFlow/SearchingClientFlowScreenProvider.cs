using Shooter.Application;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class SearchingClientFlowScreenProvider : ProgressClientFlowScreenProvider
    {
        public override ClientFlowState State => ClientFlowState.Searching;

        protected override string Status => "Searching for a 1v1 match...";
    }
}
