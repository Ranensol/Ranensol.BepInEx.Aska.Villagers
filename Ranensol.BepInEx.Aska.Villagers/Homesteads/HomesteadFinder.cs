using SSSGame;

namespace Ranensol.BepInEx.Aska.Villagers.Homesteads
{
    /// <summary>
    /// Utilities for finding and filtering homesteads in the main village
    /// </summary>
    public static class HomesteadFinder
    {
        private static readonly string[] ValidHomesteadNames =
        [
            "Shelter_L1(Clone)",
            "House_L1(Clone)",
            "House_L2(Clone)"
        ];

        /// <summary>
        /// Gets all homesteads in the main village with available bed space
        /// </summary>
        public static List<Homestead> GetAvailableHomesteads()
        {
            return [.. UnityEngine.Object.FindObjectsOfType<Homestead>().Where(h => IsValidHomestead(h) && HasAvailableBeds(h) && IsInMainVillage(h))];
        }

        /// <summary>
        /// Gets all homesteads in the main village (including full ones)
        /// </summary>
        public static List<Homestead> GetAllHomesteads()
        {
            return [.. UnityEngine.Object.FindObjectsOfType<Homestead>().Where(h => IsValidHomestead(h) && IsInMainVillage(h))];
        }

        private static bool IsValidHomestead(Homestead homestead)
        {
            return ValidHomesteadNames.Contains(homestead.name) && homestead.gameObject.activeSelf;
        }

        private static bool HasAvailableBeds(Homestead h)
        {
            return h.GetAgentsCapacity() > h.GetAgentsCount();
        }

        private static bool IsInMainVillage(Homestead h)
        {
            // If including outposts, accept all
            if (Plugin.Config.IncludeOutposts.Value)
                return true;

            // Otherwise, main village only
            var structure = h.GetComponent<Structure>();
            return structure != null && structure.OutpostId == 0;
        }
    }
}
