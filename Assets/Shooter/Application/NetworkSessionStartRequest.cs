using System;

namespace Shooter.Application
{
    public sealed class NetworkSessionStartRequest
    {
        private NetworkSessionStartRequest(
            NetworkSessionRole role,
            string sessionName,
            ushort port,
            int playerCount)
        {
            if (string.IsNullOrWhiteSpace(sessionName))
            {
                throw new ArgumentException("A session name is required.", nameof(sessionName));
            }

            if (playerCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(playerCount), playerCount, "Player count must be positive.");
            }

            if (role == NetworkSessionRole.Server && port == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), port, "A local server must use a non-zero port.");
            }

            Role = role;
            SessionName = sessionName;
            Port = port;
            PlayerCount = playerCount;
        }

        public NetworkSessionRole Role { get; }

        public string SessionName { get; }

        public ushort Port { get; }

        public int PlayerCount { get; }

        public static NetworkSessionStartRequest ForServer(string sessionName, ushort port, int playerCount = 2)
        {
            return new NetworkSessionStartRequest(NetworkSessionRole.Server, sessionName, port, playerCount);
        }

        public static NetworkSessionStartRequest ForClient(string sessionName, int playerCount = 2)
        {
            return new NetworkSessionStartRequest(NetworkSessionRole.Client, sessionName, 0, playerCount);
        }
    }
}
