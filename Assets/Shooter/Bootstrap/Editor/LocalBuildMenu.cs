using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Shooter.Bootstrap.Editor
{
    public static class LocalBuildMenu
    {
        [MenuItem("Shooter/Local Development/Build Development Client")]
        public static void BuildClient()
        {
            Build(LocalBuildLayout.ClientScenePath, LocalBuildLayout.ClientBuildPath, StandaloneBuildSubtarget.Player);
        }

        [MenuItem("Shooter/Local Development/Build Development Dedicated Server")]
        public static void BuildServer()
        {
            Build(LocalBuildLayout.ServerScenePath, LocalBuildLayout.ServerBuildPath, StandaloneBuildSubtarget.Server);
        }

        [MenuItem("Shooter/Local Development/Build Development Client and Dedicated Server")]
        public static void BuildAll()
        {
            BuildClient();
            BuildServer();
        }

        [MenuItem("Shooter/Local Development/Build Development Client and Dedicated Server (Clean)")]
        public static void BuildAllClean()
        {
            LocalBuildCleaner.Clean();
            BuildAll();
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

        private static void Build(string scenePath, string outputPath, StandaloneBuildSubtarget subtarget)
        {
            LocalBootstrapSceneGenerator.CreateBootstrapScenesIfMissing();

            if (!File.Exists(scenePath))
            {
                throw new BuildFailedException($"Required scene does not exist: {scenePath}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var buildOptions = new BuildPlayerOptions { scenes = new[] { scenePath }, locationPathName = outputPath, target = BuildTarget.StandaloneWindows64, subtarget = (int)subtarget, options = BuildOptions.Development | BuildOptions.AllowDebugging };
            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{subtarget} build failed with result {report.summary.result}.");
            }
        }
    }
}
