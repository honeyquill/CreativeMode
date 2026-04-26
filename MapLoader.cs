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

    public static System.Collections.Generic.Dictionary<string, System.Action<BlockData>> SpecialBlocks = new System.Collections.Generic.Dictionary<string, System.Action<BlockData>>()
    {
        //"sticky_piston.png", "piston.png"
        { "dark_oak_button.png", (BlockData) => ChangeSpawn(BlockData, (TeamType)2)},
        { "oak_button.png", (BlockData) => ChangeSpawn(BlockData, 0)},
        { "snow.png", (BlockData) => SetBunny(BlockData) },
        { "sticky_piston.png", (BlockData) => SpawnGoal(BlockData, TeamType.Red) },
        { "piston.png", (BlockData) => SpawnGoal(BlockData, TeamType.Blue) }
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
        try
        {
            string[] parts = Position.Split(',');
            float.TryParse(parts[0], out float x);
            float.TryParse(parts[1], out float y);
            float.TryParse(parts[2], out float z);
            Vector3 Offset = new(x * 5, y * 5, z * 5);

            s_meshCache.Clear();
            s_materialCache.Clear();

            MelonLogger.Msg($"Loading map: {Mapname} at position X: {Offset.x}, Y: {Offset.y}, Z: {Offset.z}");

            string filePath = Path.Combine(MapFolder(), Mapname + ".json");
            if (!File.Exists(filePath))
            {
                SendChatMessage("There was a error loading the map check console..");
                MelonLogger.Error("Map file not found: " + filePath);
                return;
            }

            string json = File.ReadAllText(filePath);
            // Deserialize as object with two arrays
            MapData? data = JsonConvert.DeserializeObject<MapData>(json);

            if (data == null)
                throw new System.Exception("Failed to deserialize map data because data was null");

            foreach (BlockData block in data.blocks)
            {

                block.position[0] = block.position[0]*5+ Offset.x;
                block.position[1] = block.position[1]*5+ Offset.y;
                block.position[2] = block.position[2]*5+ Offset.z;

                if (SpecialBlocks.TryGetValue(block.path, out System.Action<BlockData> action))
                {
                    action(block);
                    continue;
                }

                PlaceBlock(block);
                MelonLogger.Msg($"Pos: {block.position}, Block: {block.path}");
            }

            foreach (ChestNBT chest in data.tile_entities)
            {
                chest.x = chest.x * 5 + Offset.x;
                chest.y = chest.y * 5 + Offset.y;
                chest.z = chest.z * 5 + Offset.z;
                //SendChatMessage($"Spawning chest at X: {chest.x}, Y: {chest.y}, Z: {chest.z}, item 1 inside is {chest.items[0].itemId}");
            }

            if (IsHost())
                respawnall();
            OffsetBunny();
        }
        catch (System.Exception ex)
        {
            MelonLogger.Error("Error during map load: " + ex.Message);
            SendChatMessage("There was an error loading the map, check console.");

        }
    }

    // Helper class for JSON serialization
    private class MapData
    {
        public BlockData[] blocks = new BlockData[0];
        public ChestNBT[] tile_entities = new ChestNBT[0];
    }
    public class BlockData
    {
        public string path = "";
        public float[] position = new float[3];
        public bool[] faces = new bool[3];
        public string properties = "";
    }
    public class ChestNBT
    {
        public float x, y, z;
        public Item[] items;
    }

    public class Item
    {
        public int slot;
        public string itemId;
        public int count;
        public string name;
    }
}