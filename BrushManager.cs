#nullable enable
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.IO;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Helpers.BlockPlacer;
using static UnityEngine.GraphicsBuffer;
using System;

public class BrushManager
{
    // Array of arrays for positions (x, y, z)
    public static float[][] BlockPositions = new float[0][];
    // Parallel array for block paths
    public static string[] BlockPaths = new string[0];

    public bool toggle = false;
    public string blockPath = "stone-bricks";

    public void BrushOnUpdate()
    {
        if (!toggle) return;

        GetLocalBeetle().ModifiersController.AddModifierLocal(ModifierType.ElectricAura, 10000, 0);

        if (GetLocalBeetle().ClassData.BeetleType != BeetleType.Cyborg)
        {
            SendChatMessage("Brush can only be used on cyborg, deactivating.");
            toggle = false;
        }

        if (GetLocalBeetle()._abilityChargingNormal.ChargeLerp == 0)
        {
            Vector3 placePos = Vector3.MoveTowards(GetLaserPos(), GetLocalBeetle().transform.position, 2.5f);

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

            Teleport(GetLocalBeetle().OwnerClientId);
            GetLocalBeetle()._abilityChargingNormal.SetChargeLerp(1);
        }
    }

    public static void LoadMapFromFile()
    {
        string filePath = Path.Combine(MelonEnvironment.ModsDirectory, "Map.json");
        if (!File.Exists(filePath))
        {
            SendChatMessage("Map file not found.");
            return;
        }

        string json = File.ReadAllText(filePath);

        // Deserialize as object with two arrays
        var data = JsonConvert.DeserializeObject<MapData>(json);
        if (data != null)
        {
            BlockPositions = data.positions;
            BlockPaths = data.paths;
        }

        // Access example
        for (int i = 0; i < BlockPositions.Length; i++)
        {

            PlaceBlock(BlockPaths[i], 5, new Vector3(
                BlockPositions[i][0] * 5,
                BlockPositions[i][1] * 5,
                BlockPositions[i][2] * 5
            ));
            MelonLogger.Msg($"X: {BlockPositions[i][0]}, Y: {BlockPositions[i][1]}, Z: {BlockPositions[i][2]}, Block: {BlockPaths[i]}");
        }
    }

    public static void WriteMapTofile()
    {
        MapData data = new MapData()
        {
            positions = BlockPositions,
            paths = BlockPaths
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(Path.Combine(MelonEnvironment.ModsDirectory, "Map.json"), json);
    }

    public void BrushActivate()
    {
        if (GetLocalBeetle().ClassData.BeetleType != BeetleType.Cyborg)
        {
            SendChatMessage("Brush can only be used on cyborg");
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

    private Vector3 GetLaserPos()
    {
        return GetLocalCyborg()!._laserEndPoint;
    }

    private BeetleClass_Cyborg? GetLocalCyborg()
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
    }
}