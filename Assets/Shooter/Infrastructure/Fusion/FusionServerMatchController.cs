using System;
using System.Collections.Generic;
using global::Fusion;
using Shooter.Application;
using Shooter.Features.MatchRules;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionServerMatchController : SimulationBehaviour
    {
        private MatchRules _rules;
        private IServerMatchLifecycle _matchLifecycle;
        private MatchStateMachine _stateMachine;
        private FusionMatchState _replicatedState;
        private FusionMatchResultState _resultState;
        private readonly HashSet<PlayerRef> _roundPlayers = new HashSet<PlayerRef>();
        private FusionServerMatchResultPublisher _resultPublisher;
        private bool _reuseFinishedMatches;

        public void Configure(MatchRules rules, IServerMatchLifecycle matchLifecycle, bool reuseFinishedMatches)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _matchLifecycle = matchLifecycle ?? throw new ArgumentNullException(nameof(matchLifecycle));
            _resultPublisher = new FusionServerMatchResultPublisher(_rules.RequiredPlayerCount);
            _reuseFinishedMatches = reuseFinishedMatches;
        }

        public void Attach(FusionMatchState replicatedState, FusionMatchResultState resultState)
        {
            _replicatedState = replicatedState != null ? replicatedState : throw new ArgumentNullException(nameof(replicatedState));
            _resultState = resultState != null ? resultState : throw new ArgumentNullException(nameof(resultState));
            _stateMachine = new MatchStateMachine(_rules ?? throw new InvalidOperationException("Match rules must be configured before the replicated state is attached."));
            PublishState(CountConnectedPlayers());
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasSpawnedState())
            {
                return;
            }

            var connectedPlayerCount = CountConnectedPlayers();
            if (TryPrepareNextMatch(connectedPlayerCount))
            {
                return;
            }

            var previousPhase = _stateMachine.Phase;
            _resultPublisher.CaptureScores(Runner);
            _stateMachine.Advance(connectedPlayerCount, Runner.DeltaTime);
            CaptureRoundPlayersIfStarted(previousPhase);
            PublishResultIfReady();
            PublishStateIfPhaseChanged(previousPhase, connectedPlayerCount);
            TryPrepareNextMatch(connectedPlayerCount);
        }

        public void HandlePlayerLeft(PlayerRef player, NetworkObject playerObject)
        {
            if (_stateMachine == null || !HasSpawnedState())
            {
                return;
            }

            _roundPlayers.Remove(player);
            if (_stateMachine.Phase != MatchPhase.Playing)
            {
                TryPrepareNextMatch(CountConnectedPlayersExcluding(player));
                return;
            }

            if (!_resultPublisher.TryPublishForfeit(Runner, _resultState, player, playerObject))
            {
                Debug.LogError($"[MatchResult][Server] Could not publish forfeit result for Player {player.PlayerId}.");
            }

            if (_stateMachine.TryBeginFinishing())
            {
                PublishState(CountConnectedPlayersExcluding(player));
                ReportFinishedMatchIfNeeded();
            }
        }

        private void CaptureRoundPlayersIfStarted(MatchPhase previousPhase)
        {
            if (previousPhase != MatchPhase.WaitingForPlayers || _stateMachine.Phase == MatchPhase.WaitingForPlayers)
            {
                return;
            }

            _roundPlayers.Clear();
            foreach (var player in Runner.ActivePlayers)
            {
                _roundPlayers.Add(player);
            }
        }

        private bool TryPrepareNextMatch(int connectedPlayerCount)
        {
            if (!_reuseFinishedMatches || _stateMachine.Phase != MatchPhase.Finished || _roundPlayers.Count > 0)
            {
                return false;
            }

            _stateMachine.Reset();
            _resultState.ClearResult();
            _resultPublisher.Reset();
            _matchLifecycle.PrepareNextMatch();
            PublishState(connectedPlayerCount);
            Debug.Log("[Match][Server] Prepared the local session for the next match.");
            return true;
        }

        private void PublishResultIfReady()
        {
            if (_stateMachine.Phase != MatchPhase.Finishing && _stateMachine.Phase != MatchPhase.Finished)
            {
                return;
            }

            _resultPublisher.TryPublish(Runner, _resultState);
        }

        private bool HasSpawnedState()
        {
            return FusionNetworkBehaviourLifecycle.IsSpawned(_replicatedState) && FusionNetworkBehaviourLifecycle.IsSpawned(_resultState);
        }

        private void PublishStateIfPhaseChanged(MatchPhase previousPhase, int connectedPlayerCount)
        {
            if (previousPhase == _stateMachine.Phase)
            {
                _replicatedState.Publish(_stateMachine.Phase, connectedPlayerCount, _stateMachine.SecondsRemaining);
                return;
            }

            PublishState(connectedPlayerCount);
            ReportFinishedMatchIfNeeded();
        }

        private void ReportFinishedMatchIfNeeded()
        {
            if (_stateMachine.Phase == MatchPhase.Finished)
            {
                _matchLifecycle.ReportMatchFinished();
            }
        }

        private void PublishState(int connectedPlayerCount)
        {
            _replicatedState.Publish(_stateMachine.Phase, connectedPlayerCount, _stateMachine.SecondsRemaining);
            Debug.Log($"[Match][Server] Authoritative phase: {_stateMachine.Phase}; players: {connectedPlayerCount}; remaining: {_stateMachine.SecondsRemaining:0.0}s.");
        }

        private int CountConnectedPlayers()
        {
            var count = 0;
            foreach (var _ in Runner.ActivePlayers)
            {
                count++;
            }

            return count;
        }

        private int CountConnectedPlayersExcluding(PlayerRef excludedPlayer)
        {
            var count = 0;
            foreach (var player in Runner.ActivePlayers)
            {
                if (player != excludedPlayer)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
