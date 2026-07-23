using System;
using System.Collections.Generic;
using UnityEngine;

namespace Herringbone
{
    [Serializable]
    public class MarkerSpawnEntry
    {
        [Tooltip("Must match a marker ID painted into a RoomAsset's cellMarkers, e.g. \"chest\", \"save_point\".")]
        public string markerId;
        public GameObject prefab;
    }

    [Serializable]
    public class WeightedLootEntry
    {
        public GameObject prefab;
        [Tooltip("Relative weight — higher = more common. Doesn't need to sum to any particular total.")]
        public float weight = 1f;
    }

    [CreateAssetMenu(fileName = "HerringboneSpawnRegistry", menuName = "Herringbone/Spawn Registry")]
    public class SpawnRegistry : ScriptableObject
    {
        [Header("Room markers (explicit, hand-placed)")]
        public List<MarkerSpawnEntry> markerSpawns = new List<MarkerSpawnEntry>();

        [Header("Ambient scatter (random, throughout regular terrain)")]
        public List<WeightedLootEntry> ambientLootTable = new List<WeightedLootEntry>();

        public GameObject GetMarkerPrefab(string markerId)
        {
            foreach (var entry in markerSpawns)
                if (entry.markerId == markerId)
                    return entry.prefab;
            return null;
        }

        public GameObject PickAmbientLoot(System.Random rng)
        {
            if (ambientLootTable == null || ambientLootTable.Count == 0) return null;

            float total = 0f;
            foreach (var entry in ambientLootTable) total += Mathf.Max(0f, entry.weight);
            if (total <= 0f) return null;

            double roll = rng.NextDouble() * total;
            float cumulative = 0f;
            foreach (var entry in ambientLootTable)
            {
                cumulative += Mathf.Max(0f, entry.weight);
                if (roll <= cumulative)
                    return entry.prefab;
            }
            return ambientLootTable[ambientLootTable.Count - 1].prefab;
        }
    }
}
