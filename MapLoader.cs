#nullable enable
using Il2Cpp;
using Il2CppSystem.IO;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Helpers.BlockPlacer;
using static CreativeMode.SpecialBlocks.SpecialBlocks;
using static CreativeMode.ManageFiles;
using System.Linq;

public class MapLoader
{
    // Array of arrays for positions (x, y, z)
    public static float[][] BlockPositions = new float[0][];
    // Parallel array for block paths
    public static string[] BlockPaths = new string[0];
    public static bool[][] Faces = new bool[0][];
    public static string[] Properties = new string[0];

    public bool toggle = false;

    bool wasCommenced = false;
    MatchDataManager? matchDataManager;

    public static System.Collections.Generic.Dictionary<string, System.Action<Vector3,string>> SpecialBlocks = new System.Collections.Generic.Dictionary<string, System.Action<Vector3, string>>()
    {
        //"sticky_piston.png", "piston.png"
        { "dark_oak_button.png", (pos, Properties) => ChangeSpawn(pos,Properties,(TeamType)2)},
        { "oak_button.png", (pos, Properties) => ChangeSpawn(pos,Properties,0)},
        { "snow.png", (pos, Properties) => SetBunny(pos, Properties) },
        { "sticky_piston.png", (pos,Properties) => SpawnGoal(pos,Properties,TeamType.Red) },
        { "piston.png", (pos,Properties) => SpawnGoal(pos,Properties,TeamType.Blue) }
    };


    public void OnUpdate()
    {
        if (MapSelectionController.Instance != null)
        {
            MapSelectionController.Instance.SelectMap("egypt");
        }

        if (matchDataManager == null || matchDataManager.ActiveMatch == null)
        {
            matchDataManager = MatchDataManager.Instance;
            return;
        }

        if (wasCommenced != matchDataManager.ActiveMatch.wasCommenced && !wasCommenced) //Match just loaded
        {
            string pos = "2000,1,2000";
            LoadMapFromFile("map", pos);
        }
        wasCommenced = matchDataManager.ActiveMatch.wasCommenced;
    }


    public static void LoadMapFromFile(string Mapname, string Position)
    {
        string[] parts = Position.Split(',');
        float.TryParse(parts[0], out float x);
        float.TryParse(parts[1], out float y);
        float.TryParse(parts[2], out float z);
        Vector3 pos = new(x*5, y*5, z*5);

        foreach (var key in s_materialCache.Keys.Where(k => s_materialCache[k] == null).ToList()) s_materialCache.Remove(key);
        MelonLogger.Msg($"Loading map: {Mapname} at position X: {pos.x}, Y: {pos.y}, Z: {pos.z}");

        string filePath = MapFolder() + Mapname + ".json";
        if (!File.Exists(filePath))
        {
            SendChatMessage("There was a error loading the map check console..");
            MelonLogger.Error("Map file not found: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        // Deserialize as object with two arrays
        var data = JsonConvert.DeserializeObject<MapData>(json);
        if (data != null)
        {
            BlockPositions = data.positions;
            BlockPaths = data.paths;
            Faces = data.Faces;
            Properties = data.Properties;
        }

        for (int i = 0; i < BlockPositions.Length; i++)
        {
            Vector3 Pos = new Vector3(
                BlockPositions[i][0] * 5 + pos.x,
                BlockPositions[i][1] * 5 + pos.y,
                BlockPositions[i][2] * 5 + pos.z);

            if (SpecialBlocks.TryGetValue(BlockPaths[i], out System.Action<Vector3, string> action))
            {
                action(Pos, Properties[i]);
                continue;
            }

            PlaceBlock(BlockPaths[i], 5, Pos, Faces[i][0], Faces[i][1], Faces[i][2], Properties[i]);
            MelonLogger.Msg($"X: {BlockPositions[i][0]}, Y: {BlockPositions[i][1]}, Z: {BlockPositions[i][2]}, Block: {BlockPaths[i]}");
        }
        respawnall();
        OffsetBunny();
    }



    // Helper class for JSON serialization
    private class MapData
    {
        public float[][] positions = new float[0][];
        public string[] paths = new string[0];
        public bool[][] Faces = new bool[0][];
        public string[] Properties = new string[0];
    }
}