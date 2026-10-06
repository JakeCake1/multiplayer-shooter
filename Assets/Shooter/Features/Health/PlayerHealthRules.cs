using System;

namespace Shooter.Features.Health
{
    public sealed class PlayerHealthRules
    {
        public PlayerHealthRules(int maxHealth)
        {
            if (maxHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth));
            }

            MaxHealth = maxHealth;
        }

        public int MaxHealth { get; }

        public int ApplyDamage(int currentHealth, int damage)
        {
            ValidateCurrentHealth(currentHealth);
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            return Math.Max(0, currentHealth - damage);
        }

        public bool IsDead(int currentHealth)
        {
            ValidateCurrentHealth(currentHealth);
            return currentHealth == 0;
        }

        private void ValidateCurrentHealth(int currentHealth)
        {
            if (currentHealth < 0 || currentHealth > MaxHealth)
            {
                throw new ArgumentOutOfRangeException(nameof(currentHealth));
            }
        }
    }
}
