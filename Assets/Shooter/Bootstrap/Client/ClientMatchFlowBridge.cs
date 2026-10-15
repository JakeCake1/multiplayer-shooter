using System;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientMatchFlowBridge : ITickable
    {
        private const float SnapshotPollIntervalSeconds = 0.1f;
        private readonly IClientFlowController _controller;
        private readonly FusionClientMatchSnapshotSource _snapshotSource;
        private ClientMatchSnapshot _lastSnapshot;
        private float _nextSnapshotTime;

        public ClientMatchFlowBridge(IClientFlowController controller, FusionClientMatchSnapshotSource snapshotSource)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _snapshotSource = snapshotSource ?? throw new ArgumentNullException(nameof(snapshotSource));
        }

        public void Tick()
        {
            if (Time.unscaledTime < _nextSnapshotTime)
            {
                return;
            }

            _nextSnapshotTime = Time.unscaledTime + SnapshotPollIntervalSeconds;
            if (!_snapshotSource.TryCreate(out var snapshot))
            {
                return;
            }

            if (HasChanged(snapshot))
            {
                _lastSnapshot = snapshot;
                _controller.ObserveMatch(snapshot);
            }
        }

        private bool HasChanged(ClientMatchSnapshot snapshot)
        {
            return _lastSnapshot == null || !_lastSnapshot.HasSameObservableState(snapshot);
        }
    }
}
