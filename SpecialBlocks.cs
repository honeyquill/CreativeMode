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
                bunnyspawner.tracks[layers / 5].GetChild(0).position = pos + Dung - new Vector3(0, Dung.y, 0);
            }
        }

       public static void OffsetBunny()
       {
            var bunnyspawner = UnityEngine.Object.FindObjectOfType<BunnySpawner>();
            float spacing = 12.65f; // sqrt(4²,12²) its a number that worked before


            foreach(Transform track in bunnyspawner.tracks)
            {
                Vector3 Norm = (track.GetChild(2).position - track.GetChild(1).position).normalized;
                track.GetChild(0).position -= Norm * spacing * 10f + Vector3.up * 100f;

                for (int i = 1; i < track.childCount; i++)
                {
                    Vector3 previousPos = track.GetChild(i - 1).position;
                    Vector3 currentPos = track.GetChild(i).position;
                    Vector3 norm = (currentPos - previousPos).normalized;

                    track.GetChild(i).position = currentPos + norm * spacing;
                    MelonLogger.Msg(track.GetChild(1).position);
                }

                Norm = (track.GetChild(4).position - track.GetChild(3).position).normalized;
                track.GetChild(5).position = track.GetChild(4).position + Norm * spacing * 10f + Vector3.down * 100f;
                track.GetChild(1).position += Vector3.down * 10f;
            }
        }
    }
}
