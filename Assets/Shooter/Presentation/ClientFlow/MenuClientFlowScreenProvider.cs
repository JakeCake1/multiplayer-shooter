using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class MenuClientFlowScreenProvider : IClientFlowScreenProvider
    {
        private readonly IClientFlowController _flowController;

        public MenuClientFlowScreenProvider(IClientFlowController flowController)
        {
            _flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
        }

        public ClientFlowState State => ClientFlowState.Menu;

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return new UiScreenRequest(ClientUiScreenAddresses.MainMenu, new MainMenuViewModel(snapshot.Problem, _flowController.FindGameAsync, _flowController.Exit));
        }
    }
}
