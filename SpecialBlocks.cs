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
            Vector3 Dung = bunnyspawner.bunnyPrefab.pooSpawnPosition.position;
            if (path.Success)
            {
                layers = int.Parse(path.Groups[1].Value);
            }

            Vector3 Dungpos = bunnyspawner.bunnyPrefab.pooSpawnPosition.position;
            MelonLogger.Msg("Setting bunny block at: " + pos + " with layers: " + layers);
            bunnyspawner.tracks[layers / 5].GetChild((layers - 1) % 4 + 1).position = pos;
            if ((layers - 1) % 4 + 1 == 1)
            {
                bunnyspawner.tracks[layers / 5].GetChild(1).position = pos + Dung - new Vector3(0, Dung.y, 0);
                bunnyspawner.tracks[layers / 5].GetChild(0).position = pos + Dung - new Vector3(0, Dung.y, 0);
            }
        }

       public static void OffsetBunny()
       {
            var bunnyspawner = UnityEngine.Object.FindObjectOfType<BunnySpawner>();
            float spacing = 12.65f; // sqrt(4²,12²) its a number that worked before
            for (int i = 1; i < bunnyspawner.tracks[0].childCount; i++)
            {
                Vector3 previousPos = bunnyspawner.tracks[0].GetChild(i - 1).position;
                Vector3 currentPos = bunnyspawner.tracks[0].GetChild(i).position;
                Vector3 norm = (currentPos - previousPos).normalized;

                bunnyspawner.tracks[0].GetChild(i).position = currentPos + norm * spacing;
            }
            for (int i = 1; i < bunnyspawner.tracks[1].childCount; i++)
            {
                Vector3 previousPos = bunnyspawner.tracks[1].GetChild(i - 1).position;
                Vector3 currentPos = bunnyspawner.tracks[1].GetChild(i).position;
                Vector3 norm = (currentPos - previousPos).normalized;

                bunnyspawner.tracks[1].GetChild(i).position = currentPos + norm * spacing;
            }
        }
    }
}
