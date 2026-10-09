using PlayFab;

namespace Shooter.Infrastructure.PlayFab
{
    public interface IPlayFabPlayerSession
    {
        PFPlayerEntity PlayerEntity { get; }
    }
}
