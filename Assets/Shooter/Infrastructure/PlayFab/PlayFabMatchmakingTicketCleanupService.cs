using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingTicketCleanupService
    {
        private readonly IPlayFabPlayerSession _playerSession;
        private readonly PlayFabMatchmakingOptions _matchmakingOptions;
        private readonly PlayFabMatchmakingApiClient _apiClient;

        public PlayFabMatchmakingTicketCleanupService(IPlayFabPlayerSession playerSession, PlayFabMatchmakingOptions matchmakingOptions, PlayFabMatchmakingApiClient apiClient)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _matchmakingOptions = matchmakingOptions ?? throw new ArgumentNullException(nameof(matchmakingOptions));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task CancelOutstandingTicketsAsync(CancellationToken cancellationToken)
        {
            await _apiClient.CancelOutstandingTicketsAsync(cancellationToken);
            Debug.Log($"[PlayFab][Matchmaking] Cleared outstanding tickets for EntityId '{GetEntityId()}' in queue '{_matchmakingOptions.QueueName}'.");
        }

        private string GetEntityId()
        {
            var entityKeyResult = _playerSession.PlayerEntity.GetEntityKey();
            return entityKeyResult.Failed() ? $"unavailable (HRESULT 0x{entityKeyResult.HResult:X8})" : entityKeyResult.Result.Id;
        }
    }
}
