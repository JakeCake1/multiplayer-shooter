using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Shooter.Bootstrap.Editor
{
    public static class LocalBuildMenu
    {
        private const string ClientScenePath = "Assets/Scenes/LocalClient.unity";
        private const string ServerScenePath = "Assets/Scenes/LocalServer.unity";
        private const string ClientBuildPath = "Builds/Local/Client/ShooterClient.exe";
        private const string ServerBuildPath = "Builds/Local/Server/ShooterServer.exe";

        [MenuItem("Shooter/Local Development/Build Development Client")]
        public static void BuildClient()
        {
            Build(
                ClientScenePath,
                ClientBuildPath,
                StandaloneBuildSubtarget.Player);
        }

        [MenuItem("Shooter/Local Development/Build Development Dedicated Server")]
        public static void BuildServer()
        {
            Build(
                ServerScenePath,
                ServerBuildPath,
                StandaloneBuildSubtarget.Server);
        }

        [MenuItem("Shooter/Local Development/Build Development Client and Dedicated Server")]
        public static void BuildAll()
        {
            BuildClient();
            BuildServer();
        }

        private static void Build(
            string scenePath,
            string outputPath,
            StandaloneBuildSubtarget subtarget)
        {
            if (!File.Exists(scenePath))
            {
                throw new BuildFailedException($"Required scene does not exist: {scenePath}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)subtarget,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"{subtarget} build failed with result {report.summary.result}.");
            }
        }
    }
}
