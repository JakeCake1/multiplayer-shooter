using Shooter.Presentation.ClientFlow;
using VContainer;

namespace Shooter.Bootstrap.Client
{
    public static class ClientFlowScreenComposition
    {
        public static void Register(IContainerBuilder builder)
        {
            builder.Register<BootingClientFlowScreenProvider>(Lifetime.Singleton).As<IClientFlowScreenProvider>();
            builder.Register<MenuClientFlowScreenProvider>(Lifetime.Singleton).As<IClientFlowScreenProvider>();
            builder.Register<SearchingClientFlowScreenProvider>(Lifetime.Singleton).As<IClientFlowScreenProvider>();
            builder.Register<ConnectingClientFlowScreenProvider>(Lifetime.Singleton).As<IClientFlowScreenProvider>();
            builder.Register<WaitingForPlayersClientFlowScreenProvider>(Lifetime.Singleton).As<IClientFlowScreenProvider>();
            builder.Register<PlayingClientFlowScreenProvider>(Lifetime.Singleton).As<IClientFlowScreenProvider>();
            builder.Register<ResultsClientFlowScreenProvider>(Lifetime.Singleton).As<IClientFlowScreenProvider>();
        }
    }
}
