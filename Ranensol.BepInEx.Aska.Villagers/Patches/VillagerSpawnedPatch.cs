using HarmonyLib;
using Ranensol.BepInEx.Aska.Villagers.Components;
using SSSGame;

namespace Ranensol.BepInEx.Aska.Villagers.Patches
{
    /// <summary>
    /// Harmony patch that triggers auto-assignment when a villager is spawned from the Eye of Odin
    /// </summary>
    [HarmonyPatch(typeof(VillagerOutlet), "SpawnVillager")]
    class VillagerSpawnedPatch
    {
        /// <summary>
        /// Called after a villager spawns to trigger delayed auto-assignment
        /// </summary>
        static void Postfix()
        {
            if (!Plugin.Config.AutoHouseNewVillagers.Value) return;

            Plugin.Log.LogInfo("Villager spawned from Eye of Odin");

            VillagerAutoAssigner.Instance?.TriggerDelayedAssignment();
        }
    }
}