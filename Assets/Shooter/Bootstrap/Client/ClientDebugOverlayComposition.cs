using Shooter.Application;
using UnityEngine;

namespace Shooter.Bootstrap.Client
{
    public static class ClientDebugOverlayComposition
    {
        public static LocalProcessDebugOverlay AttachTo(GameObject target, MatchConfiguration configuration)
        {
            var processOverlay = target.AddComponent<LocalProcessDebugOverlay>();
            AttachFeatureOverlays(target, configuration);
            return processOverlay;
        }

        private static void AttachFeatureOverlays(GameObject target, MatchConfiguration configuration)
        {
            target.AddComponent<MatchStateDebugOverlay>();
            target.AddComponent<WeaponShotDebugOverlay>();
            target.AddComponent<MatchResultDebugOverlay>();
            target.AddComponent<ServerShutdownDebugOverlay>().Configure(configuration.Match.ServerShutdownGraceSeconds);
        }
    }
}
