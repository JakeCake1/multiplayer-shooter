using global::Fusion;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public struct FusionPlayerInput : INetworkInput
    {
        public Vector2 MoveDirection;
        public NetworkButtons Buttons;
    }
}
