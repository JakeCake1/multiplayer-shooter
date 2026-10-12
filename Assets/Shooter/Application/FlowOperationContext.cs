using System.Threading;

namespace Shooter.Application
{
    internal readonly struct FlowOperationContext
    {
        public FlowOperationContext(int attempt, CancellationTokenSource source)
        {
            Attempt = attempt;
            Source = source;
            Token = source.Token;
        }

        public int Attempt { get; }

        public CancellationTokenSource Source { get; }

        public CancellationToken Token { get; }
    }
}
