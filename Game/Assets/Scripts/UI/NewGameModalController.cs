using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using LogisticGame.Managers;

/// <summary>
/// AIDEV-NOTE: New Game Modal controller for company creation with multi-step setup process
/// Inherits from BaseModal to leverage modal-specific behaviors and UI patterns
/// </summary>
public class NewGameModalController : BaseModal
{
    [Header("New Game Modal Configuration")]
    [SerializeField] private VisualTreeAsset _newGameModalDocument;
    [SerializeField] private StyleSheet _newGameModalStyles;
    
    [Header("Styling")]
    [SerializeField] private StyleSheet _darkGraphiteTheme;
    [SerializeField] private StyleSheet _baseWindowStyles;
    
    [Header("Company Setup")]
    [SerializeField] private string[] _availableLogos = { "logo-1", "logo-2", "logo-3" };
    [SerializeField] private Color[] _availableColors = 
    {
        new Color(0.098f, 0.463f, 0.824f, 1f), // Blue
        new Color(0.827f, 0.184f, 0.184f, 1f), // Red
        new Color(0.220f, 0.557f, 0.235f, 1f), // Green
        new Color(0.380f, 0.380f, 0.380f, 1f), // Gray
        new Color(0.980f, 0.980f, 0.980f, 1f), // White
        new Color(0.129f, 0.129f, 0.129f, 1f)  // Black
    };
    
    [Header("Starting Configuration")]
    [SerializeField] private decimal _startingCredits = 100000m;
    
    // Setup state
    private int _currentStep = 0;
    private const int TOTAL_STEPS = 4;
    
    // Form data
    private CompanySetup _companySetup;
    
    // UI Elements
    private TextField _companyNameInput;
    private Label _companyNameError;
    private VisualElement[] _progressSteps;
    private VisualElement[] _setupSteps;
    private Button[] _logoOptions;
    private Button[] _colorOptions;
    private Button _backButton;
    private Button _nextButton;
    private Button _createButton;
    
    // Preview elements
    private VisualElement _logoPreviewDisplay;
    private VisualElement _vehiclePreviewDisplay;
    private Label _confirmCompanyName;
    private VisualElement _confirmLogo;
    private VisualElement _confirmColor;
    
    // Events
    public event Action<CompanySetup> OnCompanyCreated;
    
    protected override void Awake()
    {
        // AIDEV-NOTE: Set modal configuration before base initialization
        ModalSize = ModalSize.Large;
        CloseOnBackdropClick = false; // Prevent accidental closure during setup
        CloseOnEscapeKey = true;
        
        base.Awake();
        
        // Initialize company setup data
        _companySetup = new CompanySetup();
        _companySetup.StartingCredits = _startingCredits;
    }
    
    protected override void Start()
    {
        base.Start();
        
        // AIDEV-NOTE: Custom UXML and USS would be loaded through Unity Inspector
        // The _newGameModalDocument and _newGameModalStyles are configured in Unity Editor
        
        InitializeUIElements();
        SetupEventListeners();
        SetupInitialState();
        SetupLocalizationListeners();
        
        // Apply initial localization
        RefreshLocalization();
    }
    
    /// <summary>
    /// Initializes all UI elements and caches references for efficient access.
    /// AIDEV-NOTE: Caches all UI element references to avoid repeated queries.
    /// </summary>
    private void InitializeUIElements()
    {
        if (ContentContainer == null) return;
        
        // Company name input elements
        _companyNameInput = ContentContainer.Q<TextField>("company-name-input");
        _companyNameError = ContentContainer.Q<Label>("company-name-error");
        
        // Progress steps
        _progressSteps = new VisualElement[TOTAL_STEPS];
        for (int i = 0; i < TOTAL_STEPS; i++)
        {
            _progressSteps[i] = ContentContainer.Q<VisualElement>($"progress-step-{i + 1}");
        }
        
        // Setup steps
        _setupSteps = new VisualElement[TOTAL_STEPS];
        _setupSteps[0] = ContentContainer.Q<VisualElement>("step-company-info");
        _setupSteps[1] = ContentContainer.Q<VisualElement>("step-logo-selection");
        _setupSteps[2] = ContentContainer.Q<VisualElement>("step-color-selection");
        _setupSteps[3] = ContentContainer.Q<VisualElement>("step-confirmation");
        
        // Logo selection buttons
        _logoOptions = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            _logoOptions[i] = ContentContainer.Q<Button>($"logo-option-{i + 1}");
        }
        
        // Color selection buttons
        _colorOptions = new Button[6];
        string[] colorNames = { "blue", "red", "green", "gray", "white", "black" };
        for (int i = 0; i < 6; i++)
        {
            _colorOptions[i] = ContentContainer.Q<Button>($"color-{colorNames[i]}");
        }
        
        // Navigation buttons
        _backButton = ContentContainer.Q<Button>("back-button");
        _nextButton = ContentContainer.Q<Button>("next-button");
        _createButton = ContentContainer.Q<Button>("create-button");
        
