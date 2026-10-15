using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class MenuClientFlowScreenProvider : IClientFlowScreenProvider
    {
        private readonly IClientFlowController _flowController;
        private readonly IClientInputSettings _inputSettings;

        public MenuClientFlowScreenProvider(IClientFlowController flowController, IClientInputSettings inputSettings)
        {
            _flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
            _inputSettings = inputSettings ?? throw new ArgumentNullException(nameof(inputSettings));
        }

        public ClientFlowState State => ClientFlowState.Menu;

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return new UiScreenRequest(ClientUiScreenAddresses.MainMenu, CreateViewModel(snapshot));
        }

        private MainMenuViewModel CreateViewModel(ClientFlowSnapshot snapshot)
        {
            return new MainMenuViewModel(snapshot.Problem, snapshot.IsAuthenticationReady, _inputSettings.MouseSensitivityMultiplier, _inputSettings.MinimumMouseSensitivityMultiplier, _inputSettings.MaximumMouseSensitivityMultiplier, SetMouseSensitivity, _flowController.RetryAuthenticationAsync, _flowController.FindGameAsync, _flowController.Exit);
        }

        private void SetMouseSensitivity(float value)
        {
            _inputSettings.MouseSensitivityMultiplier = value;
        }
    }
}
