using System;

namespace Shooter.Application
{
    public sealed class ClientFlowTimeoutPolicy
    {
        private static readonly TimeSpan DefaultCompletedSessionStopTimeout = TimeSpan.FromSeconds(10d);

        public ClientFlowTimeoutPolicy() : this(DefaultCompletedSessionStopTimeout)
        {
        }

        public ClientFlowTimeoutPolicy(TimeSpan completedSessionStopTimeout)
        {
            if (completedSessionStopTimeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(completedSessionStopTimeout), "The completed-session stop timeout must be positive.");
            }

            CompletedSessionStopTimeout = completedSessionStopTimeout;
        }

        public TimeSpan CompletedSessionStopTimeout { get; }
    }
}
