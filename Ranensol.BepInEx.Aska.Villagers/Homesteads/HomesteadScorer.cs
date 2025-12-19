using SSSGame;
using UnityEngine;

namespace Ranensol.BepInEx.Aska.Villagers.Homesteads
{
    /// <summary>
    /// Calculates happiness scores for homesteads
    /// </summary>
    public static class HomesteadScorer
    {
        private const int HOUSING_POINTS_ID = 145;           // Comfort
        private const int EXTERIOR_HOUSING_POINTS_ID = 148;  // Area Desirability

        /// <summary>
        /// Gets the base happiness score (Comfort + Area Desirability) without occupancy multiplier
        /// </summary>
        public static float GetBaseHappinessScore(Homestead homestead)
        {
            try
            {
                var total = 0f;
                foreach (var manager in homestead.GetComponents<StructureScoreManager>())
                {
                    if (manager.scoreAttrConfig == null) continue;

                    var attrId = manager.scoreAttrConfig.attributeId;
                    if (attrId is HOUSING_POINTS_ID or EXTERIOR_HOUSING_POINTS_ID)
                    {
                        total += manager.Score;
                    }
                }
                return total;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Failed to get base score for {homestead._structure?.GetName()}: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Gets the agent count multiplier from the house's AnimationCurve
        /// </summary>
        public static float GetOccupancyMultiplier(Homestead homestead, int occupants)
        {
            try
            {
                foreach (var manager in homestead.GetComponents<StructureScoreManager>())
                {
                    if (manager.agentsCountMultiplierCurve != null)
                    {
                        return manager.agentsCountMultiplierCurve.Evaluate(occupants);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Failed to get multiplier for {homestead?._structure?.GetName()}: {ex.Message}");
            }

            // Fallback: simple division
            return 1f / Mathf.Max(1, occupants);
        }

        /// <summary>
        /// Predicts the final happiness score after assigning N total occupants
        /// </summary>
        public static float PredictFinalHappinessScore(Homestead homestead, int finalOccupants)
        {
            var baseScore = GetBaseHappinessScore(homestead);
            var multiplier = GetOccupancyMultiplier(homestead, finalOccupants);
            return baseScore * multiplier;
        }

        /// <summary>
        /// Gets simple house tier priority (for fallback when not using happiness scoring)
        /// </summary>
        public static int GetHouseTierPriority(Homestead homestead)
        {
            return homestead.name switch
            {
                "House_L2(Clone)" => 3,
                "House_L1(Clone)" => 2,
                "Shelter_L1(Clone)" => 1,
                _ => 0
            };
        }
    }
}
