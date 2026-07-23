using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Herringbone
{
    /// <summary>
    /// One guaranteed room in the floor layout. Position is fixed by the
    /// designer; content is either fixed (pool of 1 — e.g. your boss room,
    /// same every run) or randomly chosen from a pool (gauntlet/miniboss
    /// variety).
    /// </summary>
    [Serializable]
    public class RoomSlot
    {
        public string label = "room";
        public int x;
        public int y;
        public List<RoomAsset> pool = new List<RoomAsset>();
    }

    /// <summary>
    /// Generates a herringbone-wang-tile map from a HerringboneTilesetAsset,
    /// with optional pre-placed rooms (boss rooms, gauntlets, etc.), marker-
    /// driven spawns inside those rooms (chests, enemies, save points), and
    /// an ambient loot-scatter pass across the regular generated terrain.
    /// Attach to any GameObject, assign a Tilemap and a tileset, then call
    /// Generate() (or right-click the component and choose "Generate").
    /// </summary>
    public class HerringboneMapGenerator : MonoBehaviour
    {
        [Header("Source")]
        public HerringboneTilesetAsset tileset;

        [Header("Target")]
        public Tilemap targetTilemap;

        [Header("Map size (in Unity grid cells)")]
        public int widthCells = 64;
        public int heightCells = 32;

        [Header("Guaranteed rooms (optional)")]
        public List<RoomSlot> roomSlots = new List<RoomSlot>();

        [Header("Spawning")]
        public SpawnRegistry spawnRegistry;
        [Tooltip("Where spawned GameObjects (loot, room markers) get parented, so they're easy to find/clear. Optional.")]
        public Transform spawnParent;
        [Range(0f, 1f)]
        [Tooltip("Per-eligible-cell chance of spawning an ambient loot item during the scatter pass.")]
        public float ambientLootChance = 0.03f;

        [Header("Randomization")]
        public bool useRandomSeed = true;
        public int seed = 0;

        private struct RoomMarker
        {
            public int X, Y;
            public string MarkerId;
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (tileset == null) { Debug.LogError("HerringboneMapGenerator: no tileset assigned."); return; }
            if (targetTilemap == null) { Debug.LogError("HerringboneMapGenerator: no target Tilemap assigned."); return; }

            ClearSpawnedObjects();

            var ts = tileset.ToTileSet();
            if (ts.HTiles.Count == 0 || ts.VTiles.Count == 0)
            {
                Debug.LogError("HerringboneMapGenerator: tileset has no horizontal and/or vertical bricks.");
                return;
            }

            int actualSeed = useRandomSeed ? Environment.TickCount : seed;
            var rng = new System.Random(actualSeed);

            // Resolve each room slot to a concrete room BEFORE the stochastic
            // fill runs, using the same rng instance, so results stay
            // reproducible for a given seed. Also collect any cell markers
            // the chosen room carries, offset into world space.
            var prePlaced = new List<PrePlacedRegion<TileBase>>();
            var roomMarkers = new List<RoomMarker>();

            foreach (var slot in roomSlots)
            {
                if (slot.pool == null || slot.pool.Count == 0)
                {
                    Debug.LogWarning($"HerringboneMapGenerator: room slot '{slot.label}' has an empty pool, skipping.");
                    continue;
                }
                var chosen = slot.pool[rng.Next(slot.pool.Count)];
                if (chosen == null)
                {
                    Debug.LogWarning($"HerringboneMapGenerator: room slot '{slot.label}' picked a null entry, skipping.");
                    continue;
                }

                prePlaced.Add(new PrePlacedRegion<TileBase>
                {
                    X = slot.x,
                    Y = slot.y,
                    Content = chosen.ToContentGrid()
                });

                for (int cy = 0; cy < chosen.heightCells; cy++)
                {
                    for (int cx = 0; cx < chosen.widthCells; cx++)
                    {
                        string marker = chosen.GetMarker(cx, cy);
                        if (!string.IsNullOrEmpty(marker))
                            roomMarkers.Add(new RoomMarker { X = slot.x + cx, Y = slot.y + cy, MarkerId = marker });
                    }
                }
            }

            GenerationResult<TileBase> result;
            try
            {
                result = HerringboneWangGenerator.GenerateMap(ts, widthCells, heightCells, rng, prePlaced: prePlaced);
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError("HerringboneMapGenerator: generation failed — " + e.Message +
                                " (your tileset likely doesn't cover every required edge-color combination)");
                return;
            }

            targetTilemap.ClearAllTiles();
            for (int x = 0; x < widthCells; x++)
            {
                for (int y = 0; y < heightCells; y++)
                {
                    var tile = result.Grid[x, y];
                    if (tile != null)
                        targetTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }

            int markersSpawned = SpawnRoomMarkers(roomMarkers);
            int ambientSpawned = ScatterAmbientLoot(result.OccupiedByPrePlaced, rng);

            Debug.Log($"HerringboneMapGenerator: generated {widthCells}x{heightCells} map, " +
                      $"{prePlaced.Count} room(s), {markersSpawned} marker spawn(s), " +
                      $"{ambientSpawned} ambient loot spawn(s), seed {actualSeed}.");
        }

        private int SpawnRoomMarkers(List<RoomMarker> markers)
        {
            if (spawnRegistry == null)
            {
                if (markers.Count > 0)
                    Debug.LogWarning("HerringboneMapGenerator: rooms have markers but no Spawn Registry is assigned — skipping marker spawns.");
                return 0;
            }

            int count = 0;
            foreach (var m in markers)
            {
                var prefab = spawnRegistry.GetMarkerPrefab(m.MarkerId);
                if (prefab == null)
                {
                    Debug.LogWarning($"HerringboneMapGenerator: no prefab registered for marker '{m.MarkerId}'.");
                    continue;
                }
                Vector3 worldPos = targetTilemap.GetCellCenterWorld(new Vector3Int(m.X, m.Y, 0));
                Instantiate(prefab, worldPos, Quaternion.identity, spawnParent);
                count++;
            }
            return count;
        }

        private int ScatterAmbientLoot(bool[,] occupiedByRoom, System.Random rng)
        {
            if (spawnRegistry == null)
            {
                Debug.LogWarning("HerringboneMapGenerator: ambient scatter skipped — no Spawn Registry assigned.");
                return 0;
            }
            if (spawnRegistry.ambientLootTable == null || spawnRegistry.ambientLootTable.Count == 0)
            {
                Debug.LogWarning("HerringboneMapGenerator: ambient scatter skipped — Spawn Registry's Ambient Loot Table is empty.");
                return 0;
            }
            if (ambientLootChance <= 0f)
            {
                Debug.LogWarning("HerringboneMapGenerator: ambient scatter skipped — Ambient Loot Chance is 0.");
                return 0;
            }

            int solidCount = 0, eligibleCount = 0, passedRollCount = 0, count = 0;

            for (int x = 0; x < widthCells; x++)
            {
                for (int y = 0; y < heightCells - 1; y++)
                {
                    if (occupiedByRoom[x, y]) continue;

                    var groundCell = new Vector3Int(x, y, 0);
                    var aboveCell = new Vector3Int(x, y + 1, 0);

                    bool groundIsSolid = targetTilemap.GetColliderType(groundCell) != Tile.ColliderType.None;
                    if (!groundIsSolid) continue;
                    solidCount++;

                    bool aboveIsOpen = targetTilemap.GetTile(aboveCell) == null;
                    if (!aboveIsOpen) continue;
                    eligibleCount++;

                    if (rng.NextDouble() > ambientLootChance) continue;
                    passedRollCount++;

                    var prefab = spawnRegistry.PickAmbientLoot(rng);
                    if (prefab == null) continue;

                    Vector3 worldPos = targetTilemap.GetCellCenterWorld(aboveCell);
                    Instantiate(prefab, worldPos, Quaternion.identity, spawnParent);
                    count++;
                }
            }

            Debug.Log($"HerringboneMapGenerator ambient scatter diagnostics: " +
                      $"{solidCount} solid-ground cell(s) found, {eligibleCount} had open air above, " +
                      $"{passedRollCount} passed the chance roll, {count} actually spawned.");

            return count;
        }

        private void ClearSpawnedObjects()
        {
            if (spawnParent == null) return;
            for (int i = spawnParent.childCount - 1; i >= 0; i--)
            {
                var child = spawnParent.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }
    }
}