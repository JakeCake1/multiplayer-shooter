namespace Shooter.Application
{
    public sealed class ClientMatchSnapshot
    {
        public ClientMatchSnapshot(ClientMatchStage stage, int connectedPlayerCount, float secondsRemaining, ClientPlayerMatchSnapshot firstPlayer = null, ClientPlayerMatchSnapshot secondPlayer = null, int winnerPlayerId = 0, bool isDraw = false)
        {
            Stage = stage;
            ConnectedPlayerCount = connectedPlayerCount;
            SecondsRemaining = secondsRemaining;
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
            WinnerPlayerId = winnerPlayerId;
            IsDraw = isDraw;
        }

        public ClientMatchStage Stage { get; }

        public int ConnectedPlayerCount { get; }

        public float SecondsRemaining { get; }

        public ClientPlayerMatchSnapshot FirstPlayer { get; }

        public ClientPlayerMatchSnapshot SecondPlayer { get; }

        public int WinnerPlayerId { get; }

        public bool IsDraw { get; }
    }
}
