using UnityEditor;
using UnityEngine;

namespace Shooter.Bootstrap.Editor
{
    public static class ArenaPrefabComposition
    {
        public static GameObject Create()
        {
            var arena = new GameObject("Arena");
            AddBlock(arena.transform, "Ground", new Vector3(0f, -0.1f, 0f), new Vector3(12f, 0.2f, 12f));
            AddBlock(arena.transform, "North Wall", new Vector3(0f, 1.5f, 6f), new Vector3(12f, 3f, 0.3f));
            AddBlock(arena.transform, "South Wall", new Vector3(0f, 1.5f, -6f), new Vector3(12f, 3f, 0.3f));
            AddBlock(arena.transform, "East Wall", new Vector3(6f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f));
            AddBlock(arena.transform, "West Wall", new Vector3(-6f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f));
            AddBlock(arena.transform, "Center Cover", new Vector3(0f, 0.75f, 0f), new Vector3(1.5f, 1.5f, 3f));
            AddBlock(arena.transform, "North Cover", new Vector3(-3f, 0.75f, 3f), new Vector3(2f, 1.5f, 1f));
            AddBlock(arena.transform, "South Cover", new Vector3(3f, 0.75f, -3f), new Vector3(2f, 1.5f, 1f));
            return arena;
        }

        private static void AddBlock(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, worldPositionStays: false);
            block.transform.localPosition = position;
            block.transform.localScale = scale;
            GameObjectUtility.SetStaticEditorFlags(block, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
        }
    }
}
