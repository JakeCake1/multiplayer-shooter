using System;
using Shooter.Application;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionMatchConfigurationSource : MonoBehaviour
    {
        public MatchConfiguration Configuration { get; private set; }

        public void Configure(MatchConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
    }
}
