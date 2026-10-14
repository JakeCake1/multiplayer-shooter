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
        private const float JumpSpeed = 7f;
        private const float Gravity = 20f;
        private FusionPlayerHealthState _healthState;
        private FusionMatchState _matchState;
        private float _groundHeight;

        [Networked]
        private float VerticalVelocity { get; set; }

        [Networked]
        private NetworkButtons PreviousButtons { get; set; }

        [Networked]
        public float LookPitch { get; private set; }

        public override void Spawned()
        {
            _healthState = GetComponent<FusionPlayerHealthState>();
            _groundHeight = transform.position.y;
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out FusionPlayerInput input))
            {
                return;
            }

            var lookRotation = ResolveLookRotation(input.LookRotation);
            var jumpPressed = input.Buttons.WasPressed(PreviousButtons, FusionPlayerButton.Jump);
            PreviousButtons = input.Buttons;
            SimulatePlayer(input, lookRotation, CanAcceptGameplayInput(), jumpPressed);
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

        private void SimulatePlayer(FusionPlayerInput input, PlayerLookRotation lookRotation, bool acceptsGameplayInput, bool jumpPressed)
        {
            var planarDisplacement = CalculatePlanarDisplacement(input, lookRotation, acceptsGameplayInput);
            var jumpState = CalculateJumpState(acceptsGameplayInput && jumpPressed);
            ApplyMovement(planarDisplacement, jumpState);
            ApplyLook(lookRotation, acceptsGameplayInput);
        }

        private PlanarMovement CalculatePlanarDisplacement(FusionPlayerInput input, PlayerLookRotation lookRotation, bool acceptsGameplayInput)
        {
            var localDirection = new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            var worldDirection = Quaternion.Euler(0f, lookRotation.Yaw, 0f) * localDirection;
            var direction = acceptsGameplayInput ? new PlanarMovement(worldDirection.x, worldDirection.z) : new PlanarMovement(0f, 0f);
            return PlayerMovementRules.CalculateDisplacement(direction, MovementSpeed, Runner.DeltaTime);
        }

        private PlayerJumpState CalculateJumpState(bool jumpPressed)
        {
            var height = Mathf.Max(0f, transform.position.y - _groundHeight);
            return PlayerJumpRules.Simulate(new PlayerJumpState(height, VerticalVelocity), jumpPressed, JumpSpeed, Gravity, Runner.DeltaTime);
        }

        private void ApplyMovement(PlanarMovement planarDisplacement, PlayerJumpState jumpState)
        {
            VerticalVelocity = jumpState.Velocity;
            var position = transform.position;
            transform.position = new Vector3(position.x + planarDisplacement.Horizontal, _groundHeight + jumpState.Height, position.z + planarDisplacement.Vertical);
        }

        private PlayerLookRotation ResolveLookRotation(Vector2 inputLookRotation)
        {
            if (PlayerLookRules.TrySanitize(inputLookRotation.x, inputLookRotation.y, out var lookRotation))
            {
                return lookRotation;
            }

            return new PlayerLookRotation(transform.eulerAngles.y, LookPitch);
        }

        private void ApplyLook(PlayerLookRotation lookRotation, bool acceptsGameplayInput)
        {
            if (!acceptsGameplayInput)
            {
                return;
            }

            LookPitch = lookRotation.Pitch;
            transform.rotation = Quaternion.Euler(0f, lookRotation.Yaw, 0f);
        }
    }
}
