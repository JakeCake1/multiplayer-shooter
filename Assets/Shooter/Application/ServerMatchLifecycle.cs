using System;

namespace Shooter.Application
{
    public sealed class ServerMatchLifecycle : IServerMatchLifecycle
    {
        private bool _matchFinishedReported;

        public event Action MatchFinished;

        public void ReportMatchFinished()
        {
            if (_matchFinishedReported)
            {
                return;
            }

            _matchFinishedReported = true;
            MatchFinished?.Invoke();
        }
    }
}
