#nullable enable
using Il2Cpp;
using UnityEngine;
using UnityEngine.Playables;
using static CreativeMode.Helpers.BeetleUtils;

namespace CreativeMode;

public class BrushManager
{
    public bool toggle = false;
    
    public void BrushOnUpdate()
    {
        if (!toggle) return;
        var beetleStatsModifyer = UnityEngine.Object.FindObjectsOfType<Il2Cpp.ModifiersController>();
        
        if (GetLocalBeetle().ClassData.BeetleType != BeetleType.Cyborg) 
        { 
            SendChatMessage("Brush can only be used on cyborg, deactivating.");
            toggle = false;
        }

        if (GetLocalBeetle()._abilityChargingNormal.ChargeLerp == 0 && !GetLocalBeetle().ModifiersController.ActiveModifiers.ContainsKey(ModifierType.ElectricAura))
        {
            SendChatMessage("Test " + GetLaserPos().ToString());
            GetLocalBeetle()._abilityChargingNormal.SetChargeLerp(1);
        }
        
    }
    
    public void BrushActivate()
    {
        if (GetLocalBeetle().ClassData.BeetleType != BeetleType.Cyborg)
        {
            SendChatMessage("Brush can only be used on cyborg");
            return;
        }
        SendChatMessage("Brush activated.");
        toggle = true;
    }

    public void BrushDeactivate()
    {
        SendChatMessage("Brush deactivated.");
        toggle = false;
        
    }
    
    private Vector3 GetLaserPos()
    {
        return GetLocalCyborg()!._laserEndPoint;
    }

    private BeetleClass_Cyborg? GetLocalCyborg()
    {
        var allCyborgs = UnityEngine.Object.FindObjectsOfType<Il2Cpp.BeetleClass_Cyborg>();
        if (allCyborgs.Length == 0) { return null; }
        foreach (var cyborg in allCyborgs) 
        {
            if (cyborg.IsLocalPlayer == true) { return cyborg; }
        }
        return null;
    }
}