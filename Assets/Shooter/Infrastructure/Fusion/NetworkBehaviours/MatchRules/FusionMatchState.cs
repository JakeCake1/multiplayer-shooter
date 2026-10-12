using System;
using global::Fusion;
using Shooter.Features.MatchRules;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionMatchState : NetworkBehaviour
    {
        [Networked]
        public MatchPhase Phase { get; private set; }

        [Networked]
        public int ConnectedPlayerCount { get; private set; }

        [Networked]
        public TickTimer PhaseTimer { get; private set; }

        public float SecondsRemaining => PhaseTimer.RemainingTime(Runner) ?? 0f;

        public void Publish(MatchPhase phase, int connectedPlayerCount, float secondsRemaining)
        {
            if (!Object.HasStateAuthority)
            {
                throw new InvalidOperationException("Only state authority can publish match state.");
            }

            ConnectedPlayerCount = connectedPlayerCount;
            Phase = phase;
            PhaseTimer = secondsRemaining > 0f ? TickTimer.CreateFromSeconds(Runner, secondsRemaining) : TickTimer.None;
        }
    }
}
