using Shooter.Application;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.Fusion;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Server
{
    public static class ServerServiceComposition
    {
        public static void Register(IContainerBuilder builder, MatchRules matchRules, NetworkSessionStartRequest request, string shutdownSignalPath)
        {
            builder.RegisterInstance(matchRules);
            builder.Register<FusionNetworkAssetLoader>(Lifetime.Singleton);
            builder.Register<FusionServerNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.RegisterInstance(request);
            builder.RegisterEntryPoint<ServerLocalProcessDiagnostics>();
            builder.RegisterEntryPoint<LocalNetworkSessionStarter>();
            LocalShutdownServiceComposition.Register(builder, shutdownSignalPath);
        }
    }
}
