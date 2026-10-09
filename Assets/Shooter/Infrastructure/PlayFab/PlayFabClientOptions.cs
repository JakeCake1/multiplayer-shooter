using System;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabClientOptions
    {
        public PlayFabClientOptions(string titleId, string customId)
        {
            TitleId = RequireValue(titleId, nameof(titleId));
            CustomId = RequireValue(customId, nameof(customId));
        }

        public string TitleId { get; }

        public string CustomId { get; }

        public string ApiEndpoint => $"https://{TitleId}.playfabapi.com";

        private static string RequireValue(string value, string parameterName)
        {
            return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A non-empty value is required.", parameterName) : value.Trim();
        }
    }
}
