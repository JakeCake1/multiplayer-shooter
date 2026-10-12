using System;

namespace Shooter.Features.Player
{
    public sealed class PlayerRespawnRules
    {
        public PlayerRespawnRules(float delaySeconds)
        {
            if (float.IsNaN(delaySeconds) || float.IsInfinity(delaySeconds) || delaySeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(delaySeconds));
            }

            DelaySeconds = delaySeconds;
        }

        public float DelaySeconds { get; }
    }
}
