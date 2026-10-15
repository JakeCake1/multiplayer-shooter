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
            typeof(CharacterController),
            typeof(global::Fusion.NetworkCharacterController),
            typeof(global::Fusion.HitboxRoot),
            typeof(global::Fusion.Hitbox),
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
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var changed = RemoveLegacyPlayerComponents(prefabRoot);
                changed |= AddMissingComponents(prefabRoot, PlayerComponentTypes);
                changed |= EnsurePlayerHitboxConfiguration(prefabRoot);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        public static void AddMatchStateComponents(GameObject target)
        {
            AddMissingComponents(target, MatchStateComponentTypes);
        }

        public static void AddPlayerComponents(GameObject target)
        {
            RemoveLegacyPlayerComponents(target);
            AddMissingComponents(target, PlayerComponentTypes);
            EnsurePlayerHitboxConfiguration(target);
        }

        private static void EnsurePrefabComponents(string prefabPath, Type[] componentTypes)
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (AddMissingComponents(prefabRoot, componentTypes))
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool AddMissingComponents(GameObject target, Type[] componentTypes)
        {
            var changed = false;
            foreach (var componentType in componentTypes)
            {
                if (target.GetComponent(componentType) == null)
                {
                    target.AddComponent(componentType);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool RemoveLegacyPlayerComponents(GameObject target)
        {
            var changed = RemoveComponent<global::Fusion.NetworkTransform>(target);
            changed |= RemoveComponent<CapsuleCollider>(target);
            return changed;
        }

        private static bool EnsurePlayerHitboxConfiguration(GameObject target)
        {
            var hitboxRoot = target.GetComponent<global::Fusion.HitboxRoot>();
            var hitbox = target.GetComponent<global::Fusion.Hitbox>();
            var changed = hitboxRoot.BroadRadius != 1.1f || hitboxRoot.Offset != Vector3.zero;
            changed |= hitbox.Type != global::Fusion.HitboxTypes.Capsule || hitbox.CapsuleRadius != 0.5f || hitbox.CapsuleExtents != 2f || hitbox.Offset != Vector3.zero;
            hitboxRoot.BroadRadius = 1.1f;
            hitboxRoot.Offset = Vector3.zero;
            hitbox.Type = global::Fusion.HitboxTypes.Capsule;
            hitbox.CapsuleRadius = 0.5f;
            hitbox.CapsuleExtents = 2f;
            hitbox.Offset = Vector3.zero;
            return changed;
        }

        private static bool RemoveComponent<TComponent>(GameObject target) where TComponent : Component
        {
            var component = target.GetComponent<TComponent>();
            if (component == null)
            {
                return false;
            }

            UnityEngine.Object.DestroyImmediate(component);
            return true;
        }
    }
}
