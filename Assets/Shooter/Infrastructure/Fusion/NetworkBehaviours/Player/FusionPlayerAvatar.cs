using global::Fusion;
using Shooter.Features.MatchRules;
using Shooter.Features.Player;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkCharacterController))]
    [RequireComponent(typeof(FusionPlayerHealthState))]
    public sealed class FusionPlayerAvatar : NetworkBehaviour
    {
        private const float MovementSpeed = 5f;
        private const float MovementAcceleration = 40f;
        private const float MovementBraking = 40f;
        private const float JumpSpeed = 7f;
        private const float Gravity = -20f;
        private FusionPlayerHealthState _healthState;
        private NetworkCharacterController _characterController;
        private FusionMatchState _matchState;

        [Networked]
        private NetworkButtons PreviousButtons { get; set; }

        [Networked]
        public float LookPitch { get; private set; }

        public override void Spawned()
        {
            _healthState = GetComponent<FusionPlayerHealthState>();
            _characterController = GetComponent<NetworkCharacterController>();
            ConfigureCharacterController();
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
            var worldDirection = CalculateWorldDirection(input, lookRotation, acceptsGameplayInput);
            ApplyJump(acceptsGameplayInput && jumpPressed);
            _characterController.Move(worldDirection);
            ApplyLook(lookRotation, acceptsGameplayInput);
        }

        private static Vector3 CalculateWorldDirection(FusionPlayerInput input, PlayerLookRotation lookRotation, bool acceptsGameplayInput)
        {
            if (!acceptsGameplayInput)
            {
                return Vector3.zero;
            }

            var localDirection = new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            return Quaternion.Euler(0f, lookRotation.Yaw, 0f) * localDirection;
        }

        private void ApplyJump(bool jumpPressed)
        {
            if (PlayerJumpRules.CanStartJump(jumpPressed, _characterController.Grounded))
            {
                _characterController.Jump(overrideImpulse: JumpSpeed);
            }
        }

        private void ConfigureCharacterController()
        {
            _characterController.gravity = Gravity;
            _characterController.maxSpeed = MovementSpeed;
            _characterController.acceleration = MovementAcceleration;
            _characterController.braking = MovementBraking;
            _characterController.rotationSpeed = 0f;
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
