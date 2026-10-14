using System;

namespace Shooter.Application
{
    public interface IServerMatchLifecycle
    {
        event Action MatchFinished;

        void ReportMatchFinished();
    }
}
