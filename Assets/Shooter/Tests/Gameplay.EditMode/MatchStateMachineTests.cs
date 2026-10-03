using NUnit.Framework;

namespace Shooter.Gameplay.Tests
{
    public sealed class MatchStateMachineTests
    {
        private static MatchRules CreateRules()
        {
            return new MatchRules(
                requiredPlayerCount: 2,
                countdownSeconds: 3f,
                matchSeconds: 60f,
                finishingSeconds: 1f);
        }

        [Test]
        public void StartsInWaitingForPlayers()
        {
            var machine = new MatchStateMachine(CreateRules());

            Assert.That(machine.Phase, Is.EqualTo(MatchPhase.WaitingForPlayers));
            Assert.That(machine.SecondsRemaining, Is.Zero);
        }

        [Test]
        public void RequiredPlayersStartCountdown()
        {
            var machine = new MatchStateMachine(CreateRules());

            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 0f);

            Assert.That(machine.Phase, Is.EqualTo(MatchPhase.Starting));
            Assert.That(machine.SecondsRemaining, Is.EqualTo(3f));
        }

        [Test]
        public void PlayerLeavingDuringCountdownReturnsToWaiting()
        {
            var machine = new MatchStateMachine(CreateRules());
            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 0f);

            machine.Advance(connectedPlayerCount: 1, deltaSeconds: 1f);

            Assert.That(machine.Phase, Is.EqualTo(MatchPhase.WaitingForPlayers));
            Assert.That(machine.SecondsRemaining, Is.Zero);
        }

        [Test]
        public void TimerDrivesPlayingFinishingAndFinished()
        {
            var machine = new MatchStateMachine(CreateRules());
            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 0f);

            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 3f);
            Assert.That(machine.Phase, Is.EqualTo(MatchPhase.Playing));
            Assert.That(machine.SecondsRemaining, Is.EqualTo(60f));

            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 60f);
            Assert.That(machine.Phase, Is.EqualTo(MatchPhase.Finishing));
            Assert.That(machine.SecondsRemaining, Is.EqualTo(1f));

            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 1f);
            Assert.That(machine.Phase, Is.EqualTo(MatchPhase.Finished));
            Assert.That(machine.SecondsRemaining, Is.Zero);
        }

        [Test]
        public void LargeDeltaCanCompleteAllTimedPhases()
        {
            var machine = new MatchStateMachine(CreateRules());
            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 0f);

            machine.Advance(connectedPlayerCount: 2, deltaSeconds: 64f);

            Assert.That(machine.Phase, Is.EqualTo(MatchPhase.Finished));
        }
    }
}
