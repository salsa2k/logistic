using UnityEngine;
using UnityEngine.UIElements;
using LogisticGame.Events;
using LogisticGame.Managers;
using LogisticGame.SaveSystem;

/// <summary>
/// AIDEV-NOTE: Main Menu controller extending BaseWindow for consistent window behavior.
/// Handles navigation buttons and scene transitions for the logistics game main menu.
/// </summary>
public class MainMenuController : BaseWindow
{
    [Header("Main Menu Configuration")]
    [SerializeField] private string _gameSceneName = "Game";
    [SerializeField] private string _creditsSceneName = "Credits";
    [SerializeField] private bool _detectSaveGames = true;
    
    [Header("Menu Styling")]
    [SerializeField] private StyleSheet _darkGraphiteTheme;
    [SerializeField] private StyleSheet _mainMenuStyles;
    
    // Menu button references
    private Button _newGameButton;
    private Button _loadGameButton;
    private Button _settingsButton;
    private Button _creditsButton;
    private Button _exitButton;
    
    // Background elements
    private VisualElement _backgroundElement;
    private Label _versionLabel;
    
    // State management
    private bool _hasSaveGames = false;
    
    protected override void Awake()
    {
        // AIDEV-NOTE: Set main menu specific configuration before base initialization
        _windowTitle = "Main Menu";
        _isModal = false;
        _showCloseButton = false;
        _centerOnShow = false; // Main menu uses custom positioning
        
        base.Awake();
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize main menu specific elements
        InitializeMainMenuElements();
        
        // Apply main menu specific styles
        ApplyMainMenuStyles();
        
        // Initialize menu state
        InitializeMenuState();
        
        // Show the main menu immediately
        ShowWindow(false);
        
        // AIDEV-NOTE: Subscribe to save system events for dynamic Load Game button state
        EventBus.Subscribe<GameSavedEvent>(OnGameSaved);
    }
    
    protected override void OnDestroy()
    {
        // AIDEV-NOTE: Clean up event subscriptions to prevent memory leaks
        RemoveMenuEventListeners();
        EventBus.Unsubscribe<GameSavedEvent>(OnGameSaved);
        
        base.OnDestroy();
    }
    
    /// <summary>
    /// Initialize UI element references specific to the main menu.
    /// AIDEV-NOTE: Called after base BaseWindow initialization to get menu-specific elements.
    /// </summary>
    private void InitializeMainMenuElements()
    {
        if (_rootContainer == null) return;
        
        // Get menu button references from the simplified structure
        _newGameButton = _rootContainer.Q<Button>("new-game-button");
        _loadGameButton = _rootContainer.Q<Button>("load-game-button");
        _settingsButton = _rootContainer.Q<Button>("settings-button");
        _creditsButton = _rootContainer.Q<Button>("credits-button");
        _exitButton = _rootContainer.Q<Button>("exit-button");
        
        // Get additional elements
        _backgroundElement = _rootContainer.Q<VisualElement>("main-menu-background");
        _versionLabel = _rootContainer.Q<Label>("version-label");
        
        // Log warnings for missing elements
        if (_newGameButton == null) Debug.LogWarning($"MainMenu: new-game-button not found in UXML");
        if (_loadGameButton == null) Debug.LogWarning($"MainMenu: load-game-button not found in UXML");
        if (_settingsButton == null) Debug.LogWarning($"MainMenu: settings-button not found in UXML");
        if (_creditsButton == null) Debug.LogWarning($"MainMenu: credits-button not found in UXML");
        if (_exitButton == null) Debug.LogWarning($"MainMenu: exit-button not found in UXML");
        
        SetupMenuEventListeners();
    }
    
    /// <summary>
    /// Set up event listeners for all menu buttons.
    /// AIDEV-NOTE: Connects button clicks to their respective handler methods.
    /// </summary>
    private void SetupMenuEventListeners()
    {
        if (_newGameButton != null)
            _newGameButton.clicked += OnNewGameClicked;
            
        if (_loadGameButton != null)
            _loadGameButton.clicked += OnLoadGameClicked;
            
        if (_settingsButton != null)
            _settingsButton.clicked += OnSettingsClicked;
            
        if (_creditsButton != null)
            _creditsButton.clicked += OnCreditsClicked;
            
        if (_exitButton != null)
            _exitButton.clicked += OnExitClicked;
    }
    
