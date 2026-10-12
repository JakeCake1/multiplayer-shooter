using System;

namespace Shooter.Features.Score
{
    public sealed class PlayerScoreRules
    {
        public int AwardKill(int currentKills)
        {
            if (currentKills < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentKills));
            }

            return checked(currentKills + 1);
        }
    }
}
