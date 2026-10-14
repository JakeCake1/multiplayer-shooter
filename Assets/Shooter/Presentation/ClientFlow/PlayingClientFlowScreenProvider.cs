using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class PlayingClientFlowScreenProvider : IClientFlowScreenProvider
    {
        public ClientFlowState State => ClientFlowState.Playing;

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var viewModel = new PlayingHudViewModel(BuildTime(snapshot.Match), BuildPlayer(snapshot.Match?.FirstPlayer), BuildPlayer(snapshot.Match?.SecondPlayer));
            return new UiScreenRequest(ClientUiScreenAddresses.PlayingHud, viewModel);
        }

        private static string BuildTime(ClientMatchSnapshot match)
        {
            var secondsRemaining = match == null ? 0 : Math.Max(0, (int)Math.Ceiling(match.SecondsRemaining));
            return $"{secondsRemaining / 60:00}:{secondsRemaining % 60:00}";
        }

        private static string BuildPlayer(ClientPlayerMatchSnapshot player)
        {
            if (player == null)
            {
                return "Waiting for player state...";
            }

            var name = player.IsLocal ? $"You (Player {player.PlayerId})" : $"Player {player.PlayerId}";
            return $"{name}   HP {player.CurrentHealth}/{player.MaxHealth}   Kills {player.Kills}";
        }
    }
}
