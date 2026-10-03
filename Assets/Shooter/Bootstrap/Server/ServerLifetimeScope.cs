using System;
using Shooter.Application;
using Shooter.Infrastructure.Fusion;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Server
{
    public sealed class ServerLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private GameObject playerPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new FusionNetworkSessionOptions(playerPrefab));
            builder.Register<FusionNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.RegisterInstance(LocalNetworkLaunchArguments.CreateRequest(
                NetworkSessionRole.Server,
                Environment.GetCommandLineArgs()));
            builder.RegisterEntryPoint<LocalNetworkSessionStarter>();
        }
    }
}
