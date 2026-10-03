using System.IO;
using Shooter.Bootstrap.Client;
using Shooter.Bootstrap.Server;
using Shooter.Infrastructure.Fusion;
using UnityEditor;
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

        [InitializeOnLoadMethod]
        private static void CreateMissingScenesAfterImport()
        {
            EditorApplication.delayCall += CreateBootstrapScenesIfMissing;
        }

        [MenuItem("Shooter/Local Development/Create Bootstrap Scenes")]
        public static void CreateBootstrapScenesIfMissing()
        {
            var playerPrefab = EnsurePlayerPrefab();
            EnsureScene<ClientLifetimeScope>(
                ClientScenePath,
                "Client Lifetime Scope",
                playerPrefab,
                includeClientPresentation: true);
            EnsureScene<ServerLifetimeScope>(
                ServerScenePath,
                "Server Lifetime Scope",
                playerPrefab,
                includeClientPresentation: false);
            AssetDatabase.SaveAssets();
        }

        private static GameObject EnsurePlayerPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(PlayerPrefabPath));
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";

            try
            {
                player.AddComponent<global::Fusion.NetworkObject>();
                player.AddComponent<global::Fusion.NetworkTransform>();
                player.AddComponent<FusionPlayerAvatar>();
                return PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        private static void EnsureScene<TLifetimeScope>(
            string path,
            string rootName,
            GameObject playerPrefab,
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

                var serializedScope = new SerializedObject(lifetimeScope);
                serializedScope.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
                serializedScope.ApplyModifiedPropertiesWithoutUndo();

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
