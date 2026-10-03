using System;
using global::Fusion;
using Shooter.Features.MatchRules;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionServerMatchController : SimulationBehaviour
    {
        private MatchRules _rules;
        private MatchStateMachine _stateMachine;
        private FusionMatchState _replicatedState;

        public void Configure(MatchRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public void Attach(FusionMatchState replicatedState)
        {
            _replicatedState = replicatedState != null ? replicatedState : throw new ArgumentNullException(nameof(replicatedState));
            _stateMachine = new MatchStateMachine(_rules ?? throw new InvalidOperationException("Match rules must be configured before the replicated state is attached."));
            PublishState(CountConnectedPlayers());
        }

        public override void FixedUpdateNetwork()
        {
            if (_replicatedState == null)
            {
                return;
            }

            var connectedPlayerCount = CountConnectedPlayers();
            var previousPhase = _stateMachine.Phase;
            _stateMachine.Advance(connectedPlayerCount, Runner.DeltaTime);
            PublishStateIfPhaseChanged(previousPhase, connectedPlayerCount);
        }

        private void PublishStateIfPhaseChanged(MatchPhase previousPhase, int connectedPlayerCount)
        {
            if (previousPhase == _stateMachine.Phase)
            {
                _replicatedState.Publish(_stateMachine.Phase, connectedPlayerCount, _stateMachine.SecondsRemaining);
                return;
            }

            PublishState(connectedPlayerCount);
        }

        private void PublishState(int connectedPlayerCount)
        {
            _replicatedState.Publish(_stateMachine.Phase, connectedPlayerCount, _stateMachine.SecondsRemaining);
            Debug.Log($"[Match][Server] Authoritative phase: {_stateMachine.Phase}; players: {connectedPlayerCount}; remaining: {_stateMachine.SecondsRemaining:0.0}s.");
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
    }
}
