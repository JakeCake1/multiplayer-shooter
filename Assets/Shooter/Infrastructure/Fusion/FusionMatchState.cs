using System;
using global::Fusion;
using Shooter.Gameplay;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionMatchState : NetworkBehaviour
    {
        private MatchRules _rules;
        private MatchStateMachine _stateMachine;
        private MatchPhase _lastObservedPhase = (MatchPhase)(-1);

        [Networked]
        public MatchPhase Phase { get; private set; }

        [Networked]
        public int ConnectedPlayerCount { get; private set; }

        [Networked]
        public TickTimer PhaseTimer { get; private set; }

        public float SecondsRemaining => PhaseTimer.RemainingTime(Runner) ?? 0f;

        public void Configure(MatchRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                _stateMachine = new MatchStateMachine(
                    _rules ?? throw new InvalidOperationException(
                        "Server match rules must be configured before spawning the match state."));
                PublishState(CountConnectedPlayers(), forcePhasePublish: true);
            }

            ObserveReplicatedPhase();
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return;
            }

            var connectedPlayerCount = CountConnectedPlayers();
            var previousPhase = _stateMachine.Phase;
            _stateMachine.Advance(connectedPlayerCount, Runner.DeltaTime);
            PublishState(
                connectedPlayerCount,
                forcePhasePublish: previousPhase != _stateMachine.Phase);
        }

        public override void Render()
        {
            ObserveReplicatedPhase();
        }

        private int CountConnectedPlayers()
        {
            var count = 0;
            foreach (var _ in Runner.ActivePlayers)
            {
                count++;
            }

            return count;
        }

        private void PublishState(int connectedPlayerCount, bool forcePhasePublish)
        {
            ConnectedPlayerCount = connectedPlayerCount;
            if (!forcePhasePublish)
            {
                return;
            }

            Phase = _stateMachine.Phase;
            PhaseTimer = _stateMachine.SecondsRemaining > 0f
                ? TickTimer.CreateFromSeconds(Runner, _stateMachine.SecondsRemaining)
                : TickTimer.None;

            Debug.Log(
                $"[Match] Authoritative phase: {Phase}; players: {ConnectedPlayerCount}; " +
                $"remaining: {SecondsRemaining:0.0}s.");
        }

        private void ObserveReplicatedPhase()
        {
            if (_lastObservedPhase == Phase)
            {
                return;
            }

            _lastObservedPhase = Phase;
            Debug.Log(
                $"[Match] Observed phase: {Phase}; players: {ConnectedPlayerCount}; " +
                $"remaining: {SecondsRemaining:0.0}s.");
        }
    }
}
