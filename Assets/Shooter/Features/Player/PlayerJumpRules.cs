using System;

namespace Shooter.Features.Player
{
    public static class PlayerJumpRules
    {
        public static PlayerJumpState Simulate(PlayerJumpState state, bool jumpPressed, float jumpSpeed, float gravity, float deltaTime)
        {
            ValidateParameters(jumpSpeed, gravity, deltaTime);
            var velocity = StartJumpIfRequested(state, jumpPressed, jumpSpeed);
            velocity -= gravity * deltaTime;
            var height = state.Height + velocity * deltaTime;
            return height > 0f ? new PlayerJumpState(height, velocity) : new PlayerJumpState(0f, 0f);
        }

        private static float StartJumpIfRequested(PlayerJumpState state, bool jumpPressed, float jumpSpeed)
        {
            return jumpPressed && IsGrounded(state) ? jumpSpeed : state.Velocity;
        }

        private static bool IsGrounded(PlayerJumpState state)
        {
            return state.Height <= 0f && state.Velocity <= 0f;
        }

        private static void ValidateParameters(float jumpSpeed, float gravity, float deltaTime)
        {
            if (jumpSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(jumpSpeed));
            }

            if (gravity < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(gravity));
            }

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
        }
    }
}
