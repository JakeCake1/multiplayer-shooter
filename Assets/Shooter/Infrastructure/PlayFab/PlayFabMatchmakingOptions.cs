using System;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingOptions
    {
        private const ulong MaximumWatchdogTimeoutSeconds = int.MaxValue / 1000;

        public PlayFabMatchmakingOptions(string queueName, uint timeoutInSeconds = 120, uint completionGracePeriodInSeconds = 10, int playerCount = 2)
        {
            QueueName = RequireValue(queueName, nameof(queueName));
            TimeoutInSeconds = timeoutInSeconds > 0 ? timeoutInSeconds : throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds));
            CompletionWatchdogTimeout = CreateWatchdogTimeout(timeoutInSeconds, completionGracePeriodInSeconds);
            PlayerCount = playerCount > 0 ? playerCount : throw new ArgumentOutOfRangeException(nameof(playerCount));
        }

        public string QueueName { get; }

        public uint TimeoutInSeconds { get; }

        public TimeSpan CompletionWatchdogTimeout { get; }

        public int PlayerCount { get; }

        private static string RequireValue(string value, string parameterName)
        {
            return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", parameterName) : value;
        }

        private static TimeSpan CreateWatchdogTimeout(uint timeoutInSeconds, uint completionGracePeriodInSeconds)
        {
            var totalSeconds = (ulong)timeoutInSeconds + completionGracePeriodInSeconds;
            if (totalSeconds > MaximumWatchdogTimeoutSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(completionGracePeriodInSeconds), "The combined matchmaking timeout exceeds the supported asynchronous delay.");
            }

            return TimeSpan.FromSeconds(totalSeconds);
        }
    }
}
