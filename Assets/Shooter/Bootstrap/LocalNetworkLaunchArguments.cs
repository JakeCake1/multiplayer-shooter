using System;
using System.Globalization;
using Shooter.Application;

namespace Shooter.Bootstrap
{
    public static class LocalNetworkLaunchArguments
    {
        public const string DefaultSessionName = "local-1v1";
        public const ushort DefaultServerPort = 27015;
        public const int PlayerCount = 2;

        private const string SessionOption = "--shooter-session";
        private const string PortOption = "--shooter-port";

        public static NetworkSessionStartRequest CreateRequest(
            NetworkSessionRole role,
            string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var sessionName = ReadOption(arguments, SessionOption) ?? DefaultSessionName;

            if (role == NetworkSessionRole.Client)
            {
                return NetworkSessionStartRequest.ForClient(sessionName, PlayerCount);
            }

            var portText = ReadOption(arguments, PortOption);
            var port = portText == null ? DefaultServerPort : ParsePort(portText);
            return NetworkSessionStartRequest.ForServer(sessionName, port, PlayerCount);
        }

        private static string ReadOption(string[] arguments, string option)
        {
            for (var index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], option, StringComparison.Ordinal))
                {
                    continue;
                }

                if (index + 1 >= arguments.Length ||
                    string.IsNullOrWhiteSpace(arguments[index + 1]) ||
                    arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Command-line option '{option}' requires a value.");
                }

                return arguments[index + 1];
            }

            return null;
        }

        private static ushort ParsePort(string value)
        {
            if (!ushort.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var port) ||
                port == 0)
            {
                throw new ArgumentException(
                    $"Command-line option '{PortOption}' must be an integer from 1 to 65535.");
            }

            return port;
        }
    }
}
