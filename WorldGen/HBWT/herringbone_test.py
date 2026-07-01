#!/usr/bin/env python3
"""
herringbone_test.py — local test harness for herringbone wang tile generation.

Ported from stb_herringbone_wang_tile.h by Sean Barrett (public domain).
http://nothings.org/gamedev/herringbone

WHY THIS EXISTS
Lets you test real tile art and edge-color authoring on your own machine,
producing an actual stitched map.png, before touching Unity at all. Same
two-step workflow as Barrett's original tool: generate a template of
placeholder tiles, hand-edit/replace them with real art in an image editor,
then generate maps from the finished set.

REQUIREMENTS
    pip install pillow

QUICK START (no art yet — try it with placeholders first)
    python herringbone_test.py template --out my_tileset --side-len 32 --colors 2
    python herringbone_test.py generate my_tileset 800 600 --out map.png

Then open my_tileset/ in your file browser, replace the placeholder PNGs
with real art (keep the same filenames and pixel dimensions), and re-run
the generate command.

TILESET FORMAT
A tileset is a folder containing:
  - tileset.json   — manifest listing each tile's file and edge constraints
  - *.png          — the tile images themselves

tileset.json:
{
  "short_side_len": 32,
  "tiles": [
    {"orientation": "h", "file": "h_0_1_0_1_0_1.png", "edges": [0,1,0,1,0,1]},
    {"orientation": "v", "file": "v_1_0_1_0_1_0.png", "edges": [1,0,1,0,1,0]},
    ...
  ]
}

Each tile has 6 edge-color values (a..f). Horizontal ("h") tiles are
2*short_side_len wide by short_side_len tall. Vertical ("v") tiles are
short_side_len wide by 2*short_side_len tall.

EDGE LAYOUT (which physical edge each letter refers to)

  Horizontal (h) tile:            Vertical (v) tile:

     *---a---*---b---*               *---a---*
     |               |               |       |
     c               d               b       c
     |               |               |       |
     *---e---*---f---*               *       *
                                      |       |
                                      d       e
                                      |       |
                                      *---f---*

Two tiles that will sit edge-to-edge in the generated map must share the
same color value on the touching edge, or the algorithm won't place them
next to each other. That's the whole matching rule.

COMPLETENESS
If your tile library doesn't cover every combination of edge colors that
can occur, generation can fail partway through (no tile satisfies some
slot's constraints). The "template" command builds a complete set
automatically (every combination for N colors) so this can't happen —
if you hand-author your own reduced set instead, expect occasional
failures, which this script will report clearly rather than silently
producing a broken map.
"""

import argparse
import itertools
import json
import random
import sys
from pathlib import Path

try:
    from PIL import Image, ImageDraw
except ImportError:
    sys.exit("This script needs Pillow. Install it with: pip install pillow")


EDGE_COLOR_RGB = [(15, 110, 86), (153, 60, 29), (133, 79, 11), (83, 74, 183)]
TILE_FILL_RGB = [
    (127, 119, 221), (29, 158, 117), (216, 90, 48), (212, 83, 126),
    (55, 138, 221), (99, 153, 34), (239, 159, 39), (175, 169, 236),
]


# ---------------------------------------------------------------------------
# Tileset loading
# ---------------------------------------------------------------------------

class Tile:
    def __init__(self, edges, image):
        self.edges = tuple(edges)  # (a,b,c,d,e,f)
        self.image = image


def load_tileset(tileset_dir):
    tileset_dir = Path(tileset_dir)
    manifest_path = tileset_dir / "tileset.json"
    if not manifest_path.exists():
        sys.exit(f"No tileset.json found in {tileset_dir}")

    manifest = json.loads(manifest_path.read_text())
    side_len = manifest["short_side_len"]

    h_tiles, v_tiles = [], []
    for entry in manifest["tiles"]:
        img_path = tileset_dir / entry["file"]
        if not img_path.exists():
            sys.exit(f"Missing tile image: {img_path}")
        img = Image.open(img_path).convert("RGB")

        if entry["orientation"] == "h":
            expected = (side_len * 2, side_len)
            if img.size != expected:
                sys.exit(f"{entry['file']} is {img.size}, expected {expected} (horizontal tile)")
            h_tiles.append(Tile(entry["edges"], img))
        elif entry["orientation"] == "v":
            expected = (side_len, side_len * 2)
            if img.size != expected:
                sys.exit(f"{entry['file']} is {img.size}, expected {expected} (vertical tile)")
            v_tiles.append(Tile(entry["edges"], img))
        else:
            sys.exit(f"Unknown orientation '{entry['orientation']}' for {entry['file']}")

    if not h_tiles or not v_tiles:
        sys.exit("Tileset needs at least one horizontal and one vertical tile.")

    return side_len, h_tiles, v_tiles


# ---------------------------------------------------------------------------
# Core algorithm (faithful port of stbhw_generate_image's edge-color mode)
# ---------------------------------------------------------------------------

class GenerationError(Exception):
    pass


def choose_tile(tiles, constraints, rng):
    """constraints: list of 6 ints, -1 meaning 'unconstrained'.
    Returns (tile, new_constraints) or raises GenerationError."""
    m = float("inf")
    for _pass in range(2):
        n = 0
        for tile in tiles:
            if all(c < 0 or c == e for c, e in zip(constraints, tile.edges)):
                n += 1
                if n > m:
                    return tile, list(tile.edges)
        if n == 0:
            raise GenerationError(
                "No tile matches required edge constraints "
                f"{constraints}. Your tileset doesn't cover this "
                "combination — add more tiles or regenerate a complete template."
            )
        m = rng.randrange(n)
    raise AssertionError("unreachable")


