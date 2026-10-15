using System;
using System.IO;

namespace Shooter.Infrastructure.PlayFab
{
    public static class PlayFabServerEnvironment
    {
        private const string ConfigurationFileVariable = "GSDK_CONFIG_FILE";

        public static bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConfigurationFileVariable));
        }

        public static void ValidateConfigurationFile()
        {
            var configurationFile = Environment.GetEnvironmentVariable(ConfigurationFileVariable);
            if (string.IsNullOrWhiteSpace(configurationFile))
            {
                throw new InvalidOperationException($"Environment variable '{ConfigurationFileVariable}' is not configured.");
            }

            if (!File.Exists(configurationFile))
            {
                throw new FileNotFoundException("The PlayFab GSDK configuration file does not exist.", configurationFile);
            }
        }
    }
}
