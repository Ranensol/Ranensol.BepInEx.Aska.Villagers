using Ranensol.BepInEx.Aska.Villagers.Homesteads;
using TMPro;
using UnityEngine;

namespace Ranensol.BepInEx.Aska.Villagers.Components
{
    /// <summary>
    /// Displays available bed count in the game HUD
    /// </summary>
    public class VillagerStatsWidget : MonoBehaviour
    {
        public VillagerStatsWidget(IntPtr ptr) : base(ptr) { }

        private TMP_Text _bedStatsText;
        private float _updateTimer = 0f;
        private bool _uiCreated = false;

        private void Update()
        {
            CheckForDestroyedUI();

            if (!_uiCreated)
            {
                CreateBedStatsUI();
                return;
            }

            UpdateBedStatsText();
        }

        /// <summary>
        /// Detects if the UI was destroyed (e.g., on save reload) and marks it for recreation
        /// </summary>
        private void CheckForDestroyedUI()
        {
            if (_uiCreated && _bedStatsText == null)
            {
                _uiCreated = false;
                Plugin.Log.LogInfo("Bed stats UI was destroyed, will recreate");
            }
        }

        /// <summary>
        /// Creates the bed stats UI by cloning the existing VillagersPanel
        /// </summary>
        private void CreateBedStatsUI()
        {
            try
            {
                var tempList = GameObject.Find("TemperatureList");
                if (tempList == null) return;

                var villagersPanel = tempList.transform.Find("VillagersPanel");
                if (villagersPanel == null)
                {
                    Plugin.Log.LogError("Could not find VillagersPanel");
                    _uiCreated = true;
                    return;
                }

                var bedStatsPanel = Instantiate(villagersPanel.gameObject, tempList.transform);
                bedStatsPanel.name = "BedStatsPanel";

                _bedStatsText = bedStatsPanel.GetComponentInChildren<TMP_Text>();
                if (_bedStatsText != null)
                {
                    _bedStatsText.text = "Beds available: 0";
                }

                _uiCreated = true;
                Plugin.Log.LogInfo("Bed stats UI created successfully");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to create bed stats UI: {ex.Message}");
                _uiCreated = true;
            }
        }

        /// <summary>
        /// Updates the displayed bed count once per second
        /// </summary>
        private void UpdateBedStatsText()
        {
            if (_bedStatsText == null) return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer < 1f) return;

            _updateTimer = 0f;

            try
            {
                var homesteads = HomesteadFinder.GetAllHomesteads();
                var totalBeds = homesteads.Sum(h => h.GetAgentsCapacity());
                var usedBeds = homesteads.Sum(h => h.GetAgentsCount());
                var freeBeds = Math.Max(0, totalBeds - usedBeds);

                _bedStatsText.text = $"Beds available: {freeBeds}";
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error updating bed stats: {ex.Message}");
            }
        }
    }
}