using NUnit.Framework;

namespace Shooter.Features.Player.Tests
{
    public sealed class PlayerMovementRulesTests
    {
        [Test]
        public void CalculateDisplacement_PreservesSubUnitInput()
        {
            var displacement = PlayerMovementRules.CalculateDisplacement(
                new PlanarMovement(0.5f, 0f),
                speed: 4f,
                deltaTime: 0.25f);

            Assert.That(displacement.Horizontal, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(displacement.Vertical, Is.Zero);
        }

        [Test]
        public void CalculateDisplacement_NormalizesDiagonalInput()
        {
            var displacement = PlayerMovementRules.CalculateDisplacement(
                new PlanarMovement(1f, 1f),
                speed: 5f,
                deltaTime: 1f);

            var distance = System.Math.Sqrt(
                (displacement.Horizontal * displacement.Horizontal) +
                (displacement.Vertical * displacement.Vertical));

            Assert.That(distance, Is.EqualTo(5d).Within(0.0001d));
        }

        [TestCase(-1f, 0.02f)]
        [TestCase(1f, -0.02f)]
        public void CalculateDisplacement_RejectsNegativeParameters(float speed, float deltaTime)
        {
            Assert.That(
                () => PlayerMovementRules.CalculateDisplacement(
                    new PlanarMovement(1f, 0f),
                    speed,
                    deltaTime),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
