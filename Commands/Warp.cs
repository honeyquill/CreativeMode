using ChatCommands;
using System;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Main;
using Newtonsoft.Json;
using System.IO;
using MelonLoader.Utils;
using CreativeMode.Helpers;
using Harmony;
using Unity.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using MelonLoader;
using static CreativeMode.ManageFiles;

namespace CreativeMode.Commands;

public class Warp : ChatCommand
{
    public static string warpsPath = WarpsFolder();

    public Warp()
        : base("warp", "warps you to a location| save, delete, list, [warp name]", ExecuteWarp, 1)
    {
    }

    private static void ExecuteWarp(string[] args, string playername)
    {
        switch (args[0].ToLower())
        {
            case "save":
                if (args.Length != 2)
                {
                    SendChatMessage("Usage: !warp save <name>");
                    return;
                }

                SaveWarp(args[1], GetActorByName(playername).transform.position);
                break;
            case "delete":
                deleteWarp(args[1]);
                break;
            case "list":
                ListWarps();
                break;
            default:
                WarpPlayer(args[0], playername);
                break;
        }
    }

    public static void SaveWarp(string warpName, Vector3 Position)
    {
        WarpData[] warpList = fetchWarps();

        // check duplicates
        if (warpList.Any(w => string.Equals(w.Name, warpName, StringComparison.OrdinalIgnoreCase)))
        {
            SendChatMessage($"A warp with the name '{warpName}' already exists.");
            return;
        }

        //Real function begins here
        var newWarp = new WarpData
        {
            Name = warpName,
            Position = new float[] { Position.x, Position.y, Position.z }
        };
        var package = JsonConvert.SerializeObject(newWarp, Formatting.Indented);
        string WarpPath = Path.Combine(warpsPath, newWarp.Name + ".json");
        File.WriteAllText(WarpPath, package);

        SendChatMessage($"Warp '{warpName}' saved successfully.");
    }

    public static void deleteWarp(string warpToDelete)
    {
        string WarpPath = Path.Combine(warpsPath ,warpToDelete + ".json");

        if (!File.Exists(WarpPath))
        {
            SendChatMessage($"Warp {warpToDelete} not found please double check the name");
            return;
        }

        File.Delete(WarpPath);
    }

    private static void WarpPlayer(string location, string playerName)
    {
        WarpData[] warpList = fetchWarps();
        
        var warpData = findWarp(warpList, location);
        if (warpData == null) return;

        var actor = GetActorByName(playerName);
        var teleportPosition = new Vector3(warpData.Position[0], warpData.Position[1], warpData.Position[2]);

        Teleport(actor.OwnerClientId, teleportPosition, actor.transform.rotation);
    }

    public static void ListWarps()
    {
        Directory.CreateDirectory(warpsPath);
        WarpData[] warpList = fetchWarps();


        if (warpList.Length == 0)
        {
            SendChatMessage("No warps found.");
            return;
        }

        foreach (var warp in warpList)
        {
            SendChatMessage($"Warp: {warp.Name}");
        }
    }

    private static WarpData[] fetchWarps()
    {
        var warps = new List<WarpData>();

        if (Directory.Exists(warpsPath))
        {
            foreach (var file in Directory.GetFiles(warpsPath, "*.json"))
            {
                var json = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(json)) continue;

                try
                {
                    var warp = JsonConvert.DeserializeObject<WarpData>(json);
                    if (warp != null)
                        warps.Add(warp);
                }
                catch
                {
                    // skip malformed files
                }
            }
        }

        return warps.ToArray();
    }

    private static WarpData findWarp(WarpData[] list, string desiredWarp)
    {
        WarpData targetWarp = null;

        foreach (var warp in list)
        {
            if (string.Equals(desiredWarp, warp.Name, StringComparison.OrdinalIgnoreCase))
            {
                targetWarp = warp;
            }
        }

        if (targetWarp == null)
        {
            MelonLogger.Msg($"No warp found with the name '{desiredWarp}'.");
        }

        return targetWarp;
    }

}