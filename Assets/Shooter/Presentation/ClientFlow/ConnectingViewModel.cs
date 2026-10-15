using System;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ConnectingViewModel : IUiViewModel
    {
        private readonly Action _cancel;

        public ConnectingViewModel(string status, Action cancel = null)
        {
            Status = status ?? string.Empty;
            _cancel = cancel;
        }

        public string Status { get; }

        public bool CanCancel => _cancel != null;

        public void RequestCancel()
        {
            _cancel?.Invoke();
        }
    }
}
