using Shooter.Application;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap.Client
{
    public sealed class FusionClientMatchSnapshotSource
    {
        private FusionMatchState _matchState;
        private FusionPlayerHealthState[] _playerStates;

        public bool TryCreate(out ClientMatchSnapshot snapshot)
        {
            FindMatchStateIfNeeded();
            if (_matchState == null || _matchState.Object == null)
            {
                snapshot = null;
                return false;
            }

            snapshot = CreateSnapshot();
            return true;
        }

        private void FindMatchStateIfNeeded()
        {
            if (_matchState == null)
            {
                _matchState = Object.FindFirstObjectByType<FusionMatchState>();
            }
        }

        private ClientMatchSnapshot CreateSnapshot()
        {
            var result = _matchState.GetComponent<FusionMatchResultState>();
            if (_matchState.Phase == MatchPhase.Finished && result != null && result.HasResult)
            {
                return CreateResultsSnapshot(result);
            }

            return CreateActiveSnapshot();
        }

        private ClientMatchSnapshot CreateResultsSnapshot(FusionMatchResultState result)
        {
            var localPlayerId = GetLocalPlayerId();
            var firstPlayer = new ClientPlayerMatchSnapshot(result.FirstPlayerId, result.FirstPlayerKills, isLocal: result.FirstPlayerId == localPlayerId);
            var secondPlayer = new ClientPlayerMatchSnapshot(result.SecondPlayerId, result.SecondPlayerKills, isLocal: result.SecondPlayerId == localPlayerId);
            return new ClientMatchSnapshot(ClientMatchStage.Results, _matchState.ConnectedPlayerCount, 0f, firstPlayer, secondPlayer, result.WinnerPlayerId, result.IsDraw);
        }

        private ClientMatchSnapshot CreateActiveSnapshot()
        {
            ReadPlayers(out var firstPlayer, out var secondPlayer);
            var stage = _matchState.Phase == MatchPhase.Playing || _matchState.Phase == MatchPhase.Finishing ? ClientMatchStage.Playing : ClientMatchStage.WaitingForPlayers;
            return new ClientMatchSnapshot(stage, _matchState.ConnectedPlayerCount, _matchState.SecondsRemaining, firstPlayer, secondPlayer);
        }

        private void ReadPlayers(out ClientPlayerMatchSnapshot firstPlayer, out ClientPlayerMatchSnapshot secondPlayer)
        {
            firstPlayer = null;
            secondPlayer = null;
            RefreshPlayerStatesIfNeeded();
            var localPlayerId = GetLocalPlayerId();
            for (var index = 0; index < _playerStates.Length; index++)
            {
                InsertPlayer(CreatePlayerSnapshot(_playerStates[index], localPlayerId), ref firstPlayer, ref secondPlayer);
            }
        }

        private void RefreshPlayerStatesIfNeeded()
        {
            if (!HasCompletePlayerStateCache())
            {
                _playerStates = Object.FindObjectsByType<FusionPlayerHealthState>(FindObjectsSortMode.None);
            }
        }

        private bool HasCompletePlayerStateCache()
        {
            if (_playerStates == null || _playerStates.Length != _matchState.ConnectedPlayerCount)
            {
                return false;
            }

            for (var index = 0; index < _playerStates.Length; index++)
            {
                if (_playerStates[index] == null || _playerStates[index].Object == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static ClientPlayerMatchSnapshot CreatePlayerSnapshot(FusionPlayerHealthState healthState, int localPlayerId)
        {
            var playerId = healthState.Object.InputAuthority.PlayerId;
            var scoreState = healthState.GetComponent<FusionPlayerScoreState>();
            var kills = scoreState == null ? 0 : scoreState.Kills;
            return new ClientPlayerMatchSnapshot(playerId, kills, healthState.CurrentHealth, healthState.MaxHealth, playerId == localPlayerId);
        }

        private static void InsertPlayer(ClientPlayerMatchSnapshot player, ref ClientPlayerMatchSnapshot firstPlayer, ref ClientPlayerMatchSnapshot secondPlayer)
        {
            if (firstPlayer == null || player.PlayerId < firstPlayer.PlayerId)
            {
                secondPlayer = firstPlayer;
                firstPlayer = player;
                return;
            }

            if (secondPlayer == null || player.PlayerId < secondPlayer.PlayerId)
            {
                secondPlayer = player;
            }
        }

        private int GetLocalPlayerId()
        {
            return _matchState.Runner == null ? 0 : _matchState.Runner.LocalPlayer.PlayerId;
        }
    }
}
