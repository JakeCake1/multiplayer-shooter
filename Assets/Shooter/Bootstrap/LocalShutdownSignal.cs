using System;

namespace Shooter.Bootstrap
{
    public sealed class LocalShutdownSignal
    {
        public LocalShutdownSignal(string path)
        {
            Path = string.IsNullOrWhiteSpace(path) ? throw new ArgumentException("A shutdown signal path is required.", nameof(path)) : path;
        }

        public string Path { get; }
    }
}
