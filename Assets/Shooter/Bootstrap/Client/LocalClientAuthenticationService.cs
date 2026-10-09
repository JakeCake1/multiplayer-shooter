using System.Threading;
using System.Threading.Tasks;
using Shooter.Application;

namespace Shooter.Bootstrap.Client
{
    public sealed class LocalClientAuthenticationService : IAuthenticationService
    {
        public Task<AuthenticationResult> AuthenticateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(AuthenticationResult.Success("local-player"));
        }
    }
}
