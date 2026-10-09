using System;

namespace Shooter.Infrastructure.PlayFab
{
    [Serializable]
    public sealed class PlayFabQosLatency
    {
        public string region;
        public int latency;

        public PlayFabQosLatency(string region, int latency)
        {
            this.region = string.IsNullOrWhiteSpace(region) ? throw new ArgumentException("A region is required.", nameof(region)) : region;
            this.latency = latency >= 0 ? latency : throw new ArgumentOutOfRangeException(nameof(latency));
        }
    }
}
