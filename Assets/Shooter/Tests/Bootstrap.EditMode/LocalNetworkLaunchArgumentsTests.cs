using NUnit.Framework;
using Shooter.Application;

namespace Shooter.Bootstrap.Tests
{
    public sealed class LocalNetworkLaunchArgumentsTests
    {
        [Test]
        public void CreateRequest_ClientUsesDefaultSession()
        {
            var request = LocalNetworkLaunchArguments.CreateRequest(
                NetworkSessionRole.Client,
                new[] { "ShooterClient.exe" });

            Assert.That(request.Role, Is.EqualTo(NetworkSessionRole.Client));
            Assert.That(request.SessionName, Is.EqualTo(LocalNetworkLaunchArguments.DefaultSessionName));
            Assert.That(request.Port, Is.Zero);
            Assert.That(request.PlayerCount, Is.EqualTo(2));
        }

        [Test]
        public void CreateRequest_ServerReadsSessionAndPort()
        {
            var request = LocalNetworkLaunchArguments.CreateRequest(
                NetworkSessionRole.Server,
                new[]
                {
                    "ShooterServer.exe",
                    "--shooter-session",
                    "test-match",
                    "--shooter-port",
                    "28000"
                });

            Assert.That(request.Role, Is.EqualTo(NetworkSessionRole.Server));
            Assert.That(request.SessionName, Is.EqualTo("test-match"));
            Assert.That(request.Port, Is.EqualTo(28000));
        }

        [TestCase("0")]
        [TestCase("65536")]
        [TestCase("not-a-port")]
        public void CreateRequest_ServerRejectsInvalidPort(string value)
        {
            Assert.That(
                () => LocalNetworkLaunchArguments.CreateRequest(
                    NetworkSessionRole.Server,
                    new[] { "ShooterServer.exe", "--shooter-port", value }),
                Throws.ArgumentException);
        }

        [Test]
        public void CreateRequest_RejectsOptionWithoutValue()
        {
            Assert.That(
                () => LocalNetworkLaunchArguments.CreateRequest(
                    NetworkSessionRole.Client,
                    new[] { "ShooterClient.exe", "--shooter-session" }),
                Throws.ArgumentException);
        }
    }
}
