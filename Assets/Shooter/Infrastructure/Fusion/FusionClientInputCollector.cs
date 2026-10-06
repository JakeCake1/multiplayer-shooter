using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shooter.Infrastructure.Fusion
{
    internal static class FusionClientInputCollector
    {
        public static void Collect(NetworkRunner runner, NetworkInput networkInput)
        {
            var input = new FusionPlayerInput { MoveDirection = ReadMovement() };
            input.Buttons.Set(FusionPlayerButton.Fire, IsFireHeld());
            networkInput.Set(input);
        }

        private static bool IsFireHeld()
        {
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
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
