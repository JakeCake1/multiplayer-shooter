using System;
using System.Threading;
using System.Threading.Tasks;
using Shooter.Application;

namespace Shooter.Bootstrap.Client
{
    public sealed class LocalClientMatchmakingService : IMatchmakingService
    {
        private readonly NetworkSessionStartRequest _request;

        public LocalClientMatchmakingService(NetworkSessionStartRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public Task<MatchmakingResult> FindMatchAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(MatchmakingResult.Success(_request));
        }
    }
}
