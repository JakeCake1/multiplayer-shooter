using System;
using NUnit.Framework;

namespace Shooter.Application.Tests
{
    public sealed class ServerProcessShutdownPolicyTests
    {
        [Test]
        public void RejectsNegativeGracePeriod()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ServerProcessShutdownPolicy(true, TimeSpan.FromSeconds(-1)));
        }

        [Test]
        public void PreservesConfiguredBehavior()
        {
            var gracePeriod = TimeSpan.FromSeconds(10d);
            var policy = new ServerProcessShutdownPolicy(true, gracePeriod);

            Assert.That(policy.ShutdownAfterMatchFinished, Is.True);
            Assert.That(policy.MatchFinishedGracePeriod, Is.EqualTo(gracePeriod));
        }
    }
}
