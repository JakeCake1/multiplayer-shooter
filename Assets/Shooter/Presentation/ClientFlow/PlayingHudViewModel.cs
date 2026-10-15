using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class PlayingHudViewModel : IUiViewModel
    {
        public PlayingHudViewModel(string timeRemaining, string firstPlayer, string secondPlayer)
        {
            TimeRemaining = timeRemaining ?? string.Empty;
            FirstPlayer = firstPlayer ?? string.Empty;
            SecondPlayer = secondPlayer ?? string.Empty;
        }

        public string TimeRemaining { get; }

        public string FirstPlayer { get; }

        public string SecondPlayer { get; }
    }
}
