using Fusion;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public abstract class DebugOverlay : MonoBehaviour
    {
        protected static bool IsSpawned(NetworkBehaviour networkBehaviour)
        {
            return networkBehaviour != null && networkBehaviour.Object != null && networkBehaviour.Object.IsValid;
        }
    }
}
