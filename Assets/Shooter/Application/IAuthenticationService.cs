using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Application
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(CancellationToken cancellationToken);
    }
}