    /// <summary>
    /// Remove all menu event listeners.
    /// AIDEV-NOTE: Called in OnDestroy to prevent memory leaks.
    /// </summary>
    private void RemoveMenuEventListeners()
    {
        if (_newGameButton != null)
            _newGameButton.clicked -= OnNewGameClicked;
            
        if (_loadGameButton != null)
            _loadGameButton.clicked -= OnLoadGameClicked;
            
        if (_settingsButton != null)
            _settingsButton.clicked -= OnSettingsClicked;
            
        if (_creditsButton != null)
            _creditsButton.clicked -= OnCreditsClicked;
            
        if (_exitButton != null)
            _exitButton.clicked -= OnExitClicked;
    }
    
    /// <summary>
    /// Apply main menu specific styles and theme integration.
    /// AIDEV-NOTE: Adds main menu stylesheet and applies theme classes.
    /// </summary>
    private void ApplyMainMenuStyles()
    {
        if (_rootContainer == null) return;
        
        // Add main menu style classes to root
        _rootContainer.AddToClassList("main-menu-root");
        
        // Apply dark graphite theme first (provides CSS variables)
        if (_darkGraphiteTheme != null && !_rootContainer.styleSheets.Contains(_darkGraphiteTheme))
        {
            _rootContainer.styleSheets.Add(_darkGraphiteTheme);
        }
        
        // Apply main menu stylesheet (uses theme variables)
        if (_mainMenuStyles != null && !_rootContainer.styleSheets.Contains(_mainMenuStyles))
        {
            _rootContainer.styleSheets.Add(_mainMenuStyles);
        }
        
        // Set version information
        if (_versionLabel != null)
        {
            _versionLabel.text = $"Version {Application.version}";
        }
    }
    
    /// <summary>
    /// Initialize the menu state based on available save games and system state.
    /// AIDEV-NOTE: Updates button states based on save game availability.
    /// </summary>
    private void InitializeMenuState()
    {
        // Check for existing save games if detection is enabled
        if (_detectSaveGames)
        {
            CheckForSaveGames();
        }
        
        UpdateLoadGameButtonState();
    }
    
    /// <summary>
    /// Check if save games exist and update internal state.
    /// AIDEV-NOTE: Uses SaveFileManager to detect available save files.
    /// </summary>
    private void CheckForSaveGames()
    {
        // AIDEV-NOTE: Use SaveFileManager static methods to check for available save slots
        try
        {
            // Check for available save slots
            var saveSlots = SaveFileManager.GetAvailableSaveSlots();
            _hasSaveGames = saveSlots != null && saveSlots.Count > 0;
        }
        catch
        {
            // If save system isn't ready, assume no save games
            _hasSaveGames = false;
        }
    }
    
    /// <summary>
    /// Update the Load Game button state based on save game availability.
    /// AIDEV-NOTE: Enables/disables the Load Game button and updates its appearance.
    /// </summary>
    private void UpdateLoadGameButtonState()
    {
        if (_loadGameButton != null)
        {
            _loadGameButton.SetEnabled(_hasSaveGames);
            
            // Update button text based on availability
            if (_hasSaveGames)
            {
                _loadGameButton.text = "Load Game";
            }
            else
            {
                _loadGameButton.text = "No Saves";
            }
        }
    }
    
    // ===== BUTTON CLICK HANDLERS =====
    
