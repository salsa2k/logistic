using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace LogisticGame.UI
{
    /// <summary>
    /// Simple Credits scene controller for full-screen credits display.
    /// </summary>
    // Force recompile
    public class CreditsController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private StyleSheet _creditsStyleSheet;
        
        [Header("Credits Configuration")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        
        // UI element references
        private VisualElement _rootContainer;
        private ScrollView _creditsScrollView;
        private Button _backButton;
        private Button _skipButton;
        private Label _versionLabel;
        
        private void Awake()
        {
            // Initialize UI document
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();
                
            if (_uiDocument == null)
            {
                Debug.LogError($"CreditsController: UIDocument component is required");
                return;
            }
            
            // Get root container
            _rootContainer = _uiDocument.rootVisualElement;
            
            if (_rootContainer == null)
            {
                Debug.LogError($"CreditsController: Root visual element not found");
                return;
            }
            
            Debug.Log($"CreditsController: Initialized with {_rootContainer.childCount} children");
        }
        
        private void Start()
        {
            // Apply styles if not already applied
            ApplyStyles();
            
            // Find UI elements
            FindUIElements();
            
            // Setup event listeners
            SetupEventListeners();
            
            // Update version
            UpdateVersion();
            
            Debug.Log("CreditsController: Setup complete");
        }
        
        private void ApplyStyles()
        {
            if (_creditsStyleSheet != null && _rootContainer != null)
            {
                if (!_rootContainer.styleSheets.Contains(_creditsStyleSheet))
                {
                    _rootContainer.styleSheets.Add(_creditsStyleSheet);
                    Debug.Log("CreditsController: StyleSheet applied programmatically");
                }
            }
        }
        
        private void FindUIElements()
        {
            if (_rootContainer == null) return;
            
            _creditsScrollView = _rootContainer.Q<ScrollView>("credits-scroll");
            _backButton = _rootContainer.Q<Button>("back-button");
            _skipButton = _rootContainer.Q<Button>("skip-button");
            _versionLabel = _rootContainer.Q<Label>("version-label");
            
            Debug.Log($"Credits elements found - Scroll: {_creditsScrollView != null}, Back: {_backButton != null}, Skip: {_skipButton != null}, Version: {_versionLabel != null}");
        }
        
        private void SetupEventListeners()
        {
            if (_backButton != null)
                _backButton.clicked += OnBackClicked;
                
            if (_skipButton != null)
                _skipButton.clicked += OnSkipClicked;
        }
        
        private void UpdateVersion()
        {
            if (_versionLabel != null)
            {
                _versionLabel.text = $"Version {Application.version}";
            }
        }
        
        private void OnBackClicked()
        {
            Debug.Log("Credits: Back to menu clicked");
            
            // Load main menu scene
            SceneManager.LoadScene(_mainMenuSceneName);
        }
        
        private void OnSkipClicked()
        {
            Debug.Log("Credits: Skip clicked");
            
            // Scroll to bottom
            if (_creditsScrollView != null)
            {
                _creditsScrollView.scrollOffset = new Vector2(0, _creditsScrollView.contentContainer.resolvedStyle.height);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (_backButton != null)
                _backButton.clicked -= OnBackClicked;
                
            if (_skipButton != null)
                _skipButton.clicked -= OnSkipClicked;
        }
    }
}