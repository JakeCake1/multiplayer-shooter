using System;

namespace Shooter.Features.Player
{
    public static class PlayerLookRules
    {
        public const float MinimumPitch = -85f;
        public const float MaximumPitch = 85f;

        public static bool TrySanitize(float yaw, float pitch, out PlayerLookRotation rotation)
        {
            rotation = default;
            if (!IsFinite(yaw) || !IsFinite(pitch))
            {
                return false;
            }

            rotation = new PlayerLookRotation(NormalizeYaw(yaw), Math.Clamp(pitch, MinimumPitch, MaximumPitch));
            return true;
        }

        private static float NormalizeYaw(float yaw)
        {
            var normalized = yaw % 360f;
            return normalized < 0f ? normalized + 360f : normalized;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
