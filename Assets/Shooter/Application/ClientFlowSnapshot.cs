namespace Shooter.Application
{
    public sealed class ClientFlowSnapshot
    {
        public ClientFlowSnapshot(ClientFlowState state, string problem = "", ClientMatchSnapshot match = null)
        {
            State = state;
            Problem = problem ?? string.Empty;
            Match = match;
        }

        public ClientFlowState State { get; }

        public string Problem { get; }

        public ClientMatchSnapshot Match { get; }
    }
}
