using NUnit.Framework;
using Shooter.Features.Player;

namespace Shooter.Tests.Features.Player
{
    public sealed class PlayerLookRulesTests
    {
        [TestCase(450f, 90f, 90f, 85f)]
        [TestCase(-90f, -90f, 270f, -85f)]
        public void NormalizesYawAndClampsPitch(float yaw, float pitch, float expectedYaw, float expectedPitch)
        {
            var valid = PlayerLookRules.TrySanitize(yaw, pitch, out var rotation);

            Assert.That(valid, Is.True);
            Assert.That(rotation.Yaw, Is.EqualTo(expectedYaw));
            Assert.That(rotation.Pitch, Is.EqualTo(expectedPitch));
        }

        [TestCase(float.NaN, 0f)]
        [TestCase(0f, float.PositiveInfinity)]
        public void RejectsNonFiniteLook(float yaw, float pitch)
        {
            Assert.That(PlayerLookRules.TrySanitize(yaw, pitch, out _), Is.False);
        }
    }
}
