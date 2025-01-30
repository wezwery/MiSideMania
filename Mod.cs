using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace MiSideMania
{
    public class Mod : MelonMod
    {
        private const int SCENE_INDEX = 9;

        private string pathToMapsFolder = null!;

        public override void OnInitializeMelon()
        {
            pathToMapsFolder = MelonAssembly.Location.Replace(@"\Mods\MiSideMania.dll", @"\UserData\ManiaMaps\");
            Directory.CreateDirectory(pathToMapsFolder);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != SCENE_INDEX)
                return;

            var game = Object.FindObjectOfType<Location7_GameDance>(true);
            if (game == null)
            {
                MelonLogger.Error("Location7_GameDance not found!");
                return;
            }
            else
            {
                MelonLogger.Msg("Location7_GameDance found!");
            }

            GameDanceSetup.Setup(pathToMapsFolder, game);
        }
    }
}
