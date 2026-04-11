using ChatCommands;
using Il2Cpp;
using System;
using System.Reflection;
using static BrushManager;
using static CreativeMode.Helpers.BeetleUtils;

namespace CreativeMode.Commands;

public class Load : ChatCommand
{
    public Load() : base("load", "Loads Map from file", ExecuteLoad, 0) { }
    public static void ExecuteLoad(string[] args, string playername)
    {
        LoadMapFromFile(args[0], args[1]);
    }
}