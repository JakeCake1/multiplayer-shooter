using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Application.Tests
{
    public sealed class FakeMatchmakingService : IMatchmakingService
    {
        public MatchmakingResult Result { get; set; } = MatchmakingResult.Success(NetworkSessionStartRequest.ForClient("test-session"));

        public int CallCount { get; private set; }

        public bool WaitForCancellation { get; set; }

        public async Task<MatchmakingResult> FindMatchAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            if (WaitForCancellation)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            return Result;
        }
    }
}
