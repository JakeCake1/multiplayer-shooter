using System;
using Shooter.Bootstrap.Configuration;
using Shooter.Infrastructure.PlayFab;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var arguments = Environment.GetCommandLineArgs();
            using var configurationService = new AddressableMatchConfigurationService();
            var configuration = configurationService.Load();
            var request = LocalNetworkLaunchArguments.CreateClientRequest(arguments, configuration.Match.RequiredPlayerCount);
            var shutdownSignalPath = LocalNetworkLaunchArguments.GetShutdownSignalPath(arguments);
            var playFabOptions = new PlayFabClientOptions(LocalNetworkLaunchArguments.GetPlayFabTitleId(arguments), LocalNetworkLaunchArguments.GetPlayerId(arguments));
            var matchmakingQueue = LocalNetworkLaunchArguments.GetPlayFabMatchmakingQueue(arguments);
            var matchmakingOptions = string.IsNullOrWhiteSpace(matchmakingQueue) ? null : new PlayFabMatchmakingOptions(matchmakingQueue, playerCount: configuration.Match.RequiredPlayerCount);
            ClientServiceComposition.Register(builder, configuration, request, playFabOptions, matchmakingOptions, shutdownSignalPath);
        }
    }
}
