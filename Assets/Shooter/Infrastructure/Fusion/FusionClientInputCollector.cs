using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shooter.Infrastructure.Fusion
{
    internal sealed class FusionClientInputCollector
    {
        private readonly FusionFirstPersonLookController _lookController;

        public FusionClientInputCollector(FusionFirstPersonLookController lookController)
        {
            _lookController = lookController;
        }

        public void Collect(NetworkRunner _, NetworkInput networkInput)
        {
            var input = new FusionPlayerInput { MoveDirection = ReadMovement(), LookRotation = _lookController.NetworkLookRotation };
            input.Buttons.Set(FusionPlayerButton.Fire, IsFireHeld());
            input.Buttons.Set(FusionPlayerButton.Jump, IsJumpHeld());
            networkInput.Set(input);
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
