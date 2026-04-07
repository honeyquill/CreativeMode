using ChatCommands;
using static CreativeMode.Helpers.BeetleUtils;
    
namespace CreativeMode.Commands;

public class Ping : ChatCommand
{
    public Ping()
        : base("ping", "Replies with Pong!", ExecutePing)
    {
    }
        
    private static void ExecutePing(string[] args)
    {
        SendChatMessage("Pong!");
    }
}