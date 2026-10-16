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

        public int DetermineForfeitWinnerPlayerId(int firstPlayerId, int secondPlayerId, int forfeitingPlayerId)
        {
            ValidatePlayerId(firstPlayerId, nameof(firstPlayerId));
            ValidatePlayerId(secondPlayerId, nameof(secondPlayerId));
            ValidatePlayerId(forfeitingPlayerId, nameof(forfeitingPlayerId));
            if (firstPlayerId == secondPlayerId)
            {
                throw new ArgumentException("Match result players must be distinct.");
            }

            if (forfeitingPlayerId != firstPlayerId && forfeitingPlayerId != secondPlayerId)
            {
                throw new ArgumentException("The forfeiting player must belong to the match.", nameof(forfeitingPlayerId));
            }

            return forfeitingPlayerId == firstPlayerId ? secondPlayerId : firstPlayerId;
        }

        private static void ValidatePlayer(int playerId, int kills, string playerIdParameterName, string killsParameterName)
        {
            ValidatePlayerId(playerId, playerIdParameterName);

            if (kills < 0)
            {
                throw new ArgumentOutOfRangeException(killsParameterName);
            }
        }

        private static void ValidatePlayerId(int playerId, string parameterName)
        {
            if (playerId < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
