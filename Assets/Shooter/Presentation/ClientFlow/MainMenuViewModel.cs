using System;
using System.Threading.Tasks;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class MainMenuViewModel : IUiViewModel
    {
        public MainMenuViewModel(string problem, bool isAuthenticationReady, float mouseSensitivity, float minimumMouseSensitivity, float maximumMouseSensitivity, Action<float> setMouseSensitivity, Func<Task> retryAuthenticationAsync, Func<Task> findGameAsync, Action exit)
        {
            Problem = problem ?? string.Empty;
            IsAuthenticationReady = isAuthenticationReady;
            MouseSensitivity = mouseSensitivity;
            MinimumMouseSensitivity = minimumMouseSensitivity;
            MaximumMouseSensitivity = maximumMouseSensitivity;
            SetMouseSensitivity = setMouseSensitivity ?? throw new ArgumentNullException(nameof(setMouseSensitivity));
            RetryAuthenticationAsync = retryAuthenticationAsync ?? throw new ArgumentNullException(nameof(retryAuthenticationAsync));
            FindGameAsync = findGameAsync ?? throw new ArgumentNullException(nameof(findGameAsync));
            Exit = exit ?? throw new ArgumentNullException(nameof(exit));
        }

        public string Problem { get; }

        public bool IsAuthenticationReady { get; }

        public float MouseSensitivity { get; }

        public float MinimumMouseSensitivity { get; }

        public float MaximumMouseSensitivity { get; }

        public Action<float> SetMouseSensitivity { get; }

        public Func<Task> RetryAuthenticationAsync { get; }

        public Func<Task> FindGameAsync { get; }

        public Action Exit { get; }
    }
}
