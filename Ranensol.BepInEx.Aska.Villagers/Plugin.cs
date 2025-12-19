using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Ranensol.BepInEx.Aska.Villagers.Components;

namespace Ranensol.BepInEx.Aska.Villagers
{
    /// <summary>
    /// Main plugin entry point for the Villagers mod
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static new ManualLogSource Log;
        public static new ModConfig Config;

        public override void Load()
        {
            Log = base.Log;
            Config = new ModConfig(base.Config);

            if (!Config.ModEnabled.Value)
            {
                Log.LogInfo("Mod is disabled in config");
                return;
            }

            AddComponent<VillagerAutoAssigner>();

            if (Config.DisplayBedsAvailable.Value)
            {
                AddComponent<VillagerStatsWidget>();
            }

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
            Log.LogInfo("Mod loaded");
        }
    }
}