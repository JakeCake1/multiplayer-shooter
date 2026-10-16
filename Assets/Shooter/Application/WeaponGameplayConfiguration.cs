using System;

namespace Shooter.Application
{
    public sealed class WeaponGameplayConfiguration
    {
        public WeaponGameplayConfiguration(float roundsPerMinute, float range, int damagePerHit, float shotOriginHeight, float shotOriginForwardOffset)
        {
            RoundsPerMinute = RequirePositive(roundsPerMinute, nameof(roundsPerMinute));
            Range = RequirePositive(range, nameof(range));
            DamagePerHit = damagePerHit > 0 ? damagePerHit : throw new ArgumentOutOfRangeException(nameof(damagePerHit));
            ShotOriginHeight = RequireFinite(shotOriginHeight, nameof(shotOriginHeight));
            ShotOriginForwardOffset = RequireFinite(shotOriginForwardOffset, nameof(shotOriginForwardOffset));
        }

        public float RoundsPerMinute { get; }

        public float Range { get; }

        public int DamagePerHit { get; }

        public float ShotOriginHeight { get; }

        public float ShotOriginForwardOffset { get; }

        private static float RequirePositive(float value, string parameterName)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value) ? value : throw new ArgumentOutOfRangeException(parameterName);
        }

        private static float RequireFinite(float value, string parameterName)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) ? value : throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
