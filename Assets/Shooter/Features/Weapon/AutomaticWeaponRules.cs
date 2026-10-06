using System;

namespace Shooter.Features.Weapon
{
    public sealed class AutomaticWeaponRules
    {
        public AutomaticWeaponRules(float roundsPerMinute, float range = 20f, int damagePerHit = 25)
        {
            ValidateRoundsPerMinute(roundsPerMinute);
            ValidateRange(range);
            ValidateDamage(damagePerHit);
            RoundsPerMinute = roundsPerMinute;
            SecondsBetweenShots = 60f / roundsPerMinute;
            Range = range;
            DamagePerHit = damagePerHit;
        }

        public float RoundsPerMinute { get; }

        public float SecondsBetweenShots { get; }

        public float Range { get; }

        public int DamagePerHit { get; }

        private static void ValidateRoundsPerMinute(float roundsPerMinute)
        {
            if (float.IsNaN(roundsPerMinute) || float.IsInfinity(roundsPerMinute) || roundsPerMinute <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(roundsPerMinute));
            }
        }

        private static void ValidateRange(float range)
        {
            if (float.IsNaN(range) || float.IsInfinity(range) || range <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }
        }

        private static void ValidateDamage(int damagePerHit)
        {
            if (damagePerHit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damagePerHit));
            }
        }
    }
}
