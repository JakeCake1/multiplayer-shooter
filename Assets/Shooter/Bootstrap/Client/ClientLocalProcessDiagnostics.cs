using Shooter.Application;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientLocalProcessDiagnostics : LocalProcessDiagnosticsBase
    {
        private LocalProcessDebugOverlay _overlay;

        public ClientLocalProcessDiagnostics(INetworkSession networkSession, NetworkSessionStartRequest request) : base(networkSession, request)
        {
        }

        protected override void OnStarted(NetworkSessionState state)
        {
            if (!Debug.isDebugBuild)
            {
                return;
            }

            CreateOverlay(state);
        }

        private void CreateOverlay(NetworkSessionState state)
        {
            var overlayObject = new GameObject("Local Client Diagnostics");
            Object.DontDestroyOnLoad(overlayObject);
            _overlay = overlayObject.AddComponent<LocalProcessDebugOverlay>();
            overlayObject.AddComponent<MatchStateDebugOverlay>();
            overlayObject.AddComponent<WeaponShotDebugOverlay>();
            _overlay.SetText(BuildText(state));
        }

        protected override void OnStateChanged(NetworkSessionState state)
        {
            _overlay?.SetText(BuildText(state));
        }

        protected override void OnDisposed()
        {
            if (_overlay == null)
            {
                return;
            }

            Object.Destroy(_overlay.gameObject);
            _overlay = null;
        }

        protected override string GetRoleName()
        {
            return "Client";
        }

        protected override string GetPortDescription()
        {
            return "n/a (client joins by session)";
        }
    }
}
