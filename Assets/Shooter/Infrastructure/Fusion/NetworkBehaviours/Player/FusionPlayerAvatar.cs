using global::Fusion;
using Shooter.Features.Player;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class FusionPlayerAvatar : NetworkBehaviour
    {
        private const float MovementSpeed = 5f;

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out FusionPlayerInput input))
            {
                return;
            }

            var displacement = PlayerMovementRules.CalculateDisplacement(new PlanarMovement(input.MoveDirection.x, input.MoveDirection.y), MovementSpeed, Runner.DeltaTime);
            transform.position += new Vector3(displacement.Horizontal, 0f, displacement.Vertical);
        }
    }
}
