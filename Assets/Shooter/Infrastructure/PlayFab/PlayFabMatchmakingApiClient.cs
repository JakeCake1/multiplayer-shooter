using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingApiClient : IDisposable
    {
        private readonly IPlayFabPlayerSession _playerSession;
        private readonly PlayFabClientOptions _clientOptions;
        private readonly PlayFabMatchmakingOptions _matchmakingOptions;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _disposed;

        public PlayFabMatchmakingApiClient(IPlayFabPlayerSession playerSession, PlayFabClientOptions clientOptions, PlayFabMatchmakingOptions matchmakingOptions)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _clientOptions = clientOptions ?? throw new ArgumentNullException(nameof(clientOptions));
            _matchmakingOptions = matchmakingOptions ?? throw new ArgumentNullException(nameof(matchmakingOptions));
        }

        public async Task CancelOutstandingTicketsAsync(CancellationToken cancellationToken)
        {
            var payload = new JObject { ["QueueName"] = _matchmakingOptions.QueueName };
            await SendAsync("/Match/CancelAllMatchmakingTicketsForPlayer", payload, cancellationToken);
        }

        public async Task<string> CreateTicketAsync(string attributes, CancellationToken cancellationToken)
        {
            var entity = GetEntityKey();
            var creator = new JObject { ["Entity"] = new JObject { ["Id"] = entity.Id, ["Type"] = entity.Type }, ["Attributes"] = new JObject { ["EscapedDataObject"] = attributes } };
            var payload = new JObject { ["Creator"] = creator, ["GiveUpAfterSeconds"] = _matchmakingOptions.TimeoutInSeconds, ["QueueName"] = _matchmakingOptions.QueueName };
            var response = await SendAsync("/Match/CreateMatchmakingTicket", payload, cancellationToken);
            return RequireString(response["data"]?["TicketId"], "PlayFab create-ticket response did not contain a TicketId.");
        }

        public async Task<PlayFabMatchmakingTicketSnapshot> GetTicketAsync(string ticketId, CancellationToken cancellationToken)
        {
            var payload = new JObject { ["EscapeObject"] = true, ["QueueName"] = _matchmakingOptions.QueueName, ["TicketId"] = ticketId };
            var response = await SendAsync("/Match/GetMatchmakingTicket", payload, cancellationToken);
            var data = response["data"] ?? throw new InvalidOperationException("PlayFab get-ticket response did not contain data.");
            return new PlayFabMatchmakingTicketSnapshot(ticketId, RequireString(data["Status"], "PlayFab ticket response did not contain a status."), (string)data["MatchId"], (string)data["CancellationReasonString"]);
        }

        public async Task<PlayFabMatchSnapshot> GetMatchAsync(string matchId, CancellationToken cancellationToken)
        {
            var payload = new JObject { ["EscapeObject"] = true, ["MatchId"] = matchId, ["QueueName"] = _matchmakingOptions.QueueName, ["ReturnMemberAttributes"] = false };
            var response = await SendAsync("/Match/GetMatch", payload, cancellationToken);
            var data = response["data"] ?? throw new InvalidOperationException("PlayFab get-match response did not contain data.");
            var server = data["ServerDetails"];
            var port = server?["Ports"]?.First?["Num"]?.Value<int>() ?? 0;
            return new PlayFabMatchSnapshot(RequireString(data["MatchId"], "PlayFab match response did not contain a MatchId."), (string)server?["IPV4Address"], port, (string)server?["Region"]);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _httpClient.Dispose();
        }

        private async Task<JObject> SendAsync(string path, JObject payload, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var entityToken = await GetEntityTokenAsync();
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"{_clientOptions.ApiEndpoint}{path}"))
            {
                request.Headers.TryAddWithoutValidation("X-EntityToken", entityToken);
                request.Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                using (var response = await _httpClient.SendAsync(request, cancellationToken))
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"PlayFab request '{path}' failed (HTTP {(int)response.StatusCode}): {responseJson}");
                    }

                    return JObject.Parse(responseJson);
                }
            }
        }

        private async Task<string> GetEntityTokenAsync()
        {
            var entity = _playerSession.PlayerEntity ?? throw new InvalidOperationException("PlayFab matchmaking requires an authenticated player.");
            var tokenResult = await entity.GetEntityTokenAsync();
            if (tokenResult.Failed() || string.IsNullOrWhiteSpace(tokenResult.Result.Token))
            {
                throw new InvalidOperationException($"PlayFab failed to provide the player entity token (HRESULT 0x{tokenResult.HResult:X8}).");
            }

            return tokenResult.Result.Token;
        }

        private global::PlayFab.PFEntityKey GetEntityKey()
        {
            var entity = _playerSession.PlayerEntity ?? throw new InvalidOperationException("PlayFab matchmaking requires an authenticated player.");
            var entityKeyResult = entity.GetEntityKey();
            if (entityKeyResult.Failed())
            {
                throw new InvalidOperationException($"PlayFab failed to provide the player entity key (HRESULT 0x{entityKeyResult.HResult:X8}).");
            }

            return entityKeyResult.Result;
        }

        private static string RequireString(JToken value, string errorMessage)
        {
            var result = (string)value;
            return string.IsNullOrWhiteSpace(result) ? throw new InvalidOperationException(errorMessage) : result;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlayFabMatchmakingApiClient));
            }
        }
    }
}
