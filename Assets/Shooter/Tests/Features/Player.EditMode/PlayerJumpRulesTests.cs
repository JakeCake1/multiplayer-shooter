using NUnit.Framework;

namespace Shooter.Features.Player.Tests
{
    public sealed class PlayerJumpRulesTests
    {
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        public void JumpStartsOnlyForGroundedPressedInput(bool jumpPressed, bool isGrounded, bool expected)
        {
            Assert.That(PlayerJumpRules.CanStartJump(jumpPressed, isGrounded), Is.EqualTo(expected));
        }

        [Test]
        public void GroundedJumpStartsUpwardMovement()
        {
            var result = PlayerJumpRules.Simulate(new PlayerJumpState(0f, 0f), true, 7f, 20f, 0.02f);

            Assert.That(result.Height, Is.GreaterThan(0f));
            Assert.That(result.Velocity, Is.GreaterThan(0f));
        }

        [Test]
        public void AirborneJumpDoesNotResetVelocity()
        {
            var result = PlayerJumpRules.Simulate(new PlayerJumpState(1f, 2f), true, 7f, 20f, 0.02f);

            Assert.That(result.Velocity, Is.EqualTo(1.6f).Within(0.0001f));
        }

        [Test]
        public void FallingMovementStopsAtGround()
        {
            var result = PlayerJumpRules.Simulate(new PlayerJumpState(0.01f, -2f), false, 7f, 20f, 0.02f);

            Assert.That(result.Height, Is.Zero);
            Assert.That(result.Velocity, Is.Zero);
        }

        [TestCase(-1f, 20f, 0.02f)]
        [TestCase(7f, -1f, 0.02f)]
        [TestCase(7f, 20f, -0.02f)]
        public void RejectsNegativeParameters(float jumpSpeed, float gravity, float deltaTime)
        {
            Assert.That(() => PlayerJumpRules.Simulate(new PlayerJumpState(0f, 0f), false, jumpSpeed, gravity, deltaTime), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
