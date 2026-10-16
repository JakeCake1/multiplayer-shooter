using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Shooter.Bootstrap.Editor
{
    public static class UnityPlayerBuildPipeline
    {
        public static void PrepareBuildInputs()
        {
            LocalBootstrapSceneGenerator.CreateBootstrapScenesIfMissing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        public static void Build(string scenePath, string outputPath, StandaloneBuildSubtarget subtarget, BuildOptions options, string logCategory)
        {
            EnsureSceneExists(scenePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var buildOptions = new BuildPlayerOptions { scenes = new[] { scenePath }, locationPathName = outputPath, target = BuildTarget.StandaloneWindows64, subtarget = (int)subtarget, options = options };
            Debug.Log($"[{logCategory}] Starting {subtarget} build at '{outputPath}'. Options: {options}.");
            var previousHttpOption = ConfigureHttpPolicy(subtarget);
            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(buildOptions);
            }
            finally
            {
                PlayerSettings.insecureHttpOption = previousHttpOption;
                EditorUtility.ClearProgressBar();
            }

            EnsureBuildSucceeded(report, subtarget);
            Debug.Log($"[{logCategory}] Finished {subtarget} build in {report.summary.totalTime.TotalSeconds:F1}s at '{outputPath}'.");
        }

        private static void EnsureSceneExists(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                throw new BuildFailedException($"Required scene does not exist: {scenePath}");
            }
        }

        private static void EnsureBuildSucceeded(BuildReport report, StandaloneBuildSubtarget subtarget)
        {
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{subtarget} build failed with result {report.summary.result}.");
            }
        }

        private static InsecureHttpOption ConfigureHttpPolicy(StandaloneBuildSubtarget subtarget)
        {
            var previousOption = PlayerSettings.insecureHttpOption;
            if (subtarget == StandaloneBuildSubtarget.Server)
            {
                PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
            }

            return previousOption;
        }
    }
}
