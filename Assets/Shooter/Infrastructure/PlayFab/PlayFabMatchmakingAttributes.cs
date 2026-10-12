using System;

namespace Shooter.Infrastructure.PlayFab
{
    [Serializable]
    public sealed class PlayFabMatchmakingAttributes
    {
        public PlayFabQosLatency[] Latencies;

        public PlayFabMatchmakingAttributes(PlayFabQosLatency[] latencies)
        {
            Latencies = latencies ?? throw new ArgumentNullException(nameof(latencies));
        }
    }
}
