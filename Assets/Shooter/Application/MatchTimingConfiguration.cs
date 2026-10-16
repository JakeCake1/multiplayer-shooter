using System;

namespace Shooter.Application
{
    public sealed class MatchTimingConfiguration
    {
        public MatchTimingConfiguration(int requiredPlayerCount, float countdownSeconds, float durationSeconds, float finishingSeconds, float serverShutdownGraceSeconds)
        {
            RequiredPlayerCount = requiredPlayerCount == 2 ? requiredPlayerCount : throw new ArgumentOutOfRangeException(nameof(requiredPlayerCount), "The current match result contract supports exactly two players.");
            CountdownSeconds = RequireNonNegative(countdownSeconds, nameof(countdownSeconds));
            DurationSeconds = RequirePositive(durationSeconds, nameof(durationSeconds));
            FinishingSeconds = RequireNonNegative(finishingSeconds, nameof(finishingSeconds));
            ServerShutdownGraceSeconds = RequireNonNegative(serverShutdownGraceSeconds, nameof(serverShutdownGraceSeconds));
        }

        public int RequiredPlayerCount { get; }

        public float CountdownSeconds { get; }

        public float DurationSeconds { get; }

        public float FinishingSeconds { get; }

        public float ServerShutdownGraceSeconds { get; }

        private static float RequirePositive(float value, string parameterName)
        {
            return value > 0f && IsFinite(value) ? value : throw new ArgumentOutOfRangeException(parameterName);
        }

        private static float RequireNonNegative(float value, string parameterName)
        {
            return value >= 0f && IsFinite(value) ? value : throw new ArgumentOutOfRangeException(parameterName);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
