using System;
using NUnit.Framework;
using Shooter.Features.Player;

namespace Shooter.Tests.Features.Player
{
    public sealed class PlayerRespawnRulesTests
    {
        [Test]
        public void StoresValidDelay()
        {
            var rules = new PlayerRespawnRules(3f);

            Assert.That(rules.DelaySeconds, Is.EqualTo(3f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void RejectsInvalidDelay(float delaySeconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerRespawnRules(delaySeconds));
        }
    }
}
