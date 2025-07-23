using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using LogisticGame.Events;
using LogisticGame.Managers;

namespace LogisticGame.UI
{
    /// <summary>
    /// AIDEV-NOTE: Controller for the settings modal dialog. Manages all settings sections,
    /// handles immediate UI updates, and integrates with the save system for persistence.
    /// </summary>
    public class SettingsModalController : BaseModal
    {
        [Header("Settings Configuration")]
        [SerializeField] private SettingsData _settingsData;
        
        [Header("Styling")]
        [SerializeField] private StyleSheet _darkGraphiteTheme;
        [SerializeField] private StyleSheet _baseWindowStyles;
        [SerializeField] private StyleSheet _settingsModalStyles;
        // AIDEV-NOTE: _debugMode is inherited from BaseWindow through BaseModal
        
        // UI Elements - Language Section
        private DropdownField _languageDropdown;
        
        // UI Elements - Units Section
        private Toggle _metricToggle;
        private Toggle _timeFormatToggle;
        
        // UI Elements - Audio Section
        private Slider _masterVolumeSlider;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Toggle _audioEnabledToggle;
        
        // UI Elements - Gameplay Section
        private Slider _gameSpeedSlider;
        private Toggle _autopauseFuelToggle;
        private Toggle _autopauseContractToggle;
        private Toggle _confirmDangerousToggle;
        
        // UI Elements - UI Section
        private Slider _uiScaleSlider;
        private Toggle _showTooltipsToggle;
        private Toggle _showNotificationsToggle;
        private Slider _tooltipDelaySlider;
        
        // UI Elements - Action Buttons
        private Button _resetDefaultsButton;
        private Button _cancelButton;
        private Button _applyButton;
        
        // State tracking
        private Dictionary<string, object> _originalValues = new Dictionary<string, object>();
        private Dictionary<string, object> _currentValues = new Dictionary<string, object>();
        private bool _hasChanges = false;
        
        // Language options
        private readonly List<string> _languageOptions = new List<string> { "English", "Português" };
        private readonly List<string> _languageCodes = new List<string> { "en", "pt-BR" };
        
        // Events
        public static event Action<SettingsData> OnSettingsApplied;
        public static event Action OnSettingsCancelled;
        public static event Action OnSettingsReset;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Ensure we have settings data
            if (_settingsData == null)
            {
                _settingsData = ScriptableObject.CreateInstance<SettingsData>();
                Debug.LogWarning("SettingsModalController: No SettingsData assigned, created temporary instance");
            }
        }
        
        protected override void Start()
        {
            base.Start();
            InitializeSettingsModal();
            SubscribeToLocalizationEvents();
        }
        
