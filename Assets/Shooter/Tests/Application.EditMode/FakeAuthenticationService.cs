using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Application.Tests
{
    public sealed class FakeAuthenticationService : IAuthenticationService
    {
        public AuthenticationResult Result { get; set; } = AuthenticationResult.Success("test-player");

        public int CallCount { get; private set; }

        public Task<AuthenticationResult> AuthenticateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(Result);
        }
    }
}
