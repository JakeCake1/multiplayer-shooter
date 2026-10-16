using UnityEditor;
using UnityEditor.Build;

namespace Shooter.Bootstrap.Editor
{
    public static class LocalBuildMenu
    {
        [MenuItem("Shooter/Local Development/Build Development Client")]
        public static void BuildClient()
        {
            UnityPlayerBuildPipeline.PrepareBuildInputs();
            Build(LocalBuildLayout.ClientScenePath, LocalBuildLayout.ClientBuildPath, StandaloneBuildSubtarget.Player, cleanBuildCache: false);
        }

        [MenuItem("Shooter/Local Development/Build Development Dedicated Server")]
        public static void BuildServer()
        {
            UnityPlayerBuildPipeline.PrepareBuildInputs();
            Build(LocalBuildLayout.ServerScenePath, LocalBuildLayout.ServerBuildPath, StandaloneBuildSubtarget.Server, cleanBuildCache: false);
        }

        [MenuItem("Shooter/Local Development/Build Development Client and Dedicated Server")]
        public static void BuildAll()
        {
            BuildAll(cleanBuildCache: false);
        }

        [MenuItem("Shooter/Local Development/Build Development Client and Dedicated Server (Clean)")]
        public static void BuildAllClean()
        {
            LocalBuildCleaner.Clean();
            BuildAll(cleanBuildCache: true);
        }

        [MenuItem("Shooter/Local Development/Clean Builds")]
        public static void CleanBuilds()
        {
            if (!EditorUtility.DisplayDialog("Clean local builds", "Delete local client and dedicated-server build outputs? Local logs will be preserved.", "Clean", "Cancel"))
            {
                return;
            }

            LocalBuildCleaner.Clean();
        }

        private static void BuildAll(bool cleanBuildCache)
        {
            UnityPlayerBuildPipeline.PrepareBuildInputs();
            Build(LocalBuildLayout.ClientScenePath, LocalBuildLayout.ClientBuildPath, StandaloneBuildSubtarget.Player, cleanBuildCache);
            Build(LocalBuildLayout.ServerScenePath, LocalBuildLayout.ServerBuildPath, StandaloneBuildSubtarget.Server, cleanBuildCache);
        }

        private static void Build(string scenePath, string outputPath, StandaloneBuildSubtarget subtarget, bool cleanBuildCache)
        {
            var options = BuildOptions.Development | BuildOptions.AllowDebugging;
            if (cleanBuildCache)
            {
                options |= BuildOptions.CleanBuildCache;
            }

            UnityPlayerBuildPipeline.Build(scenePath, outputPath, subtarget, options, "LocalBuild");
        }
    }
}
