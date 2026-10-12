using System;
using System.Threading.Tasks;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class MainMenuViewModel : IUiViewModel
    {
        public MainMenuViewModel(string problem, Func<Task> findGameAsync, Action exit)
        {
            Problem = problem ?? string.Empty;
            FindGameAsync = findGameAsync ?? throw new ArgumentNullException(nameof(findGameAsync));
            Exit = exit ?? throw new ArgumentNullException(nameof(exit));
        }

        public string Problem { get; }

        public Func<Task> FindGameAsync { get; }

        public Action Exit { get; }
    }
}
