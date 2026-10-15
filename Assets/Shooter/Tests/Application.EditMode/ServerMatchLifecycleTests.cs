using NUnit.Framework;

namespace Shooter.Application.Tests
{
    public sealed class ServerMatchLifecycleTests
    {
        [Test]
        public void ReportsMatchFinishedOnce()
        {
            var lifecycle = new ServerMatchLifecycle();
            var notificationCount = 0;
            lifecycle.MatchFinished += () => notificationCount++;

            lifecycle.ReportMatchFinished();
            lifecycle.ReportMatchFinished();

            Assert.That(notificationCount, Is.EqualTo(1));
        }
    }
}
