using System;
using global::Fusion;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionServerPlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
    {
        private GameObject _playerPrefab;
        private GameObject _matchStatePrefab;
        private FusionServerMatchController _matchController;
        private NetworkObject _matchStateObject;

        public void Configure(GameObject playerPrefab, GameObject matchStatePrefab, FusionServerMatchController matchController)
        {
            _playerPrefab = playerPrefab != null ? playerPrefab : throw new ArgumentNullException(nameof(playerPrefab));
            _matchStatePrefab = matchStatePrefab != null ? matchStatePrefab : throw new ArgumentNullException(nameof(matchStatePrefab));
            _matchController = matchController != null ? matchController : throw new ArgumentNullException(nameof(matchController));
        }

        void IPlayerJoined.PlayerJoined(PlayerRef player)
        {
            if (!Runner.IsServer || _playerPrefab == null || Runner.TryGetPlayerObject(player, out _))
            {
                return;
            }

            SpawnMatchStateIfNeeded();

            var horizontal = player.PlayerId % 2 == 0 ? 2f : -2f;
            var playerObject = Runner.Spawn(_playerPrefab, new Vector3(horizontal, 1f, 0f), Quaternion.identity, player);

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

            _matchStateObject = Runner.Spawn(_matchStatePrefab, Vector3.zero, Quaternion.identity);
            _matchController.Attach(_matchStateObject.GetComponent<FusionMatchState>(), _matchStateObject.GetComponent<FusionMatchResultState>());
        }
    }
}
