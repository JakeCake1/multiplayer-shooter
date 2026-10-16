namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchSnapshot
    {
        public PlayFabMatchSnapshot(string matchId, string ipv4Address, int port, string region)
        {
            MatchId = matchId;
            Ipv4Address = ipv4Address;
            Port = port;
            Region = region;
        }

        public string MatchId { get; }

        public string Ipv4Address { get; }

        public int Port { get; }

        public string Region { get; }

        public bool HasServerAllocation => !string.IsNullOrWhiteSpace(Ipv4Address) && Port > 0;
    }
}
