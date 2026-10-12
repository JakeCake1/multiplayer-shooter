using System;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionNetworkAssets
    {
        public FusionNetworkAssets(GameObject playerPrefab, GameObject matchStatePrefab)
        {
            PlayerPrefab = playerPrefab != null ? playerPrefab : throw new ArgumentNullException(nameof(playerPrefab));
            MatchStatePrefab = matchStatePrefab != null ? matchStatePrefab : throw new ArgumentNullException(nameof(matchStatePrefab));
        }

        public GameObject PlayerPrefab { get; }

        public GameObject MatchStatePrefab { get; }
    }
}
