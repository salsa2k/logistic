using UnityEngine;
using UnityEngine.UIElements;
using System;
using LogisticGame.Events;

/// <summary>
/// AIDEV-NOTE: Base window component for all major game interfaces. Provides consistent window behavior,
/// styling, and animations across the logistics game UI system.
/// </summary>
public class BaseWindow : MonoBehaviour
{
    [Header("Window Configuration")]
    [SerializeField] protected string _windowTitle = "Window";
    [SerializeField] protected bool _isModal = false;
    [SerializeField] protected bool _showCloseButton = true;
    [SerializeField] protected bool _centerOnShow = true;
    
    [Header("UI References")]
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private StyleSheet _baseWindowStyles;
    
    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.3f;
    [SerializeField] private AnimationCurve _showCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve _hideCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    // UI Elements
    protected VisualElement _rootContainer;
    protected VisualElement _windowContainer;
    protected Label _titleLabel;
    protected Button _closeButton;
    protected VisualElement _contentContainer;
    protected VisualElement _footerContainer;
    
    // State
    private bool _isVisible = false;
    private bool _isAnimating = false;
    
    // Events
    public event Action<BaseWindow> OnWindowShown;
    public event Action<BaseWindow> OnWindowHidden;
    public event Action<BaseWindow> OnCloseButtonClicked;
    
    // Properties
    public string WindowTitle 
    { 
        get => _windowTitle; 
        set 
        { 
            _windowTitle = value;
            UpdateTitle();
        } 
    }
    
    public bool IsVisible => _isVisible;
    public bool IsModal => _isModal;
    public VisualElement ContentContainer => _contentContainer;
    public VisualElement FooterContainer => _footerContainer;
    
