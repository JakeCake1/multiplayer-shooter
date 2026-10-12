using global::Fusion;
using Shooter.Features.MatchRules;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(FusionMatchState))]
    public sealed class FusionMatchStateObserver : NetworkBehaviour
    {
        private FusionMatchState _matchState;
        private MatchPhase _lastObservedPhase = (MatchPhase)(-1);

        public override void Spawned()
        {
            if (Runner.IsServer)
            {
                enabled = false;
                return;
            }

            _matchState = GetComponent<FusionMatchState>();
            ObservePhase();
        }

        public override void Render()
        {
            ObservePhase();
        }

        private void ObservePhase()
        {
            if (_lastObservedPhase == _matchState.Phase)
            {
                return;
            }

            _lastObservedPhase = _matchState.Phase;
            Debug.Log($"[Match][Client] Replicated phase: {_matchState.Phase}; players: {_matchState.ConnectedPlayerCount}; remaining: {_matchState.SecondsRemaining:0.0}s.");
        }
    }
}
