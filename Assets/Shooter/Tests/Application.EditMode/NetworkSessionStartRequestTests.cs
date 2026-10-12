using NUnit.Framework;
using Shooter.Application;

namespace Shooter.Application.Tests
{
    public sealed class NetworkSessionStartRequestTests
    {
        [Test]
        public void ForServer_PreservesLocalServerParameters()
        {
            var request = NetworkSessionStartRequest.ForServer("local-session", 27015, 2);

            Assert.That(request.Role, Is.EqualTo(NetworkSessionRole.Server));
            Assert.That(request.SessionName, Is.EqualTo("local-session"));
            Assert.That(request.Port, Is.EqualTo(27015));
            Assert.That(request.PlayerCount, Is.EqualTo(2));
        }

        [Test]
        public void ForClient_DoesNotExposeAServerBindPort()
        {
            var request = NetworkSessionStartRequest.ForClient("local-session");

            Assert.That(request.Role, Is.EqualTo(NetworkSessionRole.Client));
            Assert.That(request.Port, Is.Zero);
        }

        [Test]
        public void EmptySessionName_IsRejected()
        {
            Assert.That(
                () => NetworkSessionStartRequest.ForClient(" "),
                Throws.ArgumentException);
        }

        [Test]
        public void ZeroServerPort_IsRejected()
        {
            Assert.That(
                () => NetworkSessionStartRequest.ForServer("local-session", 0),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void NonPositivePlayerCount_IsRejected()
        {
            Assert.That(
                () => NetworkSessionStartRequest.ForClient("local-session", 0),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
