using Ranensol.BepInEx.Aska.Villagers.Homesteads;
using SSSGame;
using SSSGame.AI;

namespace Ranensol.BepInEx.Aska.Villagers.Services
{
    /// <summary>
    /// Handles the logic for assigning villagers to homesteads optimally
    /// </summary>
    public static class VillagerAssignmentService
    {
        /// <summary>
        /// Assigns homeless villagers to best available houses based on config settings
        /// </summary>
        public static void AssignHomelessVillagers(bool assignOnlyNewest)
        {
            try
            {
                var homeless = GetHomelessVillagers(assignOnlyNewest);
                if (homeless.Count == 0)
                {
                    Plugin.Log.LogDebug("No homeless villagers found");
                    return;
                }

                var homesteads = HomesteadFinder.GetAvailableHomesteads();
                if (homesteads.Count == 0)
                {
                    Plugin.Log.LogWarning("No available homesteads!");
                    return;
                }

                Plugin.Log.LogInfo($"Assigning {homeless.Count} villagers to {homesteads.Count} available homesteads");

                var slots = CreateHousingSlotsWithFinalOccupancy(homesteads, homeless.Count, sortBestFirst: !assignOnlyNewest);

                AssignVillagersToSlots(homeless, slots);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Exception in AssignHomelessVillagers: {ex.Message}");
                Plugin.Log.LogError(ex.StackTrace);
            }
        }

