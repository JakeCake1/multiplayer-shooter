using System;

namespace Shooter.Features.Player
{
    public static class PlayerLookRules
    {
        public static bool TrySanitize(float yaw, float pitch, float minimumPitch, float maximumPitch, out PlayerLookRotation rotation)
        {
            rotation = default;
            if (!IsFinite(yaw) || !IsFinite(pitch) || !IsFinite(minimumPitch) || !IsFinite(maximumPitch) || minimumPitch >= maximumPitch)
            {
                return false;
            }

            rotation = new PlayerLookRotation(NormalizeYaw(yaw), Math.Clamp(pitch, minimumPitch, maximumPitch));
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
