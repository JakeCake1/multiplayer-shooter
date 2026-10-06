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
        private static readonly AutomaticWeaponRules Rules = new AutomaticWeaponRules(RoundsPerMinute);
        private FusionMatchState _matchState;

        [Networked]
        public int ConfirmedShotCount { get; private set; }

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

            if (!GetInput(out FusionPlayerInput input) || !input.Buttons.IsSet(FusionPlayerButton.Fire))
            {
                return;
            }

            if (!FireCooldown.ExpiredOrNotRunning(Runner))
            {
                return;
            }

            ConfirmShot();
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

        private void ConfirmShot()
        {
            ConfirmedShotCount++;
            FireCooldown = TickTimer.CreateFromSeconds(Runner, Rules.SecondsBetweenShots);
            Debug.Log($"[Weapon][Server] Confirmed shot {ConfirmedShotCount} for player {Object.InputAuthority}; interval: {Rules.SecondsBetweenShots:0.000}s.");
        }
    }
}
