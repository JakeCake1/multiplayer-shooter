using System.Threading;
using System.Threading.Tasks;

namespace Shooter.Infrastructure.PlayFab
{
    public interface IPlayFabMatchmakingAttributesProvider
    {
        Task<string> CreateAttributesAsync(CancellationToken cancellationToken);
    }
}
