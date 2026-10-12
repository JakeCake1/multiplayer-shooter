using System;

namespace Shooter.Application
{
    public sealed class AuthenticationResult
    {
        private AuthenticationResult(bool succeeded, string playerId, string problem)
        {
            Succeeded = succeeded;
            PlayerId = playerId ?? string.Empty;
            Problem = problem ?? string.Empty;
        }

        public bool Succeeded { get; }

        public string PlayerId { get; }

        public string Problem { get; }

        public static AuthenticationResult Success(string playerId)
        {
            return new AuthenticationResult(true, string.IsNullOrWhiteSpace(playerId) ? throw new ArgumentException("A player id is required.", nameof(playerId)) : playerId, string.Empty);
        }

        public static AuthenticationResult Failure(string problem)
        {
            return new AuthenticationResult(false, string.Empty, string.IsNullOrWhiteSpace(problem) ? "Authentication failed." : problem);
        }
    }
}
