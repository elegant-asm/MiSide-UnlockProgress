using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Linq;
using static Menu;

namespace UnlockProgress;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static ManualLogSource Log;

    internal static Harmony harmony = new Harmony("unlockprogress");

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Harmony.DEBUG = true;

        harmony.PatchAll(typeof(Patch_Menu));
    }

    internal static class Patch_Menu {
		internal static readonly string[] clothes = new [] { "HellVamp", "Chirfns", "FIIdClSchool" };
        internal static readonly string[] flashes = new[] { "plr1a", "plr2v", "plr3vi", "plr4asw", "plr5bz", "plr6vcx", "plr7vczn", "plr8qeqe", "plr9mpa", 
        "plr1099", "mta", "mtashh", "mtakd", "mtacap", "mtal", "mtad", "mtad2", "mla", "mtac", "mtacore", "mtachi", "mtaghost", "mtaman" };

		[HarmonyPatch(typeof(Menu), "Start")]
		[HarmonyPrefix]
		public static void Prefix_Start(Menu __instance) {
			MenuPersonage menuPersonage = __instance.GetComponent<MenuPersonage>();
            GlobalAM.SaveData("Flashes", string.Join("\n", flashes));
            Plugin.Log.LogInfo("All flashes gain!");

			foreach (CaseLoadGame caseLoad in __instance.casesLoad)
				if (!GlobalAM.ExistsData(caseLoad.nameSave)) {
                    GlobalAM.SaveData(caseLoad.nameSave, "");
                    Plugin.Log.LogInfo(string.Format("Location '{0}' gain!", caseLoad.nameScene));
                }
		}

        [HarmonyPatch(typeof(Menu), "Start")]
		[HarmonyPostfix]
		public static void Postfix_Start(Menu __instance) {
			Achievement_cloth clothControl = new();
            if (GlobalAM.ExistsData("Clothes")) {
				string[] ownClothes = GlobalAM.LoadData("Clothes");
                foreach (string cloth in clothes)
					if (!ownClothes.Contains(cloth)) {
						clothControl.ClothQuietCompleted(cloth);
                        Plugin.Log.LogInfo(string.Format("Cloth '{0}' claimed!", cloth));
                    }
            } else
				foreach (string cloth in clothes) {
                    clothControl.ClothQuietCompleted(cloth);
                    Plugin.Log.LogInfo(string.Format("Cloth '{0}' claimed!", cloth));
                }

            AchievementsController achievementsController = GlobalTag.gameOptions.GetComponent<AchievementsController>();
            foreach (DataAchievementsValues achievement in achievementsController.dataAchievements)
				if (!achievement.get) {
                    achievementsController.AchievementCompleted(achievement.steamAchievement);
                    Plugin.Log.LogInfo(string.Format("Achievement '{0}' gain!", achievement.steamAchievement));
                }
        }
	}
}
