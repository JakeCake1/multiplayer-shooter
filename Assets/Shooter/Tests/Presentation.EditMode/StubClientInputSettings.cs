using Shooter.Application;

namespace Shooter.Presentation.Tests
{
    public sealed class StubClientInputSettings : IClientInputSettings
    {
        public float MinimumMouseSensitivityMultiplier => 0.25f;

        public float MaximumMouseSensitivityMultiplier => 5f;

        public float MouseSensitivityMultiplier { get; set; } = 2.5f;
    }
}
