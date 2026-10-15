using NUnit.Framework;
using Shooter.Features.Weapon;

namespace Shooter.Tests.Features.Weapon
{
    public sealed class WeaponAimRulesTests
    {
        [Test]
        public void NormalizesValidDirection()
        {
            var valid = WeaponAimRules.TryNormalize(2f, -1f, 2f, out var direction);

            Assert.That(valid, Is.True);
            Assert.That(direction.Horizontal, Is.EqualTo(2f / 3f).Within(0.0001f));
            Assert.That(direction.Vertical, Is.EqualTo(-1f / 3f).Within(0.0001f));
            Assert.That(direction.Depth, Is.EqualTo(2f / 3f).Within(0.0001f));
        }

        [TestCase(0f, 0f, 0f)]
        [TestCase(float.NaN, 0f, 1f)]
        [TestCase(0f, float.PositiveInfinity, 1f)]
        public void RejectsInvalidDirection(float horizontal, float vertical, float depth)
        {
            Assert.That(WeaponAimRules.TryNormalize(horizontal, vertical, depth, out _), Is.False);
        }
    }
}
