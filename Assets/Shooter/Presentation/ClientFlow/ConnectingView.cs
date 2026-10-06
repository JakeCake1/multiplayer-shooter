using System;
using Shooter.Presentation.Ui;
using UnityEngine.UIElements;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ConnectingView : UiToolkitView
    {
        private Label _statusLabel;

        public override void Bind(IUiViewModel viewModel)
        {
            var connecting = viewModel as ConnectingViewModel ?? throw new ArgumentException($"{nameof(ConnectingView)} requires {nameof(ConnectingViewModel)}.", nameof(viewModel));
            _statusLabel = QueryRequired<Label>("status-label");
            _statusLabel.text = connecting.Status;
        }

        public override void Unbind()
        {
            _statusLabel = null;
        }
    }
}