        /// <summary>
        /// Initializes the settings modal UI elements and binds events.
        /// AIDEV-NOTE: Sets up all UI elements, populates dropdowns, and configures event handlers.
        /// </summary>
        private void InitializeSettingsModal()
        {
            if (ContentContainer == null)
            {
                Debug.LogError("SettingsModalController: ContentContainer is null, cannot initialize");
                return;
            }
            
            // Set modal size and configuration
            ModalSize = ModalSize.Large;
            WindowTitle = "Settings";
            CloseOnBackdropClick = true;
            CloseOnEscapeKey = true;
            
            // Enable debug mode for troubleshooting
            #if UNITY_EDITOR
            _debugMode = true;
            #endif
            
            try
            {
                BindUIElements();
                SetupLanguageDropdown();
                SetupEventHandlers();
                LoadCurrentSettings();
                
                // Ensure proper modal styling is applied
                EnsureModalStyling();
                
                // Load initial localized text
                RefreshLocalizedText();
                
                if (_debugMode)
                    Debug.Log("SettingsModalController: Initialization complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsModalController: Failed to initialize - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ensures proper modal styling is applied to the settings modal.
        /// AIDEV-NOTE: Adds additional styling classes and verifies modal appearance.
        /// </summary>
        private void EnsureModalStyling()
        {
            if (_rootContainer != null)
            {
                // Load required stylesheets for proper modal appearance
                LoadRequiredStylesheets();
                
                // Ensure settings modal specific classes are applied
                _rootContainer.AddToClassList("settings-modal");
                
                // Make sure the modal has a visible background
                if (_windowContainer != null)
                {
                    _windowContainer.AddToClassList("settings-window");
                    
                    if (_debugMode)
                        Debug.Log("SettingsModalController: Applied settings-specific styling classes");
                }
            }
        }
        
        /// <summary>
        /// Loads required stylesheets for proper modal appearance.
        /// AIDEV-NOTE: Ensures theme variables and modal styles are available.
        /// </summary>
        private void LoadRequiredStylesheets()
        {
            if (_rootContainer?.styleSheets == null) return;
            
            // Load the dark graphite theme (provides CSS variables)
            if (_darkGraphiteTheme != null && !_rootContainer.styleSheets.Contains(_darkGraphiteTheme))
            {
                _rootContainer.styleSheets.Add(_darkGraphiteTheme);
                if (_debugMode)
                    Debug.Log("SettingsModalController: Added DarkGraphiteTheme stylesheet");
            }
            
            // Load base window styles (provides modal styling)
            if (_baseWindowStyles != null && !_rootContainer.styleSheets.Contains(_baseWindowStyles))
            {
                _rootContainer.styleSheets.Add(_baseWindowStyles);
                if (_debugMode)
                    Debug.Log("SettingsModalController: Added BaseWindowStyles stylesheet");
            }
            
            // Load settings modal specific styles
            if (_settingsModalStyles != null && !_rootContainer.styleSheets.Contains(_settingsModalStyles))
            {
                _rootContainer.styleSheets.Add(_settingsModalStyles);
                if (_debugMode)
                    Debug.Log("SettingsModalController: Added SettingsModalStyles stylesheet");
            }
            
            if (_debugMode)
                Debug.Log($"SettingsModalController: Total stylesheets loaded: {_rootContainer.styleSheets.count}");
        }
        
        /// <summary>
        /// Binds all UI elements from the UXML document.
        /// AIDEV-NOTE: Retrieves references to all interactive elements in the settings modal.
        /// </summary>
        private void BindUIElements()
        {
            var contentRoot = ContentContainer;
            var footerRoot = FooterContainer;
            
            // Language Section
            _languageDropdown = contentRoot?.Q<DropdownField>("language-dropdown");
            
            // Units Section
            _metricToggle = contentRoot?.Q<Toggle>("metric-toggle");
            _timeFormatToggle = contentRoot?.Q<Toggle>("time-format-toggle");
            
            // Audio Section
            _masterVolumeSlider = contentRoot?.Q<Slider>("master-volume-slider");
            _musicVolumeSlider = contentRoot?.Q<Slider>("music-volume-slider");
            _sfxVolumeSlider = contentRoot?.Q<Slider>("sfx-volume-slider");
            _audioEnabledToggle = contentRoot?.Q<Toggle>("audio-enabled-toggle");
            
            // Gameplay Section
            _gameSpeedSlider = contentRoot?.Q<Slider>("game-speed-slider");
            _autopauseFuelToggle = contentRoot?.Q<Toggle>("autopause-fuel-toggle");
            _autopauseContractToggle = contentRoot?.Q<Toggle>("autopause-contract-toggle");
            _confirmDangerousToggle = contentRoot?.Q<Toggle>("confirm-dangerous-toggle");
            
            // UI Section
            _uiScaleSlider = contentRoot?.Q<Slider>("ui-scale-slider");
            _showTooltipsToggle = contentRoot?.Q<Toggle>("show-tooltips-toggle");
            _showNotificationsToggle = contentRoot?.Q<Toggle>("show-notifications-toggle");
            _tooltipDelaySlider = contentRoot?.Q<Slider>("tooltip-delay-slider");
            
            // Action Buttons - These are in the footer container
            _resetDefaultsButton = footerRoot?.Q<Button>("reset-defaults-button");
            _cancelButton = footerRoot?.Q<Button>("cancel-button");
            _applyButton = footerRoot?.Q<Button>("apply-button");
            
            // Validate critical elements with better error messages
            var missingElements = new List<string>();
            if (_languageDropdown == null) missingElements.Add("language-dropdown");
            if (_applyButton == null) missingElements.Add("apply-button (footer)");
            if (_cancelButton == null) missingElements.Add("cancel-button (footer)");
            
            if (missingElements.Count > 0)
            {
                var errorMessage = $"Critical UI elements not found in settings modal: {string.Join(", ", missingElements)}";
                if (contentRoot == null) errorMessage += " | ContentContainer is null";
                if (footerRoot == null) errorMessage += " | FooterContainer is null";
                throw new InvalidOperationException(errorMessage);
            }
        }
        
        /// <summary>
        /// Sets up the language dropdown with available options.
        /// AIDEV-NOTE: Populates language dropdown with supported languages.
        /// </summary>
        private void SetupLanguageDropdown()
        {
            if (_languageDropdown == null) return;
            
            _languageDropdown.choices = _languageOptions;
            // Don't set index here - let LoadCurrentSettings() handle it based on actual settings
        }
        
        /// <summary>
        /// Sets up event handlers for all interactive elements.
        /// AIDEV-NOTE: Configures change detection and button callbacks for settings management.
        /// </summary>
        private void SetupEventHandlers()
        {
            // Language Section
            _languageDropdown?.RegisterValueChangedCallback(OnLanguageChanged);
            
            // Units Section
            _metricToggle?.RegisterValueChangedCallback(OnMetricToggleChanged);
            _timeFormatToggle?.RegisterValueChangedCallback(OnTimeFormatToggleChanged);
            
            // Audio Section
            _masterVolumeSlider?.RegisterValueChangedCallback(OnMasterVolumeChanged);
            _musicVolumeSlider?.RegisterValueChangedCallback(OnMusicVolumeChanged);
            _sfxVolumeSlider?.RegisterValueChangedCallback(OnSfxVolumeChanged);
            _audioEnabledToggle?.RegisterValueChangedCallback(OnAudioEnabledToggleChanged);
            
            // Gameplay Section
            _gameSpeedSlider?.RegisterValueChangedCallback(OnGameSpeedChanged);
            _autopauseFuelToggle?.RegisterValueChangedCallback(OnAutopauseFuelToggleChanged);
            _autopauseContractToggle?.RegisterValueChangedCallback(OnAutopauseContractToggleChanged);
            _confirmDangerousToggle?.RegisterValueChangedCallback(OnConfirmDangerousToggleChanged);
            
            // UI Section
            _uiScaleSlider?.RegisterValueChangedCallback(OnUiScaleChanged);
            _showTooltipsToggle?.RegisterValueChangedCallback(OnShowTooltipsToggleChanged);
            _showNotificationsToggle?.RegisterValueChangedCallback(OnShowNotificationsToggleChanged);
            _tooltipDelaySlider?.RegisterValueChangedCallback(OnTooltipDelayChanged);
            
            // Action Buttons
            _resetDefaultsButton?.RegisterCallback<ClickEvent>(OnResetDefaultsClicked);
            _cancelButton?.RegisterCallback<ClickEvent>(OnCancelClicked);
            _applyButton?.RegisterCallback<ClickEvent>(OnApplyClicked);
        }
        
        /// <summary>
        /// Loads current settings values into the UI.
        /// AIDEV-NOTE: Populates all UI elements with current settings data and stores original values.
        /// </summary>
        private void LoadCurrentSettings()
        {
            if (_settingsData == null) return;
            
            try
            {
                // Language settings - Use current locale from LocalizationManager if available
                string currentLocaleCode = _settingsData.LocaleCode;
                if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
                {
                    var currentLocale = LocalizationManager.Instance.CurrentLocale;
                    if (currentLocale != null)
                    {
                        currentLocaleCode = currentLocale.Identifier.Code;
                    }
                }
                
                var currentLanguageIndex = _languageCodes.IndexOf(currentLocaleCode);
                if (currentLanguageIndex >= 0)
                {
                    _languageDropdown.index = currentLanguageIndex;
                }
                else
                {
                    // Fallback to English if locale not found
                    currentLanguageIndex = _languageCodes.IndexOf("en");
                    if (currentLanguageIndex >= 0)
                    {
                        _languageDropdown.index = currentLanguageIndex;
                    }
                }
                StoreOriginalValue("language", currentLanguageIndex);
                
                // Units settings
                _metricToggle.value = _settingsData.ShowDistanceInKm;
                _timeFormatToggle.value = _settingsData.Show24HourTime;
                StoreOriginalValue("metric", _settingsData.ShowDistanceInKm);
                StoreOriginalValue("timeFormat", _settingsData.Show24HourTime);
                
                // Audio settings
                _masterVolumeSlider.value = _settingsData.MasterVolume;
                _musicVolumeSlider.value = _settingsData.MusicVolume;
                _sfxVolumeSlider.value = _settingsData.SfxVolume;
                _audioEnabledToggle.value = _settingsData.AudioEnabled;
                StoreOriginalValue("masterVolume", _settingsData.MasterVolume);
                StoreOriginalValue("musicVolume", _settingsData.MusicVolume);
                StoreOriginalValue("sfxVolume", _settingsData.SfxVolume);
                StoreOriginalValue("audioEnabled", _settingsData.AudioEnabled);
                
                // Gameplay settings
                _gameSpeedSlider.value = _settingsData.GameSpeed;
                _autopauseFuelToggle.value = _settingsData.AutopauseOnLowFuel;
                _autopauseContractToggle.value = _settingsData.AutopauseOnContractExpiry;
                _confirmDangerousToggle.value = _settingsData.ConfirmDangerousActions;
                StoreOriginalValue("gameSpeed", _settingsData.GameSpeed);
                StoreOriginalValue("autopauseFuel", _settingsData.AutopauseOnLowFuel);
                StoreOriginalValue("autopauseContract", _settingsData.AutopauseOnContractExpiry);
                StoreOriginalValue("confirmDangerous", _settingsData.ConfirmDangerousActions);
                
                // UI settings
                _uiScaleSlider.value = _settingsData.UiScale;
                _showTooltipsToggle.value = _settingsData.ShowTooltips;
                _showNotificationsToggle.value = _settingsData.ShowNotifications;
                _tooltipDelaySlider.value = _settingsData.TooltipDelay;
                StoreOriginalValue("uiScale", _settingsData.UiScale);
                StoreOriginalValue("showTooltips", _settingsData.ShowTooltips);
                StoreOriginalValue("showNotifications", _settingsData.ShowNotifications);
                StoreOriginalValue("tooltipDelay", _settingsData.TooltipDelay);
                
                // Copy original values to current values
                _currentValues = new Dictionary<string, object>(_originalValues);
                
                UpdateApplyButtonState();
                
                if (_debugMode)
                    Debug.Log("SettingsModalController: Settings loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsModalController: Failed to load settings - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Stores an original value for change detection.
        /// AIDEV-NOTE: Helper method to track original setting values for comparison.
        /// </summary>
        private void StoreOriginalValue(string key, object value)
        {
            _originalValues[key] = value;
        }
        
        /// <summary>
        /// Updates the current value and checks for changes.
        /// AIDEV-NOTE: Updates tracking dictionaries and enables/disables apply button based on changes.
        /// </summary>
        private void UpdateCurrentValue(string key, object value)
        {
            _currentValues[key] = value;
            CheckForChanges();
        }
        
        /// <summary>
        /// Checks if any settings have changed from their original values.
        /// AIDEV-NOTE: Compares current values with original values to enable/disable apply button.
        /// </summary>
        private void CheckForChanges()
        {
            _hasChanges = false;
            
            foreach (var kvp in _originalValues)
            {
                if (!_currentValues.ContainsKey(kvp.Key) || 
                    !Equals(_currentValues[kvp.Key], kvp.Value))
                {
                    _hasChanges = true;
                    break;
                }
            }
            
            UpdateApplyButtonState();
        }
        
        /// <summary>
        /// Updates the apply button state based on changes.
        /// AIDEV-NOTE: Enables apply button only when settings have been modified.
        /// </summary>
        private void UpdateApplyButtonState()
        {
            if (_applyButton != null)
            {
                _applyButton.SetEnabled(_hasChanges);
            }
        }
        
        // Event Handlers - Language Section
        private void OnLanguageChanged(ChangeEvent<string> evt)
        {
            var selectedIndex = _languageDropdown.index;
            UpdateCurrentValue("language", selectedIndex);
            
            // Apply language change immediately for instant feedback
            if (selectedIndex >= 0 && selectedIndex < _languageCodes.Count)
            {
                var localeCode = _languageCodes[selectedIndex];
                ApplyLanguageChange(localeCode);
            }
        }
        
        // Event Handlers - Units Section
        private void OnMetricToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("metric", evt.newValue);
        }
        
        private void OnTimeFormatToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("timeFormat", evt.newValue);
        }
        
        // Event Handlers - Audio Section
        private void OnMasterVolumeChanged(ChangeEvent<float> evt)
        {
            UpdateCurrentValue("masterVolume", evt.newValue);
        }
        
        private void OnMusicVolumeChanged(ChangeEvent<float> evt)
        {
            UpdateCurrentValue("musicVolume", evt.newValue);
        }
        
        private void OnSfxVolumeChanged(ChangeEvent<float> evt)
        {
            UpdateCurrentValue("sfxVolume", evt.newValue);
        }
        
        private void OnAudioEnabledToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("audioEnabled", evt.newValue);
        }
        
        // Event Handlers - Gameplay Section
        private void OnGameSpeedChanged(ChangeEvent<float> evt)
        {
            UpdateCurrentValue("gameSpeed", evt.newValue);
        }
        
        private void OnAutopauseFuelToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("autopauseFuel", evt.newValue);
        }
        
        private void OnAutopauseContractToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("autopauseContract", evt.newValue);
        }
        
        private void OnConfirmDangerousToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("confirmDangerous", evt.newValue);
        }
        
        // Event Handlers - UI Section
        private void OnUiScaleChanged(ChangeEvent<float> evt)
        {
            UpdateCurrentValue("uiScale", evt.newValue);
        }
        
        private void OnShowTooltipsToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("showTooltips", evt.newValue);
        }
        
        private void OnShowNotificationsToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateCurrentValue("showNotifications", evt.newValue);
        }
        
        private void OnTooltipDelayChanged(ChangeEvent<float> evt)
        {
            UpdateCurrentValue("tooltipDelay", evt.newValue);
        }
        
        // Action Button Handlers
        private void OnResetDefaultsClicked(ClickEvent evt)
        {
            ShowResetConfirmation();
        }
        
        private void OnCancelClicked(ClickEvent evt)
        {
            if (_hasChanges)
            {
                ShowCancelConfirmation();
            }
            else
            {
                CancelSettings();
            }
        }
        
        private void OnApplyClicked(ClickEvent evt)
        {
            ApplySettings();
        }
        
        /// <summary>
        /// Shows confirmation dialog for resetting settings to defaults.
        /// AIDEV-NOTE: Displays modal confirmation before applying default settings.
        /// </summary>
        private void ShowResetConfirmation()
        {
            // AIDEV-TODO: Implement confirmation modal for reset
            // For now, reset directly with debug output
            if (_debugMode)
                Debug.Log("SettingsModalController: Reset to defaults requested");
            
            ResetToDefaults();
        }
        
        /// <summary>
        /// Shows confirmation dialog for cancelling changes.
        /// AIDEV-NOTE: Displays modal confirmation before discarding unsaved changes.
        /// </summary>
        private void ShowCancelConfirmation()
        {
            // AIDEV-TODO: Implement confirmation modal for cancel
            // For now, cancel directly with debug output
            if (_debugMode)
                Debug.Log("SettingsModalController: Cancel with changes requested");
            
            CancelSettings();
        }
        
        /// <summary>
        /// Resets all settings to their default values.
        /// AIDEV-NOTE: Loads default settings and updates UI to reflect changes.
        /// </summary>
        private void ResetToDefaults()
        {
            try
            {
                _settingsData.LoadDefaults();
                LoadCurrentSettings();
                
                OnSettingsReset?.Invoke();
                
                EventBus.Publish(new NotificationRequestedEvent(
                    NotificationType.Info,
                    "Settings reset to defaults",
                    3f
                ));
                
                if (_debugMode)
                    Debug.Log("SettingsModalController: Settings reset to defaults");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsModalController: Failed to reset settings - {ex.Message}");
                
                EventBus.Publish(new NotificationRequestedEvent(
                    NotificationType.Error,
                    "Failed to reset settings",
                    4f
                ));
            }
        }
        
        /// <summary>
        /// Cancels settings changes and closes the modal.
        /// AIDEV-NOTE: Discards all changes and returns settings to original state.
        /// </summary>
        private void CancelSettings()
        {
            try
            {
                // Restore original language if it was changed
                if (_originalValues.ContainsKey("language") && _currentValues.ContainsKey("language"))
                {
                    var originalLanguageIndex = (int)_originalValues["language"];
                    var currentLanguageIndex = (int)_currentValues["language"];
                    
                    if (originalLanguageIndex != currentLanguageIndex && 
                        originalLanguageIndex >= 0 && originalLanguageIndex < _languageCodes.Count)
                    {
                        var originalLocaleCode = _languageCodes[originalLanguageIndex];
                        ApplyLanguageChange(originalLocaleCode);
                    }
                }
                
                OnSettingsCancelled?.Invoke();
                HideWindow();
                
                if (_debugMode)
                    Debug.Log("SettingsModalController: Settings cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsModalController: Failed to cancel settings - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Applies all current settings and saves them.
        /// AIDEV-NOTE: Updates SettingsData with current values and persists changes.
        /// </summary>
        private void ApplySettings()
        {
            try
            {
                // Apply all current values to the settings data
                if (_currentValues.ContainsKey("language"))
                {
                    var languageIndex = (int)_currentValues["language"];
                    if (languageIndex >= 0 && languageIndex < _languageCodes.Count)
                    {
                        var localeCode = _languageCodes[languageIndex];
                        ApplyLanguageChange(localeCode);
                    }
                }
                
                // Units settings
                if (_currentValues.ContainsKey("metric"))
                    _settingsData.SetShowDistanceInKm((bool)_currentValues["metric"]);
                if (_currentValues.ContainsKey("timeFormat"))
                    _settingsData.SetShow24HourTime((bool)_currentValues["timeFormat"]);
                
                // Audio settings
                if (_currentValues.ContainsKey("masterVolume"))
                    _settingsData.SetMasterVolume((float)_currentValues["masterVolume"]);
                if (_currentValues.ContainsKey("musicVolume"))
                    _settingsData.SetMusicVolume((float)_currentValues["musicVolume"]);
                if (_currentValues.ContainsKey("sfxVolume"))
                    _settingsData.SetSfxVolume((float)_currentValues["sfxVolume"]);
                if (_currentValues.ContainsKey("audioEnabled"))
                    _settingsData.SetAudioEnabled((bool)_currentValues["audioEnabled"]);
                
                // Gameplay settings
                if (_currentValues.ContainsKey("gameSpeed"))
                    _settingsData.SetGameSpeed((float)_currentValues["gameSpeed"]);
                if (_currentValues.ContainsKey("autopauseFuel"))
                    _settingsData.SetAutopauseOnLowFuel((bool)_currentValues["autopauseFuel"]);
                if (_currentValues.ContainsKey("autopauseContract"))
                    _settingsData.SetAutopauseOnContractExpiry((bool)_currentValues["autopauseContract"]);
                if (_currentValues.ContainsKey("confirmDangerous"))
                    _settingsData.SetConfirmDangerousActions((bool)_currentValues["confirmDangerous"]);
                
                // UI settings
                if (_currentValues.ContainsKey("uiScale"))
                    _settingsData.SetUiScale((float)_currentValues["uiScale"]);
                if (_currentValues.ContainsKey("showTooltips"))
                    _settingsData.SetShowTooltips((bool)_currentValues["showTooltips"]);
                if (_currentValues.ContainsKey("showNotifications"))
                    _settingsData.SetShowNotifications((bool)_currentValues["showNotifications"]);
                if (_currentValues.ContainsKey("tooltipDelay"))
                    _settingsData.SetTooltipDelay((float)_currentValues["tooltipDelay"]);
                
                // Apply settings to the system
                _settingsData.ApplySettings();
                
                // Save settings (integration with save system would go here)
                // AIDEV-TODO: Integrate with save system for persistence
                
                // Update original values to current values
                _originalValues = new Dictionary<string, object>(_currentValues);
                _hasChanges = false;
                UpdateApplyButtonState();
                
                OnSettingsApplied?.Invoke(_settingsData);
                
                EventBus.Publish(new NotificationRequestedEvent(
                    NotificationType.Success,
                    "Settings applied successfully",
                    3f
                ));
                
                HideWindow();
                
                if (_debugMode)
                    Debug.Log("SettingsModalController: Settings applied successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsModalController: Failed to apply settings - {ex.Message}");
                
                EventBus.Publish(new NotificationRequestedEvent(
                    NotificationType.Error,
                    "Failed to apply settings",
                    4f
                ));
            }
        }
        
        /// <summary>
        /// Applies language change immediately for instant feedback.
        /// AIDEV-NOTE: Updates localization system immediately when language is changed.
        /// </summary>
        private void ApplyLanguageChange(string localeCode)
        {
            try
            {
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.SetLanguage(localeCode);
                    
                    // Also update the settings data to keep it synchronized
                    if (_settingsData != null)
                    {
                        _settingsData.SetLocaleCode(localeCode);
                    }
                    
                    if (_debugMode)
                        Debug.Log($"SettingsModalController: Language changed to {localeCode}");
                }
                else
                {
                    Debug.LogWarning("SettingsModalController: LocalizationManager not available for language change");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsModalController: Failed to change language - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Public method to show the settings modal with current settings.
        /// AIDEV-NOTE: Entry point for opening the settings modal from other UI systems.
        /// </summary>
        public void ShowSettings(SettingsData settingsData = null)
        {
            if (settingsData != null)
            {
                _settingsData = settingsData;
            }
            else
            {
                // Fallback to current settings from SettingsManager if none provided
                _settingsData = SettingsManager.Instance?.GetCurrentSettings();
            }
            
            // Ensure we have valid settings data
            if (_settingsData == null)
            {
                _settingsData = ScriptableObject.CreateInstance<SettingsData>();
                _settingsData.LoadDefaults();
                Debug.LogWarning("SettingsModalController: No settings data available, using defaults");
            }
            
            LoadCurrentSettings();
            ShowWindow();
            
            if (_debugMode)
                Debug.Log("SettingsModalController: Settings modal shown");
        }
        
        /// <summary>
        /// Validates settings values before applying.
        /// AIDEV-NOTE: Ensures all settings are within valid ranges and combinations.
        /// </summary>
        private bool ValidateSettings()
        {
            // AIDEV-TODO: Implement comprehensive settings validation
            // For now, basic validation using SettingsData constraints
            
            try
            {
                // Volume validation
                if (_currentValues.ContainsKey("masterVolume"))
                {
                    var volume = (float)_currentValues["masterVolume"];
                    if (volume < 0f || volume > 1f)
                        return false;
                }
                
                // Game speed validation
                if (_currentValues.ContainsKey("gameSpeed"))
                {
                    var speed = (float)_currentValues["gameSpeed"];
                    if (speed < 0.1f || speed > 5f)
                        return false;
                }
                
                // UI scale validation
                if (_currentValues.ContainsKey("uiScale"))
                {
                    var scale = (float)_currentValues["uiScale"];
                    if (scale < 0.5f || scale > 2f)
                        return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SettingsModalController: Settings validation failed - {ex.Message}");
                return false;
            }
        }
        
        protected override void OnDestroy()
        {
            // Clean up event handlers
            _originalValues?.Clear();
            _currentValues?.Clear();
            UnsubscribeFromLocalizationEvents();
            
            base.OnDestroy();
        }
        
        /// <summary>
        /// Subscribes to localization events for language change updates.
        /// AIDEV-NOTE: Ensures settings UI text updates when language is changed.
        /// </summary>
        private void SubscribeToLocalizationEvents()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.OnLanguageChanged += OnLocalizationLanguageChanged;
            }
        }
        
        /// <summary>
        /// Unsubscribes from localization events to prevent memory leaks.
        /// AIDEV-NOTE: Called in OnDestroy to clean up event subscriptions.
        /// </summary>
        private void UnsubscribeFromLocalizationEvents()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.OnLanguageChanged -= OnLocalizationLanguageChanged;
            }
        }
        
        /// <summary>
        /// Handles language change events by refreshing localized UI text.
        /// AIDEV-NOTE: Updates all settings modal text to reflect the new language selection.
        /// </summary>
        private void OnLocalizationLanguageChanged(UnityEngine.Localization.Locale newLocale)
        {
            RefreshLocalizedText();
        }
        
        /// <summary>
        /// Refreshes all localized text elements in the settings modal.
        /// AIDEV-NOTE: Gets current localized strings and updates UI text programmatically.
        /// </summary>
        private void RefreshLocalizedText()
        {
            if (LocalizationManager.Instance == null || !LocalizationManager.Instance.IsInitialized)
                return;
                
            try
            {
                // Update modal title
                var titleLabel = _rootContainer?.Q<Label>("window-title");
                if (titleLabel != null)
                    titleLabel.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Title");
                
                // Update section headers by finding them directly
                var languageSection = ContentContainer?.Q<VisualElement>("language-section");
                var languageHeader = languageSection?.Q<Label>(className: "settings-header");
                if (languageHeader != null)
                    languageHeader.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Language_Header");
                    
                var unitsSection = ContentContainer?.Q<VisualElement>("units-section");
                var unitsHeader = unitsSection?.Q<Label>(className: "settings-header");
                if (unitsHeader != null)
                    unitsHeader.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Units_Header");
                    
                var audioSection = ContentContainer?.Q<VisualElement>("audio-section");
                var audioHeader = audioSection?.Q<Label>(className: "settings-header");
                if (audioHeader != null)
                    audioHeader.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Audio_Header");
                    
                var gameplaySection = ContentContainer?.Q<VisualElement>("gameplay-section");
                var gameplayHeader = gameplaySection?.Q<Label>(className: "settings-header");
                if (gameplayHeader != null)
                    gameplayHeader.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Gameplay_Header");
                    
                var interfaceSection = ContentContainer?.Q<VisualElement>("ui-section");
                var interfaceHeader = interfaceSection?.Q<Label>(className: "settings-header");
                if (interfaceHeader != null)
                    interfaceHeader.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Interface_Header");
                
                // Update individual setting labels and toggle texts
                UpdateLanguageSectionText();
                UpdateUnitsSectionText();
                UpdateAudioSectionText();
                UpdateGameplaySectionText();
                UpdateInterfaceSectionText();
                
                // Update footer buttons
                if (_resetDefaultsButton != null)
                    _resetDefaultsButton.text = LocalizationManager.Instance.GetLocalizedString("Settings", "ResetDefaults_Button");
                    
                if (_cancelButton != null)
                    _cancelButton.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Cancel_Button");
                    
                if (_applyButton != null)
                    _applyButton.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Apply_Button");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"SettingsModalController: Failed to refresh localized text - {ex.Message}");
            }
        }
        
        /// <summary>
        /// Updates localized text for language section elements.
        /// AIDEV-NOTE: Helper method to update language section labels.
        /// </summary>
        private void UpdateLanguageSectionText()
        {
            var languageSection = ContentContainer?.Q<VisualElement>("language-section");
            if (languageSection == null) return;
            
            // Language dropdown label
            var languageLabel = languageSection.Q<Label>(className: "settings-label");
            if (languageLabel != null)
                languageLabel.text = LocalizationManager.Instance.GetLocalizedString("Settings", "Language_Dropdown_Label");
        }
        
        /// <summary>
        /// Updates localized text for units section elements.
        /// AIDEV-NOTE: Helper method to update units section labels and toggles.
        /// </summary>
        private void UpdateUnitsSectionText()
        {
            var unitsSection = ContentContainer?.Q<VisualElement>("units-section");
            if (unitsSection == null) return;
            
            var labels = unitsSection.Query<Label>(className: "settings-label").ToList();
            if (labels.Count >= 2)
            {
                // Distance units label
                labels[0].text = LocalizationManager.Instance.GetLocalizedString("Settings", "DistanceUnits_Label");
                // Time format label
                labels[1].text = LocalizationManager.Instance.GetLocalizedString("Settings", "TimeFormat_Label");
            }
            
            // Update toggle labels
            if (_metricToggle != null)
                _metricToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "UseMetric_Toggle");
            if (_timeFormatToggle != null)
                _timeFormatToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "Use24Hour_Toggle");
        }
        
        /// <summary>
        /// Updates localized text for audio section elements.
        /// AIDEV-NOTE: Helper method to update audio section labels and toggles.
        /// </summary>
        private void UpdateAudioSectionText()
        {
            var audioSection = ContentContainer?.Q<VisualElement>("audio-section");
            if (audioSection == null) return;
            
            var labels = audioSection.Query<Label>(className: "settings-label").ToList();
            if (labels.Count >= 3)
            {
                // Volume labels
                labels[0].text = LocalizationManager.Instance.GetLocalizedString("Settings", "MasterVolume_Label");
                labels[1].text = LocalizationManager.Instance.GetLocalizedString("Settings", "MusicVolume_Label");
                labels[2].text = LocalizationManager.Instance.GetLocalizedString("Settings", "SFXVolume_Label");
            }
            
            // Update audio enabled toggle
            if (_audioEnabledToggle != null)
                _audioEnabledToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "EnableAudio_Toggle");
        }
        
        /// <summary>
        /// Updates localized text for gameplay section elements.
        /// AIDEV-NOTE: Helper method to update gameplay section labels and toggles.
        /// </summary>
        private void UpdateGameplaySectionText()
        {
            var gameplaySection = ContentContainer?.Q<VisualElement>("gameplay-section");
            if (gameplaySection == null) return;
            
            var labels = gameplaySection.Query<Label>(className: "settings-label").ToList();
            if (labels.Count >= 1)
            {
                // Game speed label
                labels[0].text = LocalizationManager.Instance.GetLocalizedString("Settings", "GameSpeed_Label");
            }
            
            // Update gameplay toggles
            if (_autopauseFuelToggle != null)
                _autopauseFuelToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "AutopauseFuel_Toggle");
            if (_autopauseContractToggle != null)
                _autopauseContractToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "AutopauseContract_Toggle");
            if (_confirmDangerousToggle != null)
                _confirmDangerousToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "ConfirmDangerous_Toggle");
        }
        
        /// <summary>
        /// Updates localized text for interface section elements.
        /// AIDEV-NOTE: Helper method to update interface section labels and toggles.
        /// </summary>
        private void UpdateInterfaceSectionText()
        {
            var interfaceSection = ContentContainer?.Q<VisualElement>("ui-section");
            if (interfaceSection == null) return;
            
            var labels = interfaceSection.Query<Label>(className: "settings-label").ToList();
            if (labels.Count >= 2)
            {
                // UI scale and tooltip delay labels
                labels[0].text = LocalizationManager.Instance.GetLocalizedString("Settings", "UIScale_Label");
                labels[1].text = LocalizationManager.Instance.GetLocalizedString("Settings", "TooltipDelay_Label");
            }
            
            // Update interface toggles
            if (_showTooltipsToggle != null)
                _showTooltipsToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "ShowTooltips_Toggle");
            if (_showNotificationsToggle != null)
                _showNotificationsToggle.label = LocalizationManager.Instance.GetLocalizedString("Settings", "ShowNotifications_Toggle");
        }
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;
        
        /// <summary>
        /// AIDEV-NOTE: Debug method for testing settings modal functionality.
        /// </summary>
        [ContextMenu("Test Settings Modal")]
        private void DebugTestSettingsModal()
        {
            if (_showDebugInfo)
            {
                ShowSettings();
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Debug method for printing current settings state.
        /// </summary>
        [ContextMenu("Print Settings State")]
        private void DebugPrintSettingsState()
        {
            if (_showDebugInfo && _settingsData != null)
            {
                Debug.Log($"Current Language: {_settingsData.LocaleCode}");
                Debug.Log($"Master Volume: {_settingsData.MasterVolume}");
                Debug.Log($"Metric Units: {_settingsData.ShowDistanceInKm}");
                Debug.Log($"Changes: {_hasChanges}");
            }
        }
        #endif
    }
}