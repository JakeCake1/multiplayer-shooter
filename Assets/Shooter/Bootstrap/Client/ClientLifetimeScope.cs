using System;
using Shooter.Application;
using Shooter.Infrastructure.Fusion;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private GameObject playerPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            var request = LocalNetworkLaunchArguments.CreateRequest(
                NetworkSessionRole.Client,
                Environment.GetCommandLineArgs());

            builder.RegisterInstance(new FusionNetworkSessionOptions(playerPrefab));
            builder.Register<FusionNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.RegisterInstance(request);
            builder.RegisterEntryPoint<LocalProcessDiagnostics>();
            builder.RegisterEntryPoint<LocalNetworkSessionStarter>();
        }
    }
}
