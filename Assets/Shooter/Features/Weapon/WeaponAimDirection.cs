namespace Shooter.Features.Weapon
{
    public readonly struct WeaponAimDirection
    {
        public WeaponAimDirection(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public float Horizontal { get; }

        public float Vertical { get; }
    }
}
