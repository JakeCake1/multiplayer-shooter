using System;
using NUnit.Framework;
using Shooter.Features.Health;

namespace Shooter.Tests.Features.Health
{
    public sealed class PlayerHealthRulesTests
    {
        [Test]
        public void DamageReducesHealthWithoutGoingBelowZero()
        {
            var rules = new PlayerHealthRules(100);

            Assert.That(rules.ApplyDamage(100, 25), Is.EqualTo(75));
            Assert.That(rules.ApplyDamage(10, 25), Is.Zero);
        }

        [Test]
        public void ZeroHealthIsDead()
        {
            var rules = new PlayerHealthRules(100);

            Assert.That(rules.IsDead(0), Is.True);
            Assert.That(rules.IsDead(1), Is.False);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void RejectsInvalidMaximumHealth(int maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerHealthRules(maxHealth));
        }

        [Test]
        public void RejectsInvalidDamageAndCurrentHealth()
        {
            var rules = new PlayerHealthRules(100);

            Assert.Throws<ArgumentOutOfRangeException>(() => rules.ApplyDamage(101, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => rules.ApplyDamage(100, -1));
        }
    }
}
