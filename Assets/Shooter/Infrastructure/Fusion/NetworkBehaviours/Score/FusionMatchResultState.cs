using System;
using Fusion;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionMatchResultState : NetworkBehaviour
    {
        [Networked]
        public NetworkBool HasResult { get; private set; }

        [Networked]
        public NetworkBool IsDraw { get; private set; }

        [Networked]
        public int FirstPlayerId { get; private set; }

        [Networked]
        public int FirstPlayerKills { get; private set; }

        [Networked]
        public int SecondPlayerId { get; private set; }

        [Networked]
        public int SecondPlayerKills { get; private set; }

        [Networked]
        public int WinnerPlayerId { get; private set; }

        public void Publish(int firstPlayerId, int firstPlayerKills, int secondPlayerId, int secondPlayerKills, int winnerPlayerId)
        {
            EnsureStateAuthority();
            FirstPlayerId = firstPlayerId;
            FirstPlayerKills = firstPlayerKills;
            SecondPlayerId = secondPlayerId;
            SecondPlayerKills = secondPlayerKills;
            WinnerPlayerId = winnerPlayerId;
            IsDraw = firstPlayerKills == secondPlayerKills;
            HasResult = true;
        }

        public void ClearResult()
        {
            EnsureStateAuthority();
            HasResult = false;
            IsDraw = false;
            FirstPlayerId = 0;
            FirstPlayerKills = 0;
            SecondPlayerId = 0;
            SecondPlayerKills = 0;
            WinnerPlayerId = 0;
        }

        private void EnsureStateAuthority()
        {
            if (!Object.HasStateAuthority)
            {
                throw new InvalidOperationException("Only state authority can publish the match result.");
            }
        }
    }
}
