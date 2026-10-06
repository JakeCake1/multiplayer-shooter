using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ClientFlowScreenFactory
    {
        private readonly IClientFlowController _flowController;

        public ClientFlowScreenFactory(IClientFlowController flowController)
        {
            _flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
        }

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return snapshot.State switch { ClientFlowState.Menu => CreateMainMenu(snapshot), ClientFlowState.Connecting => CreateConnecting(), ClientFlowState.WaitingForPlayers => CreateWaitingForPlayers(snapshot.Match), ClientFlowState.Results => CreateResults(snapshot.Match), ClientFlowState.Playing => null, _ => throw new ArgumentOutOfRangeException(nameof(snapshot), snapshot.State, "Unsupported client flow state.") };
        }

        private UiScreenRequest CreateMainMenu(ClientFlowSnapshot snapshot)
        {
            var viewModel = new MainMenuViewModel(snapshot.Problem, _flowController.FindGameAsync, _flowController.Exit);
            return new UiScreenRequest(ClientUiScreenAddresses.MainMenu, viewModel);
        }

        private static UiScreenRequest CreateConnecting()
        {
            var viewModel = new ConnectingViewModel("Connecting to the local match server...");
            return new UiScreenRequest(ClientUiScreenAddresses.Connecting, viewModel);
        }

        private static UiScreenRequest CreateWaitingForPlayers(ClientMatchSnapshot match)
        {
            var players = match == null ? "Connected. Waiting for replicated match state..." : $"Players: {match.ConnectedPlayerCount}/2";
            var countdown = match == null ? string.Empty : $"Starting in: {match.SecondsRemaining:0.0}s";
            var viewModel = new WaitingForPlayersViewModel(players, countdown);
            return new UiScreenRequest(ClientUiScreenAddresses.WaitingForPlayers, viewModel);
        }

        private UiScreenRequest CreateResults(ClientMatchSnapshot match)
        {
            var outcome = BuildOutcome(match);
            var firstPlayerScore = match == null ? string.Empty : $"Player {match.FirstPlayerId}: {match.FirstPlayerKills}";
            var secondPlayerScore = match == null ? string.Empty : $"Player {match.SecondPlayerId}: {match.SecondPlayerKills}";
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
