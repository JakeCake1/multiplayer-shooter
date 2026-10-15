namespace Shooter.Features.Weapon
{
    public readonly struct WeaponAimVector
    {
        public WeaponAimVector(float horizontal, float vertical, float depth)
        {
            Horizontal = horizontal;
            Vertical = vertical;
            Depth = depth;
        }

        public float Horizontal { get; }

        public float Vertical { get; }

        public float Depth { get; }
    }
}
