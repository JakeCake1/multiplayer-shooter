using Shooter.Application;

namespace Shooter.Application.Tests
{
    public sealed class FakeApplicationQuitter : IApplicationQuitter
    {
        public bool WasRequested { get; private set; }

        public void Quit()
        {
            WasRequested = true;
        }
    }
}
