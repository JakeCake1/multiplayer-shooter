using System;

namespace Shooter.Infrastructure.PlayFab
{
    [Serializable]
    public sealed class PlayFabQosApiData
    {
        public PlayFabQosServer[] QosServers;
    }
}
