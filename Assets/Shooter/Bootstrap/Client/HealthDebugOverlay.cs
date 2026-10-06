using System.Collections.Generic;
using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public sealed class HealthDebugOverlay : MonoBehaviour
    {
        private const float Width = 500f;
        private const float Top = 280f;
        private const float RowHeight = 22f;
        private const float VerticalPadding = 16f;
        private readonly Dictionary<int, FusionPlayerHealthState> _healthByPlayer = new Dictionary<int, FusionPlayerHealthState>();
        private readonly List<int> _playerIds = new List<int>();

        private void Update()
        {
            CollectHealthStates();
            CollectPlayerIds();
        }

        private void OnGUI()
        {
            if (_playerIds.Count == 0)
            {
                return;
            }

            var height = VerticalPadding + RowHeight * _playerIds.Count;
            GUI.Box(new Rect(12f, Top, Width, height), GUIContent.none);
            for (var index = 0; index < _playerIds.Count; index++)
            {
                DrawHealthRow(_playerIds[index], index);
            }
        }

        private void CollectHealthStates()
        {
            _healthByPlayer.Clear();
            var healthStates = FindObjectsByType<FusionPlayerHealthState>(FindObjectsSortMode.None);
            foreach (var healthState in healthStates)
            {
                if (healthState.Object != null)
                {
                    _healthByPlayer[healthState.Object.InputAuthority.PlayerId] = healthState;
                }
            }
        }

        private void CollectPlayerIds()
        {
            _playerIds.Clear();
            _playerIds.AddRange(_healthByPlayer.Keys);
            _playerIds.Sort();
        }

        private void DrawHealthRow(int playerId, int index)
        {
            var healthState = _healthByPlayer[playerId];
            var previousColor = GUI.color;
            GUI.color = ClientDebugPlayerColors.Get(playerId);
            GUI.Label(new Rect(24f, Top + 8f + RowHeight * index, Width - 24f, RowHeight), $"Player:{playerId} Health: {healthState.CurrentHealth}/{healthState.MaxHealth}");
            GUI.color = previousColor;
        }
    }
}
