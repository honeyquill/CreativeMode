using ChatCommands;
using System;
using Il2Cpp;
using UnityEngine;
using static CreativeMode.Helpers.BeetleUtils;
    
namespace CreativeMode.Commands;

public class Costume : ChatCommand
{
    private static BunnyPathJumper costume;
    private static BeetleActor localBeetle = GetLocalBeetle();
    
    public Costume()
        : base("Costume", "Change your player model", ExecuteCostume, 1)
    {
    }

    private static void ExecuteCostume(string[] args, string playername)
    {
        if (args.Length != 1)
        {
            SendChatMessage("1 arg are required. on/off");
            return;
        }
        
        var bunnyPrefab = UnityEngine.Object.FindAnyObjectByType<BunnySpawner>().bunnyPrefab;
        
        switch (args[0])
        {
            case "on":
                costume = UnityEngine.Object.Instantiate(bunnyPrefab, GetLocalBeetle().transform);
                costume.transform.eulerAngles += new Vector3(0, 180, 0);
                break;
            case "off":
                UnityEngine.Object.Destroy(costume);
                break;
        }
    }
}