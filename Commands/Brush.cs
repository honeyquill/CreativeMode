using ChatCommands;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Main;

namespace CreativeMode.Commands;

public class Brush : ChatCommand
{
    public Brush()
        :base("brush", "toggle the brush feature", ExecuteBrush, 1)
    {
    }

    private static void ExecuteBrush(string[] args, string playername)
    {
        var brushManager = Main.Instance?.BrushManager;

        brushManager?.BrushActivate();
        
        brushManager.blockPath = args.Length > 0 ? args[0] : "Missing";
        
        if (brushManager != null && args.Length > 0 && args[0] == "deactivate")
        {
            brushManager.BrushDeactivate();
        }
    }
    
}