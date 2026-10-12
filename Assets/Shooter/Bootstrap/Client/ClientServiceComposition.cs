using Shooter.Application;
using Shooter.Infrastructure.Fusion;
using Shooter.Infrastructure.PlayFab;
using Shooter.Presentation.ClientFlow;
using Shooter.Presentation.Ui;
using VContainer;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public static class ClientServiceComposition
    {
        public static void Register(IContainerBuilder builder, NetworkSessionStartRequest request, PlayFabClientOptions playFabOptions, PlayFabMatchmakingOptions matchmakingOptions, string shutdownSignalPath)
        {
            builder.Register<FusionNetworkAssetLoader>(Lifetime.Singleton);
            builder.Register<FusionClientNetworkSession>(Lifetime.Singleton).As<INetworkSession>();
            builder.Register<UnityApplicationQuitter>(Lifetime.Singleton).As<IApplicationQuitter>();
            builder.Register<ClientFlowCoordinator>(Lifetime.Singleton).As<IClientFlowController>();
            builder.Register<UiController>(Lifetime.Singleton);
            builder.Register<ClientFlowScreenFactory>(Lifetime.Singleton);
            builder.Register<ClientFlowUiPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ClientLocalProcessDiagnostics>();
            builder.RegisterEntryPoint<ClientFlowPresentation>();
            builder.RegisterEntryPoint<ClientFlowStartup>();
            builder.RegisterEntryPoint<ClientMatchFlowBridge>();
            ClientFlowScreenComposition.Register(builder);
            ClientBackendComposition.Register(builder, request, playFabOptions, matchmakingOptions);
            LocalShutdownServiceComposition.Register(builder, shutdownSignalPath);
        }
    }
}
