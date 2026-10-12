using System;

namespace Shooter.Presentation.Ui
{
    public sealed class UiScreenRequest
    {
        public UiScreenRequest(string address, IUiViewModel viewModel)
        {
            Address = string.IsNullOrWhiteSpace(address) ? throw new ArgumentException("A UI screen address is required.", nameof(address)) : address;
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        public string Address { get; }

        public IUiViewModel ViewModel { get; }
    }
}