        // Preview elements
        _logoPreviewDisplay = ContentContainer.Q<VisualElement>("logo-preview-display");
        _vehiclePreviewDisplay = ContentContainer.Q<VisualElement>("vehicle-preview-display");
        _confirmCompanyName = ContentContainer.Q<Label>("confirm-company-name");
        _confirmLogo = ContentContainer.Q<VisualElement>("confirm-logo");
        _confirmColor = ContentContainer.Q<VisualElement>("confirm-color");
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): UI elements initialized successfully");
    }
    
    /// <summary>
    /// Loads required stylesheets for proper modal appearance.
    /// AIDEV-NOTE: Ensures theme variables and modal styles are available, matching SettingsModal pattern.
    /// </summary>
    private void LoadRequiredStylesheets()
    {
        if (_rootContainer == null)
        {
            Debug.LogWarning($"NewGameModal ({name}): Cannot load stylesheets - root container is null");
            return;
        }
        
        // Load dark graphite theme first (provides CSS variables)
        if (_darkGraphiteTheme != null && !_rootContainer.styleSheets.Contains(_darkGraphiteTheme))
        {
            _rootContainer.styleSheets.Add(_darkGraphiteTheme);
            if (_debugMode)
                Debug.Log($"NewGameModal ({name}): Added DarkGraphiteTheme stylesheet");
        }
        
        // Load base window styles (provides modal styling foundation)
        if (_baseWindowStyles != null && !_rootContainer.styleSheets.Contains(_baseWindowStyles))
        {
            _rootContainer.styleSheets.Add(_baseWindowStyles);
            if (_debugMode)
                Debug.Log($"NewGameModal ({name}): Added BaseWindowStyles stylesheet");
        }
        
        // Load new game modal specific styles
        if (_newGameModalStyles != null && !_rootContainer.styleSheets.Contains(_newGameModalStyles))
        {
            _rootContainer.styleSheets.Add(_newGameModalStyles);
            if (_debugMode)
                Debug.Log($"NewGameModal ({name}): Added NewGameModalStyles stylesheet");
        }
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Total stylesheets loaded: {_rootContainer.styleSheets.count}");
    }
    
    /// <summary>
    /// Sets up event listeners for all interactive UI elements.
    /// AIDEV-NOTE: Centralizes all event listener setup for maintainability.
    /// </summary>
    private void SetupEventListeners()
    {
        // Company name input validation
        if (_companyNameInput != null)
        {
            _companyNameInput.RegisterValueChangedCallback(OnCompanyNameChanged);
        }
        
        // Logo selection
        for (int i = 0; i < _logoOptions.Length; i++)
        {
            if (_logoOptions[i] != null)
            {
                int logoIndex = i; // Capture index for closure
                _logoOptions[i].clicked += () => OnLogoSelected(logoIndex);
            }
        }
        
        // Color selection
        for (int i = 0; i < _colorOptions.Length; i++)
        {
            if (_colorOptions[i] != null)
            {
                int colorIndex = i; // Capture index for closure
                _colorOptions[i].clicked += () => OnColorSelected(colorIndex);
            }
        }
        
        // Navigation buttons
        if (_backButton != null)
            _backButton.clicked += OnBackButtonClicked;
        
        if (_nextButton != null)
            _nextButton.clicked += OnNextButtonClicked;
        
        if (_createButton != null)
            _createButton.clicked += OnCreateButtonClicked;
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Event listeners setup complete");
    }
    
    /// <summary>
    /// Sets up the initial state of the modal UI.
    /// AIDEV-NOTE: Configures the modal for the first step of setup.
    /// </summary>
    private void SetupInitialState()
    {
        // Load required stylesheets for proper appearance
        LoadRequiredStylesheets();
        
        // Set window title
        WindowTitle = "Create New Company";
        
        // Show first step
        ShowStep(0);
        
        // Set default company setup values
        _companySetup.LogoIndex = 0;
        _companySetup.VehicleColorIndex = 0;
        
        // Apply default selections
        UpdateLogoPreview();
        UpdateVehiclePreview();
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Initial state setup complete");
    }
    
    /// <summary>
    /// Shows the specified setup step and updates UI accordingly.
    /// AIDEV-NOTE: Manages step visibility and progress indicator updates.
    /// </summary>
    /// <param name="stepIndex">The step index to show (0-based)</param>
    private void ShowStep(int stepIndex)
    {
        _currentStep = stepIndex;
        
        // Update progress indicator
        for (int i = 0; i < _progressSteps.Length; i++)
        {
            if (_progressSteps[i] != null)
            {
                _progressSteps[i].RemoveFromClassList("active");
                _progressSteps[i].RemoveFromClassList("completed");
                
                if (i < _currentStep)
                    _progressSteps[i].AddToClassList("completed");
                else if (i == _currentStep)
                    _progressSteps[i].AddToClassList("active");
            }
        }
        
        // Update step visibility
        for (int i = 0; i < _setupSteps.Length; i++)
        {
            if (_setupSteps[i] != null)
            {
                if (i == _currentStep)
                {
                    _setupSteps[i].RemoveFromClassList("hidden");
                    _setupSteps[i].AddToClassList("active");
                }
                else
                {
                    _setupSteps[i].RemoveFromClassList("active");
                    _setupSteps[i].AddToClassList("hidden");
                }
            }
        }
        
        // Update button visibility
        UpdateNavigationButtons();
        
        // Handle step-specific logic
        switch (_currentStep)
        {
            case 0: // Company info
                if (_companyNameInput != null)
                    _companyNameInput.Focus();
                break;
                
            case 3: // Confirmation
                UpdateConfirmationSummary();
                break;
        }
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Showing step {stepIndex + 1} of {TOTAL_STEPS}");
    }
    
    /// <summary>
    /// Updates the visibility and state of navigation buttons.
    /// AIDEV-NOTE: Controls button visibility based on current step and validation.
    /// </summary>
    private void UpdateNavigationButtons()
    {
        // Back button
        if (_backButton != null)
        {
            if (_currentStep > 0)
            {
                _backButton.RemoveFromClassList("hidden");
            }
            else
            {
                _backButton.AddToClassList("hidden");
            }
        }
        
        // Next/Create button
        bool isLastStep = _currentStep >= TOTAL_STEPS - 1;
        
        if (_nextButton != null)
        {
            if (isLastStep)
            {
                _nextButton.AddToClassList("hidden");
            }
            else
            {
                _nextButton.RemoveFromClassList("hidden");
                _nextButton.SetEnabled(IsCurrentStepValid());
            }
        }
        
        if (_createButton != null)
        {
            if (isLastStep)
            {
                _createButton.RemoveFromClassList("hidden");
                _createButton.SetEnabled(IsAllDataValid());
            }
            else
            {
                _createButton.AddToClassList("hidden");
            }
        }
    }
    
    /// <summary>
    /// Validates if the current step has all required data.
    /// AIDEV-NOTE: Provides step-specific validation for navigation control.
    /// </summary>
    /// <returns>True if current step is valid</returns>
    private bool IsCurrentStepValid()
    {
        switch (_currentStep)
        {
            case 0: // Company info
                return ValidateCompanyName(_companySetup.CompanyName);
            case 1: // Logo selection
                return _companySetup.LogoIndex >= 0;
            case 2: // Color selection
                return _companySetup.VehicleColorIndex >= 0;
            case 3: // Confirmation
                return IsAllDataValid();
            default:
                return true;
        }
    }
    
    /// <summary>
    /// Validates if all required data is provided and valid.
    /// AIDEV-NOTE: Comprehensive validation for final company creation.
    /// </summary>
    /// <returns>True if all data is valid</returns>
    private bool IsAllDataValid()
    {
        return !string.IsNullOrEmpty(_companySetup.CompanyName) &&
               ValidateCompanyName(_companySetup.CompanyName) &&
               _companySetup.LogoIndex >= 0 &&
               _companySetup.VehicleColorIndex >= 0;
    }
    
    /// <summary>
    /// Validates company name according to business rules.
    /// AIDEV-NOTE: Implements company name validation rules from documentation.
    /// </summary>
    /// <param name="companyName">Company name to validate</param>
    /// <returns>True if name is valid</returns>
    private bool ValidateCompanyName(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return false;
        
        if (companyName.Length < 3 || companyName.Length > 50)
            return false;
        
        // AIDEV-TODO: Add uniqueness check against existing saves
        // AIDEV-TODO: Add inappropriate content filtering
        
        return true;
    }
    
    /// <summary>
    /// Updates the company name error display.
    /// AIDEV-NOTE: Provides real-time validation feedback to user.
    /// </summary>
    /// <param name="errorMessage">Error message to display, or null to hide</param>
    private void UpdateCompanyNameError(string errorMessage)
    {
        if (_companyNameError != null)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                _companyNameError.AddToClassList("hidden");
                _companyNameError.text = "";
            }
            else
            {
                _companyNameError.RemoveFromClassList("hidden");
                _companyNameError.text = errorMessage;
            }
        }
    }
    
    /// <summary>
    /// Updates the logo preview display.
    /// AIDEV-NOTE: Shows selected logo in preview area using AssetManager.
    /// </summary>
    private void UpdateLogoPreview()
    {
        if (_logoPreviewDisplay == null) return;
        
        // Load and display the selected logo from AssetManager
        if (AssetManager.Instance != null && _companySetup.LogoIndex >= 0)
        {
            Sprite logoSprite = AssetManager.Instance.GetCompanyLogoByIndex(_companySetup.LogoIndex);
            if (logoSprite != null)
            {
                _logoPreviewDisplay.style.backgroundImage = new StyleBackground(logoSprite);
                _logoPreviewDisplay.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                
                if (_debugMode)
                    Debug.Log($"NewGameModal ({name}): Logo preview updated with sprite '{logoSprite.name}' for index {_companySetup.LogoIndex}");
            }
            else
            {
                _logoPreviewDisplay.style.backgroundImage = StyleKeyword.None;
                if (_debugMode)
                    Debug.LogWarning($"NewGameModal ({name}): Could not load logo sprite for index {_companySetup.LogoIndex}");
            }
        }
        
        // Update logo option selection visual feedback
        for (int i = 0; i < _logoOptions.Length; i++)
        {
            if (_logoOptions[i] != null)
            {
                if (i == _companySetup.LogoIndex)
                {
                    _logoOptions[i].AddToClassList("selected");
                    
                    // Load logo sprite into button if available
                    if (AssetManager.Instance != null)
                    {
                        Sprite logoSprite = AssetManager.Instance.GetCompanyLogoByIndex(i);
                        if (logoSprite != null)
                        {
                            _logoOptions[i].style.backgroundImage = new StyleBackground(logoSprite);
                            _logoOptions[i].style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                        }
                    }
                }
                else
                {
                    _logoOptions[i].RemoveFromClassList("selected");
                    
                    // Load logo sprite into button if available (for unselected buttons)
                    if (AssetManager.Instance != null)
                    {
                        Sprite logoSprite = AssetManager.Instance.GetCompanyLogoByIndex(i);
                        if (logoSprite != null)
                        {
                            _logoOptions[i].style.backgroundImage = new StyleBackground(logoSprite);
                            _logoOptions[i].style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Updates the vehicle color preview display.
    /// AIDEV-NOTE: Shows selected color on vehicle preview using available colors.
    /// </summary>
    private void UpdateVehiclePreview()
    {
        if (_vehiclePreviewDisplay == null) return;
        
        // Apply selected color to vehicle preview
        if (_companySetup.VehicleColorIndex >= 0 && _companySetup.VehicleColorIndex < _availableColors.Length)
        {
            var selectedColor = _availableColors[_companySetup.VehicleColorIndex];
            _vehiclePreviewDisplay.style.unityBackgroundImageTintColor = selectedColor;
        }
        
        // Update color option selection visual feedback
        for (int i = 0; i < _colorOptions.Length; i++)
        {
            if (_colorOptions[i] != null)
            {
                if (i == _companySetup.VehicleColorIndex)
                {
                    _colorOptions[i].AddToClassList("selected");
                }
                else
                {
                    _colorOptions[i].RemoveFromClassList("selected");
                }
            }
        }
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Vehicle preview updated for color index {_companySetup.VehicleColorIndex}");
    }
    
    /// <summary>
    /// Updates the confirmation summary with all selected options.
    /// AIDEV-NOTE: Displays final choices for user review before creation.
    /// </summary>
    private void UpdateConfirmationSummary()
    {
        if (_confirmCompanyName != null)
            _confirmCompanyName.text = _companySetup.CompanyName;
        
        // Update logo display in confirmation
        if (_confirmLogo != null && AssetManager.Instance != null && _companySetup.LogoIndex >= 0)
        {
            Sprite logoSprite = AssetManager.Instance.GetCompanyLogoByIndex(_companySetup.LogoIndex);
            if (logoSprite != null)
            {
                _confirmLogo.style.backgroundImage = new StyleBackground(logoSprite);
                _confirmLogo.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
        }
        
        // Update color display in confirmation
        if (_confirmColor != null && _companySetup.VehicleColorIndex >= 0 && _companySetup.VehicleColorIndex < _availableColors.Length)
        {
            _confirmColor.style.backgroundColor = _availableColors[_companySetup.VehicleColorIndex];
        }
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Confirmation summary updated");
    }
    
    // Event Handlers
    
    /// <summary>
    /// Handles company name input changes with real-time validation.
    /// AIDEV-NOTE: Provides immediate feedback on name validity.
    /// </summary>
    private void OnCompanyNameChanged(ChangeEvent<string> evt)
    {
        _companySetup.CompanyName = evt.newValue;
        
        if (string.IsNullOrWhiteSpace(evt.newValue))
        {
            UpdateCompanyNameError(null);
        }
        else if (evt.newValue.Length < 3)
        {
            UpdateCompanyNameError("Company name must be at least 3 characters long.");
        }
        else if (evt.newValue.Length > 50)
        {
            UpdateCompanyNameError("Company name cannot exceed 50 characters.");
        }
        else
        {
            UpdateCompanyNameError(null);
        }
        
        UpdateNavigationButtons();
    }
    
    /// <summary>
    /// Handles logo selection.
    /// AIDEV-NOTE: Updates company setup data and preview display.
    /// </summary>
    /// <param name="logoIndex">Selected logo index</param>
    private void OnLogoSelected(int logoIndex)
    {
        _companySetup.LogoIndex = logoIndex;
        UpdateLogoPreview();
        UpdateNavigationButtons();
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Logo {logoIndex + 1} selected");
    }
    
    /// <summary>
    /// Handles vehicle color selection.
    /// AIDEV-NOTE: Updates company setup data and vehicle preview.
    /// </summary>
    /// <param name="colorIndex">Selected color index</param>
    private void OnColorSelected(int colorIndex)
    {
        _companySetup.VehicleColorIndex = colorIndex;
        _companySetup.VehicleColor = _availableColors[colorIndex];
        UpdateVehiclePreview();
        UpdateNavigationButtons();
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Color {colorIndex} selected");
    }
    
    /// <summary>
    /// Handles back button click to go to previous step.
    /// AIDEV-NOTE: Provides navigation backward through setup steps.
    /// </summary>
    private void OnBackButtonClicked()
    {
        if (_currentStep > 0)
        {
            ShowStep(_currentStep - 1);
        }
    }
    
    /// <summary>
    /// Handles next button click to advance to next step.
    /// AIDEV-NOTE: Validates current step before advancing.
    /// </summary>
    private void OnNextButtonClicked()
    {
        if (IsCurrentStepValid() && _currentStep < TOTAL_STEPS - 1)
        {
            ShowStep(_currentStep + 1);
        }
    }
    
    /// <summary>
    /// Handles create button click to finalize company creation.
    /// AIDEV-NOTE: Validates all data and triggers company creation process.
    /// </summary>
    private void OnCreateButtonClicked()
    {
        if (IsAllDataValid())
        {
            CreateCompany();
        }
    }
    
    /// <summary>
    /// Creates the new company with the configured settings.
    /// AIDEV-NOTE: Integrates with save system to create new game state.
    /// </summary>
    private async void CreateCompany()
    {
        if (!IsAllDataValid())
        {
            Debug.LogWarning($"NewGameModal ({name}): Cannot create company - validation failed");
            return;
        }
        
        try
        {
            _companySetup.CreationDate = DateTime.Now;
            
            // Create unique save slot name from company name and timestamp
            string slotName = GenerateSaveSlotName(_companySetup.CompanyName);
            
            // Check if GameManager exists
            if (GameManager.Instance == null)
            {
                Debug.LogError($"NewGameModal ({name}): GameManager instance not found");
                ShowCreateErrorModal("Game Manager not available. Please restart the application.");
                return;
            }
            
            // Create new game through GameManager
            bool success = await CreateNewGameAsync(_companySetup, slotName);
            
            if (success)
            {
                // Notify listeners of successful company creation
                OnCompanyCreated?.Invoke(_companySetup);
                
                if (_debugMode)
                    Debug.Log($"NewGameModal ({name}): Company '{_companySetup.CompanyName}' created successfully");
                
                // Close modal after successful creation
                HideWindow();
                
                // AIDEV-NOTE: Transition to main game scene handled by GameManager
                // The GameManager.CreateNewGameAsync method should handle scene transition
            }
            else
            {
                Debug.LogError($"NewGameModal ({name}): Failed to create new game for company '{_companySetup.CompanyName}'");
                ShowCreateErrorModal("Failed to create your company. Please try again or choose a different company name.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"NewGameModal ({name}): Error creating company - {ex.Message}");
            ShowCreateErrorModal($"An error occurred while creating your company: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Creates a new game asynchronously through the GameManager.
    /// AIDEV-NOTE: Delegates to GameManager for proper save system integration.
    /// </summary>
    /// <param name="companySetup">Company setup data</param>
    /// <param name="slotName">Save slot name</param>
    /// <returns>True if creation was successful</returns>
    private async System.Threading.Tasks.Task<bool> CreateNewGameAsync(CompanySetup companySetup, string slotName)
    {
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Creating new game with company '{companySetup.CompanyName}' in slot '{slotName}'");
        
        // Call GameManager to create the new game
        return await GameManager.Instance.CreateNewGameAsync(companySetup, slotName);
    }
    
    /// <summary>
    /// Generates a unique save slot name from company name.
    /// AIDEV-NOTE: Creates filesystem-safe names with uniqueness guarantee.
    /// </summary>
    /// <param name="companyName">Company name to base slot name on</param>
    /// <returns>Unique save slot name</returns>
    private string GenerateSaveSlotName(string companyName)
    {
        // Sanitize company name for filesystem
        string sanitizedName = System.Text.RegularExpressions.Regex.Replace(companyName, @"[^\w\-_\s]", "");
        sanitizedName = sanitizedName.Replace(" ", "_").Trim();
        
        // Ensure it's not too long
        if (sanitizedName.Length > 30)
        {
            sanitizedName = sanitizedName.Substring(0, 30);
        }
        
        // Add timestamp for uniqueness
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        
        return $"{sanitizedName}_{timestamp}";
    }
    
    /// <summary>
    /// Shows an error modal when company creation fails.
    /// AIDEV-NOTE: Provides user feedback for creation failures using BaseModal's alert functionality.
    /// </summary>
    /// <param name="errorMessage">Error message to display</param>
    private void ShowCreateErrorModal(string errorMessage)
    {
        Debug.LogError($"NewGameModal ({name}): Creation Error - {errorMessage}");
        
        // Create a temporary error modal using BaseModal's alert functionality
        try
        {
            // Create a temporary GameObject with BaseModal for the error dialog
            GameObject errorModalObj = new GameObject("CompanyCreationErrorModal");
            BaseModal errorModal = errorModalObj.AddComponent<BaseModal>();
            
            // Setup the alert modal
            errorModal.SetupAlertModal("Error Creating Company", errorMessage, () => {
                // Clean up the temporary error modal
                if (errorModalObj != null)
                {
                    Destroy(errorModalObj);
                }
            });
            
            // Show the error modal
            errorModal.ShowWindow();
            
            if (_debugMode)
                Debug.Log($"NewGameModal ({name}): Error modal displayed to user");
        }
        catch (Exception ex)
        {
            Debug.LogError($"NewGameModal ({name}): Failed to show error modal - {ex.Message}");
            
            // Fallback to console output if modal creation fails
            Debug.LogError($"Company Creation Failed: {errorMessage}");
        }
    }
    
    // Localization Methods
    
    /// <summary>
    /// Sets up event listeners for localization system changes.
    /// AIDEV-NOTE: Subscribes to language change events for dynamic text updates.
    /// </summary>
    private void SetupLocalizationListeners()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.OnLanguageChanged += OnLocalizationLanguageChanged;
        }
    }
    
    /// <summary>
    /// Removes localization event listeners to prevent memory leaks.
    /// AIDEV-NOTE: Unsubscribes from localization events during cleanup.
    /// </summary>
    private void RemoveLocalizationListeners()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.OnLanguageChanged -= OnLocalizationLanguageChanged;
        }
    }
    
    /// <summary>
    /// Handles language change events from the localization system.
    /// AIDEV-NOTE: Refreshes all localized text when language changes.
    /// </summary>
    /// <param name="newLocale">The new locale</param>
    private void OnLocalizationLanguageChanged(UnityEngine.Localization.Locale newLocale)
    {
        RefreshLocalization();
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Language changed to {(newLocale != null ? newLocale.Identifier.Code : "null")}, localization refreshed");
    }
    
    /// <summary>
    /// Refreshes all localized text elements in the modal.
    /// AIDEV-NOTE: Updates all text elements with localized strings from LocalizationManager.
    /// </summary>
    private void RefreshLocalization()
    {
        if (LocalizationManager.Instance == null || !LocalizationManager.Instance.IsInitialized)
        {
            if (_debugMode)
                Debug.LogWarning($"NewGameModal ({name}): LocalizationManager not available, using default text");
            
            // Use fallback text when localization is not available
            UseFallbackText();
            return;
        }
        
        try
        {
            // Update window title
            WindowTitle = GetLocalizedStringWithFallback("NewGame", "Title", "Create New Company");
            
            // Update step titles and descriptions
            UpdateStepLocalization();
            
            // Update form labels and placeholders
            UpdateFormLocalization();
            
            // Update button text
            UpdateButtonLocalization();
            
            if (_debugMode)
                Debug.Log($"NewGameModal ({name}): All localized text updated");
        }
        catch (Exception ex)
        {
            Debug.LogError($"NewGameModal ({name}): Error refreshing localization - {ex.Message}");
            UseFallbackText();
        }
    }
    
    /// <summary>
    /// Gets a localized string with fallback to default text if localization fails.
    /// AIDEV-NOTE: Provides graceful degradation when localization is not available.
    /// </summary>
    /// <param name="table">Localization table name</param>
    /// <param name="key">Localization key</param>
    /// <param name="fallback">Fallback text to use</param>
    /// <returns>Localized text or fallback</returns>
    private string GetLocalizedStringWithFallback(string table, string key, string fallback)
    {
        try
        {
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                string localized = LocalizationManager.Instance.GetLocalizedString(table, key);
                if (!string.IsNullOrEmpty(localized) && !localized.Contains("No translation found"))
                    return localized;
            }
        }
        catch (Exception ex)
        {
            if (_debugMode)
                Debug.LogWarning($"NewGameModal ({name}): Localization failed for {table}.{key} - {ex.Message}");
        }
        
        return fallback;
    }
    
    /// <summary>
    /// Uses fallback text when localization is not available.
    /// AIDEV-NOTE: Ensures the modal is usable even without localization setup.
    /// </summary>
    private void UseFallbackText()
    {
        // Set window title
        WindowTitle = "Create New Company";
        
        // Update step titles and descriptions with fallback text
        UpdateStepLocalizationWithFallback();
        
        // Update form labels with fallback text
        UpdateFormLocalizationWithFallback();
        
        // Update button text with fallback text
        UpdateButtonLocalizationWithFallback();
        
        if (_debugMode)
            Debug.Log($"NewGameModal ({name}): Using fallback text");
    }
    
    /// <summary>
    /// Updates localization for setup step titles and descriptions.
    /// AIDEV-NOTE: Localizes step-specific text content.
    /// </summary>
    private void UpdateStepLocalization()
    {
        if (ContentContainer == null) return;
        
        // Step 1: Company Information
        var step1Title = ContentContainer.Q<Label>("step-company-info")?.Q<Label>("step-title");
        if (step1Title != null)
            step1Title.text = GetLocalizedStringWithFallback("NewGame", "Step1_Title", "Company Information");
        
        var step1Desc = ContentContainer.Q<Label>("step-company-info")?.Q<Label>("step-description");
        if (step1Desc != null)
            step1Desc.text = GetLocalizedStringWithFallback("NewGame", "Step1_Description", "Choose your company name and basic information");
        
        // Step 2: Logo Selection
        var step2Title = ContentContainer.Q<Label>("step-logo-selection")?.Q<Label>("step-title");
        if (step2Title != null)
            step2Title.text = GetLocalizedStringWithFallback("NewGame", "Step2_Title", "Company Logo");
        
        var step2Desc = ContentContainer.Q<Label>("step-logo-selection")?.Q<Label>("step-description");
        if (step2Desc != null)
            step2Desc.text = GetLocalizedStringWithFallback("NewGame", "Step2_Description", "Select a logo that represents your logistics company");
        
        // Step 3: Color Selection
        var step3Title = ContentContainer.Q<Label>("step-color-selection")?.Q<Label>("step-title");
        if (step3Title != null)
            step3Title.text = GetLocalizedStringWithFallback("NewGame", "Step3_Title", "Vehicle Colors");
        
        var step3Desc = ContentContainer.Q<Label>("step-color-selection")?.Q<Label>("step-description");
        if (step3Desc != null)
            step3Desc.text = GetLocalizedStringWithFallback("NewGame", "Step3_Description", "Choose the primary color for your company vehicles");
        
        // Step 4: Confirmation
        var step4Title = ContentContainer.Q<Label>("step-confirmation")?.Q<Label>("step-title");
        if (step4Title != null)
            step4Title.text = GetLocalizedStringWithFallback("NewGame", "Step4_Title", "Confirm Details");
        
        var step4Desc = ContentContainer.Q<Label>("step-confirmation")?.Q<Label>("step-description");
        if (step4Desc != null)
            step4Desc.text = GetLocalizedStringWithFallback("NewGame", "Step4_Description", "Review your company details and confirm creation");
    }
    
    /// <summary>
    /// Updates localization for setup step titles and descriptions with fallback text.
    /// AIDEV-NOTE: Provides fallback text when localization is not available.
    /// </summary>
    private void UpdateStepLocalizationWithFallback()
    {
        if (ContentContainer == null) return;
        
        // Step 1: Company Information
        var step1Title = ContentContainer.Q<Label>("step-company-info")?.Q<Label>("step-title");
        if (step1Title != null)
            step1Title.text = "Company Information";
        
        var step1Desc = ContentContainer.Q<Label>("step-company-info")?.Q<Label>("step-description");
        if (step1Desc != null)
            step1Desc.text = "Choose your company name and basic information";
        
        // Step 2: Logo Selection
        var step2Title = ContentContainer.Q<Label>("step-logo-selection")?.Q<Label>("step-title");
        if (step2Title != null)
            step2Title.text = "Company Logo";
        
        var step2Desc = ContentContainer.Q<Label>("step-logo-selection")?.Q<Label>("step-description");
        if (step2Desc != null)
            step2Desc.text = "Select a logo that represents your logistics company";
        
        // Step 3: Color Selection
        var step3Title = ContentContainer.Q<Label>("step-color-selection")?.Q<Label>("step-title");
        if (step3Title != null)
            step3Title.text = "Vehicle Colors";
        
        var step3Desc = ContentContainer.Q<Label>("step-color-selection")?.Q<Label>("step-description");
        if (step3Desc != null)
            step3Desc.text = "Choose the primary color for your company vehicles";
        
        // Step 4: Confirmation
        var step4Title = ContentContainer.Q<Label>("step-confirmation")?.Q<Label>("step-title");
        if (step4Title != null)
            step4Title.text = "Confirm Details";
        
        var step4Desc = ContentContainer.Q<Label>("step-confirmation")?.Q<Label>("step-description");
        if (step4Desc != null)
            step4Desc.text = "Review your company details and confirm creation";
    }
    
    /// <summary>
    /// Updates localization for form labels and input placeholders.
    /// AIDEV-NOTE: Localizes input field labels and helper text.
    /// </summary>
    private void UpdateFormLocalization()
    {
        if (ContentContainer == null) return;
        
        // Company name input label
        var companyNameLabel = ContentContainer.Q<Label>("input-label");
        if (companyNameLabel != null)
            companyNameLabel.text = GetLocalizedStringWithFallback("NewGame", "CompanyName_Label", "Company Name");
        
        // Company name input placeholder
        if (_companyNameInput != null)
            _companyNameInput.SetValueWithoutNotify(_companyNameInput.value); // Placeholder text is set in UXML
        
        // Credits section
        var creditsLabel = ContentContainer.Q<Label>("credits-label");
        if (creditsLabel != null)
            creditsLabel.text = GetLocalizedStringWithFallback("NewGame", "StartingCapital_Label", "Starting Capital");
        
        var creditsDesc = ContentContainer.Q<Label>("credits-description");
        if (creditsDesc != null)
            creditsDesc.text = GetLocalizedStringWithFallback("NewGame", "StartingCapital_Description", "Your initial budget for vehicles, contracts, and operations");
        
        // Preview labels
        var logoPreviewLabel = ContentContainer.Q<Label>("logo-preview")?.Q<Label>("preview-label");
        if (logoPreviewLabel != null)
            logoPreviewLabel.text = GetLocalizedStringWithFallback("NewGame", "Logo_Preview_Label", "Preview");
        
        var vehiclePreviewLabel = ContentContainer.Q<Label>("vehicle-preview")?.Q<Label>("preview-label");
        if (vehiclePreviewLabel != null)
            vehiclePreviewLabel.text = GetLocalizedStringWithFallback("NewGame", "Vehicle_Preview_Label", "Vehicle Preview");
    }
    
    /// <summary>
    /// Updates localization for form labels and input placeholders with fallback text.
    /// AIDEV-NOTE: Provides fallback text when localization is not available.
    /// </summary>
    private void UpdateFormLocalizationWithFallback()
    {
        if (ContentContainer == null) return;
        
        // Company name input label
        var companyNameLabel = ContentContainer.Q<Label>("input-label");
        if (companyNameLabel != null)
            companyNameLabel.text = "Company Name";
        
        // Credits section
        var creditsLabel = ContentContainer.Q<Label>("credits-label");
        if (creditsLabel != null)
            creditsLabel.text = "Starting Capital";
        
        var creditsDesc = ContentContainer.Q<Label>("credits-description");
        if (creditsDesc != null)
            creditsDesc.text = "Your initial budget for vehicles, contracts, and operations";
        
        // Preview labels
        var logoPreviewLabel = ContentContainer.Q<Label>("logo-preview")?.Q<Label>("preview-label");
        if (logoPreviewLabel != null)
            logoPreviewLabel.text = "Preview";
        
        var vehiclePreviewLabel = ContentContainer.Q<Label>("vehicle-preview")?.Q<Label>("preview-label");
        if (vehiclePreviewLabel != null)
            vehiclePreviewLabel.text = "Vehicle Preview";
    }
    
    /// <summary>
    /// Updates localization for navigation and action buttons.
    /// AIDEV-NOTE: Localizes all button text.
    /// </summary>
    private void UpdateButtonLocalization()
    {
        if (_backButton != null)
            _backButton.text = GetLocalizedStringWithFallback("NewGame", "Back_Button", "Back");
        
        if (_nextButton != null)
            _nextButton.text = GetLocalizedStringWithFallback("NewGame", "Next_Button", "Next");
        
        if (_createButton != null)
            _createButton.text = GetLocalizedStringWithFallback("NewGame", "Create_Button", "Create Company");
    }
    
    /// <summary>
    /// Updates localization for navigation and action buttons with fallback text.
    /// AIDEV-NOTE: Provides fallback text when localization is not available.
    /// </summary>
    private void UpdateButtonLocalizationWithFallback()
    {
        if (_backButton != null)
            _backButton.text = "Back";
        
        if (_nextButton != null)
            _nextButton.text = "Next";
        
        if (_createButton != null)
            _createButton.text = "Create Company";
    }
    
    protected override void OnDestroy()
    {
        RemoveLocalizationListeners();
        base.OnDestroy();
    }
}

/// <summary>
/// AIDEV-NOTE: Data class for holding company setup information during creation process.
/// Contains all necessary data for creating a new company and game state from NewGameModal.
/// </summary>
[System.Serializable]
public class CompanySetup
{
    [Header("Company Information")]
    public string CompanyName = "";
    public int LogoIndex = -1;
    public int VehicleColorIndex = -1;
    public Color VehicleColor = Color.white;
    
    [Header("Starting Configuration")]
    public decimal StartingCredits = 100000m;
    public DateTime CreationDate;
    
    /// <summary>
    /// AIDEV-NOTE: Validates that all required setup data is provided and valid.
    /// </summary>
    /// <returns>True if setup is valid for company creation</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CompanyName) &&
               CompanyName.Length >= 3 &&
               CompanyName.Length <= 50 &&
               LogoIndex >= 0 &&
               VehicleColorIndex >= 0 &&
               StartingCredits > 0;
    }
    
    /// <summary>
    /// AIDEV-NOTE: Gets a summary string of the company setup for logging and display.
    /// </summary>
    /// <returns>Formatted string with company setup details</returns>
    public override string ToString()
    {
        return $"CompanySetup: '{CompanyName}', Logo: {LogoIndex}, Color: {VehicleColorIndex}, Credits: {StartingCredits:C}, Created: {CreationDate:yyyy-MM-dd HH:mm:ss}";
    }
}

