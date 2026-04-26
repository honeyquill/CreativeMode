using ChatCommands;
using Il2Cpp;
using MelonLoader;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEngine;
using static CreativeMode.Helpers.BeetleUtils;

namespace CreativeMode.Commands;

public class Costume : ChatCommand
{
    private static BunnyPathJumper costume;
    private static bool CostumeToggle = false;
    
    public Costume()
        : base("Costume", "Change your player model", ExecuteCostume, 0,CheckIfParentIsDead)
    {
    }

    private static void ExecuteCostume(string[] args, string playername)
    {
        var bunnyPrefab = UnityEngine.Object.FindAnyObjectByType<BunnySpawner>().bunnyPrefab;
        var Actor = GetActorByName(playername);
        CostumeToggle = !CostumeToggle;

        if (CostumeToggle)
        {
            costume = UnityEngine.Object.Instantiate(bunnyPrefab, Actor.transform);

            costume.transform.localRotation = Quaternion.Euler(0, costume.transform.localEulerAngles.y + 180, 0);
            costume.transform.localScale = Vector3.one * 5f; //Shrinks the bunny down to about player size
            costume.transform.localPosition += Vector3.down * 0.6f;

            Actor.transform.Find("Model").gameObject.SetActive(false); // hide original model
        }
        else
        {
            for (int i = 0; i < Actor.transform.childCount; i++)
            {
                Actor.transform.Find("Model").gameObject.SetActive(true);
                // Cast explicitly through Il2CppObjectBase
                var child = Actor.transform.GetChild(i);
                if (child.name == "Bunny(Clone)")
                    UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }
    private static void CheckIfParentIsDead()
    {
        if (costume == null) return;
        if (costume.transform.parent == null)
        {
            UnityEngine.Object.Destroy(costume.gameObject);
            CostumeToggle = false;
        }

    }
}