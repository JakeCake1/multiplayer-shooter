using global::Fusion;
using Shooter.Features.MatchRules;
using Shooter.Features.Player;
using Shooter.Application;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkCharacterController))]
    [RequireComponent(typeof(FusionPlayerHealthState))]
    public sealed class FusionPlayerAvatar : NetworkBehaviour
    {
        private PlayerGameplayConfiguration _configuration;
        private FusionPlayerHealthState _healthState;
        private NetworkCharacterController _characterController;
        private FusionMatchState _matchState;

        [Networked]
        private NetworkButtons PreviousButtons { get; set; }

        [Networked]
        public float LookPitch { get; private set; }

        public override void Spawned()
        {
            _configuration = FusionMatchConfigurationResolver.Resolve(Runner).Player;
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
                _characterController.Jump(overrideImpulse: _configuration.JumpSpeed);
            }
        }

        private void ConfigureCharacterController()
        {
            _characterController.gravity = _configuration.Gravity;
            _characterController.maxSpeed = _configuration.MovementSpeed;
            _characterController.acceleration = _configuration.MovementAcceleration;
            _characterController.braking = _configuration.MovementBraking;
            _characterController.rotationSpeed = 0f;
        }

        private PlayerLookRotation ResolveLookRotation(Vector2 inputLookRotation)
        {
            if (PlayerLookRules.TrySanitize(inputLookRotation.x, inputLookRotation.y, _configuration.MinimumLookPitch, _configuration.MaximumLookPitch, out var lookRotation))
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
