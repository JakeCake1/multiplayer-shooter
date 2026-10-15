using System;
using Shooter.Presentation.Ui;
using UnityEngine.UIElements;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class PlayingHudView : UiToolkitView
    {
        private const long HitMarkerDurationMilliseconds = 150;
        private Label _timeRemainingLabel;
        private Label _firstPlayerLabel;
        private Label _secondPlayerLabel;
        private Label _hitMarkerLabel;
        private Label _respawnStatusLabel;
        private int _lastConfirmedHitCount = -1;
        private int _hitMarkerVersion;

        public override void Bind(IUiViewModel viewModel)
        {
            var playing = viewModel as PlayingHudViewModel ?? throw new ArgumentException($"{nameof(PlayingHudView)} requires {nameof(PlayingHudViewModel)}.", nameof(viewModel));
            ResolveElements();
            _timeRemainingLabel.text = playing.TimeRemaining;
            _firstPlayerLabel.text = playing.FirstPlayer;
            _secondPlayerLabel.text = playing.SecondPlayer;
            UpdateHitMarker(playing.LocalConfirmedHitCount);
            UpdateRespawnStatus(playing.LocalRespawnStatus);
        }

        public override void Unbind()
        {
            _timeRemainingLabel = null;
            _firstPlayerLabel = null;
            _secondPlayerLabel = null;
            _hitMarkerLabel = null;
            _respawnStatusLabel = null;
            _lastConfirmedHitCount = -1;
            _hitMarkerVersion++;
        }

        private void ResolveElements()
        {
            _timeRemainingLabel ??= QueryRequired<Label>("time-remaining-label");
            _firstPlayerLabel ??= QueryRequired<Label>("first-player-label");
            _secondPlayerLabel ??= QueryRequired<Label>("second-player-label");
            _hitMarkerLabel ??= QueryRequired<Label>("hit-marker-label");
            _respawnStatusLabel ??= QueryRequired<Label>("respawn-status-label");
        }

        private void UpdateHitMarker(int confirmedHitCount)
        {
            if (_lastConfirmedHitCount < 0)
            {
                _lastConfirmedHitCount = confirmedHitCount;
                HideHitMarker();
                return;
            }

            if (confirmedHitCount > _lastConfirmedHitCount)
            {
                ShowHitMarker();
            }

            _lastConfirmedHitCount = confirmedHitCount;
        }

        private void ShowHitMarker()
        {
            _hitMarkerLabel.style.display = DisplayStyle.Flex;
            var version = ++_hitMarkerVersion;
            _hitMarkerLabel.schedule.Execute(() => HideHitMarker(version)).StartingIn(HitMarkerDurationMilliseconds);
        }

        private void HideHitMarker(int version)
        {
            if (version == _hitMarkerVersion)
            {
                HideHitMarker();
            }
        }

        private void HideHitMarker()
        {
            if (_hitMarkerLabel != null)
            {
                _hitMarkerLabel.style.display = DisplayStyle.None;
            }
        }

        private void UpdateRespawnStatus(string status)
        {
            _respawnStatusLabel.text = status;
            _respawnStatusLabel.style.display = string.IsNullOrEmpty(status) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
