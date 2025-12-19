using BepInEx.Configuration;
using UnityEngine.InputSystem;

namespace Ranensol.BepInEx.Aska.Villagers
{
    public class ModConfig
    {
        public ConfigEntry<bool> ModEnabled { get; }
        public ConfigEntry<bool> DisplayBedsAvailable { get; }
        public ConfigEntry<bool> AutoHouseNewVillagers { get; }
        public ConfigEntry<float> AutoAssignDelay { get; }
        public ConfigEntry<Key> HotKeyManualAssignment { get; }
        public ConfigEntry<bool> TierBasedHousing { get; }
        public ConfigEntry<bool> UseHappinessScoring { get; }
        public ConfigEntry<Key> HotKeyMakeAllHomeless { get; }
        public ConfigEntry<bool> IncludeOutposts { get; }

        public ModConfig(ConfigFile config)
        {
            ModEnabled = config.Bind("1 - General", "ModEnabled", true,
                "Enable or disable the entire mod");

            DisplayBedsAvailable = config.Bind("2 - UI", "DisplayBedsAvailable", true,
                "Display the 'Beds available' counter in the top-right HUD");

            AutoHouseNewVillagers = config.Bind("3 - Housing", "AutoHouseNewVillagers", true,
                "Automatically assign newly summoned villagers to available houses");

            AutoAssignDelay = config.Bind("3 - Housing", "AutoAssignDelay", 3f,
                new ConfigDescription("Delay in seconds after villager spawns before auto-assignment",
                    new AcceptableValueRange<float>(1f, 10f)));

            TierBasedHousing = config.Bind("3 - Housing", "TierBasedHousing", true,
                "Prioritize higher-tier villagers when assigning houses (Tier 4 gets first pick, then Tier 3, etc.)");

            UseHappinessScoring = config.Bind("3 - Housing", "UseHappinessScoring", true,
                "Use actual game happiness calculations (Comfort + Area Desirability) to determine best houses. If disabled, uses simple house type priority (Longhouse > Cottage > Shelter)");

            IncludeOutposts = config.Bind("3 - Housing", "IncludeOutposts", false,
                "Include outpost houses when assigning villagers. WARNING: Villagers will be assigned to ANY available bed (main village or outposts) with no logic for which outpost they should go to. Leave disabled if you want to manage outpost housing manually.");

            HotKeyManualAssignment = config.Bind("4 - Hotkeys", "ManualAssignmentKey", Key.F8,
                "Hotkey to manually trigger homeless villager assignment");

            HotKeyMakeAllHomeless = config.Bind("4 - Hotkeys", "MakeAllVillagersHomelessKey", Key.F9,
                "Hotkey to make all villagers homeless (useful for testing or re-sorting)");
        }
    }
}
