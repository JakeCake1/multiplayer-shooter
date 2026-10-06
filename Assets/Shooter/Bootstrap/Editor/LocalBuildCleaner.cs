using System;
using System.IO;
using UnityEditor.Build;
using UnityEngine;

namespace Shooter.Bootstrap.Editor
{
    public static class LocalBuildCleaner
    {
        public static void Clean()
        {
            var projectRoot = GetProjectRoot();
            var localBuildRoot = ResolvePath(projectRoot, LocalBuildLayout.LocalBuildRootPath);
            foreach (var outputDirectoryPath in LocalBuildLayout.OutputDirectoryPaths)
            {
                CleanOutputDirectory(projectRoot, localBuildRoot, outputDirectoryPath);
            }

            Debug.Log("[LocalBuild] Cleaned local client and dedicated-server build outputs. Logs were preserved.");
        }

        private static string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static void CleanOutputDirectory(string projectRoot, string localBuildRoot, string outputDirectoryPath)
        {
            var outputDirectory = ResolvePath(projectRoot, outputDirectoryPath);
            EnsureSafeOutputDirectory(localBuildRoot, outputDirectory);
            if (!Directory.Exists(outputDirectory))
            {
                return;
            }

            try
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
            catch (Exception exception)
            {
                throw new BuildFailedException($"Could not clean local build directory '{outputDirectory}'. Stop running local build processes and try again. {exception.Message}");
            }
        }

        private static string ResolvePath(string projectRoot, string relativePath)
        {
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }

        private static void EnsureSafeOutputDirectory(string localBuildRoot, string outputDirectory)
        {
            var localBuildPrefix = localBuildRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!outputDirectory.StartsWith(localBuildPrefix, StringComparison.OrdinalIgnoreCase) || string.Equals(outputDirectory, localBuildRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new BuildFailedException($"Refusing to clean path outside the local build root: '{outputDirectory}'.");
            }
        }
    }
}
