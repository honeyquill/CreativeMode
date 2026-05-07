#nullable enable
using CreativeMode.Helpers;
using Harmony;
using Il2Cpp;
using Il2CppSystem.IO;
using MelonLoader;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Helpers.BlockPlacer;
using static CreativeMode.ManageFiles;
using static CreativeMode.SpecialBlocks.SpecialBlocks;
using static CreativeMode.Helpers.Texture;
public class MapLoader
{
    bool wasCommenced = false;
    MatchDataManager? matchDataManager;
    GameObject? Bunny = null;

    private int VideoIndex = 0;
    private int VideoLength = -1;
    private int VideoFps = 10;

    private float videoTimer = 0f;
    GameObject? VideoMap;
    bool IsInMap = false;

    public static Dictionary<string, System.Action<BlockData>> SpecialBlocks = new Dictionary<string, System.Action<BlockData>>()
    {
        //"sticky_piston.png", "piston.png"
        { "dark_oak_button", (BlockData) => ChangeSpawn(BlockData, (TeamType)2)},
        { "oak_button", (BlockData) => ChangeSpawn(BlockData, 0)},
        { "snow", (BlockData) => SetBunny(BlockData) },
        { "sticky_piston", (BlockData) => SpawnGoal(BlockData, TeamType.Red) },
        { "piston", (BlockData) => SpawnGoal(BlockData, TeamType.Blue) },
        { "dark_oak_pressure_plate", (BlockData) => SetSpawningDung(BlockData,TeamType.Red) },
        { "oak_pressure_plate", (BlockData) => SetSpawningDung(BlockData,TeamType.Blue) },

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
            IsInMap = true;
            s_meshCache.Clear();
            s_materialCache.Clear();

            string pos = "2000,1,2000";
            if(File.Exists(Path.Combine(MapFolder(), "map.json")))
            {
                MapData data = GetMapData("map");
                LoadMapFromFile(data, pos);
            }

            if (File.Exists(Path.Combine(MapFolder(), "bunny.json")))
            {
                pos = "-2000,-2000,-2000";
                MapData data = GetMapData("bunny");
                Bunny = LoadMapFromFile(data, pos);
            }
        }
        wasCommenced = matchDataManager.ActiveMatch.wasCommenced;

        if (IsInMap)
            PlayVideoFrame();
    }

    public static MapData GetMapData(string Mapname)
    {
        string filePath = Path.Combine(MapFolder(), Mapname + ".json");
        if (!File.Exists(filePath))
        {
            SendChatMessage("There was a error loading the map check console..");
            MelonLogger.Error("Map file not found: " + filePath);
            return null!;
        }

        string json = File.ReadAllText(filePath);
        // Deserialize as object with two arrays
        MapData? data = JsonConvert.DeserializeObject<MapData>(json);
        return data!;
    }

    private static GameObject LoadMapFromFile(MapData data, string Position)
    {
        try
        {
            string[] parts = Position.Split(',');
            float.TryParse(parts[0], out float x);
            float.TryParse(parts[1], out float y);
            float.TryParse(parts[2], out float z);
            Vector3 Offset = new(x * 5, y * 5, z * 5);

            GameObject MapParent = new GameObject("Map");

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
                PlaceBlock(block, MapParent);
            }

            foreach (ChestData chest in data.tile_entities)
            {
                chest.x = chest.x * 5 + Offset.x;
                chest.y = chest.y * 5 + Offset.y;
                chest.z = chest.z * 5 + Offset.z;
                SendChatMessage($"Spawning chest at X: {chest.x}, Y: {chest.y}, Z: {chest.z}, item 1 inside is {chest.items[0].Name}");
            }

            if (IsHost())
                respawnall();
            OffsetBunny();
            
            return MapParent;
        }
        catch (System.Exception ex)
        {
            MelonLogger.Error("Error during map load: " + ex.Message);
            SendChatMessage("There was an error loading the map, check console.");
            return null!;
        }
    }

    private void PlayVideoFrame()
    {
        videoTimer += Time.deltaTime;

        float frameDuration = 1f / VideoFps;

        if (videoTimer < frameDuration) return;

        videoTimer -= frameDuration;
        VideoIndex++;

        if (VideoIndex > VideoLength) return;

        Object.Destroy(VideoMap);
        s_meshCache.Clear();
        s_materialCache.Clear();

        MapData data = GetMapData(VideoIndex.ToString());
        string pos = "2000,1,2000";
        VideoMap = LoadMapFromFile(data, pos);
    }

    // Helper class for JSON serialization
    public class MapData
    {
        public BlockData[] blocks = new BlockData[0];
        public ChestData[] tile_entities = new ChestData[0];
    }
    public class BlockData
    {
        public string path = "";
        public float[] position = new float[3];
        public int mask = 0;
        public string properties = "";
    }
    public class ChestData
    {
        public float x, y, z;
        public Item[]? items;
    }

    public class Item
    {
        public int Slot;
        public string? Id;
        public int Count;
        public ItemTag? Tag;  // renamed property type

        Regex regex = new Regex("\"text\":\"(.*?)\"");
        public string Name => regex.Match(Tag?.Display?.Name).Groups[1].Value;
    }

    public class ItemTag  // renamed from Tag to ItemTag
    {
        public Display? Display { get; set; }
    }

    public class Display
    {
        public string? Name { get; set; }
    }
}