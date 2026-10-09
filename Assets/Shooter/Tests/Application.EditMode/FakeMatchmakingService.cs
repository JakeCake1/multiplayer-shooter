using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Application.Tests
{
    public sealed class FakeMatchmakingService : IMatchmakingService
    {
        public MatchmakingResult Result { get; set; } = MatchmakingResult.Success(NetworkSessionStartRequest.ForClient("test-session"));

        public int CallCount { get; private set; }

        public Task<MatchmakingResult> FindMatchAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromResult(Result);
        }
    }
}
