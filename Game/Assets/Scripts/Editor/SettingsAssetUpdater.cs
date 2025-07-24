using UnityEngine;
using UnityEditor;
using LogisticGame.Managers;

namespace LogisticGame.Editor
{
    /// <summary>
    /// AIDEV-NOTE: Editor-only utility to sync runtime settings changes back to the DefaultSettings.asset.
    /// This ensures that language and other preference changes made during play mode are reflected 
    /// in the ScriptableObject asset for immediate visual feedback in the Inspector.
    /// </summary>
    [InitializeOnLoad]
    public static class SettingsAssetUpdater
    {
        static SettingsAssetUpdater()
        {
            Debug.Log("SettingsAssetUpdater: Static constructor called - ready to sync settings");
            
            // Subscribe to play mode state changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// Handles play mode state changes to sync settings when exiting play mode.
        /// AIDEV-NOTE: When exiting play mode, sync any runtime changes back to the asset.
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Debug.Log($"SettingsAssetUpdater: Play mode state changed to {state}");
            
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    // Sync settings when exiting play mode
                    Debug.Log("SettingsAssetUpdater: Exiting play mode, syncing settings");
                    SyncSettingsToAsset();
                    break;
                    
                case PlayModeStateChange.EnteredEditMode:
                    // Entered edit mode - no additional action needed
                    Debug.Log("SettingsAssetUpdater: Entered edit mode");
                    break;
            }
        }


        /// <summary>
        /// Syncs current runtime settings back to the DefaultSettings.asset.
        /// AIDEV-NOTE: Main method to update the ScriptableObject asset with runtime changes.
        /// </summary>
        public static void SyncSettingsToAsset(SettingsData sourceSettings = null)
        {
            try
            {
                // Get the current settings from SettingsManager if not provided
                if (sourceSettings == null)
                {
                    if (SettingsManager.Instance == null || !SettingsManager.Instance.IsInitialized)
                    {
                        Debug.LogWarning("SettingsAssetUpdater: SettingsManager not available, cannot sync settings");
                        return;
                    }
                    sourceSettings = SettingsManager.Instance.CurrentSettings;
                }

                if (sourceSettings == null)
                {
                    Debug.LogWarning("SettingsAssetUpdater: No source settings available to sync");
                    return;
                }

                // Use the SettingsManager's new method to sync settings
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.SyncToDefaultSettingsAsset();
                    Debug.Log("SettingsAssetUpdater: Successfully synced settings using SettingsManager");
                }
                else
                {
                    Debug.LogWarning("SettingsAssetUpdater: SettingsManager instance not available");
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"SettingsAssetUpdater: Failed to sync settings to asset - {ex.Message}");
            }
        }


        /// <summary>
        /// Manual sync method for testing or forcing an update.
        /// AIDEV-NOTE: Can be called from custom editor windows or menu items.
        /// </summary>
        [MenuItem("Tools/Logistics Game/Sync Settings to Asset")]
        public static void ForceSyncSettingsToAsset()
        {
            SyncSettingsToAsset();
        }

        /// <summary>
        /// Validates that the sync menu item should be available.
        /// AIDEV-NOTE: Only show the menu item when it makes sense.
        /// </summary>
        [MenuItem("Tools/Logistics Game/Sync Settings to Asset", true)]
        public static bool ValidateForceSyncSettingsToAsset()
        {
            // Only enable if we have a SettingsManager instance
            return SettingsManager.Instance != null;
        }
    }
}