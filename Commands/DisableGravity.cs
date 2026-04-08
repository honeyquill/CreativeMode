using ChatCommands;
using HarmonyLib;
using Il2Cpp;
using System;
using System.Reflection;
using Unity.Netcode;
using static CreativeMode.Helpers.BeetleUtils;
using UnityEngine;
namespace CreativeMode.Commands;

public class DisableGravity : ChatCommand
{

    public static void Init()
    {
        HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.Creative.DisableGoals");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    static bool gravity = true;
    public DisableGravity() : base("Grav", "Disables / Enables Gravity", ChangeGravity, 0)
    {
    }

    private static void ChangeGravity(string[] args, string playername)
    {
        if (args.Length == 0)
        {
            gravity = !gravity;
            if (gravity) SendChatMessage("Gravity is now Enabled"); else SendChatMessage("Gravity is now Disabled");
        }
        else
        {
            switch (args[0])
            {
                case string s when s == "on" || s == "1":
                    gravity = true;
                    SendChatMessage("Gravity is now Enabled");
                    break;
                case string s when s == "off" || s == "0":
                    gravity = false;
                    SendChatMessage("Gravity is now Disabled");
                    break;
                default:
                    SendChatMessage("Usage: Grav [on/off]");
                    break;
            }
        }

    }

    [HarmonyPatch(typeof(BeetleActor), "ApplyGravity")]
    class BeetleActor_ApplyGravity_Patch
    {
        static bool Prefix(Vector3 direction)
        {
            var Matchmanager = UnityEngine.Object.FindObjectsOfType<Il2Cpp.MatchDataManager>()[0];
            if (Matchmanager.ActiveMatch.isRanked) return true;

            return gravity;
        }
    }
}
