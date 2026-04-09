using ChatCommands;
using System;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Main;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.IO;
using MelonLoader.Utils;
using CreativeMode.Helpers;
using Harmony;
using Unity.Collections;
using System.Linq;
using HarmonyLib;

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

                SaveWarp(args[1], playername);
                break;
            case "delete":
                deleteWarp(args[1]);
                break;
            case "list":
                ListWarps();
                break;
            default:
                WarpPlayer(args[0]);
                break;
        }
    }

    public static void SaveWarp(string warpName, string PlayerName)
    {
        var actor = GetActorByName(PlayerName);

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
        var pos = actor.transform.position;
        var newWarp = new WarpData
        {
            Name = warpName,
            Position = new float[] { pos.x, pos.y, pos.z }
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

        foreach (var warp in list)
        {
            if (!string.Equals(warpToDelete, warp.Name, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            targetWarp = warp;
        }

        if (targetWarp == null)
        {
            SendChatMessage($"No warp found with the name '{warpToDelete}'.");
            return;
        }

        list.Remove(targetWarp);
        warpList.Warps = list.ToArray();
        var package = JsonConvert.SerializeObject(warpList, Formatting.Indented);
        File.WriteAllText(dataFile, package);
        SendChatMessage($"Warp '{warpToDelete}' deleted successfully.");
        return;
    }


    private static void WarpPlayer(string location)
    {
        string json = File.ReadAllText(dataFile);
        var warpData = JsonConvert.DeserializeObject<WarpData>(json);
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

}