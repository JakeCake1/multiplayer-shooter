namespace Shooter.Gameplay
{
    public readonly struct PlanarMovement
    {
        public PlanarMovement(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public float Horizontal { get; }

        public float Vertical { get; }
    }
}
