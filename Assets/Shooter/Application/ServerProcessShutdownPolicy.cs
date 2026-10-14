using System;

namespace Shooter.Application
{
    public sealed class ServerProcessShutdownPolicy
    {
        public const int DefaultMatchFinishedGracePeriodSeconds = 10;

        public ServerProcessShutdownPolicy(bool shutdownAfterMatchFinished, TimeSpan matchFinishedGracePeriod)
        {
            if (matchFinishedGracePeriod < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(matchFinishedGracePeriod));
            }

            ShutdownAfterMatchFinished = shutdownAfterMatchFinished;
            MatchFinishedGracePeriod = matchFinishedGracePeriod;
        }

        public bool ShutdownAfterMatchFinished { get; }

        public TimeSpan MatchFinishedGracePeriod { get; }
    }
}
