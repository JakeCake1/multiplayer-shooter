using NUnit.Framework;
using Shooter.Application;

namespace Shooter.Tests.Application.EditMode
{
    public sealed class ClientMatchSnapshotTests
    {
        [Test]
        public void ObservableStateIgnoresSubSecondTimerChanges()
        {
            var first = CreateMatch(12.9f, confirmedHitCount: 2);
            var second = CreateMatch(12.1f, confirmedHitCount: 2);

            Assert.That(first.HasSameObservableState(second), Is.True);
        }

        [Test]
        public void ObservableStateDetectsNestedPlayerChanges()
        {
            var first = CreateMatch(12.9f, confirmedHitCount: 2);
            var second = CreateMatch(12.9f, confirmedHitCount: 3);

            Assert.That(first.HasSameObservableState(second), Is.False);
        }

        [Test]
        public void ObservableStateDetectsRespawnCountdownChanges()
        {
            var first = CreateMatch(12.9f, confirmedHitCount: 2, respawnSecondsRemaining: 4);
            var second = CreateMatch(12.9f, confirmedHitCount: 2, respawnSecondsRemaining: 3);

            Assert.That(first.HasSameObservableState(second), Is.False);
        }

        private static ClientMatchSnapshot CreateMatch(float secondsRemaining, int confirmedHitCount, int respawnSecondsRemaining = 0)
        {
            var localPlayer = new ClientPlayerMatchSnapshot(1, 0, 100, 100, true, confirmedHitCount, respawnSecondsRemaining > 0, respawnSecondsRemaining);
            var remotePlayer = new ClientPlayerMatchSnapshot(2, 0, 100, 100);
            return new ClientMatchSnapshot(ClientMatchStage.Playing, 2, secondsRemaining, localPlayer, remotePlayer);
        }
    }
}
