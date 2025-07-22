using UnityEngine;
using UnityEngine.UIElements;
using System;
using LogisticGame.Events;

/// <summary>
/// AIDEV-NOTE: Specialized modal dialog component that extends BaseWindow functionality
/// with modal-specific behaviors like backdrop clicking, ESC key handling, and focus management.
/// </summary>
public class BaseModal : BaseWindow
{
    [Header("Modal Configuration")]
    [SerializeField] private bool _closeOnBackdropClick = true;
    [SerializeField] private bool _closeOnEscapeKey = true;
    [SerializeField] private bool _blockInteractionBehind = true;
    [SerializeField] private ModalSize _modalSize = ModalSize.Medium;
    
    [Header("Modal Animation")]
    [SerializeField] private ModalAnimationType _animationType = ModalAnimationType.FadeScale;
    [SerializeField] private float _backdropFadeDuration = 0.2f;
    
    // Modal-specific UI elements
    private VisualElement _modalBackdrop;
    private bool _isInitialized = false;
    
    // Modal events
    public event Action<BaseModal> OnModalOpened;
    public event Action<BaseModal> OnModalClosed;
    public event Action<BaseModal> OnBackdropClicked;
    
    // Properties
    public ModalSize ModalSize 
    { 
        get => _modalSize; 
        set 
        { 
            _modalSize = value;
            ApplyModalSize();
        } 
    }
    
    public bool CloseOnBackdropClick 
    { 
        get => _closeOnBackdropClick; 
        set => _closeOnBackdropClick = value; 
    }
    
    public bool CloseOnEscapeKey 
    { 
        get => _closeOnEscapeKey; 
        set => _closeOnEscapeKey = value; 
    }
    
    protected override void Awake()
    {
        // AIDEV-NOTE: Ensure modal is configured before base initialization
        _isModal = true; // Force modal mode for BaseModal
        _centerOnShow = true; // Always center modals
        
        base.Awake();
        InitializeModalSpecificElements();
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Register with ModalManager
        if (ModalManager.Instance != null)
        {
            ModalManager.Instance.RegisterModal(this);
        }
        
        ApplyModalSize();
        _isInitialized = true;
    }
    
    protected override void OnDestroy()
    {
        // Unregister from ModalManager
        if (ModalManager.Instance != null)
        {
            ModalManager.Instance.UnregisterModal(this);
        }
        
        RemoveModalEventListeners();
        base.OnDestroy();
    }
    
    /// <summary>
    /// Initializes modal-specific UI elements and behaviors.
    /// AIDEV-NOTE: Sets up backdrop clicking and modal-specific styling.
    /// </summary>
    private void InitializeModalSpecificElements()
    {
        if (_rootContainer == null) return;
        
        // Find or create modal backdrop
        _modalBackdrop = _rootContainer.Q<VisualElement>("modal-backdrop");
        
        if (_modalBackdrop == null)
        {
            // Create backdrop if it doesn't exist
            _modalBackdrop = new VisualElement();
            _modalBackdrop.name = "modal-backdrop";
            _modalBackdrop.AddToClassList("modal-backdrop");
            _rootContainer.Insert(0, _modalBackdrop); // Insert as first child (behind window)
        }
        
        SetupModalEventListeners();
        
        // Add modal-specific classes
        _rootContainer.AddToClassList("modal-root");
        if (_windowContainer != null)
        {
            _windowContainer.AddToClassList("modal-window");
        }
    }
    
    /// <summary>
    /// Sets up event listeners specific to modal behavior.
    /// AIDEV-NOTE: Handles backdrop clicks and keyboard input for modal interactions.
    /// </summary>
    private void SetupModalEventListeners()
    {
        if (_modalBackdrop != null)
        {
            _modalBackdrop.RegisterCallback<ClickEvent>(OnBackdropClickedHandler);
        }
        
        // Register for global keyboard events when modal is active
        if (_rootContainer != null)
        {
            _rootContainer.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }
    }
    
    /// <summary>
    /// Removes modal-specific event listeners.
    /// AIDEV-NOTE: Called during cleanup to prevent memory leaks.
    /// </summary>
    private void RemoveModalEventListeners()
    {
        if (_modalBackdrop != null)
        {
            _modalBackdrop.UnregisterCallback<ClickEvent>(OnBackdropClickedHandler);
        }
        
        if (_rootContainer != null)
        {
            _rootContainer.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }
    }
    
