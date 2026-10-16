using PlayFab.Multiplayer;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingEventPump : MonoBehaviour
    {
        private void Update()
        {
            PlayFabMultiplayer.ProcessLobbyStateChanges();
            PlayFabMultiplayer.ProcessMatchmakingStateChanges();
        }
    }
}
