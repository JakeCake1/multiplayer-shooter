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
            var factory = new ClientFlowScreenFactory(controller);

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Menu, "offline"));
            var viewModel = request.ViewModel as MainMenuViewModel;

            Assert.That(request.Address, Is.EqualTo(ClientUiScreenAddresses.MainMenu));
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Problem, Is.EqualTo("offline"));
        }

        [Test]
        public void PlayingDoesNotRequestBlockingScreen()
        {
            var factory = new ClientFlowScreenFactory(new StubClientFlowController());

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Playing));

            Assert.That(request, Is.Null);
        }

        [Test]
        public void ResultsUseAuthoritativeSnapshot()
        {
            var factory = new ClientFlowScreenFactory(new StubClientFlowController());
            var match = new ClientMatchSnapshot(ClientMatchStage.Results, 2, 0f, 1, 4, 2, 3, 1);

            var request = factory.Create(new ClientFlowSnapshot(ClientFlowState.Results, match: match));
            var viewModel = request.ViewModel as ResultsViewModel;

            Assert.That(request.Address, Is.EqualTo(ClientUiScreenAddresses.Results));
            Assert.That(viewModel.Outcome, Is.EqualTo("Winner: Player 1"));
            Assert.That(viewModel.FirstPlayerScore, Is.EqualTo("Player 1: 4"));
            Assert.That(viewModel.SecondPlayerScore, Is.EqualTo("Player 2: 3"));
        }
    }
}
