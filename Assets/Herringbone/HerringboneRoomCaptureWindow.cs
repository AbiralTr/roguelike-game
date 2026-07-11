// Must live in a folder named "Editor" (e.g. Assets/Herringbone/Editor/).

using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Herringbone.EditorTools
{
    /// <summary>
    /// Paint your room by hand in a scratch Tilemap somewhere in your scene
    /// (a boss room, gauntlet arena, whatever), then use this window to
    /// capture that painted region into a RoomAsset — no manual array
    /// entry required.
    ///
    /// WORKFLOW:
    ///   1. Paint your room on any Tilemap using the normal Tile Palette.
    ///   2. Tools > Herringbone > Capture Room From Tilemap
    ///   3. Assign the Tilemap, click "Use Painted Bounds" (or set the
    ///      rectangle manually), name the output asset, click Capture.
    /// </summary>
    public class HerringboneRoomCaptureWindow : EditorWindow
    {
        private Tilemap sourceTilemap;
        private int originX, originY, width, height;
        private string outputPath = "Assets/Herringbone/Rooms/NewRoom.asset";

        [MenuItem("Tools/Herringbone/Capture Room From Tilemap")]
        private static void Open()
        {
            GetWindow<HerringboneRoomCaptureWindow>("Capture Room");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Paint your room on a Tilemap, then capture a rectangular region of it into a RoomAsset. " +
                "Coordinates are in Tilemap cell space.", MessageType.Info);

            sourceTilemap = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", sourceTilemap, typeof(Tilemap), true);

            using (new EditorGUI.DisabledScope(sourceTilemap == null))
            {
                if (GUILayout.Button("Use Tilemap's Painted Bounds"))
                {
                    sourceTilemap.CompressBounds();
                    var b = sourceTilemap.cellBounds;
                    originX = b.xMin;
                    originY = b.yMin;
                    width = b.size.x;
                    height = b.size.y;
                }
            }

            EditorGUILayout.Space();
            originX = EditorGUILayout.IntField("Origin X", originX);
            originY = EditorGUILayout.IntField("Origin Y", originY);
            width = EditorGUILayout.IntField("Width (cells)", width);
            height = EditorGUILayout.IntField("Height (cells)", height);

            EditorGUILayout.Space();
            outputPath = EditorGUILayout.TextField("Output Asset Path", outputPath);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(sourceTilemap == null || width <= 0 || height <= 0))
            {
                if (GUILayout.Button("Capture"))
                {
                    Capture();
                }
            }
        }

        private void Capture()
        {
            var room = AssetDatabase.LoadAssetAtPath<RoomAsset>(outputPath);
            bool isNew = room == null;
            if (isNew)
            {
                room = ScriptableObject.CreateInstance<RoomAsset>();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));
            }

            room.widthCells = width;
            room.heightCells = height;
            room.flatTiles.Clear();
            room.cellMarkers.Clear();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = new Vector3Int(originX + x, originY + y, 0);
                    var tile = sourceTilemap.GetTile(cell);
                    room.flatTiles.Add(tile);
                    room.cellMarkers.Add(""); // no marker support from capture yet — edit by hand if needed
                }
            }

            if (isNew)
                AssetDatabase.CreateAsset(room, outputPath);
            else
                EditorUtility.SetDirty(room);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Captured {width}x{height} room from {sourceTilemap.name} -> {outputPath}");
            EditorUtility.DisplayDialog("Herringbone", $"Captured room to {outputPath}", "OK");
        }
    }
}
