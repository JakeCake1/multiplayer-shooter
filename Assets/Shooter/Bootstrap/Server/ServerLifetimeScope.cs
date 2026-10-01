using System;
using Shooter.Application;
using Shooter.Infrastructure.Fusion;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Server
{
    public sealed class ServerLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<FusionNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.RegisterInstance(LocalNetworkLaunchArguments.CreateRequest(
                NetworkSessionRole.Server,
                Environment.GetCommandLineArgs()));
            builder.RegisterEntryPoint<LocalNetworkSessionStarter>();
        }
    }
}
