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

        [SerializeField]
        private GameObject matchStatePrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            var arguments = Environment.GetCommandLineArgs();
            var request = LocalNetworkLaunchArguments.CreateClientRequest(arguments);
            var shutdownSignalPath = LocalNetworkLaunchArguments.GetShutdownSignalPath(arguments);

            builder.RegisterInstance(new FusionNetworkSessionOptions(
                playerPrefab,
                matchStatePrefab));
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
