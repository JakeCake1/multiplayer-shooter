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
        private readonly IPlayFabMatchmakingAttributesProvider _attributesProvider;
        private readonly PlayFabMatchmakingOptions _options;
        private readonly PlayFabMultiplayerRuntime _runtime;
        private MatchmakingTicket _activeTicket;
        private TaskCompletionSource<MatchmakingResult> _activeCompletion;
        private bool _disposed;

        public PlayFabMatchmakingService(IPlayFabPlayerSession playerSession, IPlayFabMatchmakingAttributesProvider attributesProvider, PlayFabMatchmakingOptions options, PlayFabMultiplayerRuntime runtime)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _attributesProvider = attributesProvider ?? throw new ArgumentNullException(nameof(attributesProvider));
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
                return CreateFailureResult("A PlayFab matchmaking request is already active.");
            }

            if (_playerSession.PlayerEntity == null)
            {
                return CreateFailureResult("PlayFab matchmaking requires an authenticated player.");
            }

            try
            {
                var attributes = await _attributesProvider.CreateAttributesAsync(cancellationToken);
                StartMatchmakingRequest(attributes);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                ClearActiveRequest();
                return CreateFailureResult($"PlayFab failed to start matchmaking: {exception.Message}", exception);
            }

            LogTicketRequested();
            var completion = _activeCompletion;
            return await WaitForCompletionAsync(completion, cancellationToken);
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

        private MatchmakingTicket CreateTicket(string attributes)
        {
            var user = new MatchUser(_playerSession.PlayerEntity, attributes);
            return PlayFabMultiplayer.CreateMatchmakingTicket(user, _options.QueueName, _options.TimeoutInSeconds);
        }

        private void StartMatchmakingRequest(string attributes)
        {
            _runtime.EnsureInitialized();
            _runtime.SetMatchmakingProcessingActive(true);
            _activeCompletion = new TaskCompletionSource<MatchmakingResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _activeTicket = CreateTicket(attributes);
            if (_activeTicket == null)
            {
                throw new InvalidOperationException("The SDK returned no matchmaking ticket.");
            }
        }

        private void LogTicketRequested()
        {
            var entityKeyResult = _playerSession.PlayerEntity.GetEntityKey();
            var entityId = entityKeyResult.Failed() ? $"unavailable (HRESULT 0x{entityKeyResult.HResult:X8})" : entityKeyResult.Result.Id;
            Debug.Log($"[PlayFab][Matchmaking] EntityId '{entityId}' requested ticket '{_activeTicket.TicketId}' for queue '{_options.QueueName}'.");
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
                return CreateFailureResult($"PlayFab matchmaking failed (HRESULT 0x{result:X8}, status {ticket.Status}).");
            }

            if (ticket.Status != MatchmakingTicketStatus.Matched)
            {
                return CreateFailureResult($"PlayFab matchmaking completed with status {ticket.Status}.");
            }

            return CreateMatchedResult(ticket.GetMatchDetails());
        }

        private MatchmakingResult CreateMatchedResult(MatchmakingMatchDetails details)
        {
            if (details == null || string.IsNullOrWhiteSpace(details.MatchId))
            {
                return CreateFailureResult("PlayFab returned a match without a MatchId.");
            }

            if (details.ServerDetails == null)
            {
                return CreateFailureResult("PlayFab found a match but did not allocate a multiplayer server. Enable server allocation for the queue.");
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

        private async Task<MatchmakingResult> WaitForCompletionAsync(TaskCompletionSource<MatchmakingResult> completion, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(CancelActiveRequest))
            using (var timeoutCancellation = new CancellationTokenSource())
            {
                var timeout = Task.Delay(_options.CompletionWatchdogTimeout, timeoutCancellation.Token);
                if (await Task.WhenAny(completion.Task, timeout) == completion.Task)
                {
                    timeoutCancellation.Cancel();
                    return await completion.Task;
                }

                return CompleteTimedOutRequest(completion);
            }
        }

        private MatchmakingResult CompleteTimedOutRequest(TaskCompletionSource<MatchmakingResult> completion)
        {
            if (!ReferenceEquals(completion, _activeCompletion))
            {
                return completion.Task.GetAwaiter().GetResult();
            }

            var ticket = _activeTicket;
            var timeoutSeconds = _options.CompletionWatchdogTimeout.TotalSeconds;
            var result = CreateFailureResult($"PlayFab matchmaking timed out after {timeoutSeconds:0} seconds.");
            if (!completion.TrySetResult(result))
            {
                return completion.Task.GetAwaiter().GetResult();
            }

            ClearActiveRequest();
            ticket?.Cancel();
            Debug.LogWarning($"[PlayFab] Matchmaking completion watchdog expired after {timeoutSeconds:0} seconds; the active ticket was cancelled.");
            return result;
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
