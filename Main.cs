#nullable enable
using System.Linq;
using CreativeMode.Commands;
using MelonLoader;
using Main = CreativeMode.Main;
[assembly: MelonAdditionalDependencies("ChatCommands")]
[assembly: MelonInfo(typeof(Main), "CreativeMode", "1.0", "Bee & Spike")]

namespace CreativeMode;

public class Main : MelonMod
{
    public static Main? Instance { get; private set; }
    public BrushManager? BrushManager;

    public override void OnInitializeMelon()
    {
        var chatCommands = MelonMod.RegisteredMelons.OfType<ChatCommands.Main>().FirstOrDefault();
        BrushManager = new BrushManager();
        Instance = this;
        
        if (chatCommands == null)
        {
            MelonLogger.Warning("ChatCommands could not be found.");
            return;
        }
        
        chatCommands.RegisterCommand("Ping", new Ping());
        chatCommands.RegisterCommand("brush", new Brush());
        chatCommands.RegisterCommand("Costume", new Costume());
        chatCommands.RegisterCommand("grav", new DisableGravity());
        chatCommands.RegisterCommand("save", new Save());
        chatCommands.RegisterCommand("load", new Load());


    }

    public override void OnUpdate()
    {
        BrushManager?.BrushOnUpdate();
    }

}