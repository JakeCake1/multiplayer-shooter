using System;

namespace Shooter.Application
{
    public sealed class PlayerGameplayConfiguration
    {
        public PlayerGameplayConfiguration(float movementSpeed, float movementAcceleration, float movementBraking, float jumpSpeed, float gravity, int maximumHealth, float respawnDelaySeconds, float minimumLookPitch, float maximumLookPitch)
        {
            MovementSpeed = RequirePositive(movementSpeed, nameof(movementSpeed));
            MovementAcceleration = RequirePositive(movementAcceleration, nameof(movementAcceleration));
            MovementBraking = RequirePositive(movementBraking, nameof(movementBraking));
            JumpSpeed = RequirePositive(jumpSpeed, nameof(jumpSpeed));
            Gravity = gravity < 0f && IsFinite(gravity) ? gravity : throw new ArgumentOutOfRangeException(nameof(gravity));
            MaximumHealth = maximumHealth > 0 ? maximumHealth : throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            RespawnDelaySeconds = RequirePositive(respawnDelaySeconds, nameof(respawnDelaySeconds));
            MinimumLookPitch = RequireFinite(minimumLookPitch, nameof(minimumLookPitch));
            MaximumLookPitch = RequireFinite(maximumLookPitch, nameof(maximumLookPitch));
            if (MinimumLookPitch >= MaximumLookPitch)
            {
                throw new ArgumentException("Minimum look pitch must be less than maximum look pitch.");
            }
        }

        public float MovementSpeed { get; }

        public float MovementAcceleration { get; }

        public float MovementBraking { get; }

        public float JumpSpeed { get; }

        public float Gravity { get; }

        public int MaximumHealth { get; }

        public float RespawnDelaySeconds { get; }

        public float MinimumLookPitch { get; }

        public float MaximumLookPitch { get; }

        private static float RequirePositive(float value, string parameterName)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value) ? value : throw new ArgumentOutOfRangeException(parameterName);
        }

        private static float RequireFinite(float value, string parameterName)
        {
            return IsFinite(value) ? value : throw new ArgumentOutOfRangeException(parameterName);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
