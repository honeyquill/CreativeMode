#nullable enable
using Harmony;
using Il2Cpp;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.IO;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Helpers.BlockPlacer;
using static CreativeMode.SpecialBlocks.SpecialBlocks;
using static UnityEngine.GraphicsBuffer;

public class BrushManager
{
    // Array of arrays for positions (x, y, z)
    public static float[][] BlockPositions = new float[0][];
    // Parallel array for block paths
    public static string[] BlockPaths = new string[0];
    public static bool[][] Faces = new bool[0][];
    public static string[] Properties = new string[0];

    public bool toggle = false;
    public string blockPath = "stone-bricks";

    public static System.Collections.Generic.Dictionary<string, System.Action<Vector3,string>> SpecialBlocks = new System.Collections.Generic.Dictionary<string, System.Action<Vector3, string>>()
    {
        { "dark_oak_button.png", (pos, Properties) => SpawnRed(pos) },
        { "oak_button.png", (pos, Properties) => SpawnBlue(pos) },
        { "snow.png", (pos, Properties) => SetBunny(pos, Properties) }
    };

    public void BrushOnUpdate()
    {

        if (!toggle) return;
        var local = GetLocalBeetle();
        local.ModifiersController.AddModifierLocal(ModifierType.ElectricAura, 10000, 0);

        if (local.ClassData.BeetleType != BeetleType.Cyborg) //doesnt work lol
        {
            SendChatMessage("Brush can only be used on cyborg, deactivating.");
            toggle = false;
        } 

        if (local._abilityChargingNormal.ChargeLerp != 0) return;

        Vector3 placePos = Vector3.MoveTowards(GetLaserPos(), local.transform.position, 2.5f);

        PlaceBlock(blockPath + ".png", 5, placePos);

        // Add new entry to the arrays
        float[][] newPositions = new float[BlockPositions.Length + 1][];
        string[] newPaths = new string[BlockPaths.Length + 1];

        for (int i = 0; i < BlockPositions.Length; i++)
        {
            newPositions[i] = BlockPositions[i];
            newPaths[i] = BlockPaths[i];
        }

        // Store position and path
        newPositions[BlockPositions.Length] = new float[]
        {
            Grid(placePos, 5).x / 5,
            Grid(placePos, 5).y / 5,
            Grid(placePos, 5).z / 5
        };
        newPaths[BlockPaths.Length] = blockPath + ".png";

        BlockPositions = newPositions;
        BlockPaths = newPaths;

        Teleport(local.OwnerClientId, local.transform.position, local.transform.rotation);
        local._abilityChargingNormal.SetChargeLerp(1);
        
    }

    public static void LoadMapFromFile(string Mapname, string Position)
    {
        string[] parts = Position.Split(',');
        float.TryParse(parts[0], out float x);
        float.TryParse(parts[1], out float y);
        float.TryParse(parts[2], out float z);
        Vector3 pos = new(x*5, y*5, z*5);
        MelonLogger.Msg($"Loading map: {Mapname} at position X: {pos.x}, Y: {pos.y}, Z: {pos.z}");
        string filePath = Path.Combine(MelonEnvironment.ModsDirectory, "CreativeMode\\Maps\\", Mapname + ".json");
        if (!File.Exists(filePath))
        {
            SendChatMessage(filePath);
            return;
        }

        string json = File.ReadAllText(filePath);

        // Deserialize as object with two arrays
        var data = JsonConvert.DeserializeObject<MapData>(json);
        MelonLogger.Msg(data);
        MelonLogger.Msg(data.Faces[0]);
        if (data != null)
        {
            BlockPositions = data.positions;
            BlockPaths = data.paths;
            Faces = data.Faces;
            Properties = data.Properties;
        }

        // Access example
        for (int i = 0; i < BlockPositions.Length; i++)
        {
            if (SpecialBlocks.TryGetValue(BlockPaths[i], out System.Action<Vector3, string> action))
            {
                action(new Vector3(
                    BlockPositions[i][0] * 5 + pos.x,
                    BlockPositions[i][1] * 5 + pos.y,
                    BlockPositions[i][2] * 5 + pos.z), Properties[i]);
                continue;
            }

            PlaceBlock(BlockPaths[i], 5, new Vector3(
                BlockPositions[i][0] * 5 + pos.x,
                BlockPositions[i][1] * 5 + pos.y,
                BlockPositions[i][2] * 5 + pos.z), Faces[i][0], Faces[i][1], Faces[i][2], Properties[i]
            );
            MelonLogger.Msg($"X: {BlockPositions[i][0]}, Y: {BlockPositions[i][1]}, Z: {BlockPositions[i][2]}, Block: {BlockPaths[i]}");
        }
        respawnall();
        OffsetBunny();
    }

    public static void WriteMapTofile(string name)
    {
        MapData data = new MapData()
        {
            positions = BlockPositions,
            paths = BlockPaths
        };
        string path = Path.Combine(MelonEnvironment.ModsDirectory, "CreativeMode/Maps/", name + ".json");
        MelonLogger.Msg("Saving map to: " + path);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public void BrushActivate()
    {
        if (GetLocalBeetle().ClassData.BeetleType != BeetleType.Cyborg)
        {
            var networkPrefabSpawner = UnityEngine.Object.FindObjectOfType<Il2Cpp.NetworkPrefabSpawner>();
            networkPrefabSpawner.SpawnClassAndSetTeam(GetLocalBeetle().OwnerClientId, TeamType.Blue, (int)BeetleType.Cyborg);
            return;
        }
        SendChatMessage("Brush activated.");
        toggle = true;
    }

    public void BrushDeactivate()
    {
        SendChatMessage("Brush deactivated.");
        GetLocalBeetle().ModifiersController.RemoveModifierLocal(ModifierType.ElectricAura);
        toggle = false;
    }

    public static Vector3 GetLaserPos()
    {
        return GetLocalCyborg()!._laserEndPoint;
    }

    public static BeetleClass_Cyborg? GetLocalCyborg()
    {
        var allCyborgs = UnityEngine.Object.FindObjectsOfType<Il2Cpp.BeetleClass_Cyborg>();
        if (allCyborgs.Length == 0) { return null; }
        foreach (var cyborg in allCyborgs)
        {
            if (cyborg.IsLocalPlayer == true) { return cyborg; }
        }
        return null;
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