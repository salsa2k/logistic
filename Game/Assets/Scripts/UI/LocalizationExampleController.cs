using UnityEngine;
using UnityEngine.UIElements;
using LogisticGame.Managers;

namespace LogisticGame.UI
{
    // AIDEV-NOTE: Example controller demonstrating localization integration with UI Toolkit
    public class LocalizationExampleController : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument _uiDocument;

        // UI Elements
        private DropdownField _languageDropdown;
        private Button _newGameButton;
        private Button _settingsButton;
        private Label _titleLabel;

        private void OnEnable()
        {
            SetupUIElements();
            SetupLanguageDropdown();
            
            // Subscribe to language change events
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            
            // Unsubscribe from UI events
            if (_languageDropdown != null)
                _languageDropdown.RegisterCallback<ChangeEvent<string>>(OnLanguageDropdownChanged);
        }

        private void SetupUIElements()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
                if (_uiDocument == null)
                {
                    Debug.LogError("UIDocument component not found!");
                    return;
                }
            }

            var root = _uiDocument.rootVisualElement;

            // Get UI elements
            _languageDropdown = root.Q<DropdownField>("language-dropdown");
            _newGameButton = root.Q<Button>("new-game-button");
            _settingsButton = root.Q<Button>("settings-button");
            _titleLabel = root.Q<Label>("title-label");

            // Setup button click handlers (optional - for demo purposes)
            if (_newGameButton != null)
            {
                _newGameButton.clicked += OnNewGameClicked;
            }

            if (_settingsButton != null)
            {
                _settingsButton.clicked += OnSettingsClicked;
            }
        }

        private void SetupLanguageDropdown()
        {
            if (_languageDropdown == null || LocalizationManager.Instance == null)
                return;

            // Wait for localization to initialize
            if (!LocalizationManager.Instance.IsInitialized)
            {
                // Try again in a moment
                Invoke(nameof(SetupLanguageDropdown), 0.1f);
                return;
            }

            // Get supported languages
            string[] languageCodes = LocalizationManager.Instance.GetSupportedLanguageCodes();
            string[] languageNames = LocalizationManager.Instance.GetSupportedLanguageNames();

            // Setup dropdown choices
            _languageDropdown.choices.Clear();
            for (int i = 0; i < languageNames.Length; i++)
            {
                _languageDropdown.choices.Add(languageNames[i]);
            }

            // Set current selection
            var currentLocale = LocalizationManager.Instance.CurrentLocale;
            if (currentLocale != null)
            {
                for (int i = 0; i < languageCodes.Length; i++)
                {
                    if (languageCodes[i] == currentLocale.Identifier.Code)
                    {
                        _languageDropdown.value = languageNames[i];
                        break;
                    }
                }
            }

            // Register for changes
            _languageDropdown.RegisterCallback<ChangeEvent<string>>(OnLanguageDropdownChanged);

            Debug.Log($"Language dropdown setup complete. Available languages: {string.Join(", ", languageNames)}");
        }

        private void OnLanguageDropdownChanged(ChangeEvent<string> evt)
        {
            string selectedLanguageName = evt.newValue;
            
            // Find the corresponding language code
            string[] languageCodes = LocalizationManager.Instance.GetSupportedLanguageCodes();
            string[] languageNames = LocalizationManager.Instance.GetSupportedLanguageNames();

            for (int i = 0; i < languageNames.Length; i++)
            {
                if (languageNames[i] == selectedLanguageName)
                {
                    string languageCode = languageCodes[i];
                    
                    Debug.Log($"Changing language to: {selectedLanguageName} ({languageCode})");
                    
                    // Change the language - UI will update automatically via data binding
                    bool success = LocalizationManager.Instance.SetLanguage(languageCode);
                    
                    if (!success)
                    {
                        Debug.LogError($"Failed to change language to {languageCode}");
                    }
                    
                    break;
                }
            }
        }

        private void OnLanguageChanged(UnityEngine.Localization.Locale newLocale)
        {
            Debug.Log($"Language changed notification received: {newLocale?.LocaleName ?? "None"}");
            
            // Note: UI Toolkit elements with LocalizedString bindings will update automatically
            // This method can be used for any custom logic needed when language changes
            
            // Example: Update dropdown selection if changed externally
            if (_languageDropdown != null && newLocale != null)
            {
                string[] languageNames = LocalizationManager.Instance.GetSupportedLanguageNames();
                string[] languageCodes = LocalizationManager.Instance.GetSupportedLanguageCodes();
                
                for (int i = 0; i < languageCodes.Length; i++)
                {
                    if (languageCodes[i] == newLocale.Identifier.Code)
                    {
                        _languageDropdown.SetValueWithoutNotify(languageNames[i]);
                        break;
                    }
                }
            }
        }

        private void OnNewGameClicked()
        {
            Debug.Log("New Game button clicked!");
            
            // Example of getting localized string programmatically
            if (LocalizationManager.Instance != null)
            {
                string localizedText = LocalizationManager.Instance.GetLocalizedString("MainMenu", "NewGame_Button");
                Debug.Log($"Button text in current language: {localizedText}");
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Settings button clicked!");
            
            // Example of getting localized string programmatically
            if (LocalizationManager.Instance != null)
            {
                string localizedText = LocalizationManager.Instance.GetLocalizedString("MainMenu", "Settings_Button");
                Debug.Log($"Button text in current language: {localizedText}");
            }
        }

        #if UNITY_EDITOR
        [ContextMenu("Test Localization")]
        private void TestLocalization()
        {
            if (LocalizationManager.Instance == null)
            {
                Debug.LogError("LocalizationManager not available");
                return;
            }

            Debug.Log("=== Localization Test ===");
            Debug.Log($"Current Locale: {LocalizationManager.Instance.CurrentLocale?.LocaleName ?? "None"}");
            Debug.Log($"Supported Languages: {string.Join(", ", LocalizationManager.Instance.GetSupportedLanguageCodes())}");
            
            // Test getting localized strings
            string newGameText = LocalizationManager.Instance.GetLocalizedString("MainMenu", "NewGame_Button");
            string settingsText = LocalizationManager.Instance.GetLocalizedString("MainMenu", "Settings_Button");
            
            Debug.Log($"New Game Button: {newGameText}");
            Debug.Log($"Settings Button: {settingsText}");
        }

        [ContextMenu("Switch to Portuguese")]
        private void SwitchToPortuguese()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguage(LocalizationManager.LOCALE_PORTUGUESE);
            }
        }

        [ContextMenu("Switch to English")]
        private void SwitchToEnglish()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguage(LocalizationManager.LOCALE_ENGLISH);
            }
        }
        #endif
    }
}