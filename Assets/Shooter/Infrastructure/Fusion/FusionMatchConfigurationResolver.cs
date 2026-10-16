using System;
using Shooter.Application;
using Fusion;

namespace Shooter.Infrastructure.Fusion
{
    public static class FusionMatchConfigurationResolver
    {
        public static MatchConfiguration Resolve(NetworkRunner runner)
        {
            var source = runner != null ? runner.GetComponent<FusionMatchConfigurationSource>() : null;
            return source?.Configuration ?? throw new InvalidOperationException("The Fusion runner has no loaded match configuration.");
        }
    }
}
