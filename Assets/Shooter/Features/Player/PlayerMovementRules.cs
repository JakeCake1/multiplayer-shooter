using System;

namespace Shooter.Features.Player
{
    public static class PlayerMovementRules
    {
        public static PlanarMovement CalculateDisplacement(
            PlanarMovement input,
            float speed,
            float deltaTime)
        {
            if (speed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            var horizontal = input.Horizontal;
            var vertical = input.Vertical;
            var squaredMagnitude = (horizontal * horizontal) + (vertical * vertical);

            if (squaredMagnitude > 1f)
            {
                var inverseMagnitude = 1f / (float)Math.Sqrt(squaredMagnitude);
                horizontal *= inverseMagnitude;
                vertical *= inverseMagnitude;
            }

            var distance = speed * deltaTime;
            return new PlanarMovement(horizontal * distance, vertical * distance);
        }
    }
}
