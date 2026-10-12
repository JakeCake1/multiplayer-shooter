using UnityEngine;

namespace Shooter.Bootstrap
{
    public static class ClientDebugPlayerColors
    {
        private static readonly Color[] Colors = { new Color(0.2f, 0.85f, 1f), new Color(1f, 0.55f, 0.15f), new Color(0.45f, 1f, 0.35f), new Color(1f, 0.35f, 0.8f) };

        public static Color Get(int playerId)
        {
            var index = Mathf.Abs(playerId) % Colors.Length;
            return Colors[index];
        }
    }
}
