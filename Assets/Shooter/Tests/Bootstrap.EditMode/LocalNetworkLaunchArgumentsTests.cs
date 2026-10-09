using NUnit.Framework;
using Shooter.Application;

namespace Shooter.Bootstrap.Tests
{
    public sealed class LocalNetworkLaunchArgumentsTests
    {
        [Test]
        public void CreateRequest_ClientUsesDefaultSession()
        {
            var request = LocalNetworkLaunchArguments.CreateClientRequest(new[] { "ShooterClient.exe" });

            Assert.That(request.Role, Is.EqualTo(NetworkSessionRole.Client));
            Assert.That(request.SessionName, Is.EqualTo(LocalNetworkLaunchArguments.DefaultSessionName));
            Assert.That(request.Port, Is.Zero);
            Assert.That(request.PlayerCount, Is.EqualTo(2));
        }

        [Test]
        public void CreateRequest_ServerReadsSessionAndPort()
        {
            var request = LocalNetworkLaunchArguments.CreateServerRequest(new[] { "ShooterServer.exe", "--shooter-session", "test-match", "--shooter-port", "28000" });

            Assert.That(request.Role, Is.EqualTo(NetworkSessionRole.Server));
            Assert.That(request.SessionName, Is.EqualTo("test-match"));
            Assert.That(request.Port, Is.EqualTo(28000));
        }

        [TestCase("0")]
        [TestCase("65536")]
        [TestCase("not-a-port")]
        public void CreateRequest_ServerRejectsInvalidPort(string value)
        {
            Assert.That(() => LocalNetworkLaunchArguments.CreateServerRequest(new[] { "ShooterServer.exe", "--shooter-port", value }), Throws.ArgumentException);
        }

        [Test]
        public void CreateRequest_RejectsOptionWithoutValue()
        {
            Assert.That(() => LocalNetworkLaunchArguments.CreateClientRequest(new[] { "ShooterClient.exe", "--shooter-session" }), Throws.ArgumentException);
        }

        [Test]
        public void GetShutdownSignalPath_ReadsConfiguredPath()
        {
            var path = LocalNetworkLaunchArguments.GetShutdownSignalPath(new[] { "ShooterServer.exe", "--shooter-shutdown-signal", @"C:\temp\local-match.shutdown" });

            Assert.That(path, Is.EqualTo(@"C:\temp\local-match.shutdown"));
        }

        [Test]
        public void GetShutdownSignalPath_ReturnsNullWhenOptionIsAbsent()
        {
            var path = LocalNetworkLaunchArguments.GetShutdownSignalPath(new[] { "ShooterClient.exe" });

            Assert.That(path, Is.Null);
        }

        [Test]
        public void GetPlayFabTitleId_ReadsRequiredOption()
        {
            var titleId = LocalNetworkLaunchArguments.GetPlayFabTitleId(new[] { "ShooterClient.exe", "--shooter-playfab-title-id", "ABCDE" });

            Assert.That(titleId, Is.EqualTo("ABCDE"));
        }

        [Test]
        public void GetPlayerId_ReadsRequiredOption()
        {
            var playerId = LocalNetworkLaunchArguments.GetPlayerId(new[] { "ShooterClient.exe", "--shooter-player-id", "client-a" });

            Assert.That(playerId, Is.EqualTo("client-a"));
        }

        [Test]
        public void GetPlayFabTitleId_RejectsMissingOption()
        {
            var arguments = new[] { "ShooterClient.exe", "--shooter-player-id", "client-a" };

            Assert.That(() => LocalNetworkLaunchArguments.GetPlayFabTitleId(arguments), Throws.ArgumentException);
        }

        [Test]
        public void GetPlayerId_RejectsMissingOption()
        {
            var arguments = new[] { "ShooterClient.exe", "--shooter-playfab-title-id", "ABCDE" };

            Assert.That(() => LocalNetworkLaunchArguments.GetPlayerId(arguments), Throws.ArgumentException);
        }
    }
}
