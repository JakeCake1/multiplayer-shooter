using System;
using Shooter.Application;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.Fusion;
using Shooter.Infrastructure.PlayFab;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Server
{
    public static class ServerServiceComposition
    {
        public static void RegisterLocal(IContainerBuilder builder, MatchConfiguration configuration, MatchRules matchRules, NetworkSessionStartRequest request, string shutdownSignalPath)
        {
            RegisterShared(builder, configuration, matchRules);
            builder.RegisterInstance(new ServerProcessShutdownPolicy(false, TimeSpan.Zero));
            builder.RegisterInstance(request);
            builder.Register<LocalServerAllocationService>(Lifetime.Singleton).As<IServerAllocationService>();
            builder.RegisterEntryPoint<ServerLocalProcessDiagnostics>();
            LocalShutdownServiceComposition.Register(builder, shutdownSignalPath);
            RegisterSessionHost(builder);
        }

        public static void RegisterPlayFab(IContainerBuilder builder, MatchConfiguration configuration, MatchRules matchRules)
        {
            RegisterShared(builder, configuration, matchRules);
            builder.RegisterInstance(new ServerProcessShutdownPolicy(true, TimeSpan.FromSeconds(configuration.Match.ServerShutdownGraceSeconds)));
#if ENABLE_PLAYFABSERVER_API
            builder.Register<PlayFabServerAllocationService>(Lifetime.Singleton).As<IServerAllocationService>();
            RegisterSessionHost(builder);
#else
            throw new InvalidOperationException("PlayFab server mode requires ENABLE_PLAYFABSERVER_API.");
#endif
        }

        private static void RegisterShared(IContainerBuilder builder, MatchConfiguration configuration, MatchRules matchRules)
        {
            builder.RegisterInstance(configuration);
            builder.RegisterInstance(matchRules);
            builder.Register<ServerMatchLifecycle>(Lifetime.Singleton).As<IServerMatchLifecycle>();
            builder.Register<FusionNetworkAssetLoader>(Lifetime.Singleton);
            builder.Register<FusionArenaLoader>(Lifetime.Singleton);
            builder.Register<FusionServerNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
        }

        private static void RegisterSessionHost(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ServerNetworkSessionHost>();
        }
    }
}
