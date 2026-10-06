using System.Threading.Tasks;
using NUnit.Framework;

namespace Shooter.Application.Tests
{
    public sealed class ClientFlowCoordinatorTests
    {
        private static readonly NetworkSessionStartRequest Request = NetworkSessionStartRequest.ForClient("test-session");

        [Test]
        public void StartsInMenu()
        {
            var coordinator = CreateCoordinator(new FakeNetworkSession());

            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Menu));
        }

        [Test]
        public async Task SuccessfulFindGameWaitsForAuthoritativeMatchState()
        {
            var session = new FakeNetworkSession();
            var coordinator = CreateCoordinator(session);

            await coordinator.FindGameAsync();

            Assert.That(session.StartCount, Is.EqualTo(1));
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.WaitingForPlayers));
        }

        [Test]
        public async Task FailedFindGameReturnsToMenuWithProblem()
        {
            var session = new FakeNetworkSession { StartResult = NetworkSessionStartResult.Failure(NetworkSessionError.ConnectionFailed, "offline") };
            var coordinator = CreateCoordinator(session);

            await coordinator.FindGameAsync();

            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Menu));
            Assert.That(coordinator.Current.Problem, Does.Contain("offline"));
        }

        [Test]
        public async Task AuthoritativeSnapshotsDrivePlayingAndResults()
        {
            var coordinator = CreateCoordinator(new FakeNetworkSession());
            await coordinator.FindGameAsync();

            coordinator.ObserveMatch(new ClientMatchSnapshot(ClientMatchStage.Playing, 2, 30f));
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Playing));

            coordinator.ObserveMatch(new ClientMatchSnapshot(ClientMatchStage.Results, 2, 0f, 1, 3, 2, 1, 1));
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Results));
            Assert.That(coordinator.Current.Match.WinnerPlayerId, Is.EqualTo(1));
        }

        [Test]
        public async Task FindGameAgainStopsCompletedSessionBeforeStarting()
        {
            var session = new FakeNetworkSession();
            var coordinator = CreateCoordinator(session);
            await coordinator.FindGameAsync();
            coordinator.ObserveMatch(new ClientMatchSnapshot(ClientMatchStage.Results, 2, 0f));

            await coordinator.FindGameAsync();

            Assert.That(session.StopCount, Is.EqualTo(1));
            Assert.That(session.StartCount, Is.EqualTo(2));
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.WaitingForPlayers));
        }

        [Test]
        public void ExitUsesPlatformCapability()
        {
            var quitter = new FakeApplicationQuitter();
            var coordinator = new ClientFlowCoordinator(new FakeNetworkSession(), Request, quitter);

            coordinator.Exit();

            Assert.That(quitter.WasRequested, Is.True);
        }

        private static ClientFlowCoordinator CreateCoordinator(FakeNetworkSession session)
        {
            return new ClientFlowCoordinator(session, Request, new FakeApplicationQuitter());
        }
    }
}