    protected virtual void Awake()
    {
        // AIDEV-NOTE: Initialize UI document if not set in inspector
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
            
        if (_uiDocument == null)
        {
            Debug.LogError($"BaseWindow '{name}': UIDocument component is required");
            return;
        }
        
        InitializeUIElements();
        SetupEventListeners();
        
        // Register with ThemeManager
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.RegisterUIDocument(_uiDocument);
        }
    }
    
    protected virtual void Start()
    {
        // AIDEV-NOTE: Hide window by default
        SetWindowVisibility(false, false);
        ApplyWindowStyles();
    }
    
    protected virtual void OnDestroy()
    {
        // AIDEV-NOTE: Clean up event listeners and unregister from ThemeManager
        RemoveEventListeners();
        
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.UnregisterUIDocument(_uiDocument);
        }
    }
    
    /// <summary>
    /// Initializes UI element references from the UXML template.
    /// AIDEV-NOTE: This method binds C# references to UXML elements.
    /// </summary>
    private void InitializeUIElements()
    {
        if (_uiDocument?.rootVisualElement == null)
        {
            Debug.LogError($"BaseWindow '{name}': Root visual element is null");
            return;
        }
        
        _rootContainer = _uiDocument.rootVisualElement;
        _windowContainer = _rootContainer.Q<VisualElement>("window-container");
        _titleLabel = _rootContainer.Q<Label>("window-title");
        _closeButton = _rootContainer.Q<Button>("close-button");
        _contentContainer = _rootContainer.Q<VisualElement>("content-container");
        _footerContainer = _rootContainer.Q<VisualElement>("footer-container");
        
        // AIDEV-NOTE: Log missing elements for debugging
        if (_windowContainer == null) Debug.LogWarning($"BaseWindow '{name}': window-container not found in UXML");
        if (_titleLabel == null) Debug.LogWarning($"BaseWindow '{name}': window-title not found in UXML");
        if (_closeButton == null) Debug.LogWarning($"BaseWindow '{name}': close-button not found in UXML");
        if (_contentContainer == null) Debug.LogWarning($"BaseWindow '{name}': content-container not found in UXML");
        if (_footerContainer == null) Debug.LogWarning($"BaseWindow '{name}': footer-container not found in UXML");
        
        UpdateTitle();
        UpdateCloseButtonVisibility();
    }
    
    /// <summary>
    /// Sets up event listeners for UI interactions.
    /// AIDEV-NOTE: Handles close button clicks and other window-specific events.
    /// </summary>
    private void SetupEventListeners()
    {
        if (_closeButton != null)
        {
            _closeButton.clicked += OnCloseButtonPressed;
        }
    }
    
    /// <summary>
    /// Removes event listeners to prevent memory leaks.
    /// AIDEV-NOTE: Called in OnDestroy to ensure clean cleanup.
    /// </summary>
    private void RemoveEventListeners()
    {
        if (_closeButton != null)
        {
            _closeButton.clicked -= OnCloseButtonPressed;
        }
    }
    
    /// <summary>
    /// Shows the window with optional animation.
    /// AIDEV-NOTE: Main method for displaying the window to users.
    /// </summary>
    /// <param name="animate">Whether to animate the show transition</param>
    public virtual void ShowWindow(bool animate = true)
    {
        if (_isVisible || _isAnimating) return;
        
        if (_centerOnShow)
        {
            CenterWindow();
        }
        
        SetWindowVisibility(true, animate);
        
        if (animate && _animationDuration > 0)
        {
            StartCoroutine(AnimateWindowShow());
        }
        else
        {
            OnWindowShown?.Invoke(this);
            // AIDEV-NOTE: Publish window opened event to EventBus
            EventBus.Publish(new WindowOpenedEvent(name, _isModal));
        }
    }
    
    /// <summary>
    /// Hides the window with optional animation.
    /// AIDEV-NOTE: Main method for hiding the window from users.
    /// </summary>
    /// <param name="animate">Whether to animate the hide transition</param>
    public virtual void HideWindow(bool animate = true)
    {
        if (!_isVisible || _isAnimating) return;
        
        if (animate && _animationDuration > 0)
        {
            StartCoroutine(AnimateWindowHide());
        }
        else
        {
            SetWindowVisibility(false, false);
            OnWindowHidden?.Invoke(this);
            // AIDEV-NOTE: Publish window closed event to EventBus
            EventBus.Publish(new WindowClosedEvent(name, _isModal));
        }
    }
    
    /// <summary>
    /// Toggles window visibility.
    /// AIDEV-NOTE: Convenience method for show/hide toggle.
    /// </summary>
    /// <param name="animate">Whether to animate the transition</param>
    public void ToggleWindow(bool animate = true)
    {
        if (_isVisible)
            HideWindow(animate);
        else
            ShowWindow(animate);
    }
    
    /// <summary>
    /// Sets the window content by replacing the content container's children.
    /// AIDEV-NOTE: Use this method to dynamically load content into the window.
    /// </summary>
    /// <param name="content">The visual element to set as content</param>
    public void SetContent(VisualElement content)
    {
        if (_contentContainer == null || content == null) return;
        
        _contentContainer.Clear();
        _contentContainer.Add(content);
    }
    
    /// <summary>
    /// Adds content to the footer area.
    /// AIDEV-NOTE: Useful for action buttons like OK, Cancel, Apply, etc.
    /// </summary>
    /// <param name="footerContent">The visual element to add to footer</param>
    public void AddFooterContent(VisualElement footerContent)
    {
        if (_footerContainer != null && footerContent != null)
        {
            _footerContainer.Add(footerContent);
        }
    }
    
    /// <summary>
    /// Clears all footer content.
    /// AIDEV-NOTE: Use this to reset footer before adding new buttons.
    /// </summary>
    public void ClearFooterContent()
    {
        _footerContainer?.Clear();
    }
    
    /// <summary>
    /// Centers the window on the screen.
    /// AIDEV-NOTE: Positions window in the center of the parent container.
    /// </summary>
    public void CenterWindow()
    {
        if (_windowContainer == null) return;
        
        // AIDEV-NOTE: Unity UI Toolkit centering using flex properties
        _windowContainer.style.position = Position.Absolute;
        _windowContainer.style.left = Length.Percent(50);
        _windowContainer.style.top = Length.Percent(50);
        _windowContainer.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
    }
    
    /// <summary>
    /// Updates the window title display.
    /// AIDEV-NOTE: Called when WindowTitle property changes.
    /// </summary>
    private void UpdateTitle()
    {
        if (_titleLabel != null)
        {
            _titleLabel.text = _windowTitle;
        }
    }
    
    /// <summary>
    /// Updates close button visibility based on settings.
    /// AIDEV-NOTE: Handles show/hide of close button based on _showCloseButton field.
    /// </summary>
    private void UpdateCloseButtonVisibility()
    {
        if (_closeButton != null)
        {
            _closeButton.style.display = _showCloseButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Applies window-specific styles and theme integration.
    /// AIDEV-NOTE: Ensures proper styling is applied to window elements.
    /// </summary>
    private void ApplyWindowStyles()
    {
        if (_rootContainer == null) return;
        
        // Add base window style class
        _rootContainer.AddToClassList("base-window");
        
        // Apply additional styles from StyleSheet if available
        if (_baseWindowStyles != null && !_rootContainer.styleSheets.Contains(_baseWindowStyles))
        {
            _rootContainer.styleSheets.Add(_baseWindowStyles);
        }
    }
    
    /// <summary>
    /// Sets window visibility without animation.
    /// AIDEV-NOTE: Internal method for immediate visibility changes.
    /// </summary>
    /// <param name="visible">Target visibility state</param>
    /// <param name="updateState">Whether to update the _isVisible state</param>
    private void SetWindowVisibility(bool visible, bool updateState = true)
    {
        if (_rootContainer != null)
        {
            _rootContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        if (updateState)
        {
            _isVisible = visible;
        }
    }
    
    /// <summary>
    /// Handles close button click events.
    /// AIDEV-NOTE: Called when user clicks the X button on the window.
    /// </summary>
    private void OnCloseButtonPressed()
    {
        OnCloseButtonClicked?.Invoke(this);
        HideWindow();
    }
    
    /// <summary>
    /// Coroutine for animating window show transition.
    /// AIDEV-NOTE: Provides smooth fade-in and scale animation for window appearance.
    /// </summary>
    private System.Collections.IEnumerator AnimateWindowShow()
    {
        _isAnimating = true;
        float elapsedTime = 0f;
        
        // AIDEV-NOTE: Unity UI Toolkit doesn't support opacity or scale animations directly
        // This is a placeholder for future animation system integration
        while (elapsedTime < _animationDuration)
        {
            float progress = elapsedTime / _animationDuration;
            float curveValue = _showCurve.Evaluate(progress);
            
            // AIDEV-TODO: Implement actual animation once Unity UI Toolkit supports it
            // For now, we'll just wait for the duration
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        _isAnimating = false;
        OnWindowShown?.Invoke(this);
        // AIDEV-NOTE: Publish window opened event to EventBus after animation
        EventBus.Publish(new WindowOpenedEvent(name, _isModal));
    }
    
    /// <summary>
    /// Coroutine for animating window hide transition.
    /// AIDEV-NOTE: Provides smooth fade-out animation for window disappearance.
    /// </summary>
    private System.Collections.IEnumerator AnimateWindowHide()
    {
        _isAnimating = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < _animationDuration)
        {
            float progress = elapsedTime / _animationDuration;
            float curveValue = _hideCurve.Evaluate(progress);
            
            // AIDEV-TODO: Implement actual animation once Unity UI Toolkit supports it
            // For now, we'll just wait for the duration
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        SetWindowVisibility(false, true);
        _isAnimating = false;
        OnWindowHidden?.Invoke(this);
        // AIDEV-NOTE: Publish window closed event to EventBus after animation
        EventBus.Publish(new WindowClosedEvent(name, _isModal));
    }
    
    #if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing window show/hide in editor.
    /// </summary>
    [ContextMenu("Show Window (Debug)")]
    private void DebugShowWindow()
    {
        if (_debugMode) ShowWindow();
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing window hide in editor.
    /// </summary>
    [ContextMenu("Hide Window (Debug)")]
    private void DebugHideWindow()
    {
        if (_debugMode) HideWindow();
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing window toggle in editor.
    /// </summary>
    [ContextMenu("Toggle Window (Debug)")]
    private void DebugToggleWindow()
    {
        if (_debugMode) ToggleWindow();
    }
    #endif
}