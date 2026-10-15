using Fusion;
using Fusion.Sockets;
using Shooter.Application;
using System;
using System.Threading.Tasks;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionClientNetworkSession : FusionNetworkSessionBase
    {
        private readonly FusionNetworkAssetLoader _assetLoader;
        private readonly FusionArenaLoader _arenaLoader;
        private readonly IClientInputSettings _inputSettings;

        public FusionClientNetworkSession(FusionNetworkAssetLoader assetLoader, FusionArenaLoader arenaLoader, IClientInputSettings inputSettings)
        {
            _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _arenaLoader = arenaLoader ?? throw new ArgumentNullException(nameof(arenaLoader));
            _inputSettings = inputSettings ?? throw new ArgumentNullException(nameof(inputSettings));
        }

        protected override NetworkSessionRole Role => NetworkSessionRole.Client;

        protected override async Task PrepareAsync()
        {
            await Task.WhenAll(_assetLoader.LoadAsync(), _arenaLoader.LoadAsync());
        }

        protected override NetworkRunner CreateRunner()
        {
            var runner = CreateRunnerBase();
            runner.ProvideInput = true;
            var events = runner.gameObject.AddComponent<NetworkEvents>();
            events.OnInput ??= new NetworkEvents.InputEvent();
            var lookController = runner.gameObject.AddComponent<FusionFirstPersonLookController>();
            lookController.Configure(runner, _inputSettings);
            var inputCollector = new FusionClientInputCollector(lookController);
            events.OnInput.AddListener(inputCollector.Collect);
            var firstPersonCamera = runner.gameObject.AddComponent<FusionFirstPersonCamera>();
            firstPersonCamera.Configure(runner, lookController);
            return runner;
        }

        protected override StartGameArgs CreateStartGameArgs(NetworkSessionStartRequest request, NetworkRunner runner)
        {
            return new StartGameArgs { GameMode = GameMode.Client, SessionName = request.SessionName, Address = NetAddress.Any(), PlayerCount = request.PlayerCount, EnableClientSessionCreation = false, SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), ObjectProvider = runner.GetComponent<NetworkObjectProviderDefault>() };
        }
    }
}
