using System;
using Shooter.Application;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.Fusion;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Server
{
    public sealed class ServerLifetimeScope : LifetimeScope
    {
        [SerializeField, Min(1)]
        private int requiredPlayerCount = 2;

        [SerializeField, Min(0f)]
        private float countdownSeconds = 3f;

        [SerializeField, Min(0.1f)]
        private float matchSeconds = 60f;

        [SerializeField, Min(0f)]
        private float finishingSeconds = 1f;

        protected override void Configure(IContainerBuilder builder)
        {
            var arguments = Environment.GetCommandLineArgs();
            var request = LocalNetworkLaunchArguments.CreateServerRequest(arguments);
            var shutdownSignalPath = LocalNetworkLaunchArguments.GetShutdownSignalPath(arguments);

            var matchRules = new MatchRules(
                requiredPlayerCount,
                countdownSeconds,
                matchSeconds,
                finishingSeconds);

            builder.RegisterInstance(matchRules);
            builder.Register<FusionNetworkAssetLoader>(Lifetime.Singleton);
            builder.Register<FusionServerNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.RegisterInstance(request);
            builder.RegisterEntryPoint<ServerLocalProcessDiagnostics>();
            builder.RegisterEntryPoint<LocalNetworkSessionStarter>();

            if (!string.IsNullOrWhiteSpace(shutdownSignalPath))
            {
                builder.RegisterInstance(new LocalShutdownSignal(shutdownSignalPath));
                builder.RegisterEntryPoint<LocalShutdownWatcher>();
            }
        }
    }
}
