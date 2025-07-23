using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using LogisticGame.SaveSystem;
using LogisticGame.Events;

namespace LogisticGame.Managers
{
    /// <summary>
    /// AIDEV-NOTE: Manager for game settings persistence and global settings access.
    /// Handles loading, saving, and applying settings across the entire game.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Settings Configuration")]
        [SerializeField] private SettingsData _defaultSettings;
        [SerializeField] private bool _autoSaveSettings = true;
        [SerializeField] private bool _debugLogging = false;
        
        // Singleton instance
        private static SettingsManager _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SettingsManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(SettingsManager));
                        _instance = go.AddComponent<SettingsManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // Current settings instance
        private SettingsData _currentSettings;
        public SettingsData CurrentSettings => _currentSettings;
        
        // Settings file management
        private const string SETTINGS_FILE_NAME = "GameSettings";
        private readonly string _settingsFilePath;
        
        // Events
        public static event Action<SettingsData> OnSettingsLoaded;
        public static event Action<SettingsData> OnSettingsSaved;
        public static event Action<SettingsData> OnSettingsChanged;
        public static event Action<string> OnSettingsError;
        
        // Properties
        public bool IsInitialized { get; private set; }
        public bool HasUnsavedChanges { get; private set; }
        
        // Constructor for file path initialization
        public SettingsManager()
        {
            _settingsFilePath = Path.Combine(Application.persistentDataPath, $"{SETTINGS_FILE_NAME}.json");
        }
        
        private void Awake()
        {
            // Ensure singleton
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSettings();
        }
        
        private void Start()
        {
            // Subscribe to settings events
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            // Auto-save settings if enabled and there are unsaved changes
            if (_autoSaveSettings && HasUnsavedChanges && _currentSettings != null)
            {
                SaveSettingsAsync().ConfigureAwait(false);
            }
        }
        
        /// <summary>
        /// Initializes the settings system by loading settings or creating defaults.
        /// AIDEV-NOTE: Called during Awake to set up the settings system.
        /// </summary>
        private async void InitializeSettings()
        {
            try
            {
                if (_debugLogging)
                    Debug.Log("SettingsManager: Initializing settings system...");
                
                // Try to load existing settings
                bool settingsLoaded = await LoadSettingsAsync();
                
                if (!settingsLoaded)
                {
                    // Create default settings if loading failed
                    CreateDefaultSettings();
                }
                
                // Apply the current settings
                ApplyCurrentSettings();
                
                IsInitialized = true;
                OnSettingsLoaded?.Invoke(_currentSettings);
                
                if (_debugLogging)
                    Debug.Log("SettingsManager: Settings system initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsManager: Failed to initialize settings - {ex.Message}");
                OnSettingsError?.Invoke($"Settings initialization failed: {ex.Message}");
                
                // Fall back to default settings
                CreateDefaultSettings();
                IsInitialized = true;
            }
        }
        
        /// <summary>
        /// Creates default settings instance.
        /// AIDEV-NOTE: Used when no saved settings exist or loading fails.
        /// </summary>
        private void CreateDefaultSettings()
        {
            if (_defaultSettings != null)
            {
                // Create a copy of the default settings
                _currentSettings = Instantiate(_defaultSettings);
            }
            else
            {
                // Create a new settings instance with defaults
                _currentSettings = ScriptableObject.CreateInstance<SettingsData>();
                _currentSettings.LoadDefaults();
            }
            
            _currentSettings.name = "CurrentSettings";
            HasUnsavedChanges = true;
            
            if (_debugLogging)
                Debug.Log("SettingsManager: Created default settings");
        }
        
        /// <summary>
        /// Loads settings from persistent storage.
        /// AIDEV-NOTE: Attempts to load settings from JSON file using SaveFileManager.
        /// </summary>
        /// <returns>True if settings were loaded successfully, false otherwise</returns>
        public async Task<bool> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    if (_debugLogging)
                        Debug.Log("SettingsManager: No settings file found, will create default settings");
                    return false;
                }
                
                // Read settings file
                string settingsJson = await File.ReadAllTextAsync(_settingsFilePath);
                
                if (string.IsNullOrEmpty(settingsJson))
                {
                    Debug.LogWarning("SettingsManager: Settings file is empty");
                    return false;
                }
                
                // Create new settings instance
                _currentSettings = ScriptableObject.CreateInstance<SettingsData>();
                
                // Parse JSON and apply to settings
                var settingsWrapper = JsonUtility.FromJson<SettingsDataWrapper>(settingsJson);
                if (settingsWrapper != null)
                {
                    ApplyLoadedSettings(settingsWrapper);
                    HasUnsavedChanges = false;
                    
                    if (_debugLogging)
                        Debug.Log("SettingsManager: Settings loaded successfully");
                    
                    return true;
                }
                else
                {
                    Debug.LogError("SettingsManager: Failed to parse settings JSON");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsManager: Failed to load settings - {ex.Message}");
                OnSettingsError?.Invoke($"Settings loading failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Saves current settings to persistent storage.
        /// AIDEV-NOTE: Saves settings to JSON file using atomic write operations.
        /// </summary>
        /// <returns>True if settings were saved successfully, false otherwise</returns>
        public async Task<bool> SaveSettingsAsync()
        {
            if (_currentSettings == null)
            {
                Debug.LogWarning("SettingsManager: No current settings to save");
                return false;
            }
            
            try
            {
                // Create settings wrapper for serialization
                var settingsWrapper = CreateSettingsWrapper(_currentSettings);
                string settingsJson = JsonUtility.ToJson(settingsWrapper, true);
                
                // Write to file atomically
                await File.WriteAllTextAsync(_settingsFilePath, settingsJson);
                
                HasUnsavedChanges = false;
                OnSettingsSaved?.Invoke(_currentSettings);
                
                if (_debugLogging)
                    Debug.Log("SettingsManager: Settings saved successfully");
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsManager: Failed to save settings - {ex.Message}");
                OnSettingsError?.Invoke($"Settings saving failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Updates current settings with new data.
        /// AIDEV-NOTE: Public method for other systems to update settings.
        /// </summary>
        /// <param name="newSettings">The new settings data to apply</param>
        /// <param name="saveImmediately">Whether to save settings immediately</param>
        public async Task UpdateSettingsAsync(SettingsData newSettings, bool saveImmediately = true)
        {
            if (newSettings == null)
            {
                Debug.LogWarning("SettingsManager: Cannot update with null settings");
                return;
            }
            
            try
            {
                // Copy the new settings
                _currentSettings = Instantiate(newSettings);
                _currentSettings.name = "CurrentSettings";
                
                // Apply the settings
                ApplyCurrentSettings();
                
                HasUnsavedChanges = true;
                OnSettingsChanged?.Invoke(_currentSettings);
                
                // Save immediately if requested
                if (saveImmediately)
                {
                    await SaveSettingsAsync();
                }
                
                if (_debugLogging)
                    Debug.Log("SettingsManager: Settings updated successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsManager: Failed to update settings - {ex.Message}");
                OnSettingsError?.Invoke($"Settings update failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Resets settings to defaults and optionally saves them.
        /// AIDEV-NOTE: Utility method for resetting all settings to default values.
        /// </summary>
        /// <param name="saveImmediately">Whether to save settings immediately</param>
        public async Task ResetToDefaultsAsync(bool saveImmediately = true)
        {
            try
            {
                CreateDefaultSettings();
                ApplyCurrentSettings();
                
                OnSettingsChanged?.Invoke(_currentSettings);
                
                if (saveImmediately)
                {
                    await SaveSettingsAsync();
                }
                
                if (_debugLogging)
                    Debug.Log("SettingsManager: Settings reset to defaults");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsManager: Failed to reset settings - {ex.Message}");
                OnSettingsError?.Invoke($"Settings reset failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Applies the current settings to the system.
        /// AIDEV-NOTE: Calls SettingsData.ApplySettings() to update Unity and game systems.
        /// </summary>
        private void ApplyCurrentSettings()
        {
            if (_currentSettings != null)
            {
                _currentSettings.ApplySettings();
                
                if (_debugLogging)
                    Debug.Log("SettingsManager: Current settings applied to system");
            }
        }
        
        /// <summary>
        /// Creates a serializable wrapper for settings data.
        /// AIDEV-NOTE: Converts SettingsData to a JSON-serializable format.
        /// </summary>
        private SettingsDataWrapper CreateSettingsWrapper(SettingsData settings)
        {
            return new SettingsDataWrapper
            {
                // Audio Settings
                masterVolume = settings.MasterVolume,
                musicVolume = settings.MusicVolume,
                sfxVolume = settings.SfxVolume,
                uiVolume = settings.UiVolume,
                audioEnabled = settings.AudioEnabled,
                
                // Graphics Settings
                targetFrameRate = settings.TargetFrameRate,
                vSyncEnabled = settings.VSyncEnabled,
                screenMode = (int)settings.ScreenMode,
                resolutionWidth = settings.Resolution.width,
                resolutionHeight = settings.Resolution.height,
                resolutionRefreshRate = (int)settings.Resolution.refreshRateRatio.value,
                uiScale = settings.UiScale,
                
                // Gameplay Settings
                gameSpeed = settings.GameSpeed,
                autopauseOnLowFuel = settings.AutopauseOnLowFuel,
                autopauseOnContractExpiry = settings.AutopauseOnContractExpiry,
                showDistanceInKm = settings.ShowDistanceInKm,
                show24HourTime = settings.Show24HourTime,
                confirmDangerousActions = settings.ConfirmDangerousActions,
                
                // UI Settings
                showTooltips = settings.ShowTooltips,
                showNotifications = settings.ShowNotifications,
                tooltipDelay = settings.TooltipDelay,
                notificationDuration = settings.NotificationDuration,
                minimizeToSystemTray = settings.MinimizeToSystemTray,
                
                // Localization Settings
                language = (int)settings.Language,
                localeCode = settings.LocaleCode,
                currencySymbol = settings.CurrencySymbol,
                distanceUnit = settings.DistanceUnit,
                weightUnit = settings.WeightUnit,
                volumeUnit = settings.VolumeUnit,
                fuelUnit = settings.FuelUnit,
                
                // Advanced Settings
                enableDebugMode = settings.EnableDebugMode,
                enableTelemetry = settings.EnableTelemetry,
                autosaveInterval = settings.AutosaveInterval,
                maxAutosaves = settings.MaxAutosaves,
                loadLastSaveOnStart = settings.LoadLastSaveOnStart
            };
        }
        
        /// <summary>
        /// Applies loaded settings from wrapper to current settings instance.
        /// AIDEV-NOTE: Converts JSON data back to SettingsData properties.
        /// </summary>
        private void ApplyLoadedSettings(SettingsDataWrapper wrapper)
        {
            // Audio Settings
            _currentSettings.SetMasterVolume(wrapper.masterVolume);
            _currentSettings.SetMusicVolume(wrapper.musicVolume);
            _currentSettings.SetSfxVolume(wrapper.sfxVolume);
            _currentSettings.SetUiVolume(wrapper.uiVolume);
            _currentSettings.SetAudioEnabled(wrapper.audioEnabled);
            
            // Graphics Settings
            _currentSettings.SetTargetFrameRate(wrapper.targetFrameRate);
            _currentSettings.SetVSyncEnabled(wrapper.vSyncEnabled);
            _currentSettings.SetScreenMode((FullScreenMode)wrapper.screenMode);
            
            var resolution = new Resolution
            {
                width = wrapper.resolutionWidth,
                height = wrapper.resolutionHeight,
                refreshRateRatio = new RefreshRate()
            };
            _currentSettings.SetResolution(resolution);
            _currentSettings.SetUiScale(wrapper.uiScale);
            
            // Gameplay Settings
            _currentSettings.SetGameSpeed(wrapper.gameSpeed);
            _currentSettings.SetAutopauseOnLowFuel(wrapper.autopauseOnLowFuel);
            _currentSettings.SetAutopauseOnContractExpiry(wrapper.autopauseOnContractExpiry);
            _currentSettings.SetShowDistanceInKm(wrapper.showDistanceInKm);
            _currentSettings.SetShow24HourTime(wrapper.show24HourTime);
            _currentSettings.SetConfirmDangerousActions(wrapper.confirmDangerousActions);
            
            // UI Settings
            _currentSettings.SetShowTooltips(wrapper.showTooltips);
            _currentSettings.SetShowNotifications(wrapper.showNotifications);
            _currentSettings.SetTooltipDelay(wrapper.tooltipDelay);
            _currentSettings.SetNotificationDuration(wrapper.notificationDuration);
            
            // Localization Settings
            _currentSettings.SetLocaleCode(wrapper.localeCode);
            _currentSettings.SetCurrencySymbol(wrapper.currencySymbol);
            
            // Advanced Settings
            _currentSettings.SetEnableDebugMode(wrapper.enableDebugMode);
            _currentSettings.SetEnableTelemetry(wrapper.enableTelemetry);
            _currentSettings.SetAutosaveInterval(wrapper.autosaveInterval);
            _currentSettings.SetMaxAutosaves(wrapper.maxAutosaves);
            _currentSettings.SetLoadLastSaveOnStart(wrapper.loadLastSaveOnStart);
        }
        
        /// <summary>
        /// Subscribes to relevant events.
        /// AIDEV-NOTE: Sets up event listeners for settings-related events.
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to settings modal events
            UI.SettingsModalController.OnSettingsApplied += OnSettingsModalApplied;
            UI.SettingsModalController.OnSettingsReset += OnSettingsModalReset;
        }
        
        /// <summary>
        /// Unsubscribes from events.
        /// AIDEV-NOTE: Cleanup method to prevent memory leaks.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            UI.SettingsModalController.OnSettingsApplied -= OnSettingsModalApplied;
            UI.SettingsModalController.OnSettingsReset -= OnSettingsModalReset;
        }
        
        // Event Handlers
        private async void OnSettingsModalApplied(SettingsData newSettings)
        {
            await UpdateSettingsAsync(newSettings, _autoSaveSettings);
        }
        
        private async void OnSettingsModalReset()
        {
            await ResetToDefaultsAsync(_autoSaveSettings);
        }
        
        /// <summary>
        /// Gets the current settings, loading them if necessary.
        /// AIDEV-NOTE: Public accessor for other systems to get current settings.
        /// </summary>
        public SettingsData GetCurrentSettings()
        {
            if (_currentSettings == null && IsInitialized)
            {
                CreateDefaultSettings();
            }
            
            return _currentSettings;
        }
        
        /// <summary>
        /// Forces an immediate save of current settings.
        /// AIDEV-NOTE: Public method for manual settings saving.
        /// </summary>
        public async Task ForceSaveAsync()
        {
            await SaveSettingsAsync();
        }
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;
        
        /// <summary>
        /// AIDEV-NOTE: Debug method for testing settings persistence.
        /// </summary>
        [ContextMenu("Test Save Settings")]
        private async void DebugTestSaveSettings()
        {
            if (_showDebugInfo)
            {
                bool success = await SaveSettingsAsync();
                Debug.Log($"Settings save test: {(success ? "Success" : "Failed")}");
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Debug method for testing settings loading.
        /// </summary>
        [ContextMenu("Test Load Settings")]
        private async void DebugTestLoadSettings()
        {
            if (_showDebugInfo)
            {
                bool success = await LoadSettingsAsync();
                Debug.Log($"Settings load test: {(success ? "Success" : "Failed")}");
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Debug method for printing current settings.
        /// </summary>
        [ContextMenu("Print Current Settings")]
        private void DebugPrintCurrentSettings()
        {
            if (_showDebugInfo && _currentSettings != null)
            {
                Debug.Log($"Master Volume: {_currentSettings.MasterVolume}");
                Debug.Log($"Language: {_currentSettings.Language}");
                Debug.Log($"Distance Unit: {_currentSettings.DistanceUnit}");
                Debug.Log($"Has Unsaved Changes: {HasUnsavedChanges}");
            }
        }
        #endif
    }
    
    /// <summary>
    /// AIDEV-NOTE: Serializable wrapper for SettingsData to enable JSON persistence.
    /// ScriptableObjects can't be directly serialized with JsonUtility.
    /// </summary>
    [System.Serializable]
    public class SettingsDataWrapper
    {
        // Audio Settings
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float uiVolume;
        public bool audioEnabled;
        
        // Graphics Settings
        public int targetFrameRate;
        public bool vSyncEnabled;
        public int screenMode;
        public int resolutionWidth;
        public int resolutionHeight;
        public int resolutionRefreshRate;
        public float uiScale;
        
        // Gameplay Settings
        public float gameSpeed;
        public bool autopauseOnLowFuel;
        public bool autopauseOnContractExpiry;
        public bool showDistanceInKm;
        public bool show24HourTime;
        public bool confirmDangerousActions;
        
        // UI Settings
        public bool showTooltips;
        public bool showNotifications;
        public float tooltipDelay;
        public float notificationDuration;
        public bool minimizeToSystemTray;
        
        // Localization Settings
        public int language;
        public string localeCode;
        public string currencySymbol;
        public string distanceUnit;
        public string weightUnit;
        public string volumeUnit;
        public string fuelUnit;
        
        // Advanced Settings
        public bool enableDebugMode;
        public bool enableTelemetry;
        public int autosaveInterval;
        public int maxAutosaves;
        public bool loadLastSaveOnStart;
    }
}