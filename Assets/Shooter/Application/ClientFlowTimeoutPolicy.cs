using System;

namespace Shooter.Application
{
    public sealed class ClientFlowTimeoutPolicy
    {
        private static readonly TimeSpan DefaultCompletedSessionStopTimeout = TimeSpan.FromSeconds(10d);
        private static readonly TimeSpan DefaultConnectionStartTimeout = TimeSpan.FromSeconds(30d);

        public ClientFlowTimeoutPolicy() : this(DefaultCompletedSessionStopTimeout, DefaultConnectionStartTimeout)
        {
        }

        public ClientFlowTimeoutPolicy(TimeSpan completedSessionStopTimeout) : this(completedSessionStopTimeout, DefaultConnectionStartTimeout)
        {
        }

        public ClientFlowTimeoutPolicy(TimeSpan completedSessionStopTimeout, TimeSpan connectionStartTimeout)
        {
            ValidateTimeout(completedSessionStopTimeout, nameof(completedSessionStopTimeout));
            ValidateTimeout(connectionStartTimeout, nameof(connectionStartTimeout));
            CompletedSessionStopTimeout = completedSessionStopTimeout;
            ConnectionStartTimeout = connectionStartTimeout;
        }

        public TimeSpan CompletedSessionStopTimeout { get; }

        public TimeSpan ConnectionStartTimeout { get; }

        private static void ValidateTimeout(TimeSpan timeout, string parameterName)
        {
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Client flow timeouts must be positive.");
            }
        }
    }
}
