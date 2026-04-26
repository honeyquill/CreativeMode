using Il2Cpp;
using Il2CppTMPro;
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
        public static void ChangeSpawn(Vector3 pos,string properties, TeamType team)
        {
            Quaternion LookDir = PropertiesToQuaternium(properties);

            var MapInitializer = UnityEngine.Object.FindObjectsOfType<Il2Cpp.MapInitializer>()[0];
            MapInitializer.SpawnPositions[team].spawnTransform.position = pos;
            MapInitializer.SpawnPositions[team].spawnTransform.rotation = LookDir;
            MelonLogger.Msg("Spawned red block at: " + pos);
        }
         
        public static Quaternion PropertiesToQuaternium(string properties)
        {
            Regex regex = new Regex(@"'facing': (.*?)[},]");
            var match = regex.Match(properties);
            string Dir = "";
            if (match.Success) Dir = match.Groups[1].Value;
             
            float yAngle = 0f; // default
            switch (Dir)
            {
                case "north": yAngle = 180f; break;
                case "east": yAngle = 270f; break;
                case "south": yAngle = 0f; break;
                case "west": yAngle = 90f; break;
            }
            return Quaternion.Euler(0f, yAngle, 0f);
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
            Vector3 offset = new Vector3(0,-5,0);
            Vector3 Dungpos = bunnyspawner.bunnyPrefab.pooSpawnPosition.position;
            MelonLogger.Msg("Setting bunny block at: " + pos + " with layers: " + layers);
            bunnyspawner.tracks[layers / 5].GetChild((layers - 1) % 4 + 1).position = pos+offset;
            
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
                Vector3 norm = (track.GetChild(2).position - track.GetChild(1).position).normalized;
                track.GetChild(0).position -= norm * spacing * 10f + Vector3.up * 100f;

                for (int i = 1; i < track.childCount; i++)
                {
                    Vector3 previousPos = track.GetChild(i - 1).position;
                    Vector3 currentPos = track.GetChild(i).position;
                    norm = (currentPos - previousPos);
                    norm = norm - new Vector3(0,norm.y,0);
                    norm = norm.normalized;

                    track.GetChild(i).position = currentPos + norm * spacing;
                    MelonLogger.Msg(track.GetChild(1).position);
                }

                norm = (track.GetChild(4).position - track.GetChild(3).position).normalized;
                track.GetChild(5).position = track.GetChild(4).position + norm * spacing * 10f + Vector3.down * 100f;
            }
        }

        public static void SpawnGoal(Vector3 pos, string Properties, TeamType Team)
        {
            foreach (var Goal in UnityEngine.Object.FindObjectsOfType<Goal>())
            {
                if (Goal.OwnerTeam == Team)
                {
                    Quaternion LookDir = PropertiesToQuaternium(Properties);

                    Vector3 backward = LookDir * Vector3.back * 2f;
                    Goal.transform.position = pos + Vector3.up * 20f + backward;
                    Goal.transform.rotation = LookDir;
                    Goal.transform.localScale = Goal.transform.localScale * 1.3f;
                }
            }
        }
    }
}
