using System.Threading.Tasks;
using NUnit.Framework;

namespace Shooter.Application.Tests
{
    public sealed class ClientFlowCoordinatorTests
    {
        [Test]
        public void StartsInBooting()
        {
            var coordinator = CreateCoordinator(new FakeNetworkSession());

            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Booting));
        }

        [Test]
        public async Task SuccessfulInitializationEntersMenu()
        {
            var coordinator = CreateCoordinator(new FakeNetworkSession());

            await coordinator.InitializeAsync();

            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Menu));
            Assert.That(coordinator.Current.Problem, Is.Empty);
        }

        [Test]
        public async Task AuthenticationFailureEntersMenuWithProblem()
        {
            var authentication = new FakeAuthenticationService { Result = AuthenticationResult.Failure("authentication offline") };
            var coordinator = CreateCoordinator(new FakeNetworkSession(), authentication);

            await coordinator.InitializeAsync();

            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Menu));
            Assert.That(coordinator.Current.Problem, Does.Contain("authentication offline"));
        }

        [Test]
        public async Task SuccessfulFindGameUsesMatchmakingRequest()
        {
            var request = NetworkSessionStartRequest.ForClient("allocated-session");
            var matchmaking = new FakeMatchmakingService { Result = MatchmakingResult.Success(request) };
            var session = new FakeNetworkSession();
            var coordinator = CreateCoordinator(session, matchmakingService: matchmaking);
            await coordinator.InitializeAsync();

            await coordinator.FindGameAsync();

            Assert.That(matchmaking.CallCount, Is.EqualTo(1));
            Assert.That(session.LastRequest, Is.SameAs(request));
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.WaitingForPlayers));
        }

        [Test]
        public async Task MatchmakingFailureReturnsToMenuWithoutStartingNetwork()
        {
            var matchmaking = new FakeMatchmakingService { Result = MatchmakingResult.Failure("queue unavailable") };
            var session = new FakeNetworkSession();
            var coordinator = CreateCoordinator(session, matchmakingService: matchmaking);
            await coordinator.InitializeAsync();

            await coordinator.FindGameAsync();

            Assert.That(session.StartCount, Is.Zero);
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Menu));
            Assert.That(coordinator.Current.Problem, Does.Contain("queue unavailable"));
        }

        [Test]
        public async Task FailedConnectionReturnsToMenuWithProblem()
        {
            var session = new FakeNetworkSession { StartResult = NetworkSessionStartResult.Failure(NetworkSessionError.ConnectionFailed, "offline") };
            var coordinator = CreateCoordinator(session);
            await coordinator.InitializeAsync();

            await coordinator.FindGameAsync();

            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Menu));
            Assert.That(coordinator.Current.Problem, Does.Contain("offline"));
        }

        [Test]
        public async Task AuthoritativeSnapshotsDrivePlayingAndResults()
        {
            var coordinator = CreateCoordinator(new FakeNetworkSession());
            await coordinator.InitializeAsync();
            await coordinator.FindGameAsync();

            coordinator.ObserveMatch(new ClientMatchSnapshot(ClientMatchStage.Playing, 2, 30f));
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Playing));

            coordinator.ObserveMatch(new ClientMatchSnapshot(ClientMatchStage.Results, 2, 0f, 1, 3, 2, 1, 1));
            Assert.That(coordinator.Current.State, Is.EqualTo(ClientFlowState.Results));
            Assert.That(coordinator.Current.Match.WinnerPlayerId, Is.EqualTo(1));
        }

        [Test]
        public async Task FindGameAgainStopsCompletedSessionBeforeSearching()
        {
            var session = new FakeNetworkSession();
            var coordinator = CreateCoordinator(session);
            await coordinator.InitializeAsync();
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
            var coordinator = new ClientFlowCoordinator(new FakeAuthenticationService(), new FakeMatchmakingService(), new FakeNetworkSession(), quitter);

            coordinator.Exit();

            Assert.That(quitter.WasRequested, Is.True);
        }

        private static ClientFlowCoordinator CreateCoordinator(FakeNetworkSession session, FakeAuthenticationService authenticationService = null, FakeMatchmakingService matchmakingService = null)
        {
            return new ClientFlowCoordinator(authenticationService ?? new FakeAuthenticationService(), matchmakingService ?? new FakeMatchmakingService(), session, new FakeApplicationQuitter());
        }
    }
}
