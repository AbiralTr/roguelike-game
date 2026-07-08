// IMPORTANT: this file uses UnityEditor APIs and must live in a folder named
// "Editor" somewhere in your Assets (e.g. Assets/Herringbone/Editor/). Unity
// excludes Editor folders from player builds automatically.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Herringbone.EditorTools
{
    /// <summary>
    /// Reads a tileset folder in the same format produced by the local Python
    /// test script (tileset.json + one PNG per brick) and builds a
    /// HerringboneTilesetAsset from it, auto-slicing each brick's PNG into
    /// per-cell sprites and Tile assets.
    ///
    /// WORKFLOW:
    ///   1. Copy your tested tileset folder (tileset.json + PNGs) into your
    ///      Unity project, somewhere under Assets/ (e.g. Assets/Herringbone/MyTileset/).
    ///   2. Tools > Herringbone > Import Tileset
    ///   3. Point "Source Folder" at that folder, set "Unity Cell Pixel Size"
    ///      to match your art's pixels-per-tile-cell (e.g. 64 for 64x64 ppu),
    ///      choose an output path, click Import.
    /// </summary>
    public class HerringboneTilesetImporterWindow : EditorWindow
    {
        private DefaultAsset sourceFolder;
        private int unityCellPixelSize = 64;
        private string outputPath = "Assets/Herringbone/GeneratedTileset.asset";

        [MenuItem("Tools/Herringbone/Import Tileset")]
        private static void Open()
        {
            GetWindow<HerringboneTilesetImporterWindow>("Herringbone Import");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Source Folder must contain tileset.json plus the tile PNGs it references, " +
                "in the same format the local Python test script produces. It must be inside Assets/.",
                MessageType.Info);

            sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Source Folder", sourceFolder, typeof(DefaultAsset), false);

            unityCellPixelSize = EditorGUILayout.IntField("Unity Cell Pixel Size", unityCellPixelSize);
            outputPath = EditorGUILayout.TextField("Output Asset Path", outputPath);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(sourceFolder == null))
            {
                if (GUILayout.Button("Import"))
                {
                    string folderPath = AssetDatabase.GetAssetPath(sourceFolder);
                    try
                    {
                        Import(folderPath, unityCellPixelSize, outputPath);
                        EditorUtility.DisplayDialog("Herringbone Import", "Import complete.", "OK");
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        EditorUtility.DisplayDialog("Herringbone Import", "Import failed:\n" + e.Message, "OK");
                    }
                }
            }
        }

        // ---------------------------------------------------------------
        // JSON schema mirrors the Python script's tileset.json exactly.
        // ---------------------------------------------------------------
        [Serializable]
        private class JsonTileEntry
        {
            public string orientation; // "h" or "v"
            public string file;
            public int[] edges;        // length 6: a,b,c,d,e,f
        }

        [Serializable]
        private class JsonTileset
        {
            public int short_side_len;
            public List<JsonTileEntry> tiles;
        }

        // If your imported art appears vertically mirrored relative to the
        // source PNGs, flip this constant.
        private const bool FlipRowOrder = true;

        public static void Import(string folderPath, int cellPixelSize, string outputAssetPath)
        {
            if (cellPixelSize <= 0) throw new ArgumentException("Unity Cell Pixel Size must be > 0");

            string manifestPath = Path.Combine(folderPath, "tileset.json");
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("No tileset.json found in " + folderPath);

            var manifest = JsonUtility.FromJson<JsonTileset>(File.ReadAllText(manifestPath));
            if (manifest.short_side_len % cellPixelSize != 0)
                Debug.LogWarning($"short_side_len ({manifest.short_side_len}) is not evenly divisible by " +
                                  $"Unity Cell Pixel Size ({cellPixelSize}) — tiles may be sliced unevenly.");

            int cellsPerSide = Mathf.Max(1, manifest.short_side_len / cellPixelSize);

            // Create (or replace) the output asset up front so we can attach
            // generated Sprite/Tile sub-assets to it as we go.
            var tilesetAsset = AssetDatabase.LoadAssetAtPath<HerringboneTilesetAsset>(outputAssetPath);
            if (tilesetAsset != null)
            {
                // Clear old sub-assets (previous Tiles) before regenerating.
                foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(outputAssetPath))
                    if (sub is Tile) UnityEngine.Object.DestroyImmediate(sub, true);
                tilesetAsset.hBricks.Clear();
                tilesetAsset.vBricks.Clear();
            }
            else
            {
                tilesetAsset = ScriptableObject.CreateInstance<HerringboneTilesetAsset>();
                Directory.CreateDirectory(Path.GetDirectoryName(outputAssetPath));
                AssetDatabase.CreateAsset(tilesetAsset, outputAssetPath);
            }
            tilesetAsset.shortSideLenCells = cellsPerSide;

            int done = 0;
            foreach (var entry in manifest.tiles)
            {
                EditorUtility.DisplayProgressBar("Importing herringbone tileset",
                    entry.file, (float)done / manifest.tiles.Count);

                string texPath = Path.Combine(folderPath, entry.file).Replace("\\", "/");
                var brick = ImportOneBrick(texPath, entry, cellPixelSize, tilesetAsset, outputAssetPath);

                if (entry.orientation == "h") tilesetAsset.hBricks.Add(brick);
                else if (entry.orientation == "v") tilesetAsset.vBricks.Add(brick);
                else Debug.LogWarning($"Unknown orientation '{entry.orientation}' for {entry.file}, skipping.");

                done++;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(tilesetAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Herringbone import complete: {tilesetAsset.hBricks.Count} horizontal + " +
                      $"{tilesetAsset.vBricks.Count} vertical bricks -> {outputAssetPath}");
        }

        private static HerringboneBrickData ImportOneBrick(
            string texPath, JsonTileEntry entry, int cellPixelSize,
            HerringboneTilesetAsset container, string containerPath)
        {
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer == null)
                throw new FileNotFoundException($"Could not find/import texture at {texPath}. " +
                    "Make sure the PNG exists and Unity has imported it at least once.");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = cellPixelSize;

            importer.GetSourceTextureWidthAndHeight(out int texW, out int texH);
            int cols = texW / cellPixelSize;
            int rows = texH / cellPixelSize;

            var spriteSheet = new List<SpriteMetaData>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // Unity sprite rects are bottom-left origin; r=0 here means
                    // the TOP row of the source image.
                    float rectY = texH - (r + 1) * cellPixelSize;
                    spriteSheet.Add(new SpriteMetaData
                    {
                        name = $"{entry.file}_{r}_{c}",
                        rect = new Rect(c * cellPixelSize, rectY, cellPixelSize, cellPixelSize),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                    });
                }
            }
