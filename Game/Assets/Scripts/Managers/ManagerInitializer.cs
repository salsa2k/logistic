using UnityEngine;

namespace LogisticGame.Managers
{
    // AIDEV-NOTE: Ensures proper initialization order of managers at game start
    public class ManagerInitializer : MonoBehaviour
    {
        [Header("Manager Initialization")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _debugLogging = true;

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                InitializeManagers();
            }
        }

        [ContextMenu("Initialize Managers")]
        public void InitializeManagers()
        {
            if (_debugLogging)
                Debug.Log("Starting manager initialization...");

            // Initialize managers in order of dependency
            // GameManager should be first as other systems may depend on it
            var gameManager = GameManager.Instance;
            LogInitialization("GameManager", gameManager != null);

            // AssetManager should be early as many systems need assets
            var assetManager = AssetManager.Instance;
            LogInitialization("AssetManager", assetManager != null);

            // LocalizationManager for language support - early init for UI systems
            var localizationManager = LocalizationManager.Instance;
            LogInitialization("LocalizationManager", localizationManager != null);

            // SceneTransitionManager for scene management
            var sceneManager = SceneTransitionManager.Instance;
            LogInitialization("SceneTransitionManager", sceneManager != null);

            if (_debugLogging)
                Debug.Log("Manager initialization completed!");
        }

        private void LogInitialization(string managerName, bool success)
        {
            if (_debugLogging)
            {
                if (success)
                    Debug.Log($"✓ {managerName} initialized successfully");
                else
                    Debug.LogError($"✗ {managerName} failed to initialize");
            }
        }

        private void Start()
        {
            // Verify all managers are properly initialized
            VerifyManagerInitialization();
        }

        private void VerifyManagerInitialization()
        {
            bool allManagersReady = true;

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not initialized!");
                allManagersReady = false;
            }

            if (AssetManager.Instance == null)
            {
                Debug.LogError("AssetManager not initialized!");
                allManagersReady = false;
            }

            if (LocalizationManager.Instance == null)
            {
                Debug.LogError("LocalizationManager not initialized!");
                allManagersReady = false;
            }

            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("SceneTransitionManager not initialized!");
                allManagersReady = false;
            }

            if (allManagersReady && _debugLogging)
            {
                Debug.Log("All core managers are ready for game operation!");
            }
        }
    }
}