using Shooter.Application;

namespace Shooter.Bootstrap.Client
{
    public sealed class UnityApplicationQuitter : IApplicationQuitter
    {
        public void Quit()
        {
            UnityEngine.Application.Quit();
        }
    }
}
