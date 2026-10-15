using System;
using System.Threading.Tasks;
using Shooter.Application;

namespace Shooter.Application.Tests
{
    public sealed class FakeNetworkSession : INetworkSession
    {
        private TaskCompletionSource<bool> _stopCompletion;

        public event Action<NetworkSessionState> StateChanged;

        public NetworkSessionState State { get; private set; } = NetworkSessionState.Disconnected;

        public NetworkSessionStartResult StartResult { get; set; } = NetworkSessionStartResult.Success();

        public int StartCount { get; private set; }

        public int StopCount { get; private set; }

        public bool WaitForStopCompletion { get; set; }

        public Exception StopException { get; set; }

        public NetworkSessionStartRequest LastRequest { get; private set; }

        public Task<NetworkSessionStartResult> StartAsync(NetworkSessionStartRequest request)
        {
            StartCount++;
            LastRequest = request;
            SetState(StartResult.Succeeded ? NetworkSessionState.Connected : NetworkSessionState.Disconnected);
            return Task.FromResult(StartResult);
        }

        public Task StopAsync()
        {
            StopCount++;
            if (StopException != null)
            {
                throw StopException;
            }

            if (WaitForStopCompletion)
            {
                _stopCompletion = new TaskCompletionSource<bool>();
                return _stopCompletion.Task;
            }

            SetState(NetworkSessionState.Disconnected);
            return Task.CompletedTask;
        }

        public void CompleteStop()
        {
            SetState(NetworkSessionState.Disconnected);
            _stopCompletion?.TrySetResult(true);
        }

        private void SetState(NetworkSessionState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
