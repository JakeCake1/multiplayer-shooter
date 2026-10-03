using NUnit.Framework;
using Shooter.Features.MatchRules;

namespace Shooter.Tests.Features.MatchRules
{
    public sealed class MatchPhaseRulesTests
    {
        [TestCase(MatchPhase.WaitingForPlayers, false)]
        [TestCase(MatchPhase.Starting, false)]
        [TestCase(MatchPhase.Playing, true)]
        [TestCase(MatchPhase.Finishing, false)]
        [TestCase(MatchPhase.Finished, false)]
        public void GameplayInputIsAcceptedOnlyWhilePlaying(MatchPhase phase, bool expected)
        {
            Assert.That(MatchPhaseRules.AcceptsGameplayInput(phase), Is.EqualTo(expected));
        }
    }
}