    /// <summary>
    /// Shows the modal dialog with modal-specific setup.
    /// AIDEV-NOTE: Overrides base ShowWindow to add modal stack management.
    /// </summary>
    /// <param name="animate">Whether to animate the modal appearance</param>
    public override void ShowWindow(bool animate = true)
    {
        if (IsVisible) return;
        
        // Add to modal stack before showing
        if (ModalManager.Instance != null)
        {
            ModalManager.Instance.PushModal(this);
        }
        
        // Show modal backdrop
        if (_modalBackdrop != null)
        {
            _modalBackdrop.style.display = DisplayStyle.Flex;
        }
        
        // Call base implementation
        base.ShowWindow(animate);
        
        // Focus the modal window for keyboard navigation
        FocusModal();
        
        OnModalOpened?.Invoke(this);
        
        // Publish modal-specific event
        EventBus.Publish(new ModalOpenedEvent(name, _modalSize));
    }
    
    /// <summary>
    /// Hides the modal dialog with cleanup.
    /// AIDEV-NOTE: Overrides base HideWindow to handle modal stack and backdrop.
    /// </summary>
    /// <param name="animate">Whether to animate the modal disappearance</param>
    public override void HideWindow(bool animate = true)
    {
        if (!IsVisible) return;
        
        // Remove from modal stack
        if (ModalManager.Instance != null)
        {
            ModalManager.Instance.PopModal(this);
        }
        
        // Hide modal backdrop
        if (_modalBackdrop != null)
        {
            _modalBackdrop.style.display = DisplayStyle.None;
        }
        
        // Call base implementation
        base.HideWindow(animate);
        
        OnModalClosed?.Invoke(this);
        
        // Publish modal-specific event
        EventBus.Publish(new ModalClosedEvent(name, _modalSize));
    }
    
    /// <summary>
    /// Applies size-specific styling to the modal.
    /// AIDEV-NOTE: Handles different modal size presets for various use cases.
    /// </summary>
    private void ApplyModalSize()
    {
        if (!_isInitialized || _windowContainer == null) return;
        
        // Remove existing size classes
        _windowContainer.RemoveFromClassList("modal-small");
        _windowContainer.RemoveFromClassList("modal-medium");
        _windowContainer.RemoveFromClassList("modal-large");
        _windowContainer.RemoveFromClassList("modal-fullscreen");
        
        // Apply new size class
        switch (_modalSize)
        {
            case ModalSize.Small:
                _windowContainer.AddToClassList("modal-small");
                break;
            case ModalSize.Medium:
                _windowContainer.AddToClassList("modal-medium");
                break;
            case ModalSize.Large:
                _windowContainer.AddToClassList("modal-large");
                break;
            case ModalSize.Fullscreen:
                _windowContainer.AddToClassList("modal-fullscreen");
                break;
        }
    }
    
    /// <summary>
    /// Focuses the modal for keyboard navigation.
    /// AIDEV-NOTE: Ensures proper focus management for accessibility.
    /// </summary>
    private void FocusModal()
    {
        if (_windowContainer != null)
        {
            _windowContainer.Focus();
        }
    }
    
    /// <summary>
    /// Handles backdrop click events.
    /// AIDEV-NOTE: Closes modal when user clicks outside the window if enabled.
    /// </summary>
    /// <param name="evt">The click event</param>
    private void OnBackdropClickedHandler(ClickEvent evt)
    {
        if (!_closeOnBackdropClick) return;
        
        OnBackdropClicked?.Invoke(this);
        
        // Close modal if click was on backdrop (not on window content)
        if (evt.target == _modalBackdrop)
        {
            HideWindow();
        }
    }
    
    /// <summary>
    /// Handles keyboard input for modal interactions.
    /// AIDEV-NOTE: Processes ESC key and other modal-specific keyboard shortcuts.
    /// </summary>
    /// <param name="evt">The keyboard event</param>
    private void OnKeyDown(KeyDownEvent evt)
    {
        if (!IsVisible) return;
        
        switch (evt.keyCode)
        {
            case KeyCode.Escape:
                if (_closeOnEscapeKey)
                {
                    evt.StopImmediatePropagation();
                    HideWindow();
                }
                break;
                
            case KeyCode.Tab:
                // AIDEV-TODO: Implement tab navigation within modal
                // This should cycle through focusable elements within the modal only
                break;
        }
    }
    
