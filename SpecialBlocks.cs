using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CreativeMode2.SpecialBlocks
{
    internal class SpecialBlocks
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
    }
}
