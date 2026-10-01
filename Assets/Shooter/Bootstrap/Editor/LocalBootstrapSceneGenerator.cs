using System.IO;
using Shooter.Bootstrap.Client;
using Shooter.Bootstrap.Server;
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

        [InitializeOnLoadMethod]
        private static void CreateMissingScenesAfterImport()
        {
            EditorApplication.delayCall += CreateBootstrapScenesIfMissing;
        }

        [MenuItem("Shooter/Local Development/Create Bootstrap Scenes")]
        public static void CreateBootstrapScenesIfMissing()
        {
            EnsureScene<ClientLifetimeScope>(ClientScenePath, "Client Lifetime Scope");
            EnsureScene<ServerLifetimeScope>(ServerScenePath, "Server Lifetime Scope");
            AssetDatabase.SaveAssets();
        }

        private static void EnsureScene<TLifetimeScope>(string path, string rootName)
            where TLifetimeScope : Component
        {
            if (File.Exists(path))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            try
            {
                var root = new GameObject(rootName);
                SceneManager.MoveGameObjectToScene(root, scene);
                root.AddComponent<TLifetimeScope>();

                if (!EditorSceneManager.SaveScene(scene, path))
                {
                    throw new IOException($"Unity could not save bootstrap scene '{path}'.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
