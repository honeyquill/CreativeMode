using Il2Cpp;
using MelonLoader;
using System;
using System.Diagnostics;
using System.IO;
using static CreativeMode.Helpers.BeetleUtils;
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
                    if (folder == folders[0])
                    {
                        string YoutubeLink = "https://Google.com"; //when the tut gets uploaded add the link here
                        ShowPopUp($"Created Blocks Folder Please go here to download the needed blocks: {YoutubeLink}", PopupManager.Position.Top, 10f);
                        Process.Start(new ProcessStartInfo(YoutubeLink) { UseShellExecute = true }); //Open the link in the user's default browser
                    }
                    Directory.CreateDirectory(dir);

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
