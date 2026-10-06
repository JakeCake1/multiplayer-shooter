using System.Collections.Generic;
using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public sealed class RespawnDebugOverlay : MonoBehaviour
    {
        private const float Width = 500f;
        private const float Top = 348f;
        private const float RowHeight = 22f;
        private const float VerticalPadding = 16f;
        private readonly Dictionary<int, FusionServerPlayerRespawn> _respawnsByPlayer = new Dictionary<int, FusionServerPlayerRespawn>();
        private readonly List<int> _respawningPlayerIds = new List<int>();

        private void Update()
        {
            CollectRespawningPlayers();
        }

        private void OnGUI()
        {
            if (_respawningPlayerIds.Count == 0)
            {
                return;
            }

            var height = VerticalPadding + RowHeight * _respawningPlayerIds.Count;
            GUI.Box(new Rect(12f, Top, Width, height), GUIContent.none);
            for (var index = 0; index < _respawningPlayerIds.Count; index++)
            {
                DrawRespawnRow(_respawningPlayerIds[index], index);
            }
        }

        private void CollectRespawningPlayers()
        {
            _respawnsByPlayer.Clear();
            _respawningPlayerIds.Clear();
            var respawns = FindObjectsByType<FusionServerPlayerRespawn>(FindObjectsSortMode.None);
            foreach (var respawn in respawns)
            {
                CollectRespawningPlayer(respawn);
            }

            _respawningPlayerIds.Sort();
        }

        private void CollectRespawningPlayer(FusionServerPlayerRespawn respawn)
        {
            if (respawn.Object == null || !respawn.IsRespawning)
            {
                return;
            }

            var playerId = respawn.Object.InputAuthority.PlayerId;
            _respawnsByPlayer[playerId] = respawn;
            _respawningPlayerIds.Add(playerId);
        }

        private void DrawRespawnRow(int playerId, int index)
        {
            var respawn = _respawnsByPlayer[playerId];
            var previousColor = GUI.color;
            GUI.color = ClientDebugPlayerColors.Get(playerId);
            GUI.Label(new Rect(24f, Top + 8f + RowHeight * index, Width - 24f, RowHeight), $"Player:{playerId} Respawn: {respawn.SecondsRemaining:0.0}s");
            GUI.color = previousColor;
        }
    }
}
