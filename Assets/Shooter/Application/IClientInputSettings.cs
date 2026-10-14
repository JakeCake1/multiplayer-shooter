namespace Shooter.Application
{
    public interface IClientInputSettings
    {
        float MinimumMouseSensitivityMultiplier { get; }

        float MaximumMouseSensitivityMultiplier { get; }

        float MouseSensitivityMultiplier { get; set; }
    }
}
