using System;
using System.Threading;
using System.Threading.Tasks;
using Shooter.Application;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingService : IMatchmakingService, IDisposable
    {
        private const int StatusPollIntervalMilliseconds = 2000;
        private readonly IPlayFabPlayerSession _playerSession;
        private readonly IPlayFabMatchmakingAttributesProvider _attributesProvider;
        private readonly PlayFabMatchmakingOptions _options;
        private readonly PlayFabMatchmakingApiClient _apiClient;
        private readonly PlayFabMatchmakingTicketCleanupService _ticketCleanup;
        private CancellationTokenSource _activeSearchCancellation;
        private bool _disposed;

        public PlayFabMatchmakingService(IPlayFabPlayerSession playerSession, IPlayFabMatchmakingAttributesProvider attributesProvider, PlayFabMatchmakingOptions options, PlayFabMatchmakingApiClient apiClient, PlayFabMatchmakingTicketCleanupService ticketCleanup)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _attributesProvider = attributesProvider ?? throw new ArgumentNullException(nameof(attributesProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _ticketCleanup = ticketCleanup ?? throw new ArgumentNullException(nameof(ticketCleanup));
        }

        public async Task<MatchmakingResult> FindMatchAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (_activeSearchCancellation != null)
            {
                return CreateFailureResult("A PlayFab matchmaking request is already active.");
            }

            if (_playerSession.PlayerEntity == null)
            {
                return CreateFailureResult("PlayFab matchmaking requires an authenticated player.");
            }

            using (var searchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                _activeSearchCancellation = searchCancellation;
                searchCancellation.CancelAfter(_options.CompletionWatchdogTimeout);
                try
                {
                    return await FindMatchCoreAsync(searchCancellation.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    await TryCleanupAsync();
                    return CreateFailureResult($"PlayFab matchmaking timed out after {_options.CompletionWatchdogTimeout.TotalSeconds:0} seconds.");
                }
                catch (OperationCanceledException)
                {
                    await TryCleanupAsync();
                    throw;
                }
                catch (Exception exception)
                {
                    await TryCleanupAsync();
                    return CreateFailureResult($"PlayFab matchmaking failed: {exception.Message}", exception);
                }
                finally
                {
                    _activeSearchCancellation = null;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _activeSearchCancellation?.Cancel();
        }

        private async Task<MatchmakingResult> FindMatchCoreAsync(CancellationToken cancellationToken)
        {
            var attributes = await _attributesProvider.CreateAttributesAsync(cancellationToken);
            await _ticketCleanup.CancelOutstandingTicketsAsync(cancellationToken);
            var ticketId = await _apiClient.CreateTicketAsync(attributes, cancellationToken);
            Debug.Log($"[PlayFab][Matchmaking] EntityId '{GetPlayerEntityId()}' created ticket '{ticketId}' for queue '{_options.QueueName}'.");
            return await PollTicketAsync(ticketId, cancellationToken);
        }

        private async Task<MatchmakingResult> PollTicketAsync(string ticketId, CancellationToken cancellationToken)
        {
            string previousStatus = null;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var ticket = await _apiClient.GetTicketAsync(ticketId, cancellationToken);
                if (!string.Equals(previousStatus, ticket.Status, StringComparison.Ordinal))
                {
                    previousStatus = ticket.Status;
                    Debug.Log($"[PlayFab][Matchmaking] EntityId '{GetPlayerEntityId()}' ticket '{ticketId}' status: {ticket.Status}.");
                }

                if (string.Equals(ticket.Status, "Matched", StringComparison.OrdinalIgnoreCase))
                {
                    return await CreateMatchedResultAsync(ticket, cancellationToken);
                }

                if (string.Equals(ticket.Status, "Canceled", StringComparison.OrdinalIgnoreCase))
                {
                    return CreateFailureResult($"PlayFab matchmaking ticket '{ticketId}' was canceled: {ticket.CancellationReason ?? "unknown reason"}.");
                }

                await Task.Delay(StatusPollIntervalMilliseconds, cancellationToken);
            }
        }

        private async Task<MatchmakingResult> CreateMatchedResultAsync(PlayFabMatchmakingTicketSnapshot ticket, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ticket.MatchId))
            {
                return CreateFailureResult($"PlayFab ticket '{ticket.TicketId}' matched without a MatchId.");
            }

            var match = await _apiClient.GetMatchAsync(ticket.MatchId, cancellationToken);
            if (!match.HasServerAllocation)
            {
                return CreateFailureResult("PlayFab found a match but did not allocate a multiplayer server. Enable server allocation for the queue.");
            }

            Debug.Log($"[PlayFab] Match '{match.MatchId}' allocated server {match.Ipv4Address}:{match.Port} in region '{match.Region}'. Fusion will join session '{match.MatchId}'.");
            return MatchmakingResult.Success(NetworkSessionStartRequest.ForClient(match.MatchId, _options.PlayerCount));
        }

        private async Task TryCleanupAsync()
        {
            try
            {
                await _ticketCleanup.CancelOutstandingTicketsAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[PlayFab][Matchmaking] Failed to clean up tickets after an interrupted search: {exception.Message}");
            }
        }

        private static MatchmakingResult CreateFailureResult(string message, Exception exception = null)
        {
            if (exception == null)
            {
                Debug.LogError($"[PlayFab][Matchmaking] {message}");
            }
            else
            {
                Debug.LogError($"[PlayFab][Matchmaking] {message}\n{exception}");
            }

            return MatchmakingResult.Failure(message);
        }

        private string GetPlayerEntityId()
        {
            var entityKeyResult = _playerSession.PlayerEntity.GetEntityKey();
            return entityKeyResult.Failed() ? $"unavailable (HRESULT 0x{entityKeyResult.HResult:X8})" : entityKeyResult.Result.Id;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlayFabMatchmakingService));
            }
        }
    }
}