    /// <summary>
    /// Handles New Game button click.
    /// AIDEV-NOTE: Should start the company creation flow or transition to game scene.
    /// </summary>
    private void OnNewGameClicked()
    {
        Debug.Log("Main Menu: New Game clicked");
        
        // Publish event for new game started
        EventBus.Publish(new MenuNavigationEvent("NewGame"));
        
        // AIDEV-TODO: For now, transition directly to game scene
        // Later this should open company creation modal first
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(_gameSceneName);
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager not available, using direct scene load");
            UnityEngine.SceneManagement.SceneManager.LoadScene(_gameSceneName);
        }
    }
    
    /// <summary>
    /// Handles Load Game button click.
    /// AIDEV-NOTE: Should open the load game modal or selection screen.
    /// </summary>
    private void OnLoadGameClicked()
    {
        if (!_hasSaveGames)
        {
            Debug.LogWarning("Main Menu: Load Game clicked but no save games available");
            return;
        }
        
        Debug.Log("Main Menu: Load Game clicked");
        
        // Publish event for load game requested
        EventBus.Publish(new MenuNavigationEvent("LoadGame"));
        
        // AIDEV-TODO: Open load game modal/selection screen
        // For now, just log the action
        Debug.Log("Load Game functionality not yet implemented");
    }
    
    /// <summary>
    /// Handles Settings button click.
    /// AIDEV-NOTE: Should open the main menu settings modal.
    /// </summary>
    private void OnSettingsClicked()
    {
        Debug.Log("Main Menu: Settings clicked");
        
        // Publish event for settings opened
        EventBus.Publish(new MenuNavigationEvent("Settings"));
        
        // AIDEV-TODO: Open settings modal
        // For now, just log the action
        Debug.Log("Settings modal functionality not yet implemented");
    }
    
    /// <summary>
    /// Handles Credits button click.
    /// AIDEV-NOTE: Transitions to the credits scene.
    /// </summary>
    private void OnCreditsClicked()
    {
        Debug.Log("Main Menu: Credits clicked");
        
        // Publish event for credits navigation
        EventBus.Publish(new MenuNavigationEvent("Credits"));
        
        // Transition to credits scene
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(_creditsSceneName);
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager not available, using direct scene load");
            UnityEngine.SceneManagement.SceneManager.LoadScene(_creditsSceneName);
        }
    }
    
    /// <summary>
    /// Handles Exit button click.
    /// AIDEV-NOTE: Shows confirmation and quits the application.
    /// </summary>
    private void OnExitClicked()
    {
        Debug.Log("Main Menu: Exit clicked");
        
        // Publish event for exit requested
        EventBus.Publish(new MenuNavigationEvent("Exit"));
        
        // AIDEV-TODO: Show exit confirmation modal
        // For now, quit immediately
        QuitApplication();
    }
    
    /// <summary>
    /// Quits the application with platform-appropriate behavior.
    /// AIDEV-NOTE: Handles different quit behavior for editor vs build.
    /// </summary>
    private void QuitApplication()
    {
        Debug.Log("Quitting application...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // ===== EVENT HANDLERS =====
    
    /// <summary>
    /// Handles GameSavedEvent to update Load Game button state.
    /// AIDEV-NOTE: Called when a game is saved to refresh the save game availability.
    /// </summary>
    /// <param name="saveEvent">The game saved event data</param>
    private void OnGameSaved(GameSavedEvent saveEvent)
    {
        if (saveEvent.Success)
        {
            _hasSaveGames = true;
            UpdateLoadGameButtonState();
        }
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing menu functionality in editor.
    /// </summary>
    [ContextMenu("Test New Game")]
    private void DebugTestNewGame()
    {
        OnNewGameClicked();
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for toggling save game availability in editor.
    /// </summary>
    [ContextMenu("Toggle Save Games Available")]
    private void DebugToggleSaveGames()
    {
        _hasSaveGames = !_hasSaveGames;
        UpdateLoadGameButtonState();
        Debug.Log($"Save games available: {_hasSaveGames}");
    }
    #endif
}

// AIDEV-NOTE: Menu navigation event for EventBus integration
namespace LogisticGame.Events
{
    public struct MenuNavigationEvent : IGameEvent
    {
        public string Action { get; }
        public MenuNavigationEvent(string action) => Action = action;
    }
}