using NUnit.Framework;
using Shooter.Features.Weapon;

namespace Shooter.Tests.Features.Weapon
{
    public sealed class WeaponAimRulesTests
    {
        [Test]
        public void NormalizesValidDirection()
        {
            var valid = WeaponAimRules.TryNormalize(3f, 4f, out var direction);

            Assert.That(valid, Is.True);
            Assert.That(direction.Horizontal, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(direction.Vertical, Is.EqualTo(0.8f).Within(0.0001f));
        }

        [TestCase(0f, 0f)]
        [TestCase(float.NaN, 1f)]
        [TestCase(1f, float.PositiveInfinity)]
        public void RejectsInvalidDirection(float horizontal, float vertical)
        {
            Assert.That(WeaponAimRules.TryNormalize(horizontal, vertical, out _), Is.False);
        }
    }
}
