using ChatCommands;
using System;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using static CreativeMode.Helpers.BeetleUtils;
    
namespace CreativeMode.Commands;

public class Costume : ChatCommand
{
    private static BunnyPathJumper costume;
    private static BeetleActor localBeetle = GetLocalBeetle();
    
    public Costume()
        : base("Costume", "Change your player model", ExecuteCostume, 2)
    {
    }

    private static void ExecuteCostume(string[] args, string playername)
    {
        if (args.Length != 2)
        {
            SendChatMessage("2 arg are required. on/off, id");
            return;
        }
        
        var bunnyPrefab = UnityEngine.Object.FindAnyObjectByType<BunnySpawner>().bunnyPrefab;
        var arg1 = (ulong)int.Parse(args[1]);
        
        switch (args[0])
        {
            case "on":
                costume = UnityEngine.Object.Instantiate(bunnyPrefab, GetActorByID(arg1).transform);
                Vector3 euler = costume.transform.localEulerAngles;
                euler.x = 0;
                euler.z = 0;
                euler.y += 180;
                costume.transform.localEulerAngles = euler;
                costume.transform.localScale = new Vector3(5f, 5f, 5f);
                Vector3 pos = costume.transform.localPosition;
                pos.y = -0.6f;
                costume.transform.localPosition = pos;
                
                GetActorByID(arg1).transform.Find("Model").Find("Beetle_Cyborg_V0").Find("GEO").gameObject.SetActive(false);
                GetActorByID(arg1).transform.Find("Model").Find("Beetle_Cyborg_V0").Find("Root").gameObject.SetActive(false);
                break;
            case "off":
                for (int i = 0; i < GetActorByID(arg1).transform.childCount; i++)
                {
                    if (GetActorByID(arg1).transform.GetChild(i).name == "Bunny(clone)")
                    {
                        UnityEngine.Object.Destroy(GetActorByID(arg1).transform.GetChild(i));
                    }
                }
                
                break;
            case "default":
                SendChatMessage("wrong arg. on/off, id");
                break;
        }
    }
}