using System;
using static CreativeMode.Helpers.BeetleUtils;
using System.IO;
using Il2Cpp;

namespace CreativeMode
{
    internal class ManageFiles
    {
        static string[] folders = { @"CreativeMode\Blocks", @"CreativeMode\Maps", @"CreativeMode\Warps" };
        public static void CreateFoldersIfNeeded()
        {
            string modfolder = ModFolder();



            foreach (string folder in folders)
            {
                string dir = Path.Combine(modfolder, folder);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Console.WriteLine($"Created: {dir}");
                }
            }
        }

        public static string BlockFolder()
        {
            return Path.Combine(ModFolder(), folders[0]);
        }
        public static string MapFolder()
        {
            return Path.Combine(ModFolder(), folders[1]);
        }
        public static string WarpsFolder()
        {
            return Path.Combine(ModFolder(), folders[2]);
        }
    }
}
