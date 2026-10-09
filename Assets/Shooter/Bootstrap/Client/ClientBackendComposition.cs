using System;
using Shooter.Application;
using Shooter.Infrastructure.PlayFab;
using VContainer;

namespace Shooter.Bootstrap.Client
{
    public static class ClientBackendComposition
    {
        public static void Register(IContainerBuilder builder, NetworkSessionStartRequest request, PlayFabClientOptions playFabOptions, PlayFabMatchmakingOptions matchmakingOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(request ?? throw new ArgumentNullException(nameof(request)));
            builder.RegisterInstance(playFabOptions ?? throw new ArgumentNullException(nameof(playFabOptions)));
            builder.Register<PlayFabAuthenticationService>(Lifetime.Singleton).As<IAuthenticationService, IPlayFabPlayerSession>();
            RegisterMatchmaking(builder, matchmakingOptions);
        }

        private static void RegisterMatchmaking(IContainerBuilder builder, PlayFabMatchmakingOptions matchmakingOptions)
        {
            if (matchmakingOptions == null)
            {
                builder.Register<LocalClientMatchmakingService>(Lifetime.Singleton).As<IMatchmakingService>();
                return;
            }

            builder.RegisterInstance(matchmakingOptions);
            builder.Register<PlayFabMultiplayerRuntime>(Lifetime.Singleton);
            builder.Register<PlayFabMatchmakingService>(Lifetime.Singleton).As<IMatchmakingService>();
        }
    }
}
