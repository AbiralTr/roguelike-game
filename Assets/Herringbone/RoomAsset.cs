using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Herringbone
{
    /// <summary>
    /// A hand-authored room of arbitrary size, stamped directly into the map
    /// before the stochastic wang-tile fill runs. Use this for boss rooms,
    /// gauntlet arenas, miniboss rooms — any guaranteed content.
    ///
    /// Give room art its own border/wall margin (a cell or two of generic
    /// wall matching your biome) so the transition to surrounding stochastic
    /// terrain reads as intentional — the room doesn't participate in the
    /// wang edge-matching, so it won't blend seamlessly on its own.
    /// </summary>
    [CreateAssetMenu(fileName = "HerringboneRoom", menuName = "Herringbone/Room")]
    public class RoomAsset : ScriptableObject
    {
        public int widthCells;
        public int heightCells;
        public List<TileBase> flatTiles = new List<TileBase>(); // length = widthCells * heightCells, row-major (y then x)

        // Optional per-cell marker IDs for the future interactable/spawn
        // system (enemy spawn points, chests, etc.) — not consumed yet,
        // but captured now so room authoring doesn't need to be redone
        // when that system lands. "" or null = no marker.
        public List<string> cellMarkers = new List<string>();

        public TileBase GetTile(int x, int y) => flatTiles[y * widthCells + x];

        public string GetMarker(int x, int y)
        {
            int idx = y * widthCells + x;
            return (cellMarkers != null && idx < cellMarkers.Count) ? cellMarkers[idx] : null;
        }

        public TileBase[,] ToContentGrid()
        {
            var content = new TileBase[widthCells, heightCells];
            for (int y = 0; y < heightCells; y++)
                for (int x = 0; x < widthCells; x++)
                    content[x, y] = GetTile(x, y);
            return content;
        }
    }
}
