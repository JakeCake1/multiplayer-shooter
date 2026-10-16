using global::Fusion;
using Shooter.Features.MatchRules;
using Shooter.Features.Player;
using Shooter.Features.Weapon;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(FusionPlayerHealthState))]
    [RequireComponent(typeof(FusionPlayerScoreState))]
    public sealed class FusionServerWeapon : NetworkBehaviour
    {
        private const float RoundsPerMinute = 600f;
        private const float ShotOriginHeight = 0.5f;
        private const float ShotOriginForwardOffset = 0.6f;
        private static readonly AutomaticWeaponRules Rules = new AutomaticWeaponRules(RoundsPerMinute);
        private FusionPlayerHealthState _healthState;
        private FusionPlayerScoreState _scoreState;
        private FusionMatchState _matchState;

        [Networked]
        public int ConfirmedShotCount { get; private set; }

        [Networked]
        public int ConfirmedHitCount { get; private set; }

        [Networked]
        public Vector3 LastShotOrigin { get; private set; }

        [Networked]
        public Vector3 LastShotEnd { get; private set; }

        [Networked]
        private TickTimer FireCooldown { get; set; }

        public override void Spawned()
        {
            _healthState = GetComponent<FusionPlayerHealthState>();
            _scoreState = GetComponent<FusionPlayerScoreState>();
            enabled = Object.HasStateAuthority;
        }

        public override void FixedUpdateNetwork()
        {
            if (!CanAcceptFireInput())
            {
                return;
            }

            if (!GetInput(out FusionPlayerInput input) || !input.Buttons.IsSet(FusionPlayerButton.Fire) || !TryGetShotDirection(input, out var shotDirection))
            {
                return;
            }

            if (!FireCooldown.ExpiredOrNotRunning(Runner))
            {
                return;
            }

            ConfirmShot(shotDirection);
        }

        private bool CanAcceptFireInput()
        {
            ResolveMatchState();
            return FusionNetworkBehaviourLifecycle.IsSpawned(_matchState) && MatchPhaseRules.AcceptsGameplayInput(_matchState.Phase) && !_healthState.IsDead;
        }

        private void ResolveMatchState()
        {
            if (FusionNetworkBehaviourLifecycle.IsSpawned(_matchState))
            {
                return;
            }

            _matchState = FindFirstObjectByType<FusionMatchState>();
        }

        private static bool TryGetShotDirection(FusionPlayerInput input, out Vector3 shotDirection)
        {
            if (!PlayerLookRules.TrySanitize(input.LookRotation.x, input.LookRotation.y, out var lookRotation))
            {
                shotDirection = Vector3.zero;
                return false;
            }

            var candidate = Quaternion.Euler(lookRotation.Pitch, lookRotation.Yaw, 0f) * Vector3.forward;
            var valid = WeaponAimRules.TryNormalize(candidate.x, candidate.y, candidate.z, out var aimDirection);
            shotDirection = valid ? new Vector3(aimDirection.Horizontal, aimDirection.Vertical, aimDirection.Depth) : Vector3.zero;
            return valid;
        }

        private void ConfirmShot(Vector3 shotDirection)
        {
            RegisterConfirmedShot();
            var shotOrigin = CalculateShotOrigin(shotDirection);
            var didDamage = ResolveShot(shotOrigin, shotDirection, out var shotEnd, out var target, out var appliedDamage);
            PublishShotTrace(shotOrigin, shotEnd);
            LogShot(didDamage, target, appliedDamage);
        }

        private void RegisterConfirmedShot()
        {
            ConfirmedShotCount++;
            FireCooldown = TickTimer.CreateFromSeconds(Runner, Rules.SecondsBetweenShots);
        }

        private Vector3 CalculateShotOrigin(Vector3 shotDirection)
        {
            return transform.position + Vector3.up * ShotOriginHeight + shotDirection * ShotOriginForwardOffset;
        }

        private bool ResolveShot(Vector3 shotOrigin, Vector3 shotDirection, out Vector3 shotEnd, out FusionPlayerHealthState target, out int appliedDamage)
        {
            if (!TryResolveLagCompensatedHit(shotOrigin, shotDirection, out var hit))
            {
                shotEnd = shotOrigin + shotDirection * Rules.Range;
                target = null;
                appliedDamage = 0;
                return false;
            }

            shotEnd = hit.Point;
            target = hit.GameObject.GetComponentInParent<FusionPlayerHealthState>();
            if (target == null || target.Object == Object || target.IsDead)
            {
                appliedDamage = 0;
                return false;
            }

            appliedDamage = target.ApplyDamage(Rules.DamagePerHit);
            RegisterConfirmedHit(appliedDamage);
            AwardKillIfTargetDied(target);
            return appliedDamage > 0;
        }

        private void RegisterConfirmedHit(int appliedDamage)
        {
            if (appliedDamage > 0)
            {
                ConfirmedHitCount++;
            }
        }

        private bool TryResolveLagCompensatedHit(Vector3 shotOrigin, Vector3 shotDirection, out LagCompensatedHit hit)
        {
            const HitOptions options = HitOptions.IncludePhysX | HitOptions.SubtickAccuracy | HitOptions.IgnoreInputAuthority;
            return Runner.LagCompensation.Raycast(shotOrigin, shotDirection, Rules.Range, Object.InputAuthority, out hit, Physics.DefaultRaycastLayers, options, QueryTriggerInteraction.Ignore);
        }

        private void AwardKillIfTargetDied(FusionPlayerHealthState target)
        {
            if (!target.IsDead)
            {
                return;
            }

            _scoreState.AwardKill();
            Debug.Log($"[Score][Server] Awarded kill to player {Object.InputAuthority}; score: {_scoreState.Kills}.");
        }

        private void PublishShotTrace(Vector3 shotOrigin, Vector3 shotEnd)
        {
            LastShotOrigin = shotOrigin;
            LastShotEnd = shotEnd;
        }

        private void LogShot(bool didDamage, FusionPlayerHealthState target, int appliedDamage)
        {
            if (didDamage)
            {
                Debug.Log($"[Weapon][Server] Lag-compensated shot {ConfirmedShotCount} for player {Object.InputAuthority}; hit {target.Object.InputAuthority}; damage: {appliedDamage}; health: {target.CurrentHealth}/{target.MaxHealth}.");
                return;
            }

            Debug.Log($"[Weapon][Server] Lag-compensated shot {ConfirmedShotCount} for player {Object.InputAuthority}; miss; interval: {Rules.SecondsBetweenShots:0.000}s.");
        }
    }
}
