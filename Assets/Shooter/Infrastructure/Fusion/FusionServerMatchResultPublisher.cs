using System;
using System.Collections.Generic;
using Fusion;
using Shooter.Features.Score;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionServerMatchResultPublisher
    {
        private const int RequiredResultPlayerCount = 2;
        private static readonly MatchResultRules Rules = new MatchResultRules();

        public bool TryPublish(NetworkRunner runner, FusionMatchResultState resultState)
        {
            if (runner == null)
            {
                throw new ArgumentNullException(nameof(runner));
            }

            if (resultState == null)
            {
                throw new ArgumentNullException(nameof(resultState));
            }

            if (resultState.HasResult)
            {
                return true;
            }

            var scores = CollectPlayerScores(runner);
            if (scores.Count != RequiredResultPlayerCount)
            {
                return false;
            }

            scores.Sort(ComparePlayerIds);
            var first = scores[0];
            var second = scores[1];
            var winnerPlayerId = Rules.DetermineWinnerPlayerId(first.Key, first.Value, second.Key, second.Value);
            resultState.Publish(first.Key, first.Value, second.Key, second.Value, winnerPlayerId);
            LogResult(first, second, winnerPlayerId);
            return true;
        }

        private static List<KeyValuePair<int, int>> CollectPlayerScores(NetworkRunner runner)
        {
            var scores = new List<KeyValuePair<int, int>>(RequiredResultPlayerCount);
            foreach (var player in runner.ActivePlayers)
            {
                if (runner.TryGetPlayerObject(player, out var playerObject) && playerObject.TryGetComponent<FusionPlayerScoreState>(out var scoreState))
                {
                    scores.Add(new KeyValuePair<int, int>(player.PlayerId, scoreState.Kills));
                }
            }

            return scores;
        }

        private static int ComparePlayerIds(KeyValuePair<int, int> left, KeyValuePair<int, int> right)
        {
            return left.Key.CompareTo(right.Key);
        }

        private static void LogResult(KeyValuePair<int, int> first, KeyValuePair<int, int> second, int winnerPlayerId)
        {
            var outcome = winnerPlayerId == MatchResultRules.DrawWinnerPlayerId ? "draw" : $"winner: Player {winnerPlayerId}";
            Debug.Log($"[MatchResult][Server] Player {first.Key}: {first.Value}; Player {second.Key}: {second.Value}; {outcome}.");
        }
    }
}
