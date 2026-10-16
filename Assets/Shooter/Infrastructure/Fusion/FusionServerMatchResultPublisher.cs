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
        private readonly Dictionary<int, int> _scoresByPlayerId = new Dictionary<int, int>(RequiredResultPlayerCount);

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

            if (!FusionNetworkBehaviourLifecycle.IsSpawned(resultState))
            {
                return false;
            }

            if (resultState.HasResult)
            {
                return true;
            }

            CaptureScores(runner);
            var scores = GetCapturedScores();
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

        public bool TryPublishForfeit(NetworkRunner runner, FusionMatchResultState resultState, PlayerRef forfeitingPlayer, NetworkObject forfeitingPlayerObject)
        {
            if (resultState == null)
            {
                throw new ArgumentNullException(nameof(resultState));
            }

            if (!FusionNetworkBehaviourLifecycle.IsSpawned(resultState))
            {
                return false;
            }

            if (forfeitingPlayerObject == null)
            {
                throw new ArgumentNullException(nameof(forfeitingPlayerObject));
            }

            if (resultState.HasResult)
            {
                return true;
            }

            CaptureScores(runner);
            CapturePlayerScore(forfeitingPlayer, forfeitingPlayerObject);
            var scores = GetCapturedScores();
            if (scores.Count != RequiredResultPlayerCount)
            {
                return false;
            }

            scores.Sort(ComparePlayerIds);
            var first = scores[0];
            var second = scores[1];
            var winnerPlayerId = Rules.DetermineForfeitWinnerPlayerId(first.Key, second.Key, forfeitingPlayer.PlayerId);
            resultState.Publish(first.Key, first.Value, second.Key, second.Value, winnerPlayerId);
            LogForfeitResult(first, second, forfeitingPlayer.PlayerId, winnerPlayerId);
            return true;
        }

        public void CaptureScores(NetworkRunner runner)
        {
            if (runner == null)
            {
                throw new ArgumentNullException(nameof(runner));
            }

            foreach (var player in runner.ActivePlayers)
            {
                if (runner.TryGetPlayerObject(player, out var playerObject))
                {
                    CapturePlayerScore(player, playerObject);
                }
            }
        }

        private void CapturePlayerScore(PlayerRef player, NetworkObject playerObject)
        {
            if (playerObject.TryGetComponent<FusionPlayerScoreState>(out var scoreState) && FusionNetworkBehaviourLifecycle.IsSpawned(scoreState))
            {
                _scoresByPlayerId[player.PlayerId] = scoreState.Kills;
            }
        }

        private List<KeyValuePair<int, int>> GetCapturedScores()
        {
            return new List<KeyValuePair<int, int>>(_scoresByPlayerId);
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

        private static void LogForfeitResult(KeyValuePair<int, int> first, KeyValuePair<int, int> second, int forfeitingPlayerId, int winnerPlayerId)
        {
            Debug.Log($"[MatchResult][Server] Player {first.Key}: {first.Value}; Player {second.Key}: {second.Value}; Player {forfeitingPlayerId} forfeited; winner: Player {winnerPlayerId}.");
        }
    }
}
