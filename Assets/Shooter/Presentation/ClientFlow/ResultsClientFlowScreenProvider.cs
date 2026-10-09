using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ResultsClientFlowScreenProvider : IClientFlowScreenProvider
    {
        private readonly IClientFlowController _flowController;

        public ResultsClientFlowScreenProvider(IClientFlowController flowController)
        {
            _flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
        }

        public ClientFlowState State => ClientFlowState.Results;

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var outcome = BuildOutcome(snapshot.Match);
            var firstPlayerScore = snapshot.Match == null ? string.Empty : $"Player {snapshot.Match.FirstPlayerId}: {snapshot.Match.FirstPlayerKills}";
            var secondPlayerScore = snapshot.Match == null ? string.Empty : $"Player {snapshot.Match.SecondPlayerId}: {snapshot.Match.SecondPlayerKills}";
            var viewModel = new ResultsViewModel(outcome, firstPlayerScore, secondPlayerScore, _flowController.FindGameAsync, _flowController.Exit);
            return new UiScreenRequest(ClientUiScreenAddresses.Results, viewModel);
        }

        private static string BuildOutcome(ClientMatchSnapshot match)
        {
            if (match == null)
            {
                return "Waiting for authoritative results...";
            }

            return match.IsDraw ? "Draw" : $"Winner: Player {match.WinnerPlayerId}";
        }
    }
}
