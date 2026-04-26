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
using static CreativeMode.ManageFiles;

[assembly: MelonAdditionalDependencies("ChatCommands")]
[assembly: MelonInfo(typeof(Main), "CreativeMode", "1.0", "Bee & Spike")]

namespace CreativeMode;

public class Main : MelonMod
{
    public static Main? Instance { get; private set; }
    public MapLoader? BrushManager;
    public MapLoader? MapLoader;

    public override void OnInitializeMelon()
    {
        var chatCommands = RegisteredMelons.OfType<ChatCommands.Main>().FirstOrDefault();
        MapLoader = new MapLoader();
        Instance = this;

        //Stop Cheating by preventing matchmaking and party joining
        Il2CppSystem.Action? _onMatchmakingStarted;
        _onMatchmakingStarted = DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(NoQueuing.OnMatchmakingStarted);
        MatchmakingManager.add_OnStartMatchmaking(_onMatchmakingStarted);

        Il2CppSystem.Action? _onJoinedParty;
        _onJoinedParty = DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(NoQueuing.OnJoinedParty);
        MatchmakingPartyManager.add_OnJoinPartyLobby(_onJoinedParty);


        if (chatCommands == null)
        {
            MelonLogger.Warning("ChatCommands could not be found.");
            return;
        }
        
        chatCommands.RegisterCommand("Costume", new Costume());
        chatCommands.RegisterCommand("Warp", new Warp());
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            //Only make folder when the game loads so you can display a message if the folders are missing and not just create them without the user knowing.
            CreateFoldersIfNeeded();
        }
    }
    public override void OnUpdate()
    {
        MapLoader?.OnUpdate();
    }
}