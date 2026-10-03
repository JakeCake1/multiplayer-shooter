using UnityEngine;

namespace Shooter.Bootstrap.Client
{
    public sealed class LocalProcessDebugOverlay : MonoBehaviour
    {
        private const float Width = 500f;
        private const float Height = 86f;

        private string _text = string.Empty;

        public void SetText(string text)
        {
            _text = text ?? string.Empty;
        }

        private void OnGUI()
        {
            var area = new Rect(12f, 12f, Width, Height);
            GUI.Box(area, GUIContent.none);
            GUI.Label(new Rect(24f, 20f, Width - 24f, Height - 16f), _text);
        }
    }
}
