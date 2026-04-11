using Il2Cpp;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CreativeMode.Helpers;

public static class BeetleUtils
{
    public static void respawnall()
    {
        var prefabSpawner = UnityEngine.Object.FindObjectsOfType<Il2Cpp.NetworkPrefabSpawner>()[0];
        foreach (var beetle in GetAllBeetles())
        {
            prefabSpawner.SpawnClassAndSetTeam(beetle.OwnerClientId, beetle.Team, (int)beetle.ClassData.BeetleType);
        }
    }

    public static void Teleport(ulong beetleID, Vector3 Pos, Quaternion rotation)
    {
        BeetleActor beetleActor = GetActorByID(beetleID);
        var MapInitializer = UnityEngine.Object.FindObjectsOfType<Il2Cpp.MapInitializer>()[0];
        var prefabSpawner = UnityEngine.Object.FindObjectsOfType<Il2Cpp.NetworkPrefabSpawner>()[0];

        Vector3 OriginalSpawnPos = MapInitializer.SpawnPositions[0].spawnTransform.position;
        Quaternion OriginalSpawnRotaion = MapInitializer.SpawnPositions[0].spawnTransform.rotation;

        MapInitializer.SpawnPositions[0].spawnTransform.position = Pos;
        MapInitializer.SpawnPositions[0].spawnTransform.rotation = rotation;

        prefabSpawner.SpawnClassAndSetTeam(beetleID, TeamType.Blue, (int)beetleActor.ClassData.BeetleType);
        GetActorByID(beetleID)._team.Value = (int)beetleActor.Team;

        MapInitializer.SpawnPositions[0].spawnTransform.position = OriginalSpawnPos;
        MapInitializer.SpawnPositions[0].spawnTransform.rotation = OriginalSpawnRotaion;
    }
    public static BeetleActor GetActorByID(ulong id)
    {
        foreach (var beetle in GetAllBeetles())
        {
            if (beetle.OwnerClientId == id)
            {
                return beetle;
            }
        }
        return GetLocalBeetle();
    }
    public static BeetleActor GetActorByName(string name)
    {
        foreach (var beetle in GetAllBeetles())
        {
            if (GetPlayerName(beetle) == name)
            {
                return beetle;
            }
        }
        return GetLocalBeetle();
    }
    public static string GetMapName()
    {
        try
        {
            var mapInitializer = UnityEngine.Object.FindObjectsOfType<Il2Cpp.MapInitializer>()[0];
            return mapInitializer.mapData.ToString().Replace("MapData_", "").Replace(" (MapDataSO)", "");
        }
        catch
        {
            return null;
        }

    }

    public static bool Pressed(Key key)
    {
        return Keyboard.current[key].wasPressedThisFrame;
    }

    public static string ModFolder()
    {
        var gameData = UnityEngine.Application.dataPath;
        // parent folder = game install folder
        var gameRoot = Directory.GetParent(gameData)?.FullName;
        if (gameRoot == null) return null;
        var modsFolder = Path.Combine(gameRoot, "Mods");
        return modsFolder;
    }

    public static BeetleActor[] GetAllBeetles()
    {
        return UnityEngine.Object.FindObjectsOfType<Il2Cpp.BeetleActor>();
    }

    public static BeetleActor GetLocalBeetle()
    {
        if (NetworkActor.LocalActor is BeetleActor beetle)
        {
            return beetle;
        }
        return null;
    }

    public static bool IsHost()
    {
        if (GetLocalBeetle() == null) return false;
        return GetLocalBeetle().IsHost;
    }

    public static void ApplyModifer(ModifierType modifier, DungBall dungBall, float duration)
    {
        dungBall.ModifiersController.AddModifierRpcDispatcher(modifier, duration);
    }

    public static List<(string player, string message)> GetChatHistory()
    {
        var chatlog = UnityEngine.Object.FindObjectOfType<Il2Cpp.ChatLog>();
        if (chatlog == null) return null;
        var input = chatlog.text.text;

        var result = new List<(string player, string message)>();
        var matches = Regex.Matches(input, @"<b><color=#.*?>(.*?)<\/color><\/b>:\s*<color=#.*>(.*)<\/color>");

        foreach (Match match in matches)
        {
            string player = match.Groups[1].Value;
            string message = match.Groups[2].Value;
            result.Add((player, message));
        }
        return result;
    }

    public static void Score(Il2Cpp.TeamType team)
    {
        BeetleActor[] allBeetles = BeetleUtils.GetAllBeetles();
        Goal[] goals = UnityEngine.Object.FindObjectsOfType<Goal>();

        foreach (var goal in goals)
        {
            if (goal == null) continue;
            if (goal.OwnerTeam != team)
            {
                goal.BallEnteredGoal_ServerRpc(0);
            }
        }
    }
    public static string GetPlayerName(BeetleActor beetle)
    {
        if (beetle == null) return "Unknown";
        var nametags = PlayerNametagsController.Instance;

        if (nametags != null)
        {
            foreach (var nametag in nametags._activeNametags)
            {
                if (nametag.key == beetle)
                {
                    return nametag.value.nameText.text;
                }
            }
        }

        return "Local Player";
    }
    public static void appyForce(BeetleActor beetle, ForceParams force)
    {
        beetle.AddForce(force); 
    }
    public static string GetBeetletype(BeetleActor beetle)
    {
        var modifiedBeetle = beetle.name
.Replace("(Clone)", "")
.Replace("BeetleActor_", "").Trim().ToLower();
        return modifiedBeetle;
    }

    public static void SendChatMessage(string message)
    {
        var chatPanel = UnityEngine.Object.FindObjectOfType<ChatPanel>();
        if (chatPanel != null)
        {
            message = message.Replace(">", "\\u003E");
            message = message.Replace("<", "\\u003C");
            chatPanel.SendChatMessage(message);
        }
    }
}