using System;

namespace Shooter.Application
{
    public sealed class MatchConfiguration
    {
        public MatchConfiguration(int version, MatchTimingConfiguration match, PlayerGameplayConfiguration player, WeaponGameplayConfiguration weapon)
        {
            Version = version > 0 ? version : throw new ArgumentOutOfRangeException(nameof(version));
            Match = match ?? throw new ArgumentNullException(nameof(match));
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
        }

        public int Version { get; }

        public MatchTimingConfiguration Match { get; }

        public PlayerGameplayConfiguration Player { get; }

        public WeaponGameplayConfiguration Weapon { get; }
    }
}
