using System;
using global::Fusion;
using Shooter.Features.MatchRules;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionServerPlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
    {
        private GameObject _playerPrefab;
        private GameObject _matchStatePrefab;
        private MatchRules _matchRules;
        private NetworkObject _matchStateObject;

        public void Configure(
            GameObject playerPrefab,
            GameObject matchStatePrefab,
            MatchRules matchRules)
        {
            _playerPrefab = playerPrefab;
            _matchStatePrefab = matchStatePrefab;
            _matchRules = matchRules;
        }

        void IPlayerJoined.PlayerJoined(PlayerRef player)
        {
            if (!Runner.IsServer || _playerPrefab == null ||
                Runner.TryGetPlayerObject(player, out _))
            {
                return;
            }

            SpawnMatchStateIfNeeded();

            var horizontal = player.PlayerId % 2 == 0 ? 2f : -2f;
            var playerObject = Runner.Spawn(
                _playerPrefab,
                new Vector3(horizontal, 1f, 0f),
                Quaternion.identity,
                player);

            Runner.SetPlayerObject(player, playerObject);
        }

        void IPlayerLeft.PlayerLeft(PlayerRef player)
        {
            if (Runner.IsServer && Runner.TryGetPlayerObject(player, out var playerObject))
            {
                Runner.Despawn(playerObject);
            }
        }

        private void SpawnMatchStateIfNeeded()
        {
            if (_matchStateObject != null)
            {
                return;
            }

            if (_matchStatePrefab == null || _matchRules == null)
            {
                throw new InvalidOperationException(
                    "Server match state prefab and rules must be configured before players join.");
            }

            _matchStateObject = Runner.Spawn(
                _matchStatePrefab,
                Vector3.zero,
                Quaternion.identity,
                inputAuthority: null,
                onBeforeSpawned: (_, networkObject) =>
                    networkObject.GetComponent<FusionMatchState>().Configure(_matchRules));
        }
    }
}
