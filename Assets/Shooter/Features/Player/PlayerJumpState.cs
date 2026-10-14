using System;

namespace Shooter.Features.Player
{
    public readonly struct PlayerJumpState
    {
        public PlayerJumpState(float height, float velocity)
        {
            Height = height >= 0f ? height : throw new ArgumentOutOfRangeException(nameof(height));
            Velocity = velocity;
        }

        public float Height { get; }

        public float Velocity { get; }
    }
}