        /// <summary>
        /// Makes all villagers in the village homeless
        /// </summary>
        public static void MakeAllVillagersHomeless()
        {
            try
            {
                var villagers = UnityEngine.Object.FindObjectsOfType<Villager>();
                var count = 0;

                foreach (var villager in villagers)
                {
                    var homestead = villager.GetHomestead();
                    if (homestead != null)
                    {
                        ReleaseVillagerFromHomestead(villager, homestead);
                        count++;
                    }
                }

                Plugin.Log.LogInfo($"Made {count} villagers homeless");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Exception in MakeAllVillagersHomeless: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets homeless villagers, optionally filtered to only the newest spawned villager
        /// </summary>
        private static List<Villager> GetHomelessVillagers(bool onlyNewest)
        {
            var homeless = UnityEngine.Object.FindObjectsOfType<Villager>()
                .Where(v => v.GetHomestead() == null)
                .ToList();

            if (homeless.Count == 0)
                return homeless;

            if (Plugin.Config.TierBasedHousing.Value)
            {
                homeless = [.. homeless.OrderByDescending(v => v.GetProficiencyTier())];
                Plugin.Log.LogDebug("Sorted villagers by proficiency tier");
            }

            if (onlyNewest)
            {
                var newest = homeless.OrderByDescending(v => v.PersistentUniqueID).First();
                Plugin.Log.LogInfo($"Auto-assign: targeting newest villager ({newest.GetName()}, ID: {newest.PersistentUniqueID})");
                return [newest];
            }

            Plugin.Log.LogInfo($"Found {homeless.Count} homeless villagers");
            return homeless;
        }

        /// <summary>
        /// Creates housing slots with correct final occupancy prediction.
        /// All slots in the same house will have the same score based on final occupancy after all assignments.
        /// </summary>
        private static List<HousingSlot> CreateHousingSlotsWithFinalOccupancy(List<Homestead> homesteads, int villagersToAssign, bool sortBestFirst)
        {
            var distribution = CalculateGreedyDistribution(homesteads, villagersToAssign, pickBestHouses: sortBestFirst);
            var slots = new List<HousingSlot>();

            foreach (var (homestead, finalOccupancy) in distribution)
            {
                var currentOccupants = homestead.GetAgentsCount();
                var newAssignments = finalOccupancy - currentOccupants;

                if (newAssignments <= 0) continue;

                var finalScore = CalculateFinalScore(homestead, finalOccupancy);

                for (var i = 0; i < newAssignments; i++)
                {
                    slots.Add(new HousingSlot
                    {
                        Homestead = homestead,
                        FinalOccupants = finalOccupancy,
                        PredictedScore = finalScore
                    });
                }
            }

            slots = [.. slots.OrderByDescending(s => s.PredictedScore)];
            Plugin.Log.LogDebug($"Created {slots.Count} housing slots");

            return slots;
        }

        /// <summary>
        /// Calculates greedy distribution by incrementally assigning villagers to houses
        /// </summary>
        private static List<(Homestead homestead, int finalOccupancy)> CalculateGreedyDistribution(
            List<Homestead> homesteads, int villagersToAssign, bool pickBestHouses)
        {
            var occupancy = homesteads.ToDictionary(h => h, h => h.GetAgentsCount());
            var capacity = homesteads.ToDictionary(h => h, h => h.GetAgentsCapacity());

            Plugin.Log.LogDebug($"Greedy distribution for {villagersToAssign} villagers across {homesteads.Count} houses (picking {(pickBestHouses ? "best" : "worst")} houses):");

            for (var i = 0; i < villagersToAssign; i++)
            {
                var availableHouses = homesteads
                    .Where(h => occupancy[h] < capacity[h])
                    .Select(h => new { House = h, Score = HomesteadScorer.PredictFinalHappinessScore(h, occupancy[h] + 1) });

                var selectedHouse = pickBestHouses
                    ? availableHouses.OrderByDescending(x => x.Score).FirstOrDefault()?.House
                    : availableHouses.OrderBy(x => x.Score).FirstOrDefault()?.House;

                if (selectedHouse == null) break;

                occupancy[selectedHouse]++;
            }

            var distribution = occupancy
                .Where(kvp => kvp.Value > kvp.Key.GetAgentsCount())
                .Select(kvp =>
                {
                    var actualFinalScore = HomesteadScorer.PredictFinalHappinessScore(kvp.Key, kvp.Value);
                    var available = capacity[kvp.Key] - kvp.Key.GetAgentsCount();
                    var assigned = kvp.Value - kvp.Key.GetAgentsCount();

                    Plugin.Log.LogDebug($"  {kvp.Key._structure?.GetName()}: final score per person {actualFinalScore:F1}, assigning {assigned}/{available} beds (final: {kvp.Value}/{capacity[kvp.Key]})");

                    return (kvp.Key, kvp.Value);
                })
                .ToList();

            Plugin.Log.LogDebug($"Distribution complete: {occupancy.Values.Sum() - homesteads.Sum(h => h.GetAgentsCount())}/{villagersToAssign} villagers allocated");

            return distribution;
        }

        /// <summary>
        /// Calculates the final happiness score for a homestead with the specified occupancy
        /// </summary>
        private static float CalculateFinalScore(Homestead h, int finalOccupants)
        {
            if (Plugin.Config.UseHappinessScoring.Value)
            {
                return HomesteadScorer.PredictFinalHappinessScore(h, finalOccupants);
            }
            else
            {
                return HomesteadScorer.GetHouseTierPriority(h) * 100f;
            }
        }

        /// <summary>
        /// Assigns villagers to their calculated housing slots
        /// </summary>
        private static void AssignVillagersToSlots(List<Villager> villagers, List<HousingSlot> slots)
        {
            var assignedCount = 0;
            var maxAssignments = Math.Min(villagers.Count, slots.Count);

            for (var i = 0; i < maxAssignments; i++)
            {
                var villager = villagers[i];
                var slot = slots[i];

                if (AssignVillagerToHomestead(villager, slot.Homestead))
                {
                    assignedCount++;
                    LogAssignment(villager, slot);
                }
                else
                {
                    Plugin.Log.LogError($"Failed to assign {villager.GetName()} to {slot.Homestead._structure?.GetName()}");
                }
            }

            Plugin.Log.LogInfo($"Assignment complete: {assignedCount}/{villagers.Count} villagers housed");
        }

        /// <summary>
        /// Logs detailed assignment information if tier-based housing is enabled
        /// </summary>
        private static void LogAssignment(Villager villager, HousingSlot slot)
        {
            if (Plugin.Config.TierBasedHousing.Value)
            {
                var tier = villager.GetProficiencyTier() + 1;
                var occupancy = $"{slot.FinalOccupants}/{slot.Homestead.GetAgentsCapacity()}";
                Plugin.Log.LogDebug($"Assigned {villager.GetName()} (Tier {tier}) to {slot.Homestead._structure?.GetName()} (Score: {slot.PredictedScore:F1}, Occupancy: {occupancy})");
            }
        }

        /// <summary>
        /// Assigns a villager to a homestead using the game's workstation assignment system
        /// </summary>
        private static bool AssignVillagerToHomestead(Villager villager, Homestead homestead)
        {
            try
            {
                var taskAgent = villager.Cast<ITaskAgent>();
                var workstation = homestead.Cast<IWorkstation>();
                return taskAgent.AssignToWorkstation(workstation);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Exception assigning villager: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Releases a villager from their assigned homestead
        /// </summary>
        private static void ReleaseVillagerFromHomestead(Villager villager, Homestead homestead)
        {
            var agent = villager.Cast<ITaskAgent>();
            agent.ReleaseFromWorkstation(
                homestead.Cast<IWorkstation>(),
                villager.GetHomeOutpostId());
        }

        /// <summary>
        /// Represents a potential housing assignment with predicted happiness
        /// </summary>
        private class HousingSlot
        {
            public Homestead Homestead { get; set; }
            public int FinalOccupants { get; set; }
            public float PredictedScore { get; set; }
        }
    }
}