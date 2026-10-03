using Shooter.Application;

namespace Shooter.Bootstrap
{
    public sealed class ServerLocalProcessDiagnostics : LocalProcessDiagnosticsBase
    {
        public ServerLocalProcessDiagnostics(INetworkSession networkSession, NetworkSessionStartRequest request) : base(networkSession, request)
        {
        }

        protected override string GetRoleName()
        {
            return "Server";
        }

        protected override string GetPortDescription()
        {
            return Request.Port.ToString();
        }
    }
}
