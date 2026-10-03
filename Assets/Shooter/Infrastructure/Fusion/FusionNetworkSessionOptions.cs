using System;
using Shooter.Gameplay;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionNetworkSessionOptions
    {
        public FusionNetworkSessionOptions(
            GameObject playerPrefab,
            GameObject matchStatePrefab = null,
            MatchRules matchRules = null)
        {
            PlayerPrefab = playerPrefab != null
                ? playerPrefab
                : throw new ArgumentNullException(nameof(playerPrefab));
            MatchStatePrefab = matchStatePrefab;
            MatchRules = matchRules;
        }

        public GameObject PlayerPrefab { get; }

        public GameObject MatchStatePrefab { get; }

        public MatchRules MatchRules { get; }
    }
}
