using global::Fusion;

namespace Shooter.Infrastructure.Fusion
{
    public static class FusionNetworkBehaviourLifecycle
    {
        public static bool IsSpawned(NetworkBehaviour networkBehaviour)
        {
            return networkBehaviour != null && networkBehaviour.Object != null && networkBehaviour.Object.IsValid;
        }
    }
}