def generate_map(side_len, h_tiles, v_tiles, width, height, seed=None):
    rng = random.Random(seed)
    xmax = (width // side_len) + 6
    ymax = (height // side_len) + 6

    NEG = -1
    h_color = [[NEG] * (xmax + 10) for _ in range(ymax + 10)]
    v_color = [[NEG] * (xmax + 10) for _ in range(ymax + 10)]

    canvas = Image.new("RGB", (width, height), (30, 30, 30))

    ypos = -side_len
    j = -1
    while ypos < height:
        phase = j & 3
        i = 0 if phase == 0 else phase - 4
        while True:
            xpos = i * side_len
            if xpos >= width:
                break

            if xpos + side_len * 2 >= 0 and ypos >= 0:
                constraints = [
                    h_color[j+2][i+2], h_color[j+2][i+3],
                    v_color[j+2][i+2], v_color[j+2][i+4],
                    h_color[j+3][i+2], h_color[j+3][i+3],
                ]
                tile, new_c = choose_tile(h_tiles, constraints, rng)
                h_color[j+2][i+2], h_color[j+2][i+3] = new_c[0], new_c[1]
                v_color[j+2][i+2], v_color[j+2][i+4] = new_c[2], new_c[3]
                h_color[j+3][i+2], h_color[j+3][i+3] = new_c[4], new_c[5]
                canvas.paste(tile.image, (xpos, ypos))

            xpos += side_len * 2
            xpos += side_len
            if xpos < width:
                constraints = [
                    h_color[j+2][i+5],
                    v_color[j+2][i+5], v_color[j+2][i+6],
                    v_color[j+3][i+5], v_color[j+3][i+6],
                    h_color[j+4][i+5],
                ]
                tile, new_c = choose_tile(v_tiles, constraints, rng)
                h_color[j+2][i+5] = new_c[0]
                v_color[j+2][i+5], v_color[j+2][i+6] = new_c[1], new_c[2]
                v_color[j+3][i+5], v_color[j+3][i+6] = new_c[3], new_c[4]
                h_color[j+4][i+5] = new_c[5]
                canvas.paste(tile.image, (xpos, ypos))

            i += 4
        ypos += side_len
        j += 1

    return canvas


# ---------------------------------------------------------------------------
# Template generation (placeholder art + manifest, complete stochastic set)
# ---------------------------------------------------------------------------

def draw_placeholder(size, edges, orientation, side_len):
    w, h = size
    img = Image.new("RGB", size)
    draw = ImageDraw.Draw(img)

    fill_idx = sum(v * (4 ** k) for k, v in enumerate(edges)) % len(TILE_FILL_RGB)
    draw.rectangle([0, 0, w, h], fill=TILE_FILL_RGB[fill_idx])
    draw.rectangle([0, 0, w-1, h-1], outline=(0, 0, 0))

    r = 4
    if orientation == "h":
        points = [
            (w*0.25, 0), (w*0.75, 0),
            (0, h*0.5), (w, h*0.5),
            (w*0.25, h), (w*0.75, h),
        ]
    else:
        points = [
            (w*0.5, 0),
            (0, h*0.25), (w, h*0.25),
            (0, h*0.75), (w, h*0.75),
            (w*0.5, h),
        ]
    for (px, py), color_idx in zip(points, edges):
        color = EDGE_COLOR_RGB[color_idx % len(EDGE_COLOR_RGB)]
        draw.ellipse([px-r, py-r, px+r, py+r], fill=color)

    return img


def make_template(out_dir, side_len, num_colors):
    out_dir = Path(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)

    tiles_manifest = []
    combos = list(itertools.product(range(num_colors), repeat=6))

    for orientation, size in (("h", (side_len*2, side_len)), ("v", (side_len, side_len*2))):
        for edges in combos:
            fname = f"{orientation}_{'_'.join(map(str, edges))}.png"
            img = draw_placeholder(size, edges, orientation, side_len)
            img.save(out_dir / fname)
            tiles_manifest.append({
                "orientation": orientation,
                "file": fname,
                "edges": list(edges),
            })

    manifest = {"short_side_len": side_len, "tiles": tiles_manifest}
    (out_dir / "tileset.json").write_text(json.dumps(manifest, indent=2))

    print(f"Wrote {len(tiles_manifest)} placeholder tiles + tileset.json to {out_dir}/")
    print(f"({num_colors} edge colors -> {len(combos)} horizontal + {len(combos)} vertical tiles)")
    print("Open the folder, replace PNGs with real art (keep filenames and pixel sizes), "
          "then run the 'generate' command.")


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True)

    p_template = sub.add_parser("template", help="Generate a starter tileset (placeholder art) to edit by hand")
    p_template.add_argument("--out", required=True, help="Output directory for the tileset")
    p_template.add_argument("--side-len", type=int, default=32, help="Short side length in pixels (default 32)")
    p_template.add_argument("--colors", type=int, default=2, help="Number of edge colors, 1-8 (default 2)")

    p_gen = sub.add_parser("generate", help="Generate a map from a tileset")
    p_gen.add_argument("tileset_dir", help="Directory containing tileset.json and tile PNGs")
    p_gen.add_argument("width", type=int, help="Output map width in pixels")
    p_gen.add_argument("height", type=int, help="Output map height in pixels")
    p_gen.add_argument("--out", default="map.png", help="Output PNG path (default map.png)")
    p_gen.add_argument("--seed", type=int, default=None, help="Random seed (omit for random each run)")

    args = parser.parse_args()

    if args.command == "template":
        if not (1 <= args.colors <= 8):
            sys.exit("--colors must be between 1 and 8")
        make_template(args.out, args.side_len, args.colors)

    elif args.command == "generate":
        side_len, h_tiles, v_tiles = load_tileset(args.tileset_dir)
        print(f"Loaded {len(h_tiles)} horizontal + {len(v_tiles)} vertical tiles "
              f"(short_side_len={side_len})")
        try:
            result = generate_map(side_len, h_tiles, v_tiles, args.width, args.height, args.seed)
        except GenerationError as e:
            sys.exit(
                f"Generation failed: {e}\n\n"
                "This means your tileset doesn't cover every edge-color combination "
                "it needs. Either add more tiles by hand, or run 'template' to build "
                "a complete set and copy your art into matching filenames."
            )
        result.save(args.out)
        print(f"Saved {args.out} ({args.width}x{args.height})")


if __name__ == "__main__":
    main()