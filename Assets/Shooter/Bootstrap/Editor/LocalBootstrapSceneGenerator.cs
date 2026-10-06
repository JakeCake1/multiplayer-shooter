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

            EnsurePrefabComponent<FusionMatchStateObserver>(MatchStatePrefabPath);
        }

        private static void CreateMatchStatePrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MatchStatePrefabPath));
            var matchState = new GameObject("Match State");

            try
            {
                matchState.AddComponent<global::Fusion.NetworkObject>();
                matchState.AddComponent<FusionMatchState>();
                matchState.AddComponent<FusionMatchStateObserver>();
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

            EnsurePrefabComponent<FusionServerWeapon>(PlayerPrefabPath);
            EnsurePrefabComponent<FusionPlayerHealthState>(PlayerPrefabPath);
        }

        private static void CreatePlayerPrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PlayerPrefabPath));
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";

            try
            {
                player.AddComponent<global::Fusion.NetworkObject>();
                player.AddComponent<global::Fusion.NetworkTransform>();
                player.AddComponent<FusionPlayerAvatar>();
                player.AddComponent<FusionServerWeapon>();
                player.AddComponent<FusionPlayerHealthState>();
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        private static void EnsurePrefabComponent<TComponent>(string prefabPath)
            where TComponent : Component
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                if (prefabRoot.GetComponent<TComponent>() == null)
                {
                    prefabRoot.AddComponent<TComponent>();
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
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

        private static void EnsureScene<TLifetimeScope>(
            string path,
            string rootName,
            bool includeClientPresentation)
            where TLifetimeScope : Component
        {
            var loadedScene = SceneManager.GetSceneByPath(path);
            var wasLoaded = loadedScene.IsValid() && loadedScene.isLoaded;
            var scene = wasLoaded
                ? loadedScene
                : File.Exists(path)
                    ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive)
                    : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

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
                    EnsureClientPresentation(scene);
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

        private static void EnsureClientPresentation(Scene scene)
        {
            if (FindRoot(scene, "Main Camera") == null)
            {
                var cameraObject = new GameObject("Main Camera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                cameraObject.tag = "MainCamera";
                cameraObject.transform.SetPositionAndRotation(
                    new Vector3(0f, 8f, -10f),
                    Quaternion.Euler(30f, 0f, 0f));
                cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            if (FindRoot(scene, "Directional Light") == null)
            {
                var lightObject = new GameObject("Directional Light");
                SceneManager.MoveGameObjectToScene(lightObject, scene);
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            if (FindRoot(scene, "Ground") == null)
            {
                var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.name = "Ground";
                SceneManager.MoveGameObjectToScene(ground, scene);
                ground.transform.position = new Vector3(0f, -0.1f, 0f);
                ground.transform.localScale = new Vector3(12f, 0.2f, 12f);
            }
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            return null;
        }

        private static TComponent FindComponentInScene<TComponent>(Scene scene)
            where TComponent : Component
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
