using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shooter.Bootstrap.Editor
{
    public static class LocalClientSceneComposition
    {
        public static void Ensure(Scene scene)
        {
            EnsureCamera(scene);
            EnsureDirectionalLight(scene);
            EnsureGround(scene);
        }

        private static void EnsureCamera(Scene scene)
        {
            if (FindRoot(scene, "Main Camera") != null)
            {
                return;
            }

            var cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 8f, -10f), Quaternion.Euler(30f, 0f, 0f));
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        private static void EnsureDirectionalLight(Scene scene)
        {
            if (FindRoot(scene, "Directional Light") != null)
            {
                return;
            }

            var lightObject = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
        }

        private static void EnsureGround(Scene scene)
        {
            if (FindRoot(scene, "Ground") != null)
            {
                return;
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            SceneManager.MoveGameObjectToScene(ground, scene);
            ground.transform.position = new Vector3(0f, -0.1f, 0f);
            ground.transform.localScale = new Vector3(12f, 0.2f, 12f);
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
    }
}
