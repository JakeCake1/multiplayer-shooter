using System;

namespace Shooter.Features.Weapon
{
    public static class WeaponAimRules
    {
        public static bool TryNormalize(float horizontal, float vertical, float depth, out WeaponAimVector direction)
        {
            direction = default;
            if (!IsFinite(horizontal) || !IsFinite(vertical) || !IsFinite(depth))
            {
                return false;
            }

            var magnitudeSquared = horizontal * horizontal + vertical * vertical + depth * depth;
            if (magnitudeSquared <= float.Epsilon)
            {
                return false;
            }

            var inverseMagnitude = 1f / (float)Math.Sqrt(magnitudeSquared);
            direction = new WeaponAimVector(horizontal * inverseMagnitude, vertical * inverseMagnitude, depth * inverseMagnitude);
            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
