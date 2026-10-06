using System.Collections.Generic;
using System.Text;
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
        private readonly StringBuilder _textBuilder = new StringBuilder();
        private string _text = string.Empty;

        private void Update()
        {
            CollectHealthStates();
            RebuildText();
        }

        private void OnGUI()
        {
            if (_playerIds.Count == 0)
            {
                return;
            }

            var height = VerticalPadding + RowHeight * _playerIds.Count;
            GUI.Box(new Rect(12f, Top, Width, height), GUIContent.none);
            GUI.Label(new Rect(24f, Top + 8f, Width - 24f, height - VerticalPadding), _text);
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

        private void RebuildText()
        {
            CollectPlayerIds();
            _textBuilder.Clear();
            foreach (var playerId in _playerIds)
            {
                AppendHealthText(playerId);
            }

            _text = _textBuilder.ToString();
        }

        private void CollectPlayerIds()
        {
            _playerIds.Clear();
            _playerIds.AddRange(_healthByPlayer.Keys);
            _playerIds.Sort();
        }

        private void AppendHealthText(int playerId)
        {
            if (_textBuilder.Length > 0)
            {
                _textBuilder.AppendLine();
            }

            var healthState = _healthByPlayer[playerId];
            _textBuilder.Append("Player:");
            _textBuilder.Append(playerId);
            _textBuilder.Append(" Health: ");
            _textBuilder.Append(healthState.CurrentHealth);
            _textBuilder.Append('/');
            _textBuilder.Append(healthState.MaxHealth);
        }
    }
}
