using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using LogisticGame.Events;

/// <summary>
/// AIDEV-NOTE: Base card component for displaying various types of data in a consistent card format.
/// Supports data binding, interactive states, and theme integration.
/// </summary>
public class BaseCard : MonoBehaviour
{
    [Header("Card Configuration")]
    [SerializeField] private bool _isSelectable = true;
    [SerializeField] private bool _showFooter = true;
    [SerializeField] private bool _enableDoubleClickAction = true;
    [SerializeField] private bool _enableContextMenu = true;
    
    [Header("UI References")]
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private StyleSheet _baseCardStyles;
    
    [Header("Animation Settings")]
    // AIDEV-NOTE: Animation duration for future implementation of card transitions
    // Currently not used but reserved for animation system integration
    [SerializeField] private float _animationDuration = 0.2f;
    
    // UI Elements
    private VisualElement _cardRoot;
    private VisualElement _badgeContainer;
    private Label _badgeLabel;
    private VisualElement _headerContainer;
    private VisualElement _iconContainer;
    private Image _iconImage;
    private Label _titleLabel;
    private Label _subtitleLabel;
    private VisualElement _imageContainer;
    private Image _previewImage;
    private VisualElement _contentContainer;
    private Label _descriptionLabel;
    private VisualElement _detailsContainer;
    private VisualElement _footerContainer;
    private VisualElement _statusContainer;
    private VisualElement _actionsContainer;
    private Button _primaryButton;
    private Button _secondaryButton;
    private VisualElement _loadingOverlay;
    
    // State
    private ICardData _currentData;
    private bool _isSelected = false;
    private bool _isDisabled = false;
    private bool _isLoading = false;
    
    // Events
    public event Action<BaseCard, ICardData> OnCardClicked;
    public event Action<BaseCard, ICardData> OnCardDoubleClicked;
    public event Action<BaseCard, ICardData> OnCardSelected;
    public event Action<BaseCard, ICardData> OnCardDeselected;
    public event Action<BaseCard, ICardData> OnPrimaryActionClicked;
    public event Action<BaseCard, ICardData> OnSecondaryActionClicked;
    public event Action<BaseCard, ICardData, Vector2> OnCardRightClicked;
    
    // Properties
    public ICardData CurrentData => _currentData;
    public bool IsSelected => _isSelected;
    public bool IsDisabled => _isDisabled;
    public bool IsLoading => _isLoading;
    public string CardId => _currentData?.Id ?? "";
    
    // Click handling
    private float _lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.5f;
    
