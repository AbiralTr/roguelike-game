using System;
using System.Collections.Generic;

namespace Herringbone
{
    // =========================================================================
    // Generic herringbone wang tile generator.
    //
    // Same algorithm as before (ported from Sean Barrett's
    // stb_herringbone_wang_tile.h), now generic over content type T so it can
    // return a T[,] directly — e.g. T = UnityEngine.Tilemaps.TileBase, so the
    // output can be stamped straight into a Tilemap with no int-id lookup step.
    // =========================================================================

    public class HerringboneTile<T>
    {
        public readonly sbyte A, B, C, D, E, F;

        /// <summary>
        /// Content grid, indexed [x, y]. Horizontal tiles: [sideLen*2, sideLen].
        /// Vertical tiles: [sideLen, sideLen*2].
        /// </summary>
        public readonly T[,] Content;

        public HerringboneTile(sbyte a, sbyte b, sbyte c, sbyte d, sbyte e, sbyte f, T[,] content)
        {
            A = a; B = b; C = c; D = d; E = e; F = f;
            Content = content;
        }
    }

    public class HerringboneTileSet<T>
    {
        /// <summary>Short side length, in the same units as the content grid (e.g. Unity cells).</summary>
        public int ShortSideLen;
        public readonly List<HerringboneTile<T>> HTiles = new List<HerringboneTile<T>>();
        public readonly List<HerringboneTile<T>> VTiles = new List<HerringboneTile<T>>();
    }

    /// <summary>
    /// A block of content to stamp directly into the map before the stochastic
    /// wang-tile sweep runs — for boss rooms, gauntlet rooms, or any other
    /// guaranteed set-piece. The sweep will skip every cell this occupies,
    /// so the room's art is never overwritten. Cells the sweep would have
    /// placed a normal brick into right at the room's boundary still get
    /// chosen normally (their far side just won't be color-matched against
    /// the room, since the room doesn't participate in edge bookkeeping) —
    /// so give room art its own border/wall margin for a clean transition.
    /// </summary>
    public class PrePlacedRegion<T>
    {
        public int X, Y;
        public T[,] Content; // [width, height]
    }

    public static class HerringboneWangGenerator
    {
        /// <summary>
        /// Generates a width x height grid of content using herringbone wang
        /// tile placement. Throws InvalidOperationException if the tileset
        /// can't satisfy some slot's edge constraints (incomplete stochastic set).
        /// </summary>
        public static T[,] GenerateMap<T>(HerringboneTileSet<T> ts, int width, int height, Random rng, T emptyValue = default, List<PrePlacedRegion<T>> prePlaced = null)
        {
            if (ts.ShortSideLen <= 0) throw new ArgumentException("ShortSideLen must be > 0");
            int sideLen = ts.ShortSideLen;

            int xmax = (width / sideLen) + 6;
            int ymax = (height / sideLen) + 6;

            sbyte[,] hColor = new sbyte[ymax + 10, xmax + 10];
            sbyte[,] vColor = new sbyte[ymax + 10, xmax + 10];
            for (int j = 0; j < hColor.GetLength(0); j++)
                for (int i = 0; i < hColor.GetLength(1); i++)
                {
                    hColor[j, i] = -1;
                    vColor[j, i] = -1;
                }

            T[,] output = new T[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    output[x, y] = emptyValue;

            bool[,] occupied = new bool[width, height];

            if (prePlaced != null)
            {
                foreach (var region in prePlaced)
                {
                    int rw = region.Content.GetLength(0);
                    int rh = region.Content.GetLength(1);
                    for (int cy = 0; cy < rh; cy++)
                    {
                        int oy = region.Y + cy;
                        if (oy < 0 || oy >= height) continue;
                        for (int cx = 0; cx < rw; cx++)
                        {
                            int ox = region.X + cx;
                            if (ox < 0 || ox >= width) continue;
                            output[ox, oy] = region.Content[cx, cy];
                            occupied[ox, oy] = true;
                        }
                    }
                }
            }

            int ypos = -sideLen;
            for (int j = -1; ypos < height; j++)
            {
                int phase = j & 3;
                int i = phase == 0 ? 0 : phase - 4;

                for (; ; i += 4)
                {
                    int xpos = i * sideLen;
                    if (xpos >= width) break;

                    if (xpos + sideLen * 2 >= 0 && ypos >= 0)
                    {
                        var t = ChooseTile(ts.HTiles, rng,
                            ref hColor[j + 2, i + 2], ref hColor[j + 2, i + 3],
                            ref vColor[j + 2, i + 2], ref vColor[j + 2, i + 4],
                            ref hColor[j + 3, i + 2], ref hColor[j + 3, i + 3]);

                        if (t == null)
                            throw new InvalidOperationException(
                                "No horizontal tile satisfies the required edge constraints. " +
                                "Add more tiles to cover this edge-color combination.");

                        DrawTile(output, occupied, xpos, ypos, t.Content, sideLen * 2, sideLen, width, height);
                    }

                    xpos += sideLen * 2;
                    xpos += sideLen;

                    if (xpos < width)
                    {
                        var t = ChooseTile(ts.VTiles, rng,
                            ref hColor[j + 2, i + 5],
                            ref vColor[j + 2, i + 5], ref vColor[j + 2, i + 6],
                            ref vColor[j + 3, i + 5], ref vColor[j + 3, i + 6],
                            ref hColor[j + 4, i + 5]);

                        if (t == null)
                            throw new InvalidOperationException(
                                "No vertical tile satisfies the required edge constraints. " +
                                "Add more tiles to cover this edge-color combination.");

                        DrawTile(output, occupied, xpos, ypos, t.Content, sideLen, sideLen * 2, width, height);
                    }
                }
                ypos += sideLen;
            }

            return output;
        }

        private static HerringboneTile<T> ChooseTile<T>(
            List<HerringboneTile<T>> list, Random rng,
            ref sbyte a, ref sbyte b, ref sbyte c, ref sbyte d, ref sbyte e, ref sbyte f)
        {
            int m = int.MaxValue;
            for (int pass = 0; pass < 2; pass++)
            {
                int n = 0;
                for (int idx = 0; idx < list.Count; idx++)
                {
                    var t = list[idx];
                    if ((a < 0 || a == t.A) && (b < 0 || b == t.B) &&
                        (c < 0 || c == t.C) && (d < 0 || d == t.D) &&
                        (e < 0 || e == t.E) && (f < 0 || f == t.F))
                    {
                        n += 1;
                        if (n > m)
                        {
                            a = t.A; b = t.B; c = t.C; d = t.D; e = t.E; f = t.F;
                            return t;
                        }
                    }
                }
                if (n == 0) return null;
                m = rng.Next(n);
            }
            return null;
        }

        private static void DrawTile<T>(T[,] output, bool[,] occupied, int x, int y, T[,] content, int w, int h, int xmax, int ymax)
        {
            for (int cy = 0; cy < h; cy++)
            {
                int oy = y + cy;
                if (oy < 0 || oy >= ymax) continue;
                for (int cx = 0; cx < w; cx++)
                {
                    int ox = x + cx;
                    if (ox < 0 || ox >= xmax) continue;
                    if (occupied[ox, oy]) continue;
                    output[ox, oy] = content[cx, cy];
                }
            }
        }
    }
}