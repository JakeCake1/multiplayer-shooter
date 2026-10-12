using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class WaitingForPlayersViewModel : IUiViewModel
    {
        public WaitingForPlayersViewModel(string players, string countdown)
        {
            Players = players ?? string.Empty;
            Countdown = countdown ?? string.Empty;
        }

        public string Players { get; }

        public string Countdown { get; }
    }
}
