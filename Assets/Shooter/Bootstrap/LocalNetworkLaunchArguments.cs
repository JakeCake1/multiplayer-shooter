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
        private const string ShutdownSignalOption = "--shooter-shutdown-signal";
        private const string PlayFabTitleIdOption = "--shooter-playfab-title-id";
        private const string PlayerIdOption = "--shooter-player-id";
        private const string PlayFabMatchmakingQueueOption = "--shooter-playfab-matchmaking-queue";

        public static NetworkSessionStartRequest CreateClientRequest(string[] arguments)
        {
            ValidateArguments(arguments);
            return NetworkSessionStartRequest.ForClient(ReadSessionName(arguments), PlayerCount);
        }

        public static NetworkSessionStartRequest CreateServerRequest(string[] arguments)
        {
            ValidateArguments(arguments);
            var sessionName = ReadSessionName(arguments);
            var portText = ReadOption(arguments, PortOption);
            var port = portText == null ? DefaultServerPort : ParsePort(portText);
            return NetworkSessionStartRequest.ForServer(sessionName, port, PlayerCount);
        }

        public static string GetShutdownSignalPath(string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            return ReadOption(arguments, ShutdownSignalOption);
        }

        public static string GetPlayFabTitleId(string[] arguments)
        {
            ValidateArguments(arguments);
            return ReadRequiredOption(arguments, PlayFabTitleIdOption);
        }

        public static string GetPlayerId(string[] arguments)
        {
            ValidateArguments(arguments);
            return ReadRequiredOption(arguments, PlayerIdOption);
        }

        public static string GetPlayFabMatchmakingQueue(string[] arguments)
        {
            ValidateArguments(arguments);
            return ReadOption(arguments, PlayFabMatchmakingQueueOption);
        }

        private static void ValidateArguments(string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }
        }

        private static string ReadSessionName(string[] arguments)
        {
            return ReadOption(arguments, SessionOption) ?? DefaultSessionName;
        }

        private static string ReadOption(string[] arguments, string option)
        {
            for (var index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(arguments[index], option, StringComparison.Ordinal))
                {
                    continue;
                }

                if (index + 1 >= arguments.Length || string.IsNullOrWhiteSpace(arguments[index + 1]) || arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Command-line option '{option}' requires a value.");
                }

                return arguments[index + 1];
            }

            return null;
        }

        private static string ReadRequiredOption(string[] arguments, string option)
        {
            return ReadOption(arguments, option) ?? throw new ArgumentException($"Required command-line option '{option}' is missing.");
        }

        private static ushort ParsePort(string value)
        {
            if (!ushort.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var port) || port == 0)
            {
                throw new ArgumentException($"Command-line option '{PortOption}' must be an integer from 1 to 65535.");
            }

            return port;
        }
    }
}
