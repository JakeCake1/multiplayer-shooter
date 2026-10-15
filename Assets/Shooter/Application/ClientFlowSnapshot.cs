namespace Shooter.Application
{
    public sealed class ClientFlowSnapshot
    {
        public ClientFlowSnapshot(ClientFlowState state, string problem = "", ClientMatchSnapshot match = null, bool isAuthenticationReady = false, bool canFindGame = false)
        {
            State = state;
            Problem = problem ?? string.Empty;
            Match = match;
            IsAuthenticationReady = isAuthenticationReady;
            CanFindGame = canFindGame;
        }

        public ClientFlowState State { get; }

        public string Problem { get; }

        public ClientMatchSnapshot Match { get; }

        public bool IsAuthenticationReady { get; }

        public bool CanFindGame { get; }
    }
}
