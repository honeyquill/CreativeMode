using ChatCommands;
using System;
using static CreativeMode.Helpers.BeetleUtils;
    
namespace CreativeMode.Commands;

public class Ping : ChatCommand
{
    public Ping()
        : base("ping", "Replies with Pong!", ExecutePing,0)
    {
    }

    private static void ExecutePing(string[] args, string playername)
    {
        SendChatMessage("Pong!");
    }
}