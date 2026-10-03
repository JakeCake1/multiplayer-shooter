using System;

namespace Shooter.Features.MatchRules
{
    public sealed class MatchRules
    {
        public MatchRules(
            int requiredPlayerCount,
            float countdownSeconds,
            float matchSeconds,
            float finishingSeconds)
        {
            if (requiredPlayerCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredPlayerCount));
            }

            if (countdownSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(countdownSeconds));
            }

            if (matchSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(matchSeconds));
            }

            if (finishingSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(finishingSeconds));
            }

            RequiredPlayerCount = requiredPlayerCount;
            CountdownSeconds = countdownSeconds;
            MatchSeconds = matchSeconds;
            FinishingSeconds = finishingSeconds;
        }

        public int RequiredPlayerCount { get; }

        public float CountdownSeconds { get; }

        public float MatchSeconds { get; }

        public float FinishingSeconds { get; }
    }
}
