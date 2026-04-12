using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CreativeMode.Commands.Load;
using UnityEngine;

namespace CreativeMode
{
    public class MapLoader
    {
        bool wasCommenced = false;
        MatchDataManager matchDataManager;
        BunnyPathJumper bunny = null;
        public void OnUpdate()
        {



            if(matchDataManager == null || matchDataManager.ActiveMatch == null)
            {
                matchDataManager = UnityEngine.Object.FindObjectOfType<MatchDataManager>();
                return;
            }

            if (wasCommenced != matchDataManager.ActiveMatch.wasCommenced && !wasCommenced) //Match just loaded
            {
                string[] arr = { "map", "1000,0,1000" };
                ExecuteLoad(arr,"");
            }
            wasCommenced = matchDataManager.ActiveMatch.wasCommenced;
        }
    }
}
