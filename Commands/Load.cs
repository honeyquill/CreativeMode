using ChatCommands;
using System;
using static CreativeMode.Helpers.BeetleUtils;
using static BrushManager;

namespace CreativeMode.Commands;

public class Load : ChatCommand
{
    public Load() : base("load", "Loads Map from file", ExecutePing, 0) { }
    private static void ExecutePing(string[] args, string playername)
    {
        LoadMapFromFile(args[0]);
    }
}