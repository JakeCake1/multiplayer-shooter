using global::Fusion;
using Shooter.Application;
using Shooter.Features.Player;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionFirstPersonLookController : MonoBehaviour
    {
        private const float BaseDegreesPerPixel = 0.12f;
        private const float VisualSmoothTime = 0.035f;
        private NetworkRunner _runner;
        private IClientInputSettings _settings;
        private PlayerGameplayConfiguration _configuration;
        private NetworkObject _lookSource;
        private float _targetYaw;
        private float _targetPitch;
        private float _visualYaw;
        private float _visualPitch;
        private float _yawVelocity;
        private float _pitchVelocity;

        public Vector2 NetworkLookRotation => new Vector2(_targetYaw, _targetPitch);

        public Quaternion VisualLookRotation => Quaternion.Euler(_visualPitch, _visualYaw, 0f);

        public void Configure(NetworkRunner runner, IClientInputSettings settings, PlayerGameplayConfiguration configuration)
        {
            _runner = runner;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private void Update()
        {
            InitializeLookIfNeeded();
            ReadMouseDelta();
            SmoothVisualRotation();
        }

        private void InitializeLookIfNeeded()
        {
            if (_runner == null || !_runner.TryGetPlayerObject(_runner.LocalPlayer, out var playerObject) || _lookSource == playerObject)
            {
                return;
            }

            _lookSource = playerObject;
            _targetYaw = playerObject.transform.eulerAngles.y;
            _targetPitch = 0f;
            _visualYaw = _targetYaw;
            _visualPitch = _targetPitch;
            _yawVelocity = 0f;
            _pitchVelocity = 0f;
        }

        private void ReadMouseDelta()
        {
            if (_lookSource == null || Mouse.current == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            var delta = Mouse.current.delta.ReadValue();
            var sensitivity = BaseDegreesPerPixel * _settings.MouseSensitivityMultiplier;
            _targetYaw = Mathf.Repeat(_targetYaw + delta.x * sensitivity, 360f);
            _targetPitch = Mathf.Clamp(_targetPitch - delta.y * sensitivity, _configuration.MinimumLookPitch, _configuration.MaximumLookPitch);
        }

        private void SmoothVisualRotation()
        {
            if (_lookSource == null)
            {
                return;
            }

            _visualYaw = Mathf.SmoothDampAngle(_visualYaw, _targetYaw, ref _yawVelocity, VisualSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            _visualPitch = Mathf.SmoothDamp(_visualPitch, _targetPitch, ref _pitchVelocity, VisualSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }
    }
}
