using ChatCommands;
using System;
using static CreativeMode.Helpers.BeetleUtils;
using static BrushManager;

namespace CreativeMode.Commands;

public class Save : ChatCommand
{
    public Save() : base("save", "Saves Map to file", ExecutePing, 0) { }
    private static void ExecutePing(string[] args, string playername)
    {
        WriteMapTofile(args[0]);
    }
}