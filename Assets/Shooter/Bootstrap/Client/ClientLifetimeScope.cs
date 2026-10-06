using System;
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
            ClientServiceComposition.Register(builder, request, shutdownSignalPath);
        }
    }
}
