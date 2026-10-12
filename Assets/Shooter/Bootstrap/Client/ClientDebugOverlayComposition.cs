using UnityEngine;

namespace Shooter.Bootstrap.Client
{
    public static class ClientDebugOverlayComposition
    {
        public static LocalProcessDebugOverlay AttachTo(GameObject target)
        {
            var processOverlay = target.AddComponent<LocalProcessDebugOverlay>();
            AttachFeatureOverlays(target);
            return processOverlay;
        }

        private static void AttachFeatureOverlays(GameObject target)
        {
            target.AddComponent<MatchStateDebugOverlay>();
            target.AddComponent<WeaponShotDebugOverlay>();
            target.AddComponent<HealthDebugOverlay>();
            target.AddComponent<RespawnDebugOverlay>();
            target.AddComponent<ScoreDebugOverlay>();
            target.AddComponent<MatchResultDebugOverlay>();
        }
    }
}
