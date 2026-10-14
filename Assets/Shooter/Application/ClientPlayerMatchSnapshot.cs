namespace Shooter.Application
{
    public sealed class ClientPlayerMatchSnapshot
    {
        public ClientPlayerMatchSnapshot(int playerId, int kills, int currentHealth = 0, int maxHealth = 0, bool isLocal = false)
        {
            PlayerId = playerId;
            Kills = kills;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            IsLocal = isLocal;
        }

        public int PlayerId { get; }

        public int Kills { get; }

        public int CurrentHealth { get; }

        public int MaxHealth { get; }

        public bool IsLocal { get; }
    }
}
