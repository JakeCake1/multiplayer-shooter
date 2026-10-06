using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap
{
    public static class LocalShutdownServiceComposition
    {
        public static void Register(IContainerBuilder builder, string shutdownSignalPath)
        {
            if (string.IsNullOrWhiteSpace(shutdownSignalPath))
            {
                return;
            }

            builder.RegisterInstance(new LocalShutdownSignal(shutdownSignalPath));
            builder.RegisterEntryPoint<LocalShutdownWatcher>();
        }
    }
}
