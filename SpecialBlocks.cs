using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace CreativeMode.SpecialBlocks
{

    public class SpecialBlocks
    {


        public static void SpawnBlue(Vector3 pos)
        {
            var MapInitializer = UnityEngine.Object.FindObjectsOfType<Il2Cpp.MapInitializer>()[0];
            MapInitializer.SpawnPositions[0].spawnTransform.position = pos;
            MelonLogger.Msg("Spawned blue block at: " + pos);
        }
        public static void SpawnRed(Vector3 pos)
        {
            var MapInitializer = UnityEngine.Object.FindObjectsOfType<Il2Cpp.MapInitializer>()[0];
            MapInitializer.SpawnPositions[(TeamType)2].spawnTransform.position = pos;
            MelonLogger.Msg("Spawned red block at: " + pos);
        }

        public static void SetBunny(Vector3 pos, string properties)
        {
            Regex regex = new Regex(@"'layers'\s*:\s*(\d+)");
            var bunnyspawner = UnityEngine.Object.FindObjectOfType<BunnySpawner>();
            var path = regex.Match(properties);
            int layers = -1;
            if (path.Success)
            {
                layers = int.Parse(path.Groups[1].Value);
            }

            MelonLogger.Msg("Setting bunny block at: " + pos + " with layers: " + layers);
            bunnyspawner.tracks[layers / 5].GetChild((layers - 1) % 4 + 1).position = pos ;
            if ((layers - 1) % 4 + 1 == 1)
                bunnyspawner.tracks[layers / 5].GetChild(0).position = pos;
        }
    }
}
