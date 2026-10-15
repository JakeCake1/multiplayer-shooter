using System;
using Shooter.Presentation.Ui;
using UnityEngine.UIElements;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class PlayingHudView : UiToolkitView
    {
        private Label _timeRemainingLabel;
        private Label _firstPlayerLabel;
        private Label _secondPlayerLabel;

        public override void Bind(IUiViewModel viewModel)
        {
            var playing = viewModel as PlayingHudViewModel ?? throw new ArgumentException($"{nameof(PlayingHudView)} requires {nameof(PlayingHudViewModel)}.", nameof(viewModel));
            ResolveElements();
            _timeRemainingLabel.text = playing.TimeRemaining;
            _firstPlayerLabel.text = playing.FirstPlayer;
            _secondPlayerLabel.text = playing.SecondPlayer;
        }

        public override void Unbind()
        {
            _timeRemainingLabel = null;
            _firstPlayerLabel = null;
            _secondPlayerLabel = null;
        }

        private void ResolveElements()
        {
            _timeRemainingLabel ??= QueryRequired<Label>("time-remaining-label");
            _firstPlayerLabel ??= QueryRequired<Label>("first-player-label");
            _secondPlayerLabel ??= QueryRequired<Label>("second-player-label");
        }
    }
}
