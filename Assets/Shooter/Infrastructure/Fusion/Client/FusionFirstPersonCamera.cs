using global::Fusion;
using Shooter.Features.MatchRules;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionFirstPersonCamera : MonoBehaviour
    {
        private const float EyeHeight = 0.65f;
        private NetworkRunner _runner;
        private FusionFirstPersonLookController _lookController;
        private Camera _camera;
        private FusionPlayerAvatar _avatar;
        private FusionMatchState _matchState;
        private Renderer[] _localRenderers;
        private bool _gameplayWasActive;

        public void Configure(NetworkRunner runner, FusionFirstPersonLookController lookController)
        {
            _runner = runner;
            _lookController = lookController;
        }

        private void LateUpdate()
        {
            ResolveMatchStateIfNeeded();
            UpdateCursorCapture();
            ResolveAvatarIfNeeded();
            UpdateCamera();
        }

        private void OnDestroy()
        {
            RestoreLocalRenderers();
            ReleaseCursor();
        }

        private void ResolveMatchStateIfNeeded()
        {
            if (_matchState == null)
            {
                _matchState = FindFirstObjectByType<FusionMatchState>();
            }
        }

        private void ResolveAvatarIfNeeded()
        {
            if (_avatar != null && _avatar.Object != null)
            {
                return;
            }

            RestoreLocalRenderers();
            _avatar = null;
            if (_runner == null || !_runner.TryGetPlayerObject(_runner.LocalPlayer, out var playerObject))
            {
                return;
            }

            _avatar = playerObject.GetComponent<FusionPlayerAvatar>();
            HideLocalRenderers();
        }

        private void HideLocalRenderers()
        {
            if (_avatar == null)
            {
                return;
            }

            _localRenderers = _avatar.GetComponentsInChildren<Renderer>(true);
            for (var index = 0; index < _localRenderers.Length; index++)
            {
                _localRenderers[index].enabled = false;
            }
        }

        private void RestoreLocalRenderers()
        {
            if (_localRenderers == null)
            {
                return;
            }

            for (var index = 0; index < _localRenderers.Length; index++)
            {
                if (_localRenderers[index] != null)
                {
                    _localRenderers[index].enabled = true;
                }
            }

            _localRenderers = null;
        }

        private void UpdateCamera()
        {
            ResolveCameraIfNeeded();
            if (_camera == null || _avatar == null)
            {
                return;
            }

            var eyePosition = _avatar.transform.position + Vector3.up * EyeHeight;
            _camera.transform.SetPositionAndRotation(eyePosition, _lookController.VisualLookRotation);
        }

        private void ResolveCameraIfNeeded()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera != null)
                {
                    _camera.nearClipPlane = 0.05f;
                }
            }
        }

        private void UpdateCursorCapture()
        {
            var gameplayActive = _matchState != null && MatchPhaseRules.AcceptsGameplayInput(_matchState.Phase);
            if (!gameplayActive)
            {
                _gameplayWasActive = false;
                ReleaseCursor();
                return;
            }

            if (!_gameplayWasActive)
            {
                CaptureCursor();
            }

            _gameplayWasActive = true;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReleaseCursor();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                CaptureCursor();
            }
        }

        private static void CaptureCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
