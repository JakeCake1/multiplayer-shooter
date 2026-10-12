using System;
using Shooter.Application;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.Fusion;
using UnityEngine;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientMatchFlowBridge : ITickable
    {
        private readonly IClientFlowController _controller;
        private FusionMatchState _matchState;
        private ClientMatchSnapshot _lastSnapshot;

        public ClientMatchFlowBridge(IClientFlowController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void Tick()
        {
            FindMatchStateIfNeeded();
            if (_matchState == null || _matchState.Object == null)
            {
                return;
            }

            var snapshot = CreateSnapshot();
            if (HasChanged(snapshot))
            {
                _lastSnapshot = snapshot;
                _controller.ObserveMatch(snapshot);
            }
        }

        private void FindMatchStateIfNeeded()
        {
            if (_matchState == null)
            {
                _matchState = UnityEngine.Object.FindFirstObjectByType<FusionMatchState>();
            }
        }

        private ClientMatchSnapshot CreateSnapshot()
        {
            var result = _matchState.GetComponent<FusionMatchResultState>();
            if (_matchState.Phase == MatchPhase.Finished && result != null && result.HasResult)
            {
                return new ClientMatchSnapshot(ClientMatchStage.Results, _matchState.ConnectedPlayerCount, 0f, result.FirstPlayerId, result.FirstPlayerKills, result.SecondPlayerId, result.SecondPlayerKills, result.WinnerPlayerId, result.IsDraw);
            }

            var stage = _matchState.Phase == MatchPhase.Playing || _matchState.Phase == MatchPhase.Finishing ? ClientMatchStage.Playing : ClientMatchStage.WaitingForPlayers;
            return new ClientMatchSnapshot(stage, _matchState.ConnectedPlayerCount, _matchState.SecondsRemaining);
        }

        private bool HasChanged(ClientMatchSnapshot snapshot)
        {
            return _lastSnapshot == null || _lastSnapshot.Stage != snapshot.Stage || _lastSnapshot.ConnectedPlayerCount != snapshot.ConnectedPlayerCount || Mathf.CeilToInt(_lastSnapshot.SecondsRemaining) != Mathf.CeilToInt(snapshot.SecondsRemaining) || _lastSnapshot.FirstPlayerKills != snapshot.FirstPlayerKills || _lastSnapshot.SecondPlayerKills != snapshot.SecondPlayerKills;
        }
    }
}
