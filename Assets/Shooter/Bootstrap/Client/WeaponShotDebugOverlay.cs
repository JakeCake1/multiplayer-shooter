using System.Collections.Generic;
using System.Text;
using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public sealed class WeaponShotDebugOverlay : MonoBehaviour
    {
        private const float VisibilitySeconds = 5f;
        private const float Width = 500f;
        private const float Top = 204f;
        private const float RowHeight = 22f;
        private const float VerticalPadding = 16f;
        private readonly Dictionary<int, int> _confirmedShotCounts = new Dictionary<int, int>();
        private readonly Dictionary<int, float> _lastShotTimes = new Dictionary<int, float>();
        private readonly List<int> _visiblePlayerIds = new List<int>();
        private readonly StringBuilder _textBuilder = new StringBuilder();
        private string _text = string.Empty;

        private void Update()
        {
            ObserveConfirmedShots();
            RebuildVisibleText();
        }

        private void OnGUI()
        {
            if (_visiblePlayerIds.Count == 0)
            {
                return;
            }

            var height = VerticalPadding + RowHeight * _visiblePlayerIds.Count;
            GUI.Box(new Rect(12f, Top, Width, height), GUIContent.none);
            GUI.Label(new Rect(24f, Top + 8f, Width - 24f, height - VerticalPadding), _text);
        }

        private void ObserveConfirmedShots()
        {
            var weapons = FindObjectsByType<FusionServerWeapon>(FindObjectsSortMode.None);
            foreach (var weapon in weapons)
            {
                ObserveConfirmedShots(weapon);
            }
        }

        private void ObserveConfirmedShots(FusionServerWeapon weapon)
        {
            if (weapon.Object == null)
            {
                return;
            }

            var playerId = weapon.Object.InputAuthority.PlayerId;
            var confirmedShotCount = weapon.ConfirmedShotCount;
            if (!_confirmedShotCounts.TryGetValue(playerId, out var previousShotCount))
            {
                RegisterPlayer(playerId, confirmedShotCount);
                return;
            }

            _confirmedShotCounts[playerId] = confirmedShotCount;
            if (confirmedShotCount > previousShotCount)
            {
                _lastShotTimes[playerId] = Time.unscaledTime;
            }
        }

        private void RegisterPlayer(int playerId, int confirmedShotCount)
        {
            _confirmedShotCounts[playerId] = confirmedShotCount;
            if (confirmedShotCount > 0)
            {
                _lastShotTimes[playerId] = Time.unscaledTime;
            }
        }

        private void RebuildVisibleText()
        {
            CollectVisiblePlayerIds();
            _textBuilder.Clear();
            foreach (var playerId in _visiblePlayerIds)
            {
                AppendPlayerText(playerId);
            }

            _text = _textBuilder.ToString();
        }

        private void CollectVisiblePlayerIds()
        {
            _visiblePlayerIds.Clear();
            foreach (var pair in _lastShotTimes)
            {
                if (Time.unscaledTime - pair.Value < VisibilitySeconds)
                {
                    _visiblePlayerIds.Add(pair.Key);
                }
            }

            _visiblePlayerIds.Sort();
        }

        private void AppendPlayerText(int playerId)
        {
            if (_textBuilder.Length > 0)
            {
                _textBuilder.AppendLine();
            }

            _textBuilder.Append("Player:");
            _textBuilder.Append(playerId);
            _textBuilder.Append(": ");
            _textBuilder.Append(_confirmedShotCounts[playerId]);
            _textBuilder.Append(" shots");
        }
    }
}
