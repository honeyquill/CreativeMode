using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using static CreativeMode.Helpers.BeetleUtils;
public class NoQueuing
{
    public static void OnMatchmakingStarted()
    {
        MatchmakingManager.Instance.SetIsMatchmaking(false);
        ShowPopUp("'You Cannot Queue With The Mod 'CreativeMode' by 'Spike and Bee'", PopupManager.Position.Bottom ,5f ,null ,3f);
    }

    public static void OnJoinedParty()
    {
        MatchmakingPartyManager.Instance.LeaveLobby();
        ShowPopUp("'You Cannot Join Parties With The Mod 'CreativeMode' by 'Spike and Bee'", PopupManager.Position.Bottom, 5f, null, 3f);
    }
}