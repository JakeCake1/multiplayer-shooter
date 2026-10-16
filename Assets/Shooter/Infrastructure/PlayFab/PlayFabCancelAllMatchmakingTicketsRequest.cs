using System;

namespace Shooter.Infrastructure.PlayFab
{
    [Serializable]
    public sealed class PlayFabCancelAllMatchmakingTicketsRequest
    {
        public string QueueName;

        public PlayFabCancelAllMatchmakingTicketsRequest(string queueName)
        {
            QueueName = queueName;
        }
    }
}
