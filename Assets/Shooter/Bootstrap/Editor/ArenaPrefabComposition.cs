using UnityEditor;
using UnityEngine;

namespace Shooter.Bootstrap.Editor
{
    public static class ArenaPrefabComposition
    {
        private const float PartitionHeight = 3f;

        public static GameObject Create()
        {
            var arena = new GameObject("Arena");
            Ensure(arena);
            return arena;
        }

        public static bool Ensure(GameObject arena)
        {
            var changed = EnsureBlock(arena.transform, "Ground", new Vector3(0f, -0.1f, 0f), new Vector3(12f, 0.2f, 12f));
            changed |= EnsureBlock(arena.transform, "North Wall", new Vector3(0f, 1.5f, 6f), new Vector3(12f, 3f, 0.3f));
            changed |= EnsureBlock(arena.transform, "South Wall", new Vector3(0f, 1.5f, -6f), new Vector3(12f, 3f, 0.3f));
            changed |= EnsureBlock(arena.transform, "East Wall", new Vector3(6f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f));
            changed |= EnsureBlock(arena.transform, "West Wall", new Vector3(-6f, 1.5f, 0f), new Vector3(0.3f, 3f, 12f));
            changed |= EnsureBlock(arena.transform, "Center Cover", new Vector3(0f, PartitionHeight * 0.5f, 0f), new Vector3(1.5f, PartitionHeight, 3f));
            changed |= EnsureBlock(arena.transform, "North Cover", new Vector3(-3f, PartitionHeight * 0.5f, 3f), new Vector3(2f, PartitionHeight, 1f));
            changed |= EnsureBlock(arena.transform, "South Cover", new Vector3(3f, PartitionHeight * 0.5f, -3f), new Vector3(2f, PartitionHeight, 1f));
            return changed;
        }

        private static bool EnsureBlock(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var blockTransform = parent.Find(name);
            var changed = blockTransform == null;
            if (blockTransform == null)
            {
                blockTransform = CreateBlock(parent, name).transform;
            }

            changed |= EnsureTransform(blockTransform, position, scale);
            changed |= EnsureStaticFlags(blockTransform.gameObject);
            return changed;
        }

        private static GameObject CreateBlock(Transform parent, string name)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, worldPositionStays: false);
            return block;
        }

        private static bool EnsureTransform(Transform blockTransform, Vector3 position, Vector3 scale)
        {
            var changed = blockTransform.localPosition != position || blockTransform.localRotation != Quaternion.identity || blockTransform.localScale != scale;
            blockTransform.localPosition = position;
            blockTransform.localRotation = Quaternion.identity;
            blockTransform.localScale = scale;
            return changed;
        }

        private static bool EnsureStaticFlags(GameObject block)
        {
            const StaticEditorFlags expectedFlags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic;
            if (GameObjectUtility.GetStaticEditorFlags(block) == expectedFlags)
            {
                return false;
            }

            GameObjectUtility.SetStaticEditorFlags(block, expectedFlags);
            return true;
        }
    }
}
