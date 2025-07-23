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

        private void OnLocalizationInitialized()
        {
            try
            {
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
            // Try to load saved language preference
            string savedLanguage = GetSavedLanguage();
            
            if (!string.IsNullOrEmpty(savedLanguage))
            {
                SetLanguage(savedLanguage);
                if (_debugLogging)
                    Debug.Log($"Loaded saved language: {savedLanguage}");
                return;
            }

            // If using system language, try to detect it
            if (_useSystemLanguage)
            {
                string systemLanguage = GetSystemLanguageCode();
                if (IsLanguageSupported(systemLanguage))
                {
                    SetLanguage(systemLanguage);
                    if (_debugLogging)
                        Debug.Log($"Using system language: {systemLanguage}");
                    return;
                }
            }

            // Default to English if no preference or unsupported system language
            SetLanguage(LOCALE_ENGLISH);
            if (_debugLogging)
                Debug.Log($"Using default language: {LOCALE_ENGLISH}");
        }

        public bool SetLanguage(string localeCode)
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
                SaveLanguage(localeCode);

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
            PlayerPrefs.SetString("SelectedLanguage", localeCode);
            PlayerPrefs.Save();
        }

        private string GetSavedLanguage()
        {
            string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "");
            
            // Handle legacy locale codes that might be saved from previous versions
            if (savedLanguage == "en-US")
            {
                savedLanguage = LOCALE_ENGLISH;
                SaveLanguage(savedLanguage); // Update the saved preference
            }
            
            return savedLanguage;
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

        private void OnDestroy()
        {
            if (IsInitialized)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
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