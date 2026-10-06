namespace Shooter.Application
{
    public sealed class ClientMatchSnapshot
    {
        public ClientMatchSnapshot(ClientMatchStage stage, int connectedPlayerCount, float secondsRemaining, int firstPlayerId = 0, int firstPlayerKills = 0, int secondPlayerId = 0, int secondPlayerKills = 0, int winnerPlayerId = 0, bool isDraw = false)
        {
            Stage = stage;
            ConnectedPlayerCount = connectedPlayerCount;
            SecondsRemaining = secondsRemaining;
            FirstPlayerId = firstPlayerId;
            FirstPlayerKills = firstPlayerKills;
            SecondPlayerId = secondPlayerId;
            SecondPlayerKills = secondPlayerKills;
            WinnerPlayerId = winnerPlayerId;
            IsDraw = isDraw;
        }

        public ClientMatchStage Stage { get; }

        public int ConnectedPlayerCount { get; }

        public float SecondsRemaining { get; }

        public int FirstPlayerId { get; }

        public int FirstPlayerKills { get; }

        public int SecondPlayerId { get; }

        public int SecondPlayerKills { get; }

        public int WinnerPlayerId { get; }

        public bool IsDraw { get; }
    }
}
