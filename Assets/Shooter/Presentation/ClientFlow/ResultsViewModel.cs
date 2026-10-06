using System;
using System.Threading.Tasks;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ResultsViewModel : IUiViewModel
    {
        public ResultsViewModel(string outcome, string firstPlayerScore, string secondPlayerScore, Func<Task> findGameAgainAsync, Action exit)
        {
            Outcome = outcome ?? string.Empty;
            FirstPlayerScore = firstPlayerScore ?? string.Empty;
            SecondPlayerScore = secondPlayerScore ?? string.Empty;
            FindGameAgainAsync = findGameAgainAsync ?? throw new ArgumentNullException(nameof(findGameAgainAsync));
            Exit = exit ?? throw new ArgumentNullException(nameof(exit));
        }

        public string Outcome { get; }

        public string FirstPlayerScore { get; }

        public string SecondPlayerScore { get; }

        public Func<Task> FindGameAgainAsync { get; }

        public Action Exit { get; }
    }
}
