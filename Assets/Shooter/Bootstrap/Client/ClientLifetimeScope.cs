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
            builder.Register<FusionNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.RegisterInstance(LocalNetworkLaunchArguments.CreateRequest(
                NetworkSessionRole.Client,
                Environment.GetCommandLineArgs()));
            builder.RegisterEntryPoint<LocalNetworkSessionStarter>();
        }
    }
}
