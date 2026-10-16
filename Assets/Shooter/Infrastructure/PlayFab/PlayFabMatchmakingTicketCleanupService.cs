using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingTicketCleanupService : IDisposable
    {
        private readonly IPlayFabPlayerSession _playerSession;
        private readonly PlayFabClientOptions _clientOptions;
        private readonly PlayFabMatchmakingOptions _matchmakingOptions;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _disposed;

        public PlayFabMatchmakingTicketCleanupService(IPlayFabPlayerSession playerSession, PlayFabClientOptions clientOptions, PlayFabMatchmakingOptions matchmakingOptions)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _clientOptions = clientOptions ?? throw new ArgumentNullException(nameof(clientOptions));
            _matchmakingOptions = matchmakingOptions ?? throw new ArgumentNullException(nameof(matchmakingOptions));
        }

        public async Task CancelOutstandingTicketsAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var entity = _playerSession.PlayerEntity ?? throw new InvalidOperationException("PlayFab ticket cleanup requires an authenticated player.");
            var tokenResult = await entity.GetEntityTokenAsync();
            if (tokenResult.Failed() || string.IsNullOrWhiteSpace(tokenResult.Result.Token))
            {
                throw new InvalidOperationException($"PlayFab failed to provide the player entity token for ticket cleanup (HRESULT 0x{tokenResult.HResult:X8}).");
            }

            await SendCleanupRequestAsync(tokenResult.Result.Token, cancellationToken);
            Debug.Log($"[PlayFab][Matchmaking] Cleared outstanding tickets for EntityId '{GetEntityId()}' in queue '{_matchmakingOptions.QueueName}'.");
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

        private async Task SendCleanupRequestAsync(string entityToken, CancellationToken cancellationToken)
        {
            var endpoint = $"{_clientOptions.ApiEndpoint}/Match/CancelAllMatchmakingTicketsForPlayer";
            var requestJson = JsonUtility.ToJson(new PlayFabCancelAllMatchmakingTicketsRequest(_matchmakingOptions.QueueName));
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.TryAddWithoutValidation("X-EntityToken", entityToken);
                request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                using (var response = await _httpClient.SendAsync(request, cancellationToken))
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"PlayFab failed to clear outstanding matchmaking tickets (HTTP {(int)response.StatusCode}): {responseJson}");
                    }
                }
            }
        }

        private string GetEntityId()
        {
            var entityKeyResult = _playerSession.PlayerEntity.GetEntityKey();
            return entityKeyResult.Failed() ? $"unavailable (HRESULT 0x{entityKeyResult.HResult:X8})" : entityKeyResult.Result.Id;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlayFabMatchmakingTicketCleanupService));
            }
        }
    }
}
