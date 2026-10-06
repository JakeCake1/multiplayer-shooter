using Shooter.Application;
using Shooter.Infrastructure.Fusion;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public static class ClientServiceComposition
    {
        public static void Register(IContainerBuilder builder, NetworkSessionStartRequest request, string shutdownSignalPath)
        {
            builder.Register<FusionNetworkAssetLoader>(Lifetime.Singleton);
            builder.Register<FusionClientNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.Register<UnityApplicationQuitter>(Lifetime.Singleton).As<IApplicationQuitter>();
            builder.Register<ClientFlowCoordinator>(Lifetime.Singleton).As<IClientFlowController>();
            builder.RegisterInstance(request);
            builder.RegisterEntryPoint<ClientLocalProcessDiagnostics>();
            builder.RegisterEntryPoint<ClientFlowPresentation>();
            builder.RegisterEntryPoint<ClientMatchFlowBridge>();
            LocalShutdownServiceComposition.Register(builder, shutdownSignalPath);
        }
    }
}
