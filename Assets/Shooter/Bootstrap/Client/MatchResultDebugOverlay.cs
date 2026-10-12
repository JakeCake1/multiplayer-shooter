using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public sealed class MatchResultDebugOverlay : DebugOverlay
    {
        private const float Width = 500f;
        private const float Height = 82f;
        private const float Top = 484f;
        private const float RowHeight = 22f;
        private FusionMatchResultState _resultState;

        private void Update()
        {
            if (_resultState == null)
            {
                _resultState = FindFirstObjectByType<FusionMatchResultState>();
            }
        }

        private void OnGUI()
        {
            if (!IsSpawned(_resultState) || !_resultState.HasResult)
            {
                return;
            }

            GUI.Box(new Rect(12f, Top, Width, Height), GUIContent.none);
            DrawOutcome();
            DrawPlayerScore(_resultState.FirstPlayerId, _resultState.FirstPlayerKills, 1);
            DrawPlayerScore(_resultState.SecondPlayerId, _resultState.SecondPlayerKills, 2);
        }

        private void DrawOutcome()
        {
            var outcome = _resultState.IsDraw ? "Draw" : $"Winner: Player {_resultState.WinnerPlayerId}";
            GUI.Label(new Rect(24f, Top + 6f, Width - 24f, RowHeight), $"Match Result: {outcome}");
        }

        private static void DrawPlayerScore(int playerId, int kills, int row)
        {
            var previousColor = GUI.color;
            GUI.color = ClientDebugPlayerColors.Get(playerId);
            GUI.Label(new Rect(24f, Top + 6f + RowHeight * row, Width - 24f, RowHeight), $"Player:{playerId} Kills: {kills}");
            GUI.color = previousColor;
        }
    }
}
