using System.IO;
using UnityEngine;
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
            PrepareBuildInputs();
            Build(LocalBuildLayout.ClientScenePath, LocalBuildLayout.ClientBuildPath, StandaloneBuildSubtarget.Player, cleanBuildCache: false);
        }

        [MenuItem("Shooter/Local Development/Build Development Dedicated Server")]
        public static void BuildServer()
        {
            PrepareBuildInputs();
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
            PrepareBuildInputs();
            Build(LocalBuildLayout.ClientScenePath, LocalBuildLayout.ClientBuildPath, StandaloneBuildSubtarget.Player, cleanBuildCache);
            Build(LocalBuildLayout.ServerScenePath, LocalBuildLayout.ServerBuildPath, StandaloneBuildSubtarget.Server, cleanBuildCache);
        }

        private static void PrepareBuildInputs()
        {
            LocalBootstrapSceneGenerator.CreateBootstrapScenesIfMissing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        private static void Build(string scenePath, string outputPath, StandaloneBuildSubtarget subtarget, bool cleanBuildCache)
        {
            if (!File.Exists(scenePath))
            {
                throw new BuildFailedException($"Required scene does not exist: {scenePath}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var options = BuildOptions.Development | BuildOptions.AllowDebugging;
            if (cleanBuildCache)
            {
                options |= BuildOptions.CleanBuildCache;
            }

            var buildOptions = new BuildPlayerOptions { scenes = new[] { scenePath }, locationPathName = outputPath, target = BuildTarget.StandaloneWindows64, subtarget = (int)subtarget, options = options };
            Debug.Log($"[LocalBuild] Starting {subtarget} build at '{outputPath}'. Clean build cache: {cleanBuildCache}.");
            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(buildOptions);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{subtarget} build failed with result {report.summary.result}.");
            }

            Debug.Log($"[LocalBuild] Finished {subtarget} build in {report.summary.totalTime.TotalSeconds:F1}s at '{outputPath}'.");
        }
    }
}
