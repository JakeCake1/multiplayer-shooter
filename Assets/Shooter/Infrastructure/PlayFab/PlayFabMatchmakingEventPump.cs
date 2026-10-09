using PlayFab.Multiplayer;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingEventPump : MonoBehaviour
    {
        private const float ProcessingIntervalSeconds = 0.1f;

        private float _nextProcessingTime;

        private void OnEnable()
        {
            _nextProcessingTime = 0f;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextProcessingTime)
            {
                return;
            }

            _nextProcessingTime = Time.unscaledTime + ProcessingIntervalSeconds;
            PlayFabMultiplayer.ProcessMatchmakingStateChanges();
        }
    }
}
