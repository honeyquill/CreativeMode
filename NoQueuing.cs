using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

public class NoQueuing
{

    public static void ShowPopUp()
    {
        var PopUpColor = new Il2CppSystem.Nullable<Color>(Color.white);
        var PopUpFadeTime = new Il2CppSystem.Nullable<float>(1f);
        PopupManager.Instance.ShowSimpleTextPopup("You cannot Queue With The Mod 'CreativeMode' By 'Spike And Bee'", PopupManager.Position.Bottom, 5f, PopUpColor, PopUpFadeTime);
    }

    public static void OnMatchmakingStarted()
    {
        MatchmakingManager.Instance.SetIsMatchmaking(false);
        ShowPopUp();
    }

    public static void OnJoinedParty()
    {
        MatchmakingPartyManager.Instance.LeaveLobby();
        ShowPopUp();
    }
}