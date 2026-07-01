0   using System;
using System.Collections.Generic;

namespace ProceduralGen
{
    // =========================================================================
    // Herringbone Wang Tile generator — C# port
    //
    // Ported from stb_herringbone_wang_tile.h by Sean Barrett (public domain).
    // Original: http://nothings.org/gamedev/herringbone
    // This is a faithful port of the *edge-color* generation mode only
    // (corner-color mode from the original is omitted for simplicity).
    //
    // WHAT THIS DOES
    // Places 2:1 and 1:2 "brick" tiles in a herringbone weave. Each tile has
    // 6 edge constraints (a..f). Neighboring tiles must agree on the shared
    // edge's color. Content is stored as a generic int[,] grid of IDs so you
    // can map them to whatever you want on the Unity side (a TileBase, a
    // material class, a floor-height class, etc.) via your own lookup table.
    //
    // EDGE LAYOUT (matches the original's diagram)
    //
    //   Horizontal (2:1) tile — width = 2*sideLen, height = sideLen:
    //
    //        *---a---*---b---*
    //        |               |
    //        c               d
    //        |               |
    //        *---e---*---f---*
    //
    //   Vertical (1:2) tile — width = sideLen, height = 2*sideLen:
    //
    //        *---a---*
    //        |       |
    //        b       c
    //        |       |
    //        *       *
    //        |       |
    //        d       e
    //        |       |
    //        *---f---*
    //
    // =========================================================================

    /// <summary>
    /// A single hand-authored herringbone brick: 6 edge-color constraints
    /// plus its content payload (a grid of arbitrary IDs).
    /// </summary>
    public class HerringboneTile
    {
        public readonly sbyte A, B, C, D, E, F;

        /// <summary>
        /// Content grid, indexed [x, y]. For horizontal tiles this must be
        /// sized [sideLen*2, sideLen]; for vertical tiles [sideLen, sideLen*2].
        /// </summary>
        public readonly int[,] Content;

        public HerringboneTile(sbyte a, sbyte b, sbyte c, sbyte d, sbyte e, sbyte f, int[,] content)
        {
            A = a; B = b; C = c; D = d; E = e; F = f;
            Content = content;
        }
    }

    /// <summary>
    /// A full set of authored bricks for one biome/area. You need enough
    /// tiles to cover every edge-color combination your neighbors can
    /// produce, or generation will throw when it hits an unsatisfiable slot.
    /// A "complete stochastic set" (one tile per possible combination of the
    /// 6 edge colors) guarantees this never happens — see Barrett's paper
    /// for the tile-count math (e.g. 64 h-tiles + 64 v-tiles for 2 colors
    /// per edge).
    /// </summary>
    public class HerringboneTileSet
    {
        public int ShortSideLen;
        public readonly List<HerringboneTile> HTiles = new List<HerringboneTile>(); // 2:1 wide tiles
        public readonly List<HerringboneTile> VTiles = new List<HerringboneTile>(); // 1:2 tall tiles
    }

    public static class HerringboneWangGenerator
    {
        /// <summary>
        /// Generates a width x height grid of content IDs using herringbone
        /// wang tile placement. Throws InvalidOperationException if the
        /// tileset can't satisfy a required edge combination — this means
        /// your tile library isn't a complete-enough stochastic set.
        /// </summary>
        /// <param name="ts">Tileset to draw from.</param>
        /// <param name="width">Output grid width, in the same units as tile content (not "bricks").</param>
        /// <param name="height">Output grid height.</param>
        /// <param name="rng">Seeded RNG — reuse the same seed for reproducible levels.</param>
        /// <param name="emptyId">Fill value for any cell nothing gets drawn into (shouldn't happen in practice).</param>
        public static int[,] GenerateMap(HerringboneTileSet ts, int width, int height, Random rng, int emptyId = -1)
        {
            if (ts.ShortSideLen <= 0) throw new ArgumentException("ShortSideLen must be > 0");
            int sideLen = ts.ShortSideLen;

            // Same sizing formula as the original (xmax/ymax measured in
            // multiples of sideLen), plus generous slack for the index
            // offsets used below (+2..+6) so we never go out of bounds.
            int xmax = (width / sideLen) + 6;
            int ymax = (height / sideLen) + 6;

            sbyte[,] hColor = new sbyte[ymax + 10, xmax + 10];
            sbyte[,] vColor = new sbyte[ymax + 10, xmax + 10];
            for (int j = 0; j < hColor.GetLength(0); j++)
            {
                for (int i = 0; i < hColor.GetLength(1); i++)
                {
                    hColor[j, i] = -1;
                    vColor[j, i] = -1;
                }
            }

            int[,] output = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    output[x, y] = emptyId;

            // --- main herringbone sweep ---
            // Each "row" of the weave alternates: a horizontal (wide) block,
            // then the top of a new vertical (tall) block, repeating every
            // 4 short-side-lengths horizontally. Rows cycle through 4 phases
            // vertically before the offset pattern repeats. This stepping
            // (phase = j & 3, i += 4) is exactly what produces the woven
            // look — it's not arbitrary.
            int ypos = -sideLen;
            for (int j = -1; ypos < height; j++)
            {
                int phase = j & 3;
                int i = phase == 0 ? 0 : phase - 4;

                for (; ; i += 4)
                {
                    int xpos = i * sideLen;
                    if (xpos >= width) break;

                    // --- horizontal (2:1) tile ---
                    if (xpos + sideLen * 2 >= 0 && ypos >= 0)
                    {
                        var t = ChooseTile(ts.HTiles, rng,
                            ref hColor[j + 2, i + 2], ref hColor[j + 2, i + 3],
                            ref vColor[j + 2, i + 2], ref vColor[j + 2, i + 4],
                            ref hColor[j + 3, i + 2], ref hColor[j + 3, i + 3]);

                        if (t == null)
                            throw new InvalidOperationException(
                                "No horizontal tile satisfies the required edge constraints. " +
                                "Your tileset doesn't cover this edge-color combination — add more tiles " +
                                "or build a complete stochastic set.");

                        DrawTile(output, xpos, ypos, t.Content, sideLen * 2, sideLen, width, height);
                    }

                    xpos += sideLen * 2; // step past the horizontal block
                    xpos += sideLen;     // step to the start of the following vertical block

                    // --- vertical (1:2) tile ---
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
                                "Your tileset doesn't cover this edge-color combination — add more tiles " +
                                "or build a complete stochastic set.");

                        DrawTile(output, xpos, ypos, t.Content, sideLen, sideLen * 2, width, height);
                    }
                }
                ypos += sideLen;
            }

            return output;
        }

        /// <summary>
        /// Randomly picks a tile whose edges match the given constraints
        /// (a negative constraint value means "unconstrained — any color OK").
        /// Updates the constraints in place to the chosen tile's edges, which
        /// is how the propagation to not-yet-placed neighbors happens.
        /// Two-pass uniform selection: pass 1 counts matches, pass 2 walks
        /// again and stops on a uniformly random one among them.
        /// </summary>
        private static HerringboneTile ChooseTile(
            List<HerringboneTile> list, Random rng,
            ref sbyte a, ref sbyte b, ref sbyte c, ref sbyte d, ref sbyte e, ref sbyte f)
        {
            int m = int.MaxValue;
            for (int pass = 0; pass < 2; pass++)
            {
                int n = 0;
                for (int idx = 0; idx < list.Count; idx++)
                {
                    var t = list[idx];
                    if ((a < 0 || a == t.A) &&
                        (b < 0 || b == t.B) &&
                        (c < 0 || c == t.C) &&
                        (d < 0 || d == t.D) &&
                        (e < 0 || e == t.E) &&
                        (f < 0 || f == t.F))
                    {
                        n += 1;
                        if (n > m)
                        {
                            a = t.A; b = t.B; c = t.C; d = t.D; e = t.E; f = t.F;
                            return t;
                        }
                    }
                }
                if (n == 0) return null; // no tile matches — caller reports the error
                m = rng.Next(n);
            }
            return null; // unreachable
        }

        private static void DrawTile(int[,] output, int x, int y, int[,] content, int w, int h, int xmax, int ymax)
        {
            for (int cy = 0; cy < h; cy++)
            {
                int oy = y + cy;
                if (oy < 0 || oy >= ymax) continue;
                for (int cx = 0; cx < w; cx++)
                {
                    int ox = x + cx;
                    if (ox < 0 || ox >= xmax) continue;
                    output[ox, oy] = content[cx, cy];
                }
            }
        }
    }

    // =========================================================================
    // USAGE (pseudocode — wire this into your generation step, not per-frame):
    //
    //   var tileset = new HerringboneTileSet { ShortSideLen = 4 };
    //   // ... populate tileset.HTiles / tileset.VTiles from your authored
    //   //     ScriptableObject brick definitions ...
    //
    //   var rng = new System.Random(levelSeed);
    //   int[,] grid = HerringboneWangGenerator.GenerateMap(tileset, mapWidth, mapHeight, rng);
    //
    //   for (int x = 0; x < mapWidth; x++)
    //   for (int y = 0; y < mapHeight; y++)
    //   {
    //       int contentId = grid[x, y];
    //       var def = biomeLookup[contentId];         // your ScriptableObject lookup
    //       tilemap.SetTile(new Vector3Int(x, y, 0), def.unityTile);
    //   }
    //
    // NOTES
    // - "width"/"height" here are measured in content-grid cells (i.e. your
    //   Unity tile units), not in bricks — sideLen tells the generator how
    //   many cells make up one brick's short side.
    // - This port omits: weighting (biasing certain tiles), corner-color
    //   mode, and repetition reduction. Add weighting to ChooseTile if you
    //   want some tiles to appear more/less often than others.
    // - For pre-placed "large tile" content (boss rooms, T/L shapes from
    //   our earlier discussion), stamp directly into the output grid BEFORE
    //   calling GenerateMap by pre-filling hColor/vColor at the relevant
    //   indices — or, simpler, write your own pre-pass that fills those
    //   cells in `output` directly and have GenerateMap skip already-filled
    //   regions. Happy to sketch that extension if you want it.
    // =========================================================================
}