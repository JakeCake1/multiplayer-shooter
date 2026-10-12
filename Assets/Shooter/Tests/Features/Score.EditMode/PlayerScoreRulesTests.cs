using System;
using NUnit.Framework;

namespace Shooter.Features.Score.Tests
{
    public sealed class PlayerScoreRulesTests
    {
        [Test]
        public void AwardKillIncrementsCurrentKills()
        {
            var rules = new PlayerScoreRules();

            Assert.That(rules.AwardKill(0), Is.EqualTo(1));
            Assert.That(rules.AwardKill(4), Is.EqualTo(5));
        }

        [Test]
        public void AwardKillRejectsInvalidCurrentKills()
        {
            var rules = new PlayerScoreRules();

            Assert.Throws<ArgumentOutOfRangeException>(() => rules.AwardKill(-1));
            Assert.Throws<OverflowException>(() => rules.AwardKill(int.MaxValue));
        }
    }
}
