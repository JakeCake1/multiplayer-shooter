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
            var factory = CreateFactory(controller);

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Menu, "offline"));
            var viewModel = request.ViewModel as MainMenuViewModel;

            Assert.That(request.Address, Is.EqualTo(ClientUiScreenAddresses.MainMenu));
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Problem, Is.EqualTo("offline"));
        }

        [Test]
        public void PlayingDoesNotRequestBlockingScreen()
        {
            var factory = CreateFactory(new StubClientFlowController());

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Playing));

            Assert.That(request, Is.Null);
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
            var match = new ClientMatchSnapshot(ClientMatchStage.Results, 2, 0f, 1, 4, 2, 3, 1);

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
            var providers = new IClientFlowScreenProvider[] { new BootingClientFlowScreenProvider(), new MenuClientFlowScreenProvider(controller), new SearchingClientFlowScreenProvider(), new ConnectingClientFlowScreenProvider(), new WaitingForPlayersClientFlowScreenProvider(), new PlayingClientFlowScreenProvider(), new ResultsClientFlowScreenProvider(controller) };
            return new ClientFlowScreenFactory(providers);
        }
    }
}
