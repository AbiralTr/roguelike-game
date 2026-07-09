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
    /// with optional pre-placed rooms (boss rooms, gauntlets, etc.), and
    /// stamps it onto a target Tilemap. Attach to any GameObject, assign a
    /// Tilemap and a tileset, then call Generate() (or right-click the
    /// component in the Inspector and choose "Generate" via the context menu).
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

        [Header("Randomization")]
        public bool useRandomSeed = true;
        public int seed = 0;

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (tileset == null) { Debug.LogError("HerringboneMapGenerator: no tileset assigned."); return; }
            if (targetTilemap == null) { Debug.LogError("HerringboneMapGenerator: no target Tilemap assigned."); return; }

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
            // reproducible for a given seed.
            var prePlaced = new List<PrePlacedRegion<TileBase>>();
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
            }

            TileBase[,] grid;
            try
            {
                grid = HerringboneWangGenerator.GenerateMap(ts, widthCells, heightCells, rng, prePlaced: prePlaced);
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
                    var tile = grid[x, y];
                    if (tile != null)
                        targetTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }

            Debug.Log($"HerringboneMapGenerator: generated {widthCells}x{heightCells} map, " +
                      $"{prePlaced.Count} room(s) placed, seed {actualSeed}.");
        }
    }
}