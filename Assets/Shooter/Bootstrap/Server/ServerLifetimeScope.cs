using System;
using Shooter.Application;
using Shooter.Bootstrap.Configuration;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.PlayFab;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Server
{
    public sealed class ServerLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            using var configurationService = new AddressableMatchConfigurationService();
            var configuration = configurationService.Load();
            var matchRules = CreateMatchRules(configuration);
            if (PlayFabServerEnvironment.IsConfigured())
            {
                ServerServiceComposition.RegisterPlayFab(builder, configuration, matchRules);
                return;
            }

            var arguments = Environment.GetCommandLineArgs();
            var request = LocalNetworkLaunchArguments.CreateServerRequest(arguments, configuration.Match.RequiredPlayerCount);
            var shutdownSignalPath = LocalNetworkLaunchArguments.GetShutdownSignalPath(arguments);
            ServerServiceComposition.RegisterLocal(builder, configuration, matchRules, request, shutdownSignalPath);
        }

        private static MatchRules CreateMatchRules(MatchConfiguration configuration)
        {
            var match = configuration.Match;
            return new MatchRules(match.RequiredPlayerCount, match.CountdownSeconds, match.DurationSeconds, match.FinishingSeconds);
        }
    }
}
