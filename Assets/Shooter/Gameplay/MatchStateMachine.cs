using System;

namespace Shooter.Gameplay
{
    public sealed class MatchStateMachine
    {
        private readonly MatchRules _rules;

        public MatchStateMachine(MatchRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public MatchPhase Phase { get; private set; } = MatchPhase.WaitingForPlayers;

        public float SecondsRemaining { get; private set; }

        public void Advance(int connectedPlayerCount, float deltaSeconds)
        {
            if (connectedPlayerCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(connectedPlayerCount));
            }

            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            if (Phase == MatchPhase.WaitingForPlayers)
            {
                if (connectedPlayerCount >= _rules.RequiredPlayerCount)
                {
                    Enter(MatchPhase.Starting, _rules.CountdownSeconds);
                    CompleteZeroDurationPhases();
                }

                return;
            }

            if (Phase == MatchPhase.Starting &&
                connectedPlayerCount < _rules.RequiredPlayerCount)
            {
                Enter(MatchPhase.WaitingForPlayers, 0f);
                return;
            }

            var remainingDelta = deltaSeconds;
            while (remainingDelta > 0f && HasTimedPhase(Phase))
            {
                if (remainingDelta < SecondsRemaining)
                {
                    SecondsRemaining -= remainingDelta;
                    return;
                }

                remainingDelta -= SecondsRemaining;
                EnterNextPhase();
                CompleteZeroDurationPhases();
            }
        }

        private static bool HasTimedPhase(MatchPhase phase)
        {
            return phase == MatchPhase.Starting ||
                   phase == MatchPhase.Playing ||
                   phase == MatchPhase.Finishing;
        }

        private void CompleteZeroDurationPhases()
        {
            while (HasTimedPhase(Phase) && SecondsRemaining <= 0f)
            {
                EnterNextPhase();
            }
        }

        private void EnterNextPhase()
        {
            switch (Phase)
            {
                case MatchPhase.Starting:
                    Enter(MatchPhase.Playing, _rules.MatchSeconds);
                    break;
                case MatchPhase.Playing:
                    Enter(MatchPhase.Finishing, _rules.FinishingSeconds);
                    break;
                case MatchPhase.Finishing:
                    Enter(MatchPhase.Finished, 0f);
                    break;
            }
        }

        private void Enter(MatchPhase phase, float secondsRemaining)
        {
            Phase = phase;
            SecondsRemaining = secondsRemaining;
        }
    }
}
