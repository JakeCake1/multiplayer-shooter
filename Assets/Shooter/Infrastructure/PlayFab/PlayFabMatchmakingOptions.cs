using System;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingOptions
    {
        public PlayFabMatchmakingOptions(string queueName, uint timeoutInSeconds = 120, int playerCount = 2)
        {
            QueueName = RequireValue(queueName, nameof(queueName));
            TimeoutInSeconds = timeoutInSeconds > 0 ? timeoutInSeconds : throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds));
            PlayerCount = playerCount > 0 ? playerCount : throw new ArgumentOutOfRangeException(nameof(playerCount));
        }

        public string QueueName { get; }

        public uint TimeoutInSeconds { get; }

        public int PlayerCount { get; }

        private static string RequireValue(string value, string parameterName)
        {
            return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", parameterName) : value;
        }
    }
}
