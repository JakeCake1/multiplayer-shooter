using System.IO;
using Shooter.Bootstrap.Client;
using Shooter.Bootstrap.Server;
using Shooter.Infrastructure.Fusion;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shooter.Bootstrap.Editor
{
    public static class LocalBootstrapSceneGenerator
    {
        private const string ClientScenePath = "Assets/Scenes/LocalClient.unity";
        private const string ServerScenePath = "Assets/Scenes/LocalServer.unity";
        private const string PlayerPrefabPath = "Assets/Shooter/Infrastructure/Fusion/Prefabs/Player.prefab";
        private const string MatchStatePrefabPath = "Assets/Shooter/Infrastructure/Fusion/Prefabs/MatchState.prefab";

        [InitializeOnLoadMethod]
        private static void CreateMissingScenesAfterImport()
        {
            EditorApplication.delayCall += CreateBootstrapScenesIfMissing;
        }

        [MenuItem("Shooter/Local Development/Create Bootstrap Scenes")]
        public static void CreateBootstrapScenesIfMissing()
        {
            EnsurePlayerPrefab();
            EnsureMatchStatePrefab();
            EnsureAddressableNetworkAssets();
            EnsureScene<ClientLifetimeScope>(ClientScenePath, "Client Lifetime Scope", includeClientPresentation: true);
            EnsureScene<ServerLifetimeScope>(ServerScenePath, "Server Lifetime Scope", includeClientPresentation: false);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureMatchStatePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(MatchStatePrefabPath);
            if (existing == null)
            {
                CreateMatchStatePrefab();
            }

            LocalNetworkPrefabComposition.EnsureMatchStateComponents(MatchStatePrefabPath);
        }

        private static void CreateMatchStatePrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MatchStatePrefabPath));
            var matchState = new GameObject("Match State");

            try
            {
                LocalNetworkPrefabComposition.AddMatchStateComponents(matchState);
                PrefabUtility.SaveAsPrefabAsset(matchState, MatchStatePrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(matchState);
            }
        }

        private static void EnsurePlayerPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existing == null)
            {
                CreatePlayerPrefab();
            }

            LocalNetworkPrefabComposition.EnsurePlayerComponents(PlayerPrefabPath);
        }

        private static void CreatePlayerPrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PlayerPrefabPath));
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";

            try
            {
                LocalNetworkPrefabComposition.AddPlayerComponents(player);
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        private static void EnsureAddressableNetworkAssets()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(create: true);
            if (settings.BuildAddressablesWithPlayerBuild != AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer)
            {
                settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
            }

            MarkAddressable(settings, PlayerPrefabPath, FusionNetworkAssetAddresses.PlayerPrefab);
            MarkAddressable(settings, MatchStatePrefabPath, FusionNetworkAssetAddresses.MatchStatePrefab);
            EditorUtility.SetDirty(settings);
        }

        private static void MarkAddressable(AddressableAssetSettings settings, string assetPath, string address)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            if (entry.address != address)
            {
                entry.SetAddress(address);
            }
        }

        private static void EnsureBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (HasExpectedBuildScenes(scenes))
            {
                return;
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ClientScenePath, enabled: true), new EditorBuildSettingsScene(ServerScenePath, enabled: true) };
        }

        private static bool HasExpectedBuildScenes(EditorBuildSettingsScene[] scenes)
        {
            return scenes.Length == 2 && scenes[0].enabled && scenes[0].path == ClientScenePath && scenes[1].enabled && scenes[1].path == ServerScenePath;
        }

        private static void EnsureScene<TLifetimeScope>(string path, string rootName, bool includeClientPresentation) where TLifetimeScope : Component
        {
            var loadedScene = SceneManager.GetSceneByPath(path);
            var wasLoaded = loadedScene.IsValid() && loadedScene.isLoaded;
            var scene = wasLoaded ? loadedScene : File.Exists(path) ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            try
            {
                var lifetimeScope = FindComponentInScene<TLifetimeScope>(scene);
                if (lifetimeScope == null)
                {
                    var root = new GameObject(rootName);
                    SceneManager.MoveGameObjectToScene(root, scene);
                    lifetimeScope = root.AddComponent<TLifetimeScope>();
                }

                if (includeClientPresentation)
                {
                    LocalClientSceneComposition.Ensure(scene);
                }

                if (!EditorSceneManager.SaveScene(scene, path))
                {
                    throw new IOException($"Unity could not save bootstrap scene '{path}'.");
                }
            }
            finally
            {
                if (!wasLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static TComponent FindComponentInScene<TComponent>(Scene scene) where TComponent : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<TComponent>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}
