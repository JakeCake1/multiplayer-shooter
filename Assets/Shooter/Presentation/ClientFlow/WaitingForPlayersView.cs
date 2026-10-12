using System;
using Shooter.Presentation.Ui;
using UnityEngine.UIElements;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class WaitingForPlayersView : UiToolkitView
    {
        private Label _playersLabel;
        private Label _countdownLabel;

        public override void Bind(IUiViewModel viewModel)
        {
            var waiting = viewModel as WaitingForPlayersViewModel ?? throw new ArgumentException($"{nameof(WaitingForPlayersView)} requires {nameof(WaitingForPlayersViewModel)}.", nameof(viewModel));
            ResolveElements();
            _playersLabel.text = waiting.Players;
            _countdownLabel.text = waiting.Countdown;
        }

        public override void Unbind()
        {
            _playersLabel = null;
            _countdownLabel = null;
        }

        private void ResolveElements()
        {
            _playersLabel ??= QueryRequired<Label>("players-label");
            _countdownLabel ??= QueryRequired<Label>("countdown-label");
        }
    }
}
