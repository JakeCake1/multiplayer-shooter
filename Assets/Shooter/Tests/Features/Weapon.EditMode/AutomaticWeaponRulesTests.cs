using System;
using NUnit.Framework;
using Shooter.Features.Weapon;

namespace Shooter.Tests.Features.Weapon
{
    public sealed class AutomaticWeaponRulesTests
    {
        [Test]
        public void ConvertsRoundsPerMinuteToShotInterval()
        {
            var rules = new AutomaticWeaponRules(600f);

            Assert.That(rules.RoundsPerMinute, Is.EqualTo(600f));
            Assert.That(rules.SecondsBetweenShots, Is.EqualTo(0.1f).Within(0.0001f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void RejectsInvalidFireRates(float roundsPerMinute)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutomaticWeaponRules(roundsPerMinute));
        }
    }
}
