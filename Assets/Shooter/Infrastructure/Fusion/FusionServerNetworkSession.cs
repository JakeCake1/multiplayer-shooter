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
        private readonly FusionArenaLoader _arenaLoader;
        private readonly MatchRules _matchRules;
        private readonly IServerMatchLifecycle _matchLifecycle;
        private readonly ServerProcessShutdownPolicy _shutdownPolicy;

        public FusionServerNetworkSession(FusionNetworkAssetLoader assetLoader, FusionArenaLoader arenaLoader, MatchRules matchRules, IServerMatchLifecycle matchLifecycle, ServerProcessShutdownPolicy shutdownPolicy)
        {
            _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _arenaLoader = arenaLoader ?? throw new ArgumentNullException(nameof(arenaLoader));
            _matchRules = matchRules ?? throw new ArgumentNullException(nameof(matchRules));
            _matchLifecycle = matchLifecycle ?? throw new ArgumentNullException(nameof(matchLifecycle));
            _shutdownPolicy = shutdownPolicy ?? throw new ArgumentNullException(nameof(shutdownPolicy));
        }

        protected override NetworkSessionRole Role => NetworkSessionRole.Server;

        protected override async Task PrepareAsync()
        {
            await Task.WhenAll(_assetLoader.LoadAsync(), _arenaLoader.LoadAsync());
        }

        protected override NetworkRunner CreateRunner()
        {
            var runner = CreateRunnerBase();
            runner.ProvideInput = false;
            var matchController = runner.gameObject.AddComponent<FusionServerMatchController>();
            matchController.Configure(_matchRules, _matchLifecycle, reuseFinishedMatches: !_shutdownPolicy.ShutdownAfterMatchFinished);
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
