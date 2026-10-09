using System;
using System.Threading;
using System.Threading.Tasks;
using PlayFab.Multiplayer;
using Shooter.Application;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabMatchmakingService : IMatchmakingService, IDisposable
    {
        private readonly IPlayFabPlayerSession _playerSession;
        private readonly PlayFabMatchmakingOptions _options;
        private readonly PlayFabMultiplayerRuntime _runtime;
        private MatchmakingTicket _activeTicket;
        private TaskCompletionSource<MatchmakingResult> _activeCompletion;
        private bool _disposed;

        public PlayFabMatchmakingService(IPlayFabPlayerSession playerSession, PlayFabMatchmakingOptions options, PlayFabMultiplayerRuntime runtime)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            RegisterEvents();
        }

        public async Task<MatchmakingResult> FindMatchAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (_activeCompletion != null)
            {
                return MatchmakingResult.Failure("A PlayFab matchmaking request is already active.");
            }

            if (_playerSession.PlayerEntity == null)
            {
                return MatchmakingResult.Failure("PlayFab matchmaking requires an authenticated player.");
            }

            try
            {
                StartMatchmakingRequest();
            }
            catch (Exception exception)
            {
                ClearActiveRequest();
                return MatchmakingResult.Failure($"PlayFab failed to start matchmaking: {exception.Message}");
            }

            Debug.Log($"[PlayFab] Matchmaking ticket requested for queue '{_options.QueueName}'.");
            var completion = _activeCompletion;
            using (cancellationToken.Register(CancelActiveRequest))
            {
                return await completion.Task;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            UnregisterEvents();
            CancelActiveRequest();
        }

        private MatchmakingTicket CreateTicket()
        {
            var user = new MatchUser(_playerSession.PlayerEntity, "{}");
            return PlayFabMultiplayer.CreateMatchmakingTicket(user, _options.QueueName, _options.TimeoutInSeconds);
        }

        private void StartMatchmakingRequest()
        {
            _runtime.EnsureInitialized();
            _runtime.SetMatchmakingProcessingActive(true);
            _activeCompletion = new TaskCompletionSource<MatchmakingResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _activeTicket = CreateTicket();
            if (_activeTicket == null)
            {
                throw new InvalidOperationException("The SDK returned no matchmaking ticket.");
            }
        }

        private void RegisterEvents()
        {
            PlayFabMultiplayer.OnMatchmakingTicketStatusChanged += HandleTicketStatusChanged;
            PlayFabMultiplayer.OnMatchmakingTicketCompleted += HandleTicketCompleted;
        }

        private void UnregisterEvents()
        {
            PlayFabMultiplayer.OnMatchmakingTicketStatusChanged -= HandleTicketStatusChanged;
            PlayFabMultiplayer.OnMatchmakingTicketCompleted -= HandleTicketCompleted;
        }

        private void HandleTicketStatusChanged(MatchmakingTicket ticket)
        {
            if (!ReferenceEquals(ticket, _activeTicket))
            {
                return;
            }

            Debug.Log($"[PlayFab] Matchmaking ticket '{ticket.TicketId}' status: {ticket.Status}.");
        }

        private void HandleTicketCompleted(MatchmakingTicket ticket, int result)
        {
            if (!ReferenceEquals(ticket, _activeTicket) || _activeCompletion == null)
            {
                return;
            }

            var completion = _activeCompletion;
            var matchmakingResult = CreateCompletionResult(ticket, result);
            ClearActiveRequest();
            completion.TrySetResult(matchmakingResult);
        }

        private MatchmakingResult CreateCompletionResult(MatchmakingTicket ticket, int result)
        {
            if (result != 0)
            {
                return MatchmakingResult.Failure($"PlayFab matchmaking failed (HRESULT 0x{result:X8}, status {ticket.Status}).");
            }

            if (ticket.Status != MatchmakingTicketStatus.Matched)
            {
                return MatchmakingResult.Failure($"PlayFab matchmaking completed with status {ticket.Status}.");
            }

            return CreateMatchedResult(ticket.GetMatchDetails());
        }

        private MatchmakingResult CreateMatchedResult(MatchmakingMatchDetails details)
        {
            if (details == null || string.IsNullOrWhiteSpace(details.MatchId))
            {
                return MatchmakingResult.Failure("PlayFab returned a match without a MatchId.");
            }

            if (details.ServerDetails == null)
            {
                return MatchmakingResult.Failure("PlayFab found a match but did not allocate a multiplayer server. Enable server allocation for the queue.");
            }

            LogAllocatedServer(details);
            return MatchmakingResult.Success(NetworkSessionStartRequest.ForClient(details.MatchId, _options.PlayerCount));
        }

        private static void LogAllocatedServer(MatchmakingMatchDetails details)
        {
            var server = details.ServerDetails;
            var endpoint = server.Ports.Count > 0 ? $"{server.Ipv4Address}:{server.Ports[0].Num}" : server.Ipv4Address;
            Debug.Log($"[PlayFab] Match '{details.MatchId}' allocated server {endpoint} in region '{server.Region}'. Fusion will join session '{details.MatchId}'.");
        }

        private void CancelActiveRequest()
        {
            var ticket = _activeTicket;
            var completion = _activeCompletion;
            ClearActiveRequest();
            ticket?.Cancel();
            completion?.TrySetCanceled();
        }

        private void ClearActiveRequest()
        {
            _activeTicket = null;
            _activeCompletion = null;
            _runtime.SetMatchmakingProcessingActive(false);
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
