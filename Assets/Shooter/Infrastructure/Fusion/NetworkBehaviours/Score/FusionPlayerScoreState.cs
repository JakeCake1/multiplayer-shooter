using System;
using Fusion;
using Shooter.Features.Score;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class FusionPlayerScoreState : NetworkBehaviour
    {
        private static readonly PlayerScoreRules Rules = new PlayerScoreRules();

        [Networked]
        public int Kills { get; private set; }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                Kills = 0;
            }
        }

        public void AwardKill()
        {
            EnsureStateAuthority();
            Kills = Rules.AwardKill(Kills);
        }

        private void EnsureStateAuthority()
        {
            if (!Object.HasStateAuthority)
            {
                throw new InvalidOperationException("Only state authority can change player score.");
            }
        }
    }
}
