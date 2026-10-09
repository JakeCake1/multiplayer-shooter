using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PlayFab;
using UnityEngine;

namespace Shooter.Infrastructure.PlayFab
{
    public sealed class PlayFabQosMatchmakingAttributesProvider : IPlayFabMatchmakingAttributesProvider, IDisposable
    {
        private const int QosPort = 3075;
        private const int PingCount = 3;
        private const int PingTimeoutMilliseconds = 250;
        private const int MaximumParallelRegions = 4;
        private readonly IPlayFabPlayerSession _playerSession;
        private readonly PlayFabClientOptions _clientOptions;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly SemaphoreSlim _parallelRegionGate = new SemaphoreSlim(MaximumParallelRegions, MaximumParallelRegions);
        private bool _disposed;

        public PlayFabQosMatchmakingAttributesProvider(IPlayFabPlayerSession playerSession, PlayFabClientOptions clientOptions)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            _clientOptions = clientOptions ?? throw new ArgumentNullException(nameof(clientOptions));
        }

        public async Task<string> CreateAttributesAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            var qosServers = await GetQosServersAsync(cancellationToken);
            var latencies = await MeasureLatenciesAsync(qosServers, cancellationToken);
            if (latencies.Length == 0)
            {
                throw new InvalidOperationException("PlayFab QoS measurement did not receive a response from any region.");
            }

            LogLatencies(latencies);
            return JsonUtility.ToJson(new PlayFabMatchmakingAttributes(latencies));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _httpClient.Dispose();
            _parallelRegionGate.Dispose();
        }

        private async Task<PlayFabQosServer[]> GetQosServersAsync(CancellationToken cancellationToken)
        {
            var playerEntity = _playerSession.PlayerEntity ?? throw new InvalidOperationException("PlayFab QoS requires an authenticated player.");
            var tokenResult = await playerEntity.GetEntityTokenAsync();
            if (tokenResult.Failed() || string.IsNullOrWhiteSpace(tokenResult.Result.Token))
            {
                throw new InvalidOperationException($"PlayFab failed to provide the player entity token (HRESULT 0x{tokenResult.HResult:X8}).");
            }

            var responseJson = await SendQosServerRequestAsync(tokenResult.Result.Token, cancellationToken);
            var response = JsonUtility.FromJson<PlayFabQosApiResponse>(responseJson);
            var qosServers = response?.data?.QosServers ?? Array.Empty<PlayFabQosServer>();
            return qosServers.Where(IsValidServer).ToArray();
        }

        private async Task<string> SendQosServerRequestAsync(string entityToken, CancellationToken cancellationToken)
        {
            var endpoint = $"{_clientOptions.ApiEndpoint}/MultiplayerServer/ListQosServersForTitle";
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.TryAddWithoutValidation("X-EntityToken", entityToken);
                request.Content = new StringContent("{\"IncludeAllRegions\":false}", Encoding.UTF8, "application/json");
                using (var response = await _httpClient.SendAsync(request, cancellationToken))
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"PlayFab failed to list QoS servers (HTTP {(int)response.StatusCode}): {responseJson}");
                    }

                    return responseJson;
                }
            }
        }

        private async Task<PlayFabQosLatency[]> MeasureLatenciesAsync(PlayFabQosServer[] qosServers, CancellationToken cancellationToken)
        {
            if (qosServers.Length == 0)
            {
                throw new InvalidOperationException("PlayFab returned no QoS regions. Deploy an active Multiplayer Servers build in at least one region.");
            }

            var measurementTasks = qosServers.Select(server => MeasureRegionWithThrottleAsync(server, cancellationToken));
            var measurements = await Task.WhenAll(measurementTasks);
            return measurements.Where(measurement => measurement != null).OrderBy(measurement => measurement.latency).ToArray();
        }

        private async Task<PlayFabQosLatency> MeasureRegionWithThrottleAsync(PlayFabQosServer server, CancellationToken cancellationToken)
        {
            await _parallelRegionGate.WaitAsync(cancellationToken);
            try
            {
                return await MeasureRegionAsync(server, cancellationToken);
            }
            finally
            {
                _parallelRegionGate.Release();
            }
        }

        private static async Task<PlayFabQosLatency> MeasureRegionAsync(PlayFabQosServer server, CancellationToken cancellationToken)
        {
            var successfulLatencies = new List<int>(PingCount);
            var host = GetHost(server.ServerUrl);
            for (var index = 0; index < PingCount; index++)
            {
                var latency = await MeasurePingAsync(host, cancellationToken);
                if (latency.HasValue)
                {
                    successfulLatencies.Add(latency.Value);
                }
            }

            return successfulLatencies.Count == 0 ? null : new PlayFabQosLatency(server.Region, (int)Math.Round(successfulLatencies.Average()));
        }

        private static async Task<int?> MeasurePingAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = new UdpClient())
                {
                    client.Connect(host, QosPort);
                    var payload = CreatePingPayload();
                    var stopwatch = Stopwatch.StartNew();
                    await client.SendAsync(payload, payload.Length);
                    var response = await ReceiveWithTimeoutAsync(client, cancellationToken);
                    stopwatch.Stop();
                    return IsValidResponse(payload, response) ? (int?)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue) : null;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning($"[PlayFab][QoS] Ping to '{host}' failed: {exception.Message}");
                return null;
            }
        }

        private static async Task<byte[]> ReceiveWithTimeoutAsync(UdpClient client, CancellationToken cancellationToken)
        {
            var receiveTask = client.ReceiveAsync();
            var timeoutTask = Task.Delay(PingTimeoutMilliseconds, cancellationToken);
            if (await Task.WhenAny(receiveTask, timeoutTask) == receiveTask)
            {
                return (await receiveTask).Buffer;
            }

            client.Close();
            await ObserveReceiveCancellationAsync(receiveTask);
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }

        private static async Task ObserveReceiveCancellationAsync(Task<UdpReceiveResult> receiveTask)
        {
            try
            {
                await receiveTask;
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
        }

        private static byte[] CreatePingPayload()
        {
            var payload = Guid.NewGuid().ToByteArray();
            payload[0] = byte.MaxValue;
            payload[1] = byte.MaxValue;
            return payload;
        }

        private static bool IsValidResponse(byte[] request, byte[] response)
        {
            if (response == null || response.Length != request.Length || response[0] != 0 || response[1] != 0)
            {
                return false;
            }

            for (var index = 2; index < request.Length; index++)
            {
                if (response[index] != request[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidServer(PlayFabQosServer server)
        {
            return !string.IsNullOrWhiteSpace(server.Region) && !string.IsNullOrWhiteSpace(server.ServerUrl);
        }

        private static string GetHost(string serverUrl)
        {
            if (Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri))
            {
                return uri.Host;
            }

            return serverUrl;
        }

        private static void LogLatencies(IEnumerable<PlayFabQosLatency> latencies)
        {
            var summary = string.Join(", ", latencies.Select(measurement => $"{measurement.region}={measurement.latency}ms"));
            UnityEngine.Debug.Log($"[PlayFab][QoS] Matchmaking latencies: {summary}.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlayFabQosMatchmakingAttributesProvider));
            }
        }
    }
}
