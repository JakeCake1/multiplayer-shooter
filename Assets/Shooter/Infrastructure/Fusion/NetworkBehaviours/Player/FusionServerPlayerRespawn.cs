using global::Fusion;
using Shooter.Features.MatchRules;
using Shooter.Features.Player;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(FusionPlayerHealthState))]
    public sealed class FusionServerPlayerRespawn : NetworkBehaviour
    {
        private const float RespawnDelaySeconds = 7f;
        private static readonly PlayerRespawnRules Rules = new PlayerRespawnRules(RespawnDelaySeconds);
        private FusionPlayerHealthState _healthState;
        private FusionMatchState _matchState;
        private Vector3 _spawnPosition;

        [Networked]
        public NetworkBool IsRespawning { get; private set; }

        [Networked]
        public TickTimer RespawnTimer { get; private set; }

        [Networked]
        public int RespawnCount { get; private set; }

        public float SecondsRemaining => RespawnTimer.RemainingTime(Runner) ?? 0f;

        public override void Spawned()
        {
            enabled = Object.HasStateAuthority;
            if (!enabled)
            {
                return;
            }

            _healthState = GetComponent<FusionPlayerHealthState>();
            _spawnPosition = transform.position;
        }

        public override void FixedUpdateNetwork()
        {
            if (!IsMatchPlaying())
            {
                CancelRespawnIfNeeded();
                return;
            }

            if (IsRespawning)
            {
                CompleteRespawnIfReady();
                return;
            }

            if (_healthState.IsDead)
            {
                StartRespawn();
            }
        }

        private bool IsMatchPlaying()
        {
            ResolveMatchState();
            return _matchState != null && MatchPhaseRules.AcceptsGameplayInput(_matchState.Phase);
        }

        private void ResolveMatchState()
        {
            if (_matchState == null)
            {
                _matchState = FindFirstObjectByType<FusionMatchState>();
            }
        }

        private void StartRespawn()
        {
            IsRespawning = true;
            RespawnTimer = TickTimer.CreateFromSeconds(Runner, Rules.DelaySeconds);
            Debug.Log($"[Respawn][Server] Player {Object.InputAuthority} will respawn in {Rules.DelaySeconds:0.0}s.");
        }

        private void CompleteRespawnIfReady()
        {
            if (!RespawnTimer.Expired(Runner))
            {
                return;
            }

            transform.position = _spawnPosition;
            _healthState.RestoreFullHealth();
            RespawnCount++;
            IsRespawning = false;
            RespawnTimer = TickTimer.None;
            Debug.Log($"[Respawn][Server] Player {Object.InputAuthority} respawned at {_spawnPosition}; count: {RespawnCount}.");
        }

        private void CancelRespawnIfNeeded()
        {
            if (!IsRespawning)
            {
                return;
            }

            IsRespawning = false;
            RespawnTimer = TickTimer.None;
            Debug.Log($"[Respawn][Server] Cancelled respawn for player {Object.InputAuthority} because the match is no longer playing.");
        }
    }
}