#pragma warning disable 0618 // spritesheet API is marked obsolete in newer Unity but still functional
            importer.spritesheet = spriteSheet.ToArray();
#pragma warning restore 0618
            importer.SaveAndReimport();

            var sprites = new Dictionary<string, Sprite>();
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(texPath))
                if (obj is Sprite spr) sprites[spr.name] = spr;

            var brick = new HerringboneBrickData
            {
                sourceName = entry.file,
                orientation = entry.orientation == "h" ? BrickOrientation.Horizontal : BrickOrientation.Vertical,
                edgeA = (sbyte)entry.edges[0], edgeB = (sbyte)entry.edges[1], edgeC = (sbyte)entry.edges[2],
                edgeD = (sbyte)entry.edges[3], edgeE = (sbyte)entry.edges[4], edgeF = (sbyte)entry.edges[5],
                widthCells = cols,
                heightCells = rows,
            };
            brick.flatTiles = new List<TileBase>(new TileBase[cols * rows]);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    string spriteName = $"{entry.file}_{r}_{c}";
                    if (!sprites.TryGetValue(spriteName, out var sprite))
                    {
                        Debug.LogWarning($"Missing sliced sprite '{spriteName}' for {entry.file}");
                        continue;
                    }

                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.name = spriteName + "_tile";
                    AssetDatabase.AddObjectToAsset(tile, containerPath);

                    int cellY = FlipRowOrder ? (rows - 1 - r) : r;
                    brick.flatTiles[cellY * cols + c] = tile;
                }
            }

            return brick;
        }
    }
}
