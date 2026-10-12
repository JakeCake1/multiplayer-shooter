using System;

namespace Shooter.Features.Score
{
    public sealed class MatchResultRules
    {
        public const int DrawWinnerPlayerId = -1;

        public int DetermineWinnerPlayerId(int firstPlayerId, int firstKills, int secondPlayerId, int secondKills)
        {
            ValidatePlayer(firstPlayerId, firstKills, nameof(firstPlayerId), nameof(firstKills));
            ValidatePlayer(secondPlayerId, secondKills, nameof(secondPlayerId), nameof(secondKills));
            if (firstPlayerId == secondPlayerId)
            {
                throw new ArgumentException("Match result players must be distinct.");
            }

            if (firstKills == secondKills)
            {
                return DrawWinnerPlayerId;
            }

            return firstKills > secondKills ? firstPlayerId : secondPlayerId;
        }

        private static void ValidatePlayer(int playerId, int kills, string playerIdParameterName, string killsParameterName)
        {
            if (playerId < 0)
            {
                throw new ArgumentOutOfRangeException(playerIdParameterName);
            }

            if (kills < 0)
            {
                throw new ArgumentOutOfRangeException(killsParameterName);
            }
        }
    }
}
