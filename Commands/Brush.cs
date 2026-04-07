using ChatCommands;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Main;

namespace CreativeMode.Commands;

public class Brush : ChatCommand
{
    public Brush()
        :base("brush", "toggle the brush feature", ExecuteBrush)
    {
    }

    private static void ExecuteBrush(string[] args)
    {
        var brushManager = Main.Instance?.BrushManager;
        if (brushManager != null && brushManager.toggle)
        {
            brushManager.BrushDeactivate();
        }
        else
        {
            brushManager?.BrushActivate();
        }
        
    }
    
}