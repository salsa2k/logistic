using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LogisticGame.Managers
{
    // AIDEV-NOTE: Manages localization system, language switching, and provides convenient access to localized content
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LocalizationManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(LocalizationManager));
                        _instance = go.AddComponent<LocalizationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Localization Settings")]
        [SerializeField] private bool _debugLogging = true;
        [SerializeField] private bool _useSystemLanguage = true;

        // Events for language change notifications
        public static System.Action<Locale> OnLanguageChanged;

        // Properties
        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;
        public bool IsInitialized { get; private set; }

        // Common locale identifiers
        public const string LOCALE_ENGLISH = "en";
        public const string LOCALE_PORTUGUESE = "pt-BR";

        private void Awake()
        {
            // AIDEV-NOTE: Ensure only one instance exists
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLocalization();
        }

        private void InitializeLocalization()
        {
            if (_debugLogging)
                Debug.Log("Initializing Localization Manager...");

            try
            {
                // AIDEV-NOTE: Check if SettingsManager is available for language persistence
                WaitForSettingsManagerIfNeeded();
                
                // Wait for localization to initialize using Unity's async operation
                var initOperation = LocalizationSettings.InitializationOperation;
                
                if (initOperation.IsDone)
                {
                    // Already initialized
                    OnLocalizationInitialized();
                }
                else
                {
                    // Wait for initialization to complete
                    initOperation.Completed += (operation) => OnLocalizationInitialized();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize Localization Manager: {ex.Message}");
            }
        }

        private void WaitForSettingsManagerIfNeeded()
        {
            // AIDEV-NOTE: Wait a short time for SettingsManager to initialize if it's not ready yet
            var settingsManager = FindFirstObjectByType<SettingsManager>();
            if (settingsManager == null || !settingsManager.IsInitialized)
            {
                if (_debugLogging)
                    Debug.Log("LocalizationManager: Waiting for SettingsManager to initialize...");
                
                // Use a coroutine-like approach with Invoke to wait briefly
                Invoke(nameof(CheckSettingsManagerStatus), 0.1f);
            }
        }

        private void CheckSettingsManagerStatus()
        {
            var settingsManager = FindFirstObjectByType<SettingsManager>();
            if (settingsManager != null && settingsManager.IsInitialized)
            {
                if (_debugLogging)
                    Debug.Log("LocalizationManager: SettingsManager is now available, continuing initialization");
                
                // AIDEV-NOTE: Continue initialization now that SettingsManager is ready
                OnLocalizationInitialized();
            }
            else
            {               
                // AIDEV-NOTE: Keep waiting - retry after a short delay
                Invoke(nameof(CheckSettingsManagerStatus), 0.1f);
            }
        }

        private void OnLocalizationInitialized()
        {
            try
            {
                // Subscribe to SettingsManager events for proper coordination
                SubscribeToSettingsManagerEvents();
                
                // Set up initial locale
                SetupInitialLocale();

                // Subscribe to locale changes
                LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

                IsInitialized = true;

                if (_debugLogging)
                    Debug.Log($"Localization Manager initialized. Current locale: {CurrentLocale?.LocaleName ?? "None"}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to complete localization initialization: {ex.Message}");
            }
        }

        private void SetupInitialLocale()
        {
            // Check if SettingsManager is available and initialized
            var settingsManager = FindFirstObjectByType<SettingsManager>();
            if (settingsManager != null && settingsManager.IsInitialized)
            {
                // Try to load saved language preference from SettingsManager
                string savedLanguage = GetSavedLanguage();
                
                if (!string.IsNullOrEmpty(savedLanguage))
                {
                    SetLanguage(savedLanguage, skipSave: true);
                    if (_debugLogging)
                        Debug.Log($"Loaded saved language from SettingsManager: {savedLanguage}");
                    return;
                }
            }
            else
            {
                // SettingsManager not ready yet - event handler will retry when ready
                if (_debugLogging)
                    Debug.Log("LocalizationManager: SettingsManager not ready, will wait for OnSettingsLoaded event");
                return;
            }

            // If using system language, try to detect it
            if (_useSystemLanguage)
            {
                string systemLanguage = GetSystemLanguageCode();
                if (IsLanguageSupported(systemLanguage))
                {
                    SetLanguage(systemLanguage, skipSave: true);
                    if (_debugLogging)
                        Debug.Log($"Using system language: {systemLanguage}");
                    return;
                }
            }

            // Default to English if no preference or unsupported system language
            SetLanguage(LOCALE_ENGLISH, skipSave: true);
            if (_debugLogging)
                Debug.Log($"Using default language: {LOCALE_ENGLISH}");
        }

        /// <summary>
        /// Subscribes to SettingsManager events for proper coordination during startup.
        /// AIDEV-NOTE: Uses event-driven approach instead of polling to wait for settings to load.
        /// </summary>
        private void SubscribeToSettingsManagerEvents()
        {
            // Subscribe to SettingsManager events to know when settings are loaded
            SettingsManager.OnSettingsLoaded += OnSettingsManagerLoaded;
        }

        /// <summary>
        /// Event handler called when SettingsManager finishes loading settings.
        /// AIDEV-NOTE: Retries language setup now that settings are available.
        /// </summary>
        private void OnSettingsManagerLoaded(SettingsData settingsData)
        {
            if (_debugLogging)
                Debug.Log("LocalizationManager: SettingsManager loaded, retrying language setup");
            
            // Now that SettingsManager is ready, retry language setup
            SetupInitialLocale();
        }

        public bool SetLanguage(string localeCode, bool skipSave = false)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("LocalizationManager not initialized yet");
                return false;
            }

            try
            {
                var locale = GetLocaleByCode(localeCode);
                if (locale == null)
                {
                    Debug.LogError($"Locale not found: {localeCode}");
                    return false;
                }

                LocalizationSettings.SelectedLocale = locale;
                
                // Only save if not skipping (to avoid redundant saves during initialization)
                if (!skipSave)
                {
                    SaveLanguage(localeCode);
                }

                if (_debugLogging)
                    Debug.Log($"Language changed to: {locale.LocaleName} ({localeCode})");

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to set language to {localeCode}: {ex.Message}");
                return false;
            }
        }

        public bool IsLanguageSupported(string localeCode)
        {
            return GetLocaleByCode(localeCode) != null;
        }

        public string[] GetSupportedLanguageCodes()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            string[] codes = new string[locales.Count];
            
            for (int i = 0; i < locales.Count; i++)
            {
                codes[i] = locales[i].Identifier.Code;
            }
            
            return codes;
        }

        public string[] GetSupportedLanguageNames()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            string[] names = new string[locales.Count];
            
            for (int i = 0; i < locales.Count; i++)
            {
                names[i] = locales[i].LocaleName;
            }
            
            return names;
        }

        private Locale GetLocaleByCode(string localeCode)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            foreach (var locale in locales)
            {
                if (locale.Identifier.Code == localeCode)
                    return locale;
            }
            return null;
        }

        private string GetSystemLanguageCode()
        {
            // Convert Unity's SystemLanguage to locale codes
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    return LOCALE_ENGLISH;
                case SystemLanguage.Portuguese:
                    return LOCALE_PORTUGUESE;
                default:
                    return LOCALE_ENGLISH; // Default fallback
            }
        }

        private void OnLocaleChanged(Locale newLocale)
        {
            if (_debugLogging)
                Debug.Log($"Locale changed to: {newLocale?.LocaleName ?? "None"}");

            // Notify other systems about language change
            OnLanguageChanged?.Invoke(newLocale);
        }

        private void SaveLanguage(string localeCode)
        {
            // AIDEV-NOTE: Save language through SettingsManager instead of PlayerPrefs for consistency
            var settingsManager = FindFirstObjectByType<SettingsManager>();
            if (settingsManager != null && settingsManager.IsInitialized)
            {
                var currentSettings = settingsManager.CurrentSettings;
                if (currentSettings != null)
                {
                    Debug.Log($"LocalizationManager: Saving language '{localeCode}' through SettingsManager");
                    currentSettings.SetLocaleCode(localeCode);
                    _ = settingsManager.ForceSaveAsync();
                }
                else
                {
                    Debug.LogError("LocalizationManager: SettingsManager.CurrentSettings is null");
                }
            }
            else
            {
                // Fallback to PlayerPrefs if SettingsManager is not available
                Debug.LogWarning($"LocalizationManager: SettingsManager not available, saving '{localeCode}' to PlayerPrefs");
                PlayerPrefs.SetString("SelectedLanguage", localeCode);
                PlayerPrefs.Save();
            }
        }

        private string GetSavedLanguage()
        {
            // AIDEV-NOTE: Get language from SettingsManager instead of PlayerPrefs
            var settingsManager = FindFirstObjectByType<SettingsManager>();
            if (settingsManager != null && settingsManager.IsInitialized)
            {
                var currentSettings = settingsManager.CurrentSettings;
                if (currentSettings != null)
                {
                    string savedLanguage = currentSettings.LocaleCode;
                    Debug.Log($"LocalizationManager: Got saved language '{savedLanguage}' from SettingsManager");
                    
                    // Handle legacy locale codes that might be saved from previous versions
                    if (savedLanguage == "en-US")
                    {
                        savedLanguage = LOCALE_ENGLISH;
                        currentSettings.SetLocaleCode(savedLanguage);
                    }
                    
                    return savedLanguage;
                }
                else
                {
                    Debug.LogError("LocalizationManager: SettingsManager.CurrentSettings is null when getting saved language");
                }
            }
            else
            {
                Debug.LogWarning("LocalizationManager: SettingsManager not available when getting saved language");
            }
            
            // Fallback to PlayerPrefs if SettingsManager is not available
            string fallbackLanguage = PlayerPrefs.GetString("SelectedLanguage", "");
            Debug.Log($"LocalizationManager: Got fallback language '{fallbackLanguage}' from PlayerPrefs");
            
            // Handle legacy locale codes that might be saved from previous versions
            if (fallbackLanguage == "en-US")
            {
                fallbackLanguage = LOCALE_ENGLISH;
            }
            
            return fallbackLanguage;
        }

        // Utility method for getting localized strings programmatically
        public string GetLocalizedString(string tableCollection, string entryKey)
        {
            var localizedString = new LocalizedString(tableCollection, entryKey);
            return localizedString.GetLocalizedString();
        }

        // Utility method for getting localized strings with variables
        public string GetLocalizedString(string tableCollection, string entryKey, params object[] arguments)
        {
            var localizedString = new LocalizedString(tableCollection, entryKey);
            return localizedString.GetLocalizedString(arguments);
        }

        /// <summary>
        /// AIDEV-NOTE: Public method to refresh language from SettingsManager when it becomes available
        /// </summary>
        public void RefreshLanguageFromSettings()
        {
            if (!IsInitialized) return;
            
            string savedLanguage = GetSavedLanguage();
            if (!string.IsNullOrEmpty(savedLanguage) && savedLanguage != CurrentLocale?.Identifier.Code)
            {
                SetLanguage(savedLanguage, skipSave: true);
                if (_debugLogging)
                    Debug.Log($"LocalizationManager: Refreshed language from settings to {savedLanguage}");
            }
        }

        private void OnDestroy()
        {
            // Cancel any pending Invoke calls to prevent accessing SettingsManager during cleanup
            CancelInvoke();
            
            // Unsubscribe from SettingsManager events
            SettingsManager.OnSettingsLoaded -= OnSettingsManagerLoaded;
            
            if (IsInitialized)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            }
            
            // Clear the instance reference
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #if UNITY_EDITOR
        [ContextMenu("Force Reinitialize")]
        private void ForceReinitialize()
        {
            IsInitialized = false;
            InitializeLocalization();
        }

        [ContextMenu("Print Current Locale Info")]
        private void PrintCurrentLocaleInfo()
        {
            if (CurrentLocale != null)
            {
                Debug.Log($"Current Locale: {CurrentLocale.LocaleName} ({CurrentLocale.Identifier.Code})");
                Debug.Log($"Supported Languages: {string.Join(", ", GetSupportedLanguageCodes())}");
            }
            else
            {
                Debug.Log("No locale currently selected");
            }
        }
        #endif
    }
}