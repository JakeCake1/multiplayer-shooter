using System;
using Shooter.Application;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class SearchingClientFlowScreenProvider : ProgressClientFlowScreenProvider
    {
        private readonly IClientFlowController _controller;

        public SearchingClientFlowScreenProvider(IClientFlowController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public override ClientFlowState State => ClientFlowState.Searching;

        protected override string Status => "Searching for a 1v1 match...";

        protected override Action Cancel => _controller.CancelSearch;
    }
}
