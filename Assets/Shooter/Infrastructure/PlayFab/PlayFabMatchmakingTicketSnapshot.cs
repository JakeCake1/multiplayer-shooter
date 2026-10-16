namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingTicketSnapshot
    {
        public PlayFabMatchmakingTicketSnapshot(string ticketId, string status, string matchId, string cancellationReason)
        {
            TicketId = ticketId;
            Status = status;
            MatchId = matchId;
            CancellationReason = cancellationReason;
        }

        public string TicketId { get; }

        public string Status { get; }

        public string MatchId { get; }

        public string CancellationReason { get; }
    }
}
