using Fusion;
using Fusion.Sockets;
using Shooter.Application;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionClientNetworkSession : FusionNetworkSessionBase
    {
        protected override NetworkSessionRole Role => NetworkSessionRole.Client;

        protected override NetworkRunner CreateRunner()
        {
            var runner = CreateRunnerBase();
            runner.ProvideInput = true;
            var events = runner.gameObject.AddComponent<NetworkEvents>();
            events.OnInput ??= new NetworkEvents.InputEvent();
            events.OnInput.AddListener(FusionClientInputCollector.Collect);
            return runner;
        }

        protected override StartGameArgs CreateStartGameArgs(NetworkSessionStartRequest request, NetworkRunner runner)
        {
            return new StartGameArgs { GameMode = GameMode.Client, SessionName = request.SessionName, Address = NetAddress.Any(), PlayerCount = request.PlayerCount, EnableClientSessionCreation = false, SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), ObjectProvider = runner.GetComponent<NetworkObjectProviderDefault>() };
        }
    }
}
