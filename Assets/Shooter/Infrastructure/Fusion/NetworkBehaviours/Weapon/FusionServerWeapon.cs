using global::Fusion;
using Shooter.Features.MatchRules;
using Shooter.Features.Weapon;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class FusionServerWeapon : NetworkBehaviour
    {
        private const float RoundsPerMinute = 600f;
        private const float ShotOriginHeight = 0.5f;
        private const float ShotOriginForwardOffset = 0.6f;
        private static readonly AutomaticWeaponRules Rules = new AutomaticWeaponRules(RoundsPerMinute);
        private FusionMatchState _matchState;

        [Networked]
        public int ConfirmedShotCount { get; private set; }

        [Networked]
        public Vector3 LastShotOrigin { get; private set; }

        [Networked]
        public Vector3 LastShotEnd { get; private set; }

        [Networked]
        private TickTimer FireCooldown { get; set; }

        public override void Spawned()
        {
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
            return _matchState != null && MatchPhaseRules.AcceptsGameplayInput(_matchState.Phase);
        }

        private void ResolveMatchState()
        {
            if (_matchState == null)
            {
                _matchState = FindFirstObjectByType<FusionMatchState>();
            }
        }

        private static bool TryGetShotDirection(FusionPlayerInput input, out Vector3 shotDirection)
        {
            var valid = WeaponAimRules.TryNormalize(input.AimDirection.x, input.AimDirection.y, out var aimDirection);
            shotDirection = valid ? new Vector3(aimDirection.Horizontal, 0f, aimDirection.Vertical) : Vector3.zero;
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
            if (!Physics.Raycast(shotOrigin, shotDirection, out var hit, Rules.Range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                shotEnd = shotOrigin + shotDirection * Rules.Range;
                target = null;
                appliedDamage = 0;
                return false;
            }

            shotEnd = hit.point;
            target = hit.collider.GetComponentInParent<FusionPlayerHealthState>();
            if (target == null || target.Object == Object || target.IsDead)
            {
                appliedDamage = 0;
                return false;
            }

            appliedDamage = target.ApplyDamage(Rules.DamagePerHit);
            return appliedDamage > 0;
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
                Debug.Log($"[Weapon][Server] Confirmed shot {ConfirmedShotCount} for player {Object.InputAuthority}; hit {target.Object.InputAuthority}; damage: {appliedDamage}; health: {target.CurrentHealth}/{target.MaxHealth}.");
                return;
            }

            Debug.Log($"[Weapon][Server] Confirmed shot {ConfirmedShotCount} for player {Object.InputAuthority}; miss; interval: {Rules.SecondsBetweenShots:0.000}s.");
        }
    }
}
