#nullable enable
using CreativeMode;
using CreativeMode.Commands;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using MelonLoader.Utils;
using System.IO;
using System.Linq;
using Main = CreativeMode.Main;
[assembly: MelonAdditionalDependencies("ChatCommands")]
[assembly: MelonInfo(typeof(Main), "CreativeMode", "1.0", "Bee & Spike")]

namespace CreativeMode;

public class Main : MelonMod
{
    public static Main? Instance { get; private set; }
    public BrushManager? BrushManager;
    public MapLoader? MapLoader;

    public string warpsPath = Path.Combine(MelonEnvironment.ModsDirectory, "Warps");

    public override void OnInitializeMelon()
    {
        Il2CppSystem.Action _onMatchmakingStarted;
        Il2CppSystem.Action _onJoinedParty;
        var chatCommands = MelonMod.RegisteredMelons.OfType<ChatCommands.Main>().FirstOrDefault();
        BrushManager = new BrushManager();
        MapLoader = new MapLoader();
        Instance = this;

        _onMatchmakingStarted = DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(NoQueuing.OnMatchmakingStarted);
        MatchmakingManager.add_OnStartMatchmaking(_onMatchmakingStarted);
        _onJoinedParty = DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(NoQueuing.OnJoinedParty);
        MatchmakingPartyManager.add_OnJoinPartyLobby(_onJoinedParty);

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
        chatCommands.RegisterCommand("Warp", new Warp());
        Directory.CreateDirectory(warpsPath);

    }

    public override void OnUpdate()
    {
        MapLoader?.OnUpdate();
        BrushManager?.BrushOnUpdate();
    }

}