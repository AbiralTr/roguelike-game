using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Herringbone
{
    public enum BrickOrientation { Horizontal, Vertical }

    /// <summary>
    /// One authored brick: its 6 edge-color constraints plus the Unity tiles
    /// that make up its footprint. widthCells/heightCells describe the shape
    /// of flatTiles (row-major, y then x — see FlatIndex).
    /// Horizontal bricks: widthCells = 2 * tileset.shortSideLenCells, heightCells = shortSideLenCells.
    /// Vertical bricks: widthCells = shortSideLenCells, heightCells = 2 * shortSideLenCells.
    /// </summary>
    [Serializable]
    public class HerringboneBrickData
    {
        public string sourceName;           // for debugging — which source file this came from
        public BrickOrientation orientation;
        public sbyte edgeA, edgeB, edgeC, edgeD, edgeE, edgeF;
        public int widthCells;
        public int heightCells;
        public List<TileBase> flatTiles = new List<TileBase>(); // length = widthCells * heightCells

        public TileBase GetTile(int x, int y) => flatTiles[y * widthCells + x];
    }

    [CreateAssetMenu(fileName = "HerringboneTileset", menuName = "Herringbone/Tileset")]
    public class HerringboneTilesetAsset : ScriptableObject
    {
        [Tooltip("Cells per brick short side, in Unity grid-cell units (not pixels).")]
        public int shortSideLenCells = 1;

        public List<HerringboneBrickData> hBricks = new List<HerringboneBrickData>();
        public List<HerringboneBrickData> vBricks = new List<HerringboneBrickData>();

        /// <summary>Converts this authored data into the generic algorithm's tileset structure.</summary>
        public HerringboneTileSet<TileBase> ToTileSet()
        {
            var ts = new HerringboneTileSet<TileBase> { ShortSideLen = shortSideLenCells };

            foreach (var b in hBricks)
                ts.HTiles.Add(ToTile(b));
            foreach (var b in vBricks)
                ts.VTiles.Add(ToTile(b));

            return ts;
        }

        private static HerringboneTile<TileBase> ToTile(HerringboneBrickData b)
        {
            var content = new TileBase[b.widthCells, b.heightCells];
            for (int y = 0; y < b.heightCells; y++)
                for (int x = 0; x < b.widthCells; x++)
                    content[x, y] = b.GetTile(x, y);

            return new HerringboneTile<TileBase>(b.edgeA, b.edgeB, b.edgeC, b.edgeD, b.edgeE, b.edgeF, content);
        }
    }
}
