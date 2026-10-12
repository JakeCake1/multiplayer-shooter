using System;

namespace Shooter.Features.Weapon
{
    public static class WeaponAimRules
    {
        public static bool TryNormalize(float horizontal, float vertical, out WeaponAimDirection direction)
        {
            direction = default;
            if (!IsFinite(horizontal) || !IsFinite(vertical))
            {
                return false;
            }

            var magnitudeSquared = horizontal * horizontal + vertical * vertical;
            if (magnitudeSquared <= float.Epsilon)
            {
                return false;
            }

            var inverseMagnitude = 1f / (float)Math.Sqrt(magnitudeSquared);
            direction = new WeaponAimDirection(horizontal * inverseMagnitude, vertical * inverseMagnitude);
            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
