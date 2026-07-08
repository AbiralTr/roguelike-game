using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Herringbone
{
    /// <summary>
    /// Generates a herringbone-wang-tile map from a HerringboneTilesetAsset
    /// and stamps it onto a target Tilemap. Attach to any GameObject, assign
    /// a Tilemap and a tileset, then call Generate() (or right-click the
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

            TileBase[,] grid;
            try
            {
                grid = HerringboneWangGenerator.GenerateMap(ts, widthCells, heightCells, rng);
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

            Debug.Log($"HerringboneMapGenerator: generated {widthCells}x{heightCells} map, seed {actualSeed}.");
        }
    }
}
