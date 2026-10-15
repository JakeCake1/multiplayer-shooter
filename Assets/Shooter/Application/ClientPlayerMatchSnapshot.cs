using System;

namespace Shooter.Application
{
    public sealed class ClientPlayerMatchSnapshot : IEquatable<ClientPlayerMatchSnapshot>
    {
        public ClientPlayerMatchSnapshot(int playerId, int kills, int currentHealth = 0, int maxHealth = 0, bool isLocal = false, int confirmedHitCount = 0)
        {
            PlayerId = playerId;
            Kills = kills;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            IsLocal = isLocal;
            ConfirmedHitCount = confirmedHitCount;
        }

        public int PlayerId { get; }

        public int Kills { get; }

        public int CurrentHealth { get; }

        public int MaxHealth { get; }

        public bool IsLocal { get; }

        public int ConfirmedHitCount { get; }

        private (int PlayerId, int Kills, int CurrentHealth, int MaxHealth, bool IsLocal, int ConfirmedHitCount) EqualityKey => (PlayerId, Kills, CurrentHealth, MaxHealth, IsLocal, ConfirmedHitCount);

        public bool Equals(ClientPlayerMatchSnapshot other)
        {
            return other != null && EqualityKey.Equals(other.EqualityKey);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClientPlayerMatchSnapshot);
        }

        public override int GetHashCode()
        {
            return EqualityKey.GetHashCode();
        }
    }
}
