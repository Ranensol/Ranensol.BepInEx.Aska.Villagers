using Ranensol.BepInEx.Aska.Villagers.Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ranensol.BepInEx.Aska.Villagers.Components
{
    /// <summary>
    /// Handles hotkey input and triggers villager assignment
    /// </summary>
    public class VillagerAutoAssigner : MonoBehaviour
    {
        public VillagerAutoAssigner(IntPtr ptr) : base(ptr) { }

        public static VillagerAutoAssigner Instance { get; private set; }

        private float _delayedCheckTimer = -1f;

        private void Awake()
        {
            Instance = this;
            Plugin.Log.LogInfo("VillagerAutoAssigner initialized");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            HandleHotkeys();
            HandleDelayedAssignment();
        }

        /// <summary>
        /// Triggers a delayed assignment check after the configured delay
        /// </summary>
        public void TriggerDelayedAssignment()
        {
            _delayedCheckTimer = Plugin.Config.AutoAssignDelay.Value;
        }

        /// <summary>
        /// Checks for manual assignment and make homeless hotkey presses
        /// </summary>
        private static void HandleHotkeys()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current[Plugin.Config.HotKeyManualAssignment.Value].wasPressedThisFrame)
            {
                Plugin.Log.LogInfo("Manual assignment hotkey pressed");
                VillagerAssignmentService.AssignHomelessVillagers(assignOnlyNewest: false);
            }

            if (Keyboard.current[Plugin.Config.HotKeyMakeAllHomeless.Value].wasPressedThisFrame)
            {
                Plugin.Log.LogInfo("Make all homeless hotkey pressed");
                VillagerAssignmentService.MakeAllVillagersHomeless();
            }
        }

        /// <summary>
        /// Handles the countdown timer for delayed auto-assignment
        /// </summary>
        private void HandleDelayedAssignment()
        {
            if (_delayedCheckTimer < 0f) return;

            _delayedCheckTimer -= Time.deltaTime;
            if (_delayedCheckTimer <= 0f)
            {
                Plugin.Log.LogInfo("Running delayed auto-assignment");
                VillagerAssignmentService.AssignHomelessVillagers(assignOnlyNewest: true);
                _delayedCheckTimer = -1f;
            }
        }
    }
}