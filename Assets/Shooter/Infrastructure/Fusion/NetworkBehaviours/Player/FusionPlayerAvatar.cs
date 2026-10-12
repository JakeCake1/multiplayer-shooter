using global::Fusion;
using Shooter.Features.MatchRules;
using Shooter.Features.Player;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(FusionPlayerHealthState))]
    public sealed class FusionPlayerAvatar : NetworkBehaviour
    {
        private const float MovementSpeed = 5f;
        private FusionPlayerHealthState _healthState;
        private FusionMatchState _matchState;

        public override void Spawned()
        {
            _healthState = GetComponent<FusionPlayerHealthState>();
        }

        public override void FixedUpdateNetwork()
        {
            if (!CanAcceptGameplayInput())
            {
                return;
            }

            if (!GetInput(out FusionPlayerInput input))
            {
                return;
            }

            Move(input);
        }

        private bool CanAcceptGameplayInput()
        {
            ResolveMatchState();
            return _matchState != null && MatchPhaseRules.AcceptsGameplayInput(_matchState.Phase) && !_healthState.IsDead;
        }

        private void ResolveMatchState()
        {
            if (_matchState == null)
            {
                _matchState = FindFirstObjectByType<FusionMatchState>();
            }
        }

        private void Move(FusionPlayerInput input)
        {
            var displacement = PlayerMovementRules.CalculateDisplacement(new PlanarMovement(input.MoveDirection.x, input.MoveDirection.y), MovementSpeed, Runner.DeltaTime);
            transform.position += new Vector3(displacement.Horizontal, 0f, displacement.Vertical);
        }
    }
}
