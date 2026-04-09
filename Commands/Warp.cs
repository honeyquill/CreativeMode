using ChatCommands;
using System;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Main;
using Newtonsoft.Json;

namespace CreativeMode.Commands;

public class Warp : ChatCommand
{
    public string warpsPath = Path.Combine(MelonEnvironment.ModsDirectory, "Warps");
    private string dataFile = Path.Combine(MelonEnvironment.ModsDirectory, "Warps", "data.json");

    public Warp()
        : base("ping", "Replies with Pong!", ExecuteWarp, 1)
    {
    }

    private static void ExecuteWarp(string[] args, string playername)
    {
        switch (args[0].ToLower())
        {
            case "save":
                SaveWarp();
                break;
            case "delete":
                deleteWarp();
                break;
            case "list":
                ListWarps();
                break;
            default:
                WarpPlayer(args[0]);
                break;
        }
    }

    private static void WarpPlayer(string location)
    {

    }

    public static void SaveWarp()
    {
        if args.Length != 2
        {
            SendChatMessage("Usage: !warp save <name>");
            return;
        }

        var json = JsonConvert.SerializeObject(new { setting = true });
    }

    public static void deleteWarp()
    {

    }

    public static void ListWarps()
    {

    }



}