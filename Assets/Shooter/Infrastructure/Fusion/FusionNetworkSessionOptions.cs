using System;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionNetworkSessionOptions
    {
        public FusionNetworkSessionOptions(GameObject playerPrefab)
        {
            PlayerPrefab = playerPrefab != null
                ? playerPrefab
                : throw new ArgumentNullException(nameof(playerPrefab));
        }

        public GameObject PlayerPrefab { get; }
    }
}
