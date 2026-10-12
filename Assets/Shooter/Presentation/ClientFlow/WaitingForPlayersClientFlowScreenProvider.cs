using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class WaitingForPlayersClientFlowScreenProvider : IClientFlowScreenProvider
    {
        public ClientFlowState State => ClientFlowState.WaitingForPlayers;

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var players = snapshot.Match == null ? "Connected. Waiting for replicated match state..." : $"Players: {snapshot.Match.ConnectedPlayerCount}/2";
            var countdown = snapshot.Match == null ? string.Empty : $"Starting in: {snapshot.Match.SecondsRemaining:0.0}s";
            return new UiScreenRequest(ClientUiScreenAddresses.WaitingForPlayers, new WaitingForPlayersViewModel(players, countdown));
        }
    }
}
