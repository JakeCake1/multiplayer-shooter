using System;
using System.Threading.Tasks;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class MainMenuViewModel : IUiViewModel
    {
        public MainMenuViewModel(string problem, float mouseSensitivity, float minimumMouseSensitivity, float maximumMouseSensitivity, Action<float> setMouseSensitivity, Func<Task> findGameAsync, Action exit)
        {
            Problem = problem ?? string.Empty;
            MouseSensitivity = mouseSensitivity;
            MinimumMouseSensitivity = minimumMouseSensitivity;
            MaximumMouseSensitivity = maximumMouseSensitivity;
            SetMouseSensitivity = setMouseSensitivity ?? throw new ArgumentNullException(nameof(setMouseSensitivity));
            FindGameAsync = findGameAsync ?? throw new ArgumentNullException(nameof(findGameAsync));
            Exit = exit ?? throw new ArgumentNullException(nameof(exit));
        }

        public string Problem { get; }

        public float MouseSensitivity { get; }

        public float MinimumMouseSensitivity { get; }

        public float MaximumMouseSensitivity { get; }

        public Action<float> SetMouseSensitivity { get; }

        public Func<Task> FindGameAsync { get; }

        public Action Exit { get; }
    }
}
