using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ConnectingViewModel : IUiViewModel
    {
        public ConnectingViewModel(string status)
        {
            Status = status ?? string.Empty;
        }

        public string Status { get; }
    }
}
