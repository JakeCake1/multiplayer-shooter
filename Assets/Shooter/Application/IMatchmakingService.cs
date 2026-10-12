using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Application
{
    public interface IMatchmakingService
    {
        Task<MatchmakingResult> FindMatchAsync(CancellationToken cancellationToken);
    }
}
