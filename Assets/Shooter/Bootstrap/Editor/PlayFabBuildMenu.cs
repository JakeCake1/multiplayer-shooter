using UnityEditor;
using UnityEditor.Build;

namespace Shooter.Bootstrap.Editor
{
    public static class PlayFabBuildMenu
    {
        [MenuItem("Shooter/PlayFab Deployment/Build Production Dedicated Server Package")]
        public static void BuildPackage()
        {
            PlayFabServerPackager.Clean();
            UnityPlayerBuildPipeline.PrepareBuildInputs();
            UnityPlayerBuildPipeline.Build(LocalBuildLayout.ServerScenePath, PlayFabBuildLayout.ServerBuildPath, StandaloneBuildSubtarget.Server, BuildOptions.CleanBuildCache, "PlayFabBuild");
            PlayFabServerPackager.Package();
        }

        [MenuItem("Shooter/PlayFab Deployment/Package Existing Dedicated Server")]
        public static void PackageExistingBuild()
        {
            PlayFabServerPackager.Package();
        }

        [MenuItem("Shooter/PlayFab Deployment/Clean Build and Package")]
        public static void CleanBuild()
        {
            if (!EditorUtility.DisplayDialog("Clean PlayFab build", "Delete the PlayFab dedicated-server build and ZIP package?", "Clean", "Cancel"))
            {
                return;
            }

            PlayFabServerPackager.Clean();
        }
    }
}
