using System;
using Shooter.Application;
using VContainer;

namespace Shooter.Bootstrap.Client
{
    public static class LocalClientBackendComposition
    {
        public static void Register(IContainerBuilder builder, NetworkSessionStartRequest request)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(request ?? throw new ArgumentNullException(nameof(request)));
            builder.Register<LocalClientAuthenticationService>(Lifetime.Singleton).As<IAuthenticationService>();
            builder.Register<LocalClientMatchmakingService>(Lifetime.Singleton).As<IMatchmakingService>();
        }
    }
}