    /// <summary>
    /// Creates a simple confirmation modal with OK and Cancel buttons.
    /// AIDEV-NOTE: Utility method for creating common modal dialogs.
    /// </summary>
    /// <param name="title">Modal title</param>
    /// <param name="message">Confirmation message</param>
    /// <param name="onConfirm">Action to execute on confirmation</param>
    /// <param name="onCancel">Action to execute on cancellation</param>
    public void SetupConfirmationModal(string title, string message, Action onConfirm, Action onCancel = null)
    {
        WindowTitle = title;
        
        // Clear existing content
        if (ContentContainer != null)
        {
            ContentContainer.Clear();
            
            // Add message label
            var messageLabel = new Label(message);
            messageLabel.AddToClassList("modal-message");
            ContentContainer.Add(messageLabel);
        }
        
        // Clear and setup footer with buttons
        ClearFooterContent();
        
        // Cancel button
        var cancelButton = new Button(() => 
        {
            onCancel?.Invoke();
            HideWindow();
        });
        cancelButton.text = "Cancel";
        cancelButton.AddToClassList("modal-button");
        cancelButton.AddToClassList("modal-button-cancel");
        AddFooterContent(cancelButton);
        
        // Confirm button
        var confirmButton = new Button(() => 
        {
            onConfirm?.Invoke();
            HideWindow();
        });
        confirmButton.text = "OK";
        confirmButton.AddToClassList("modal-button");
        confirmButton.AddToClassList("modal-button-confirm");
        AddFooterContent(confirmButton);
    }
    
    /// <summary>
    /// Creates a simple alert modal with just an OK button.
    /// AIDEV-NOTE: Utility method for showing information or error messages.
    /// </summary>
    /// <param name="title">Modal title</param>
    /// <param name="message">Alert message</param>
    /// <param name="onClose">Action to execute when closed</param>
    public void SetupAlertModal(string title, string message, Action onClose = null)
    {
        WindowTitle = title;
        
        // Clear existing content
        if (ContentContainer != null)
        {
            ContentContainer.Clear();
            
            // Add message label
            var messageLabel = new Label(message);
            messageLabel.AddToClassList("modal-message");
            ContentContainer.Add(messageLabel);
        }
        
        // Clear and setup footer with OK button
        ClearFooterContent();
        
        var okButton = new Button(() => 
        {
            onClose?.Invoke();
            HideWindow();
        });
        okButton.text = "OK";
        okButton.AddToClassList("modal-button");
        okButton.AddToClassList("modal-button-primary");
        AddFooterContent(okButton);
    }
    
    #if UNITY_EDITOR
    [Header("Modal Debug")]
    [SerializeField] private bool _modalDebugMode = false;
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing modal-specific functionality.
    /// </summary>
    [ContextMenu("Test Confirmation Modal (Debug)")]
    private void DebugTestConfirmationModal()
    {
        if (_modalDebugMode)
        {
            SetupConfirmationModal("Test Confirmation", "This is a test confirmation modal. Do you want to proceed?", 
                () => Debug.Log("User confirmed"), 
                () => Debug.Log("User cancelled"));
            ShowWindow();
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing alert modal functionality.
    /// </summary>
    [ContextMenu("Test Alert Modal (Debug)")]
    private void DebugTestAlertModal()
    {
        if (_modalDebugMode)
        {
            SetupAlertModal("Test Alert", "This is a test alert modal message.", 
                () => Debug.Log("Alert closed"));
            ShowWindow();
        }
    }
    #endif
}

/// <summary>
/// AIDEV-NOTE: Enumeration for different modal size presets.
/// </summary>
public enum ModalSize
{
    Small,      // Small confirmation dialogs, alerts
    Medium,     // Standard modals, forms
    Large,      // Complex forms, detailed views
    Fullscreen  // Full-screen overlays
}

/// <summary>
/// AIDEV-NOTE: Enumeration for modal animation types.
/// </summary>
public enum ModalAnimationType
{
    None,           // No animation
    Fade,           // Simple fade in/out
    Scale,          // Scale from center
    FadeScale,      // Combined fade and scale
    SlideDown,      // Slide from top
    SlideUp         // Slide from bottom
}