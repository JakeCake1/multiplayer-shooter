using NUnit.Framework;
using Shooter.Application;
using Shooter.Presentation.ClientFlow;

namespace Shooter.Presentation.Tests
{
    public sealed class ClientFlowScreenFactoryTests
    {
        [Test]
        public void MenuMapsToMainMenuViewModelAndCommands()
        {
            var controller = new StubClientFlowController();
            var inputSettings = new StubClientInputSettings();
            var factory = CreateFactory(controller, inputSettings);

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Menu, "offline"));
            var viewModel = request.ViewModel as MainMenuViewModel;

            Assert.That(request.Address, Is.EqualTo(ClientUiScreenAddresses.MainMenu));
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Problem, Is.EqualTo("offline"));
            Assert.That(viewModel.MouseSensitivity, Is.EqualTo(2.5f));
            viewModel.SetMouseSensitivity(3f);
            Assert.That(inputSettings.MouseSensitivityMultiplier, Is.EqualTo(3f));
        }

        [Test]
        public void PlayingUsesLiveMatchHud()
        {
            var factory = CreateFactory(new StubClientFlowController());
            var firstPlayer = new ClientPlayerMatchSnapshot(1, 2, 75, 100, true);
            var secondPlayer = new ClientPlayerMatchSnapshot(2, 1, 40, 100);
            var match = new ClientMatchSnapshot(ClientMatchStage.Playing, 2, 65f, firstPlayer, secondPlayer);

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Playing, match: match));
            var viewModel = request.ViewModel as PlayingHudViewModel;

            Assert.That(request.Address, Is.EqualTo(ClientUiScreenAddresses.PlayingHud));
            Assert.That(viewModel.TimeRemaining, Is.EqualTo("01:05"));
            Assert.That(viewModel.FirstPlayer, Is.EqualTo("You (Player 1)   HP 75/100   Kills 2"));
            Assert.That(viewModel.SecondPlayer, Is.EqualTo("Player 2   HP 40/100   Kills 1"));
        }

        [TestCase(ClientFlowState.Booting, "Preparing player authentication...")]
        [TestCase(ClientFlowState.Searching, "Searching for a 1v1 match...")]
        [TestCase(ClientFlowState.Connecting, "Connecting to the allocated match server...")]
        public void ProgressStatesUseConnectingScreen(ClientFlowState state, string status)
        {
            var factory = CreateFactory(new StubClientFlowController());

            var request = factory.Create(new ClientFlowSnapshot(state));
            var viewModel = request.ViewModel as ConnectingViewModel;

            Assert.That(request.Address, Is.EqualTo(ClientUiScreenAddresses.Connecting));
            Assert.That(viewModel.Status, Is.EqualTo(status));
        }

        [Test]
        public void ResultsUseAuthoritativeSnapshot()
        {
            var factory = CreateFactory(new StubClientFlowController());
            var match = new ClientMatchSnapshot(ClientMatchStage.Results, 2, 0f, new ClientPlayerMatchSnapshot(1, 4), new ClientPlayerMatchSnapshot(2, 3), 1);

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Results, match: match));
            var viewModel = request.ViewModel as ResultsViewModel;

            Assert.That(request.Address, Is.EqualTo(ClientUiScreenAddresses.Results));
            Assert.That(viewModel.Outcome, Is.EqualTo("Winner: Player 1"));
            Assert.That(viewModel.FirstPlayerScore, Is.EqualTo("Player 1: 4"));
            Assert.That(viewModel.SecondPlayerScore, Is.EqualTo("Player 2: 3"));
        }

        [Test]
        public void DuplicateStateProvidersAreRejected()
        {
            var providers = new IClientFlowScreenProvider[] { new BootingClientFlowScreenProvider(), new BootingClientFlowScreenProvider() };

            Assert.That(() => new ClientFlowScreenFactory(providers), Throws.InvalidOperationException);
        }

        [Test]
        public void MissingStateProviderIsRejectedWhenRequested()
        {
            var providers = new IClientFlowScreenProvider[] { new BootingClientFlowScreenProvider() };
            var factory = new ClientFlowScreenFactory(providers);

            Assert.That(() => factory.Create(new ClientFlowSnapshot(ClientFlowState.Menu)), Throws.InvalidOperationException);
        }

        private static ClientFlowScreenFactory CreateFactory(StubClientFlowController controller)
        {
            return CreateFactory(controller, new StubClientInputSettings());
        }

        private static ClientFlowScreenFactory CreateFactory(StubClientFlowController controller, StubClientInputSettings inputSettings)
        {
            var providers = new IClientFlowScreenProvider[] { new BootingClientFlowScreenProvider(), new MenuClientFlowScreenProvider(controller, inputSettings), new SearchingClientFlowScreenProvider(), new ConnectingClientFlowScreenProvider(), new WaitingForPlayersClientFlowScreenProvider(), new PlayingClientFlowScreenProvider(), new ResultsClientFlowScreenProvider(controller) };
            return new ClientFlowScreenFactory(providers);
        }
    }
}
