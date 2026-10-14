using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shooter.Infrastructure.Fusion
{
    internal sealed class FusionClientInputCollector
    {
        private const float MouseSensitivity = 0.12f;
        private float _yaw;
        private float _pitch;
        private bool _lookInitialized;

        public void Collect(NetworkRunner runner, NetworkInput networkInput)
        {
            var input = new FusionPlayerInput { MoveDirection = ReadMovement(), LookRotation = ReadLookRotation(runner) };
            input.Buttons.Set(FusionPlayerButton.Fire, IsFireHeld());
            input.Buttons.Set(FusionPlayerButton.Jump, IsJumpHeld());
            networkInput.Set(input);
        }

        private Vector2 ReadLookRotation(NetworkRunner runner)
        {
            InitializeLookIfNeeded(runner);
            if (Mouse.current == null || Cursor.lockState != CursorLockMode.Locked)
            {
                return new Vector2(_yaw, _pitch);
            }

            var delta = Mouse.current.delta.ReadValue();
            _yaw = Mathf.Repeat(_yaw + delta.x * MouseSensitivity, 360f);
            _pitch = Mathf.Clamp(_pitch - delta.y * MouseSensitivity, -85f, 85f);
            return new Vector2(_yaw, _pitch);
        }

        private void InitializeLookIfNeeded(NetworkRunner runner)
        {
            if (_lookInitialized || !runner.TryGetPlayerObject(runner.LocalPlayer, out var playerObject))
            {
                return;
            }

            _yaw = playerObject.transform.eulerAngles.y;
            _pitch = 0f;
            _lookInitialized = true;
        }

        private static bool IsFireHeld()
        {
            return Mouse.current != null && Cursor.lockState == CursorLockMode.Locked && Mouse.current.leftButton.isPressed;
        }

        private static bool IsJumpHeld()
        {
            return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        }

        private static Vector2 ReadMovement()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            var movement = Vector2.zero;
            movement += keyboard.wKey.isPressed ? Vector2.up : Vector2.zero;
            movement += keyboard.sKey.isPressed ? Vector2.down : Vector2.zero;
            movement += keyboard.aKey.isPressed ? Vector2.left : Vector2.zero;
            movement += keyboard.dKey.isPressed ? Vector2.right : Vector2.zero;
            return movement.normalized;
        }
    }
}
