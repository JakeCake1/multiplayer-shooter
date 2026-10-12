using System;
using System.Diagnostics;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;
using Debug = UnityEngine.Debug;

namespace Shooter.Bootstrap
{
    public abstract class LocalProcessDiagnosticsBase : IStartable, IDisposable
    {
        private readonly INetworkSession _networkSession;
        private int _processId;

        protected LocalProcessDiagnosticsBase(INetworkSession networkSession, NetworkSessionStartRequest request)
        {
            _networkSession = networkSession ?? throw new ArgumentNullException(nameof(networkSession));
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        protected NetworkSessionStartRequest Request { get; }

        public void Start()
        {
            _processId = GetCurrentProcessId();
            _networkSession.StateChanged += HandleNetworkStateChanged;
            Publish(_networkSession.State);
            OnStarted(_networkSession.State);
        }

        public void Dispose()
        {
            _networkSession.StateChanged -= HandleNetworkStateChanged;
            OnDisposed();
        }

        protected virtual void OnStarted(NetworkSessionState state)
        {
        }

        protected virtual void OnStateChanged(NetworkSessionState state)
        {
        }

        protected virtual void OnDisposed()
        {
        }

        protected abstract string GetRoleName();

        protected abstract string GetPortDescription();

        protected string BuildText(NetworkSessionState state)
        {
            return $"Role: {GetRoleName()}    PID: {_processId}\nSession: {Request.SessionName}    Port: {GetPortDescription()}\nNetwork: {state}    Development: {Debug.isDebugBuild}";
        }

        private void HandleNetworkStateChanged(NetworkSessionState state)
        {
            Publish(state);
            OnStateChanged(state);
        }

        private void Publish(NetworkSessionState state)
        {
            Debug.Log($"[LocalProcess] {BuildText(state).Replace('\n', ' ')}");
        }

        private static int GetCurrentProcessId()
        {
            using var process = Process.GetCurrentProcess();
            return process.Id;
        }
    }
}
