using System;
using Fusion;
using Fusion.Sockets;
using Shooter.Application;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionServerNetworkSession : FusionNetworkSessionBase
    {
        private readonly FusionNetworkSessionOptions _options;

        public FusionServerNetworkSession(FusionNetworkSessionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override NetworkSessionRole Role => NetworkSessionRole.Server;

        protected override NetworkRunner CreateRunner()
        {
            var runner = CreateRunnerBase();
            runner.ProvideInput = false;
            runner.gameObject.AddComponent<FusionServerPlayerSpawner>().Configure(_options.PlayerPrefab, _options.MatchStatePrefab, _options.MatchRules);
            return runner;
        }

        protected override StartGameArgs CreateStartGameArgs(NetworkSessionStartRequest request, NetworkRunner runner)
        {
            return new StartGameArgs { GameMode = GameMode.Server, SessionName = request.SessionName, Address = NetAddress.Any(request.Port), PlayerCount = request.PlayerCount, EnableClientSessionCreation = false, SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(), ObjectProvider = runner.GetComponent<NetworkObjectProviderDefault>() };
        }
    }
}
