using System;
using NUnit.Framework;

namespace Shooter.Features.Score.Tests
{
    public sealed class MatchResultRulesTests
    {
        [TestCase(3, 1, 1)]
        [TestCase(1, 3, 2)]
        public void HigherScoreWins(int firstKills, int secondKills, int expectedWinnerPlayerId)
        {
            var rules = new MatchResultRules();

            Assert.That(rules.DetermineWinnerPlayerId(1, firstKills, 2, secondKills), Is.EqualTo(expectedWinnerPlayerId));
        }

        [Test]
        public void EqualScoresProduceDraw()
        {
            var rules = new MatchResultRules();

            Assert.That(rules.DetermineWinnerPlayerId(1, 2, 2, 2), Is.EqualTo(MatchResultRules.DrawWinnerPlayerId));
        }

        [Test]
        public void InvalidPlayersAndScoresAreRejected()
        {
            var rules = new MatchResultRules();

            Assert.Throws<ArgumentOutOfRangeException>(() => rules.DetermineWinnerPlayerId(-1, 0, 2, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => rules.DetermineWinnerPlayerId(1, -1, 2, 0));
            Assert.Throws<ArgumentException>(() => rules.DetermineWinnerPlayerId(1, 0, 1, 0));
        }
    }
}
