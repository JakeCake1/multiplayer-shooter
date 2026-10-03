using System;
using Fusion;
using Fusion.Sockets;
using Shooter.Application;
using Shooter.Features.MatchRules;
using System.Threading.Tasks;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionServerNetworkSession : FusionNetworkSessionBase
    {
        private readonly FusionNetworkAssetLoader _assetLoader;
        private readonly MatchRules _matchRules;

        public FusionServerNetworkSession(FusionNetworkAssetLoader assetLoader, MatchRules matchRules)
        {
            _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _matchRules = matchRules ?? throw new ArgumentNullException(nameof(matchRules));
        }

        protected override NetworkSessionRole Role => NetworkSessionRole.Server;

        protected override async Task PrepareAsync()
        {
            await _assetLoader.LoadAsync();
        }

        protected override NetworkRunner CreateRunner()
        {
            var runner = CreateRunnerBase();
            runner.ProvideInput = false;
            var matchController = runner.gameObject.AddComponent<FusionServerMatchController>();
            matchController.Configure(_matchRules);
            var assets = _assetLoader.Assets;
            runner.gameObject.AddComponent<FusionServerPlayerSpawner>().Configure(assets.PlayerPrefab, assets.MatchStatePrefab, matchController);
            return runner;
        }

        protected override StartGameArgs CreateStartGameArgs(NetworkSessionStartRequest request, NetworkRunner runner)
        {
            return new StartGameArgs { GameMode = GameMode.Server, SessionName = request.SessionName, Address = NetAddress.Any(request.Port), PlayerCount = request.PlayerCount, EnableClientSessionCreation = false, SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), ObjectProvider = runner.GetComponent<NetworkObjectProviderDefault>() };
        }
    }
}
