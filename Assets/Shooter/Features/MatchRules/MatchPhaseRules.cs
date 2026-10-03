namespace Shooter.Features.MatchRules
{
    public static class MatchPhaseRules
    {
        public static bool AcceptsGameplayInput(MatchPhase phase)
        {
            return phase == MatchPhase.Playing;
        }
    }
}
