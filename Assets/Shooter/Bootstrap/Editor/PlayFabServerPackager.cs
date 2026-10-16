using System;
using System.IO;
using System.IO.Compression;
using UnityEditor.Build;
using UnityEngine;

namespace Shooter.Bootstrap.Editor
{
    public static class PlayFabServerPackager
    {
        public static void Clean()
        {
            var projectRoot = GetProjectRoot();
            var buildRoot = ResolvePath(projectRoot, PlayFabBuildLayout.BuildRootPath);
            EnsureSafeBuildRoot(projectRoot, buildRoot);
            if (Directory.Exists(buildRoot))
            {
                Directory.Delete(buildRoot, recursive: true);
            }

            Debug.Log("[PlayFabBuild] Cleaned PlayFab build and package outputs.");
        }

        public static void Package()
        {
            var projectRoot = GetProjectRoot();
            var serverDirectory = ResolvePath(projectRoot, PlayFabBuildLayout.ServerDirectoryPath);
            var serverExecutable = ResolvePath(projectRoot, PlayFabBuildLayout.ServerBuildPath);
            var packagePath = ResolvePath(projectRoot, PlayFabBuildLayout.ServerPackagePath);
            EnsureServerBuildExists(serverDirectory, serverExecutable);
            Directory.CreateDirectory(Path.GetDirectoryName(packagePath));
            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }

            ZipFile.CreateFromDirectory(serverDirectory, packagePath, System.IO.Compression.CompressionLevel.Optimal, includeBaseDirectory: false);
            Debug.Log($"[PlayFabBuild] Packaged dedicated server at '{packagePath}'.");
        }

        private static string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static string ResolvePath(string projectRoot, string relativePath)
        {
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }

        private static void EnsureSafeBuildRoot(string projectRoot, string buildRoot)
        {
            var expectedRoot = ResolvePath(projectRoot, PlayFabBuildLayout.BuildRootPath);
            if (!string.Equals(buildRoot, expectedRoot, StringComparison.OrdinalIgnoreCase) || string.Equals(buildRoot, projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new BuildFailedException($"Refusing to clean unexpected PlayFab build path: '{buildRoot}'.");
            }
        }

        private static void EnsureServerBuildExists(string serverDirectory, string serverExecutable)
        {
            if (!Directory.Exists(serverDirectory) || !File.Exists(serverExecutable))
            {
                throw new BuildFailedException($"PlayFab dedicated-server build does not exist at '{serverExecutable}'. Build it before packaging.");
            }
        }
    }
}
