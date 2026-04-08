#nullable enable
using Il2Cpp;
using UnityEngine;
using UnityEngine.Playables;
using static CreativeMode.Helpers.BeetleUtils;
using static CreativeMode.Helpers.BlockPlacer;
using static UnityEngine.GraphicsBuffer;

namespace CreativeMode;

public class BrushManager
{
    public bool toggle = false;
    public bool deleteMode = false;
    public string blockPath = "stone-bricks";
    public void BrushOnUpdate()
    {
        if (!toggle) return;
        var beetleStatsModifyer = UnityEngine.Object.FindObjectsOfType<Il2Cpp.ModifiersController>();
        GetLocalBeetle().ModifiersController.AddModifierLocal(ModifierType.ElectricAura, 10000, 0);

        if (GetLocalBeetle().ClassData.BeetleType != BeetleType.Cyborg) //doesnt work lol
        {
            SendChatMessage("Brush can only be used on cyborg, deactivating.");
            toggle = false;
        } 

        if (Pressed(UnityEngine.InputSystem.Key.F3))
        {
            ToggleDeleteMode();
        }

        if (GetLocalBeetle()._abilityChargingNormal.ChargeLerp != 0) return;

        if (!deleteMode)
        {
            Vector3 placePos = Vector3.MoveTowards(GetLaserPos(), GetLocalBeetle().transform.position, 2.5f);

            PlaceBlock(blockPath + ".png", 5, placePos);
            Teleport(GetLocalBeetle().OwnerClientId);
            GetLocalBeetle()._abilityChargingNormal.SetChargeLerp(1);
        }
        else
        {
            Vector3 destroyPos = GetLaserPos();

            RemoveBlock(destroyPos);
            Teleport(GetLocalBeetle().OwnerClientId);
            GetLocalBeetle()._abilityChargingNormal.SetChargeLerp(1);
        }
    }
    

    private void ToggleDeleteMode()
    {
        deleteMode = !deleteMode;
        SendChatMessage("Delete mode: " + (deleteMode ? "ON" : "OFF"));
    }

    public void BrushActivate()
    {
        if (GetLocalBeetle().ClassData.BeetleType != BeetleType.Cyborg)
        {
            var networkPrefabSpawner = UnityEngine.Object.FindObjectOfType<Il2Cpp.NetworkPrefabSpawner>();
            networkPrefabSpawner.SpawnClassAndSetTeam(GetLocalBeetle().OwnerClientId, TeamType.Blue, (int)BeetleType.Cyborg);
            return;
        }
        SendChatMessage("Brush activated.");
        toggle = true;
    }

    public void BrushDeactivate()
    {
        SendChatMessage("Brush deactivated.");
        GetLocalBeetle().ModifiersController.RemoveModifierLocal(ModifierType.ElectricAura);
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