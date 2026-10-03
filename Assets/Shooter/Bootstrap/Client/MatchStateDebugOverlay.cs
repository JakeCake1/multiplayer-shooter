using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap.Client
{
    public sealed class MatchStateDebugOverlay : MonoBehaviour
    {
        private const float Width = 500f;
        private const float Height = 86f;
        private const float Top = 110f;

        private FusionMatchState _matchState;
        private string _text = "Match: waiting for replicated state";

        private void Update()
        {
            FindMatchStateIfNeeded();
            _text = HasReplicatedState() ? BuildText(_matchState) : "Match: waiting for replicated state";
        }

        private void OnGUI()
        {
            var area = new Rect(12f, Top, Width, Height);
            GUI.Box(area, GUIContent.none);
            GUI.Label(new Rect(24f, Top + 8f, Width - 24f, Height - 16f), _text);
        }

        private void FindMatchStateIfNeeded()
        {
            if (_matchState == null)
            {
                _matchState = FindFirstObjectByType<FusionMatchState>();
            }
        }

        private static string BuildText(FusionMatchState matchState)
        {
            return $"Match Phase: {matchState.Phase}\nConnected Players: {matchState.ConnectedPlayerCount}\nPhase Time Remaining: {matchState.SecondsRemaining:0.0}s";
        }

        private bool HasReplicatedState()
        {
            return _matchState != null && _matchState.Object != null;
        }
    }
}
