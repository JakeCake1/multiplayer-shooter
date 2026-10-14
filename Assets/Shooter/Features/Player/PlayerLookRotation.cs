namespace Shooter.Features.Player
{
    public readonly struct PlayerLookRotation
    {
        public PlayerLookRotation(float yaw, float pitch)
        {
            Yaw = yaw;
            Pitch = pitch;
        }

        public float Yaw { get; }

        public float Pitch { get; }
    }
}
