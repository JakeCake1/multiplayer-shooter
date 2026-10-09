using System;

namespace Shooter.Application
{
    public sealed class MatchmakingResult
    {
        private MatchmakingResult(bool succeeded, NetworkSessionStartRequest sessionRequest, string problem)
        {
            Succeeded = succeeded;
            SessionRequest = sessionRequest;
            Problem = problem ?? string.Empty;
        }

        public bool Succeeded { get; }

        public NetworkSessionStartRequest SessionRequest { get; }

        public string Problem { get; }

        public static MatchmakingResult Success(NetworkSessionStartRequest sessionRequest)
        {
            return new MatchmakingResult(true, sessionRequest ?? throw new ArgumentNullException(nameof(sessionRequest)), string.Empty);
        }

        public static MatchmakingResult Failure(string problem)
        {
            return new MatchmakingResult(false, null, string.IsNullOrWhiteSpace(problem) ? "Matchmaking failed." : problem);
        }
    }
}
