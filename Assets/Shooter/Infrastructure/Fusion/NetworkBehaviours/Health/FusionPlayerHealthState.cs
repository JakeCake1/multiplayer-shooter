using System;
using global::Fusion;
using Shooter.Features.Health;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class FusionPlayerHealthState : NetworkBehaviour
    {
        private PlayerHealthRules _rules;

        [Networked]
        public int CurrentHealth { get; private set; }

        public int MaxHealth => _rules.MaxHealth;

        public bool IsDead => _rules.IsDead(CurrentHealth);

        public override void Spawned()
        {
            _rules = new PlayerHealthRules(FusionMatchConfigurationResolver.Resolve(Runner).Player.MaximumHealth);
            if (Object.HasStateAuthority)
            {
                CurrentHealth = _rules.MaxHealth;
            }
        }

        public int ApplyDamage(int damage)
        {
            EnsureStateAuthority();

            var previousHealth = CurrentHealth;
            CurrentHealth = _rules.ApplyDamage(CurrentHealth, damage);
            return previousHealth - CurrentHealth;
        }

        public void RestoreFullHealth()
        {
            EnsureStateAuthority();
            CurrentHealth = _rules.MaxHealth;
        }

        private void EnsureStateAuthority()
        {
            if (!Object.HasStateAuthority)
            {
                throw new InvalidOperationException("Only state authority can change player health.");
            }
        }
    }
}
