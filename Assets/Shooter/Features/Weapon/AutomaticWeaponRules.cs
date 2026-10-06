using System;

namespace Shooter.Features.Weapon
{
    public sealed class AutomaticWeaponRules
    {
        public AutomaticWeaponRules(float roundsPerMinute)
        {
            ValidateRoundsPerMinute(roundsPerMinute);
            RoundsPerMinute = roundsPerMinute;
            SecondsBetweenShots = 60f / roundsPerMinute;
        }

        public float RoundsPerMinute { get; }

        public float SecondsBetweenShots { get; }

        private static void ValidateRoundsPerMinute(float roundsPerMinute)
        {
            if (float.IsNaN(roundsPerMinute) || float.IsInfinity(roundsPerMinute) || roundsPerMinute <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(roundsPerMinute));
            }
        }
    }
}
