using System;
using global::Fusion;
using Shooter.Features.Health;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class FusionPlayerHealthState : NetworkBehaviour
    {
        private const int DefaultMaxHealth = 100;
        private static readonly PlayerHealthRules Rules = new PlayerHealthRules(DefaultMaxHealth);

        [Networked]
        public int CurrentHealth { get; private set; }

        public int MaxHealth => Rules.MaxHealth;

        public bool IsDead => Rules.IsDead(CurrentHealth);

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                CurrentHealth = Rules.MaxHealth;
            }
        }

        public int ApplyDamage(int damage)
        {
            if (!Object.HasStateAuthority)
            {
                throw new InvalidOperationException("Only state authority can apply player damage.");
            }

            var previousHealth = CurrentHealth;
            CurrentHealth = Rules.ApplyDamage(CurrentHealth, damage);
            return previousHealth - CurrentHealth;
        }
    }
}
