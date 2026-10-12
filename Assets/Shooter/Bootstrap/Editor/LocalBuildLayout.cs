namespace Shooter.Bootstrap.Editor
{
    public static class LocalBuildLayout
    {
        public const string ClientScenePath = "Assets/Scenes/LocalClient.unity";
        public const string ServerScenePath = "Assets/Scenes/LocalServer.unity";
        public const string ClientBuildPath = "Builds/Local/Client/ShooterClient.exe";
        public const string ServerBuildPath = "Builds/Local/Server/ShooterServer.exe";
        public const string LocalBuildRootPath = "Builds/Local";
        public static readonly string[] OutputDirectoryPaths = { "Builds/Local/Client", "Builds/Local/Server" };
    }
}