    private void Awake()
    {
        // AIDEV-NOTE: Initialize UI document if not set in inspector
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
            
        if (_uiDocument == null)
        {
            Debug.LogError($"BaseCard '{name}': UIDocument component is required");
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
    
    private void Start()
    {
        ApplyCardStyles();
        UpdateVisualState();
    }
    
    private void OnDestroy()
    {
        // AIDEV-NOTE: Clean up event listeners and unregister from ThemeManager
        RemoveEventListeners();
        
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.UnregisterUIDocument(_uiDocument);
        }
    }
    
    /// <summary>
    /// Binds data to the card and updates the display.
    /// AIDEV-NOTE: Main method for populating card content with data.
    /// </summary>
    /// <param name="data">The data to bind to this card</param>
    public void BindData(ICardData data)
    {
        _currentData = data;
        
        if (data == null)
        {
            ClearCard();
            return;
        }
        
        UpdateCardContent();
        UpdateVisualState();
        
        // AIDEV-NOTE: Publish card data bound event
        EventBus.Publish(new CardDataBoundEvent(CardId, data.GetType().Name));
    }
    
    /// <summary>
    /// Sets the card's selected state.
    /// AIDEV-NOTE: Updates visual state and triggers selection events.
    /// </summary>
    /// <param name="selected">Whether the card should be selected</param>
    public void SetSelected(bool selected)
    {
        if (!_isSelectable || _isSelected == selected) return;
        
        bool previousState = _isSelected;
        _isSelected = selected;
        
        UpdateVisualState();
        
        // Trigger selection events
        if (_isSelected && !previousState)
        {
            OnCardSelected?.Invoke(this, _currentData);
            EventBus.Publish(new CardSelectedEvent(CardId));
        }
        else if (!_isSelected && previousState)
        {
            OnCardDeselected?.Invoke(this, _currentData);
            EventBus.Publish(new CardDeselectedEvent(CardId));
        }
    }
    
    /// <summary>
    /// Sets the card's disabled state.
    /// AIDEV-NOTE: Disables interactions and updates visual appearance.
    /// </summary>
    /// <param name="disabled">Whether the card should be disabled</param>
    public void SetDisabled(bool disabled)
    {
        _isDisabled = disabled;
        UpdateVisualState();
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Sets the card's loading state.
    /// AIDEV-NOTE: Shows/hides loading overlay and disables interactions.
    /// </summary>
    /// <param name="loading">Whether the card should show loading state</param>
    public void SetLoading(bool loading)
    {
        _isLoading = loading;
        
        if (_loadingOverlay != null)
        {
            _loadingOverlay.style.display = loading ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        UpdateVisualState();
    }
    
    /// <summary>
    /// Adds a custom detail row to the card content.
    /// AIDEV-NOTE: Useful for dynamic content that's not in the base interface.
    /// </summary>
    /// <param name="label">The label text</param>
    /// <param name="value">The value text</param>
    public void AddDetailRow(string label, string value)
    {
        if (_detailsContainer == null || string.IsNullOrEmpty(label)) return;
        
        var detailRow = new VisualElement();
        detailRow.AddToClassList("card-detail-row");
        
        var labelElement = new Label(label);
        labelElement.AddToClassList("card-detail-label");
        
        var valueElement = new Label(value);
        valueElement.AddToClassList("card-detail-value");
        
        detailRow.Add(labelElement);
        detailRow.Add(valueElement);
        
        _detailsContainer.Add(detailRow);
    }
    
    /// <summary>
    /// Clears all dynamic detail rows from the card.
    /// AIDEV-NOTE: Use this before repopulating with new data.
    /// </summary>
    public void ClearDetailRows()
    {
        if (_detailsContainer != null)
        {
            _detailsContainer.Clear();
        }
    }
    
    /// <summary>
    /// Initializes UI element references from the UXML template.
    /// AIDEV-NOTE: Binds C# references to UXML elements for manipulation.
    /// </summary>
    private void InitializeUIElements()
    {
        if (_uiDocument?.rootVisualElement == null)
        {
            Debug.LogError($"BaseCard '{name}': Root visual element is null");
            return;
        }
        
        var root = _uiDocument.rootVisualElement;
        
        _cardRoot = root.Q<VisualElement>("card-root");
        _badgeContainer = root.Q<VisualElement>("card-badge-container");
        _badgeLabel = root.Q<Label>("card-badge");
        _headerContainer = root.Q<VisualElement>("card-header");
        _iconContainer = root.Q<VisualElement>("card-icon-container");
        _iconImage = root.Q<Image>("card-icon");
        _titleLabel = root.Q<Label>("card-title");
        _subtitleLabel = root.Q<Label>("card-subtitle");
        _imageContainer = root.Q<VisualElement>("card-image-container");
        _previewImage = root.Q<Image>("card-image");
        _contentContainer = root.Q<VisualElement>("card-content");
        _descriptionLabel = root.Q<Label>("card-description");
        _detailsContainer = root.Q<VisualElement>("card-details-container");
        _footerContainer = root.Q<VisualElement>("card-footer");
        _statusContainer = root.Q<VisualElement>("card-status-container");
        _actionsContainer = root.Q<VisualElement>("card-actions-container");
        _primaryButton = root.Q<Button>("card-primary-button");
        _secondaryButton = root.Q<Button>("card-secondary-button");
        _loadingOverlay = root.Q<VisualElement>("card-loading-overlay");
        
        // AIDEV-NOTE: Log missing critical elements
        if (_cardRoot == null) Debug.LogError($"BaseCard '{name}': card-root not found in UXML");
        if (_titleLabel == null) Debug.LogError($"BaseCard '{name}': card-title not found in UXML");
    }
    
    /// <summary>
    /// Sets up event listeners for UI interactions.
    /// AIDEV-NOTE: Handles clicks, button actions, and keyboard navigation.
    /// </summary>
    private void SetupEventListeners()
    {
        if (_cardRoot != null)
        {
            // Register for mouse and keyboard events
            _cardRoot.RegisterCallback<ClickEvent>(OnCardClickedHandler);
            _cardRoot.RegisterCallback<PointerUpEvent>(OnCardPointerUp);
            _cardRoot.RegisterCallback<KeyDownEvent>(OnCardKeyDown);
            
            // Make the card focusable for keyboard navigation
            _cardRoot.focusable = true;
            _cardRoot.tabIndex = 0;
        }
        
        if (_primaryButton != null)
        {
            _primaryButton.clicked += OnPrimaryButtonClicked;
        }
        
        if (_secondaryButton != null)
        {
            _secondaryButton.clicked += OnSecondaryButtonClicked;
        }
    }
    
    /// <summary>
    /// Removes event listeners to prevent memory leaks.
    /// AIDEV-NOTE: Called in OnDestroy to ensure clean cleanup.
    /// </summary>
    private void RemoveEventListeners()
    {
        if (_cardRoot != null)
        {
            _cardRoot.UnregisterCallback<ClickEvent>(OnCardClickedHandler);
            _cardRoot.UnregisterCallback<PointerUpEvent>(OnCardPointerUp);
            _cardRoot.UnregisterCallback<KeyDownEvent>(OnCardKeyDown);
        }
        
        if (_primaryButton != null)
        {
            _primaryButton.clicked -= OnPrimaryButtonClicked;
        }
        
        if (_secondaryButton != null)
        {
            _secondaryButton.clicked -= OnSecondaryButtonClicked;
        }
    }
    
    /// <summary>
    /// Updates the card content based on the bound data.
    /// AIDEV-NOTE: Core method for refreshing card display when data changes.
    /// </summary>
    private void UpdateCardContent()
    {
        if (_currentData == null) return;
        
        // Update basic content
        UpdateTitle(_currentData.Title);
        UpdateSubtitle(_currentData.Subtitle);
        UpdateIcon(_currentData.Icon);
        UpdateBadge(_currentData.BadgeText);
        
        // Update detailed content if available
        if (_currentData is IDetailedCardData detailedData)
        {
            UpdateDescription(detailedData.Description);
            UpdatePreviewImage(detailedData.PreviewImage);
            UpdateDetails(detailedData.Details);
        }
        
        // Update action buttons if available
        if (_currentData is IActionCardData actionData)
        {
            UpdateActionButtons(actionData);
        }
        
        // Update footer visibility
        UpdateFooterVisibility();
    }
    
    /// <summary>
    /// Updates the card's visual state based on current flags.
    /// AIDEV-NOTE: Applies CSS classes for different card states.
    /// </summary>
    private void UpdateVisualState()
    {
        if (_cardRoot == null) return;
        
        // Clear existing state classes
        _cardRoot.RemoveFromClassList("card-selected");
        _cardRoot.RemoveFromClassList("card-disabled");
        
        // Apply current state classes
        if (_isSelected)
            _cardRoot.AddToClassList("card-selected");
            
        if (_isDisabled)
            _cardRoot.AddToClassList("card-disabled");
        
        // Update interaction state from data
        if (_currentData != null)
        {
            SetSelected(_currentData.IsSelected);
            SetDisabled(_currentData.IsDisabled);
        }
    }
    
    /// <summary>
    /// Updates the title label with null checking and visibility.
    /// AIDEV-NOTE: Handles empty titles gracefully.
    /// </summary>
    private void UpdateTitle(string title)
    {
        if (_titleLabel == null) return;
        
        if (!string.IsNullOrEmpty(title))
        {
            _titleLabel.text = title;
            _titleLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            _titleLabel.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates the subtitle label with null checking and visibility.
    /// AIDEV-NOTE: Hides subtitle if empty to maintain clean layout.
    /// </summary>
    private void UpdateSubtitle(string subtitle)
    {
        if (_subtitleLabel == null) return;
        
        if (!string.IsNullOrEmpty(subtitle))
        {
            _subtitleLabel.text = subtitle;
            _subtitleLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            _subtitleLabel.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates the icon image with null checking and visibility.
    /// AIDEV-NOTE: Uses AssetManager if available for consistent asset loading.
    /// </summary>
    private void UpdateIcon(Sprite icon)
    {
        if (_iconContainer == null || _iconImage == null) return;
        
        if (icon != null)
        {
            _iconImage.sprite = icon;
            _iconContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            _iconContainer.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates the badge label with null checking and visibility.
    /// AIDEV-NOTE: Badge is used for categorization and status indicators.
    /// </summary>
    private void UpdateBadge(string badgeText)
    {
        if (_badgeContainer == null || _badgeLabel == null) return;
        
        if (!string.IsNullOrEmpty(badgeText))
        {
            _badgeLabel.text = badgeText;
            _badgeContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            _badgeContainer.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates the description label for detailed card content.
    /// AIDEV-NOTE: Used for longer descriptive text in card body.
    /// </summary>
    private void UpdateDescription(string description)
    {
        if (_descriptionLabel == null) return;
        
        if (!string.IsNullOrEmpty(description))
        {
            _descriptionLabel.text = description;
            _descriptionLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            _descriptionLabel.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates the preview image for detailed card content.
    /// AIDEV-NOTE: Used for larger images that showcase the card content.
    /// </summary>
    private void UpdatePreviewImage(Sprite previewImage)
    {
        if (_imageContainer == null || _previewImage == null) return;
        
        if (previewImage != null)
        {
            _previewImage.sprite = previewImage;
            _imageContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            _imageContainer.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates the details section with key-value pairs.
    /// AIDEV-NOTE: Dynamically creates detail rows for structured information.
    /// </summary>
    private void UpdateDetails(Dictionary<string, string> details)
    {
        ClearDetailRows();
        
        if (details == null || details.Count == 0) return;
        
        foreach (var kvp in details)
        {
            AddDetailRow(kvp.Key, kvp.Value);
        }
    }
    
    /// <summary>
    /// Updates action buttons based on card data.
    /// AIDEV-NOTE: Handles button text, visibility, and enabled state.
    /// </summary>
    private void UpdateActionButtons(IActionCardData actionData)
    {
        // Update primary button
        if (_primaryButton != null)
        {
            if (!string.IsNullOrEmpty(actionData.PrimaryActionText))
            {
                _primaryButton.text = actionData.PrimaryActionText;
                _primaryButton.SetEnabled(actionData.IsPrimaryActionEnabled && !_isDisabled);
                _primaryButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _primaryButton.style.display = DisplayStyle.None;
            }
        }
        
        // Update secondary button
        if (_secondaryButton != null)
        {
            if (!string.IsNullOrEmpty(actionData.SecondaryActionText))
            {
                _secondaryButton.text = actionData.SecondaryActionText;
                _secondaryButton.SetEnabled(actionData.IsSecondaryActionEnabled && !_isDisabled);
                _secondaryButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _secondaryButton.style.display = DisplayStyle.None;
            }
        }
    }
    
    /// <summary>
    /// Updates button enabled states based on card state.
    /// AIDEV-NOTE: Called when card is disabled/enabled to update button states.
    /// </summary>
    private void UpdateButtonStates()
    {
        bool buttonsEnabled = !_isDisabled && !_isLoading;
        
        if (_primaryButton != null && _currentData is IActionCardData actionData)
        {
            _primaryButton.SetEnabled(buttonsEnabled && actionData.IsPrimaryActionEnabled);
        }
        
        if (_secondaryButton != null && _currentData is IActionCardData actionData2)
        {
            _secondaryButton.SetEnabled(buttonsEnabled && actionData2.IsSecondaryActionEnabled);
        }
    }
    
    /// <summary>
    /// Updates footer visibility based on content and settings.
    /// AIDEV-NOTE: Hides footer if no buttons or content are available.
    /// </summary>
    private void UpdateFooterVisibility()
    {
        if (_footerContainer == null) return;
        
        bool hasVisibleContent = false;
        
        // Check if any buttons are visible
        if (_primaryButton != null && _primaryButton.style.display == DisplayStyle.Flex)
            hasVisibleContent = true;
        if (_secondaryButton != null && _secondaryButton.style.display == DisplayStyle.Flex)
            hasVisibleContent = true;
        
        // Show footer if it has content and is enabled
        _footerContainer.style.display = (_showFooter && hasVisibleContent) ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    /// <summary>
    /// Clears all card content when no data is bound.
    /// AIDEV-NOTE: Resets card to empty state.
    /// </summary>
    private void ClearCard()
    {
        UpdateTitle("");
        UpdateSubtitle("");
        UpdateIcon(null);
        UpdateBadge("");
        UpdateDescription("");
        UpdatePreviewImage(null);
        ClearDetailRows();
        
        if (_footerContainer != null)
            _footerContainer.style.display = DisplayStyle.None;
    }
    
    /// <summary>
    /// Applies card-specific styles and theme integration.
    /// AIDEV-NOTE: Ensures proper styling is applied to card elements.
    /// </summary>
    private void ApplyCardStyles()
    {
        if (_cardRoot == null) return;
        
        // Add base card style class
        _cardRoot.AddToClassList("base-card");
        
        // Apply additional styles from StyleSheet if available
        if (_baseCardStyles != null && !_uiDocument.rootVisualElement.styleSheets.Contains(_baseCardStyles))
        {
            _uiDocument.rootVisualElement.styleSheets.Add(_baseCardStyles);
        }
    }
    
    // Event Handlers
    
    /// <summary>
    /// Handles card click events with double-click detection.
    /// AIDEV-NOTE: Manages single and double-click actions with timing.
    /// </summary>
    private void OnCardClickedHandler(ClickEvent evt)
    {
        if (_isDisabled || _isLoading) return;
        
        float currentTime = Time.time;
        
        if (_enableDoubleClickAction && (currentTime - _lastClickTime) < DOUBLE_CLICK_TIME)
        {
            // Double click detected
            OnCardDoubleClicked?.Invoke(this, _currentData);
            _lastClickTime = 0f; // Reset to prevent triple-click
        }
        else
        {
            // Single click
            if (_isSelectable)
                SetSelected(!_isSelected);
                
            OnCardClicked?.Invoke(this, _currentData);
            _lastClickTime = currentTime;
        }
        
        evt.StopPropagation();
    }
    
    /// <summary>
    /// Handles pointer up events for right-click detection.
    /// AIDEV-NOTE: Unity UI Toolkit doesn't have right-click events, so we use pointer events.
    /// </summary>
    private void OnCardPointerUp(PointerUpEvent evt)
    {
        if (_isDisabled || _isLoading || !_enableContextMenu) return;
        
        if (evt.button == 1) // Right mouse button
        {
            OnCardRightClicked?.Invoke(this, _currentData, evt.position);
            evt.StopPropagation();
        }
    }
    
    /// <summary>
    /// Handles keyboard input for accessibility.
    /// AIDEV-NOTE: Supports Space/Enter for selection and arrow keys for navigation.
    /// </summary>
    private void OnCardKeyDown(KeyDownEvent evt)
    {
        if (_isDisabled || _isLoading) return;
        
        switch (evt.keyCode)
        {
            case KeyCode.Space:
            case KeyCode.Return:
                if (_isSelectable)
                    SetSelected(!_isSelected);
                OnCardClicked?.Invoke(this, _currentData);
                evt.StopPropagation();
                break;
        }
    }
    
    /// <summary>
    /// Handles primary button clicks.
    /// AIDEV-NOTE: Triggers primary action event and prevents card selection.
    /// </summary>
    private void OnPrimaryButtonClicked()
    {
        if (_isDisabled || _isLoading) return;
        
        OnPrimaryActionClicked?.Invoke(this, _currentData);
        EventBus.Publish(new CardActionEvent(CardId, "Primary"));
    }
    
    /// <summary>
    /// Handles secondary button clicks.
    /// AIDEV-NOTE: Triggers secondary action event and prevents card selection.
    /// </summary>
    private void OnSecondaryButtonClicked()
    {
        if (_isDisabled || _isLoading) return;
        
        OnSecondaryActionClicked?.Invoke(this, _currentData);
        EventBus.Publish(new CardActionEvent(CardId, "Secondary"));
    }
    
    #if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing card interactions in editor.
    /// </summary>
    [ContextMenu("Test Card Click (Debug)")]
    private void DebugTestClick()
    {
        if (_debugMode)
        {
            OnCardClicked?.Invoke(this, _currentData);
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing card selection in editor.
    /// </summary>
    [ContextMenu("Toggle Selection (Debug)")]
    private void DebugToggleSelection()
    {
        if (_debugMode)
        {
            SetSelected(!_isSelected);
        }
    }
    #endif
}