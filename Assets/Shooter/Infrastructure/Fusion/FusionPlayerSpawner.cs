using global::Fusion;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionPlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
    {
        private GameObject _playerPrefab;

        public void Configure(GameObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
        }

        void IPlayerJoined.PlayerJoined(PlayerRef player)
        {
            if (!Runner.IsServer || _playerPrefab == null ||
                Runner.TryGetPlayerObject(player, out _))
            {
                return;
            }

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
    }
}
