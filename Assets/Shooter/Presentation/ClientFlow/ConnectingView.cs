using System;
using Shooter.Presentation.Ui;
using UnityEngine.UIElements;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ConnectingView : UiToolkitView
    {
        private Label _statusLabel;
        private Button _cancelButton;
        private ConnectingViewModel _viewModel;

        public override void Bind(IUiViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel as ConnectingViewModel ?? throw new ArgumentException($"{nameof(ConnectingView)} requires {nameof(ConnectingViewModel)}.", nameof(viewModel));
            ResolveElements();
            Render();
            Subscribe();
        }

        public override void Unbind()
        {
            Unsubscribe();
            _statusLabel = null;
            _cancelButton = null;
            _viewModel = null;
        }

        private void ResolveElements()
        {
            _statusLabel = QueryRequired<Label>("status-label");
            _cancelButton = QueryRequired<Button>("cancel-button");
        }

        private void Render()
        {
            _statusLabel.text = _viewModel.Status;
            _cancelButton.style.display = _viewModel.CanCancel ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Subscribe()
        {
            _cancelButton.clicked += _viewModel.RequestCancel;
        }

        private void Unsubscribe()
        {
            if (_cancelButton != null && _viewModel != null)
            {
                _cancelButton.clicked -= _viewModel.RequestCancel;
            }
        }
    }
}
