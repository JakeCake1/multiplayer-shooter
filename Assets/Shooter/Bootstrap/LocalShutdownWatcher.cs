using System;
using System.IO;
using System.Threading.Tasks;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;
using Debug = UnityEngine.Debug;

namespace Shooter.Bootstrap
{
    public sealed class LocalShutdownWatcher : ITickable
    {
        private const float PollIntervalSeconds = 0.25f;
        private readonly INetworkSession _networkSession;
        private readonly LocalShutdownSignal _signal;
        private float _nextPollTime;
        private bool _shutdownStarted;

        public LocalShutdownWatcher(INetworkSession networkSession, LocalShutdownSignal signal)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            _signal = signal ?? throw new ArgumentNullException(nameof(signal));
        }

        public void Tick()
        {
            if (!ShouldStartShutdown())
            {
                return;
            }

            _shutdownStarted = true;
            Debug.Log($"[LocalProcess] Graceful shutdown requested through '{_signal.Path}'.");
            _ = ShutdownAsync();
        }

        private bool ShouldStartShutdown()
        {
            if (_shutdownStarted || Time.unscaledTime < _nextPollTime)
            {
                return false;
            }

            _nextPollTime = Time.unscaledTime + PollIntervalSeconds;
            return File.Exists(_signal.Path);
        }

        private async Task ShutdownAsync()
        {
            try
            {
                await _networkSession.StopAsync();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                UnityEngine.Application.Quit();
            }
        }
    }
}
