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

namespace CreativeMode.Commands;

public class Warp : ChatCommand
{
    public static string warpsPath = Path.Combine(MelonEnvironment.ModsDirectory, "Warps");
    private static string dataFile = Path.Combine(MelonEnvironment.ModsDirectory, "Warps", "data.json");

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
        Directory.CreateDirectory(warpsPath);
        WarpList warpList = fetchWarps();

        var list = warpList.Warps.ToList();

        // check duplicates
        if (list.Any(w => string.Equals(w.Name, warpName, StringComparison.OrdinalIgnoreCase)))
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

        list.Add(newWarp);
        warpList.Warps = list.ToArray();

        var package = JsonConvert.SerializeObject(warpList, Formatting.Indented);
        File.WriteAllText(dataFile, package);

        SendChatMessage($"Warp '{warpName}' saved successfully.");
    }

    public static void deleteWarp(string warpToDelete)
    {
        Directory.CreateDirectory(warpsPath);
        WarpList warpList = fetchWarps();
        WarpData targetWarp = null;

        var list = warpList.Warps.ToList();

        list.Remove(findWarp(list, warpToDelete));
        warpList.Warps = list.ToArray();
        var package = JsonConvert.SerializeObject(warpList, Formatting.Indented);
        File.WriteAllText(dataFile, package);
        SendChatMessage($"Warp '{warpToDelete}' deleted successfully.");
        return;
    }

    private static void WarpPlayer(string location, string playerName)
    {
        WarpList warpList = fetchWarps();
        var list = warpList.Warps.ToList();
        
        var warpData = findWarp(list, location);
        if (warpData == null) return;

        var actor = GetActorByName(playerName);
        var teleportPosition = new Vector3(warpData.Position[0], warpData.Position[1], warpData.Position[2]);

        Teleport(actor.OwnerClientId, teleportPosition, actor.transform.rotation);
    }

    public static void ListWarps()
    {
        Directory.CreateDirectory(warpsPath);
        WarpList warpList = fetchWarps();

        var list = warpList.Warps.ToList();

        if (list.Count == 0)
        {
            SendChatMessage("No warps found.");
            return;
        }

        foreach (var warp in list)
        {
            SendChatMessage($"Warp: {warp.Name}");
        }
    }

    private static WarpList fetchWarps()
    {
        WarpList warpList = null;
        if (File.Exists(dataFile))
        {
            var json = File.ReadAllText(dataFile);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    warpList = JsonConvert.DeserializeObject<WarpList>(json);
                }
                catch
                {
                    warpList = null;
                }
            }
        }

        if (warpList == null || warpList.Warps == null)
            warpList = new WarpList { Warps = new WarpData[0] };

        return warpList;
    }

    private static WarpData findWarp(List<WarpData> list, string desiredWarp)
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