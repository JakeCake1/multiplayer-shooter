using System;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.PlayFab;
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
            var matchRules = CreateMatchRules();
            if (PlayFabServerEnvironment.IsConfigured())
            {
                ServerServiceComposition.RegisterPlayFab(builder, matchRules);
                return;
            }

            var arguments = Environment.GetCommandLineArgs();
            var request = LocalNetworkLaunchArguments.CreateServerRequest(arguments);
            var shutdownSignalPath = LocalNetworkLaunchArguments.GetShutdownSignalPath(arguments);
            ServerServiceComposition.RegisterLocal(builder, matchRules, request, shutdownSignalPath);
        }

        private MatchRules CreateMatchRules()
        {
            return new MatchRules(requiredPlayerCount, countdownSeconds, matchSeconds, finishingSeconds);
        }
    }
}
