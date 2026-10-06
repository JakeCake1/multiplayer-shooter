using System;
using Shooter.Infrastructure.Fusion;
using UnityEditor;
using UnityEngine;

namespace Shooter.Bootstrap.Editor
{
    public static class LocalNetworkPrefabComposition
    {
        private static readonly Type[] MatchStateComponentTypes =
        {
            typeof(global::Fusion.NetworkObject),
            typeof(FusionMatchState),
            typeof(FusionMatchStateObserver),
            typeof(FusionMatchResultState)
        };

        private static readonly Type[] PlayerComponentTypes =
        {
            typeof(global::Fusion.NetworkObject),
            typeof(global::Fusion.NetworkTransform),
            typeof(FusionPlayerAvatar),
            typeof(FusionServerWeapon),
            typeof(FusionPlayerHealthState),
            typeof(FusionServerPlayerRespawn),
            typeof(FusionPlayerScoreState)
        };

        public static void EnsureMatchStateComponents(string prefabPath)
        {
            EnsurePrefabComponents(prefabPath, MatchStateComponentTypes);
        }

        public static void EnsurePlayerComponents(string prefabPath)
        {
            EnsurePrefabComponents(prefabPath, PlayerComponentTypes);
        }

        public static void AddMatchStateComponents(GameObject target)
        {
            AddMissingComponents(target, MatchStateComponentTypes);
        }

        public static void AddPlayerComponents(GameObject target)
        {
            AddMissingComponents(target, PlayerComponentTypes);
        }

        private static void EnsurePrefabComponents(string prefabPath, Type[] componentTypes)
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                AddMissingComponents(prefabRoot, componentTypes);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void AddMissingComponents(GameObject target, Type[] componentTypes)
        {
            foreach (var componentType in componentTypes)
            {
                if (target.GetComponent(componentType) == null)
                {
                    target.AddComponent(componentType);
                }
            }
        }
    }
}
