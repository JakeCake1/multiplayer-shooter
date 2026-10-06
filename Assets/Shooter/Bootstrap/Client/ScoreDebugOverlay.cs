using System.Collections.Generic;
using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public sealed class ScoreDebugOverlay : DebugOverlay
    {
        private const float Width = 500f;
        private const float Top = 416f;
        private const float RowHeight = 22f;
        private const float VerticalPadding = 16f;
        private readonly Dictionary<int, FusionPlayerScoreState> _scoresByPlayer = new Dictionary<int, FusionPlayerScoreState>();
        private readonly List<int> _playerIds = new List<int>();

        private void Update()
        {
            CollectScoreStates();
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
                DrawScoreRow(_playerIds[index], index);
            }
        }

        private void CollectScoreStates()
        {
            _scoresByPlayer.Clear();
            var scoreStates = FindObjectsByType<FusionPlayerScoreState>(FindObjectsSortMode.None);
            foreach (var scoreState in scoreStates)
            {
                CollectScoreState(scoreState);
            }
        }

        private void CollectScoreState(FusionPlayerScoreState scoreState)
        {
            if (!IsSpawned(scoreState))
            {
                return;
            }

            _scoresByPlayer[scoreState.Object.InputAuthority.PlayerId] = scoreState;
        }

        private void CollectPlayerIds()
        {
            _playerIds.Clear();
            _playerIds.AddRange(_scoresByPlayer.Keys);
            _playerIds.Sort();
        }

        private void DrawScoreRow(int playerId, int index)
        {
            var scoreState = _scoresByPlayer[playerId];
            if (!IsSpawned(scoreState))
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = ClientDebugPlayerColors.Get(playerId);
            GUI.Label(new Rect(24f, Top + 8f + RowHeight * index, Width - 24f, RowHeight), $"Player:{playerId} Kills: {scoreState.Kills}");
            GUI.color = previousColor;
        }
    }
}
