using Shooter.Application;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public sealed class UnityClientInputSettings : IClientInputSettings
    {
        private const string MouseSensitivityKey = "Shooter.MouseSensitivityMultiplier";
        private const float DefaultMouseSensitivityMultiplier = 2.5f;

        public float MinimumMouseSensitivityMultiplier => 0.25f;

        public float MaximumMouseSensitivityMultiplier => 5f;

        public float MouseSensitivityMultiplier
        {
            get => Mathf.Clamp(PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivityMultiplier), MinimumMouseSensitivityMultiplier, MaximumMouseSensitivityMultiplier);
            set => SaveMouseSensitivity(value);
        }

        private void SaveMouseSensitivity(float value)
        {
            PlayerPrefs.SetFloat(MouseSensitivityKey, Mathf.Clamp(value, MinimumMouseSensitivityMultiplier, MaximumMouseSensitivityMultiplier));
            PlayerPrefs.Save();
        }
    }
}
