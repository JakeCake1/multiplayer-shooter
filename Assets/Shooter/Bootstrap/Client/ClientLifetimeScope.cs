using System;
using Shooter.Application;
using Shooter.Infrastructure.Fusion;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var arguments = Environment.GetCommandLineArgs();
            var request = LocalNetworkLaunchArguments.CreateClientRequest(arguments);
            var shutdownSignalPath = LocalNetworkLaunchArguments.GetShutdownSignalPath(arguments);

            builder.Register<FusionNetworkAssetLoader>(Lifetime.Singleton);
            builder.Register<FusionClientNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.RegisterInstance(request);
            builder.RegisterEntryPoint<ClientLocalProcessDiagnostics>();
            builder.RegisterEntryPoint<LocalNetworkSessionStarter>();

            if (!string.IsNullOrWhiteSpace(shutdownSignalPath))
            {
                builder.RegisterInstance(new LocalShutdownSignal(shutdownSignalPath));
                builder.RegisterEntryPoint<LocalShutdownWatcher>();
            }
        }
    }
}
