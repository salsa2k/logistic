using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using LogisticGame.Events;
using LogisticGame.UI.Lists;

/// <summary>
/// AIDEV-NOTE: Base list component for displaying collections of cards with virtualization,
/// search, filtering, and sorting capabilities. Integrates with BaseCard system.
/// </summary>
public class BaseList<T> : MonoBehaviour where T : ICardData
{
    [Header("List Configuration")]
    [SerializeField] private string _listId = "";
    [SerializeField] private bool _showHeader = true;
    [SerializeField] private bool _showSearch = true;
    [SerializeField] private bool _showInfoBar = true;
    [SerializeField] private bool _showFooter = false;
    [SerializeField] private ListSelectionMode _selectionMode = ListSelectionMode.Single;
    [SerializeField] private ListVirtualizationMode _virtualizationMode = ListVirtualizationMode.Fixed;
    
    [Header("UI References")]
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private StyleSheet _baseListStyles;
    [SerializeField] private GameObject _cardPrefab;
    
    [Header("Virtualization Settings")]
    [SerializeField] private float _fixedItemHeight = 60f;
    [SerializeField] private int _bufferItems = 3;
    [SerializeField] private int _maxVisibleItems = 50;
    
    [Header("Search Settings")]
    [SerializeField] private bool _caseSensitiveSearch = false;
    [SerializeField] private bool _useRegexSearch = false;
    [SerializeField] private float _searchDebounceTime = 0.3f;
    
    // UI Elements
    private VisualElement _listRoot;
    private VisualElement _listHeader;
    private Label _listTitle;
    private Label _listSubtitle;
    private Button _refreshButton;
    private Button _settingsButton;
    private VisualElement _searchContainer;
    private TextField _searchField;
    private Button _clearSearchButton;
    private DropdownField _sortDropdown;
    private Button _sortDirectionButton;
    private VisualElement _infoBar;
    private Label _itemCountLabel;
    private Label _selectionInfoLabel;
    private ScrollView _scrollView;
    private VisualElement _viewport;
    private VisualElement _itemsContainer;
    private VisualElement _topSpacer;
    private VisualElement _bottomSpacer;
    private VisualElement _emptyState;
    private VisualElement _loadingState;
    private VisualElement _footer;
    
    // Systems
    private ListVirtualization _virtualization;
    private ListFiltering<T> _filtering;
    private Dictionary<int, BaseCard> _visibleCards;
    private Queue<BaseCard> _recycledCards;
    
    // State
    private List<T> _items;
    private List<T> _selectedItems;
    private bool _isLoading;
    private string _currentTitle = "List";
    private string _currentSubtitle = "";
    private float _lastSearchTime;
    
    // Events
    public event Action<T> OnItemSelected;
    public event Action<IEnumerable<T>> OnItemsSelected;
    public event Action<T> OnItemDoubleClicked;
    public event Action<T, string> OnItemActionTriggered;
    public event Action<T, Vector2> OnItemContextMenuRequested;
    public event Action OnRefreshRequested;
    
    // Properties
    public string ListId => string.IsNullOrEmpty(_listId) ? gameObject.name : _listId;
    public int ItemCount => _items?.Count ?? 0;
    public int FilteredCount => _filtering?.FilteredCount ?? 0;
    public bool IsLoading => _isLoading;
    public ListSelectionMode SelectionMode => _selectionMode;
    public IEnumerable<T> SelectedItems => _selectedItems?.AsEnumerable() ?? Enumerable.Empty<T>();
    public IEnumerable<T> Items => _items?.AsEnumerable() ?? Enumerable.Empty<T>();
    public IEnumerable<T> FilteredItems => _filtering?.GetFilteredItems() ?? Enumerable.Empty<T>();
    
    private void Awake()
    {
        // AIDEV-NOTE: Initialize components and systems
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
            
        if (_uiDocument == null)
        {
            Debug.LogError($"BaseList '{name}': UIDocument component is required");
            return;
        }
        
        InitializeSystems();
        InitializeUIElements();
        SetupEventListeners();
        ApplyConfiguration();
        
        // Register with ThemeManager
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.RegisterUIDocument(_uiDocument);
        }
    }
    
    private void Start()
    {
        ApplyListStyles();
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        // AIDEV-NOTE: Clean up systems and event listeners
        RemoveEventListeners();
        _virtualization?.Dispose();
        
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.UnregisterUIDocument(_uiDocument);
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Set the list data and refresh display
    /// </summary>
    public void SetItems(List<T> items)
    {
        _items = new List<T>(items ?? new List<T>());
        _selectedItems?.Clear();
        
        _filtering.SetItems(_items);
        UpdateUI();
        
        EventBus.Publish(new ListDataRefreshedEvent(ListId, ItemCount, FilteredCount));
    }
    
    /// <summary>
    /// AIDEV-NOTE: Add an item to the list
    /// </summary>
    public void AddItem(T item)
    {
        if (item == null) return;
        
        if (_items == null)
            _items = new List<T>();
            
        _items.Add(item);
        _filtering.AddItem(item);
        UpdateUI();
        
        EventBus.Publish(new ListDataRefreshedEvent(ListId, ItemCount, FilteredCount));
    }
    
    /// <summary>
    /// AIDEV-NOTE: Remove an item from the list
    /// </summary>
    public void RemoveItem(T item)
    {
        if (item == null || _items == null) return;
        
        _items.Remove(item);
        _selectedItems?.Remove(item);
        _filtering.RemoveItem(item);
        UpdateUI();
        
        EventBus.Publish(new ListDataRefreshedEvent(ListId, ItemCount, FilteredCount));
    }
    
    /// <summary>
    /// AIDEV-NOTE: Clear all items from the list
    /// </summary>
    public void ClearItems()
    {
        _items?.Clear();
        _selectedItems?.Clear();
        _filtering.ClearItems();
        UpdateUI();
        
        EventBus.Publish(new ListDataRefreshedEvent(ListId, 0, 0));
    }
    
    /// <summary>
    /// AIDEV-NOTE: Set list title and subtitle
    /// </summary>
    public void SetTitle(string title, string subtitle = "")
    {
        _currentTitle = title ?? "List";
        _currentSubtitle = subtitle ?? "";
        UpdateTitleDisplay();
    }
    
    /// <summary>
    /// AIDEV-NOTE: Set loading state
    /// </summary>
    public void SetLoading(bool loading, string message = "Loading...")
    {
        _isLoading = loading;
        UpdateLoadingState(message);
        
        EventBus.Publish(new ListLoadingStateChangedEvent(ListId, loading, message));
    }
    
    /// <summary>
    /// AIDEV-NOTE: Set search text
    /// </summary>
    public void SetSearchText(string searchText)
    {
        if (_searchField != null)
        {
            _searchField.value = searchText ?? "";
        }
        
        _filtering.SetSearchText(searchText);
    }
    
    /// <summary>
    /// AIDEV-NOTE: Clear search
    /// </summary>
    public void ClearSearch()
    {
        SetSearchText("");
    }
    
    /// <summary>
    /// AIDEV-NOTE: Set sort criteria
    /// </summary>
    public void SetSort(ListSortCriteria criteria, bool ascending = true)
    {
        _filtering.SetSort(criteria, ascending);
        UpdateSortDisplay();
    }
    
    /// <summary>
    /// AIDEV-NOTE: Select an item
    /// </summary>
    public void SelectItem(T item, bool addToSelection = false)
    {
        if (item == null || _selectionMode == ListSelectionMode.None) return;
        
        if (_selectedItems == null)
            _selectedItems = new List<T>();
        
        if (!addToSelection || _selectionMode == ListSelectionMode.Single)
        {
            _selectedItems.Clear();
        }
        
        if (!_selectedItems.Contains(item))
        {
            _selectedItems.Add(item);
        }
        
        UpdateSelectionDisplay();
        OnItemSelected?.Invoke(item);
        OnItemsSelected?.Invoke(_selectedItems);
        
        var itemIndex = _items.IndexOf(item);
        EventBus.Publish(new ListItemSelectedEvent(ListId, item.Id, itemIndex));
    }
    
    /// <summary>
    /// AIDEV-NOTE: Deselect an item
    /// </summary>
    public void DeselectItem(T item)
    {
        if (item == null || _selectedItems == null) return;
        
        _selectedItems.Remove(item);
        UpdateSelectionDisplay();
        OnItemsSelected?.Invoke(_selectedItems);
    }
    
    /// <summary>
    /// AIDEV-NOTE: Clear all selections
    /// </summary>
    public void ClearSelection()
    {
        _selectedItems?.Clear();
        UpdateSelectionDisplay();
        OnItemsSelected?.Invoke(Enumerable.Empty<T>());
    }
    
    /// <summary>
    /// AIDEV-NOTE: Refresh the list data and display
    /// </summary>
    public void Refresh()
    {
        _filtering.RefreshFilters();
        UpdateUI();
        OnRefreshRequested?.Invoke();
    }
    
    /// <summary>
    /// AIDEV-NOTE: Initialize internal systems
    /// </summary>
    private void InitializeSystems()
    {
        _virtualization = new ListVirtualization();
        _filtering = new ListFiltering<T>();
        _visibleCards = new Dictionary<int, BaseCard>();
        _recycledCards = new Queue<BaseCard>();
        _selectedItems = new List<T>();
        _items = new List<T>();
        
        // Configure filtering
        _filtering.Configure(_caseSensitiveSearch, _useRegexSearch, true, _searchDebounceTime);
        
        // Setup filtering events
        _filtering.OnFilteredItemsChanged += OnFilteredItemsChanged;
        _filtering.OnSearchCompleted += OnSearchCompleted;
        _filtering.OnSortChanged += OnSortChanged;
        
        // Setup virtualization events
        _virtualization.OnScrollPositionChanged += OnScrollPositionChanged;
        _virtualization.OnItemElementRequested += OnItemElementRequested;
        _virtualization.OnItemElementReleased += OnItemElementReleased;
    }
    
    /// <summary>
    /// AIDEV-NOTE: Initialize UI element references
    /// </summary>
    private void InitializeUIElements()
    {
        if (_uiDocument?.rootVisualElement == null) return;
        
        var root = _uiDocument.rootVisualElement;
        
        _listRoot = root.Q<VisualElement>("list-root");
        _listHeader = root.Q<VisualElement>("list-header");
        _listTitle = root.Q<Label>("list-title");
        _listSubtitle = root.Q<Label>("list-subtitle");
        _refreshButton = root.Q<Button>("list-refresh-button");
        _settingsButton = root.Q<Button>("list-settings-button");
        _searchContainer = root.Q<VisualElement>("list-search-container");
        _searchField = root.Q<TextField>("list-search-field");
        _clearSearchButton = root.Q<Button>("list-clear-search-button");
        _sortDropdown = root.Q<DropdownField>("list-sort-dropdown");
        _sortDirectionButton = root.Q<Button>("list-sort-direction-button");
        _infoBar = root.Q<VisualElement>("list-info-bar");
        _itemCountLabel = root.Q<Label>("list-item-count");
        _selectionInfoLabel = root.Q<Label>("list-selection-info");
        _scrollView = root.Q<ScrollView>("list-scroll-view");
        _viewport = root.Q<VisualElement>("list-viewport");
        _itemsContainer = root.Q<VisualElement>("list-items-container");
        _topSpacer = root.Q<VisualElement>("list-spacer-top");
        _bottomSpacer = root.Q<VisualElement>("list-spacer-bottom");
        _emptyState = root.Q<VisualElement>("list-empty-state");
        _loadingState = root.Q<VisualElement>("list-loading-state");
        _footer = root.Q<VisualElement>("list-footer");
        
        // Initialize virtualization with UI references
        if (_virtualization != null && _scrollView != null)
        {
            _virtualization.Initialize(_scrollView, _viewport, _itemsContainer, _topSpacer, _bottomSpacer);
            _virtualization.Configure(_virtualizationMode, _fixedItemHeight, _bufferItems, _maxVisibleItems);
        }
        
        // Setup sort dropdown options
        SetupSortDropdown();
        
        // AIDEV-NOTE: Log missing critical elements
        if (_listRoot == null) Debug.LogError($"BaseList '{name}': list-root not found in UXML");
        if (_itemsContainer == null) Debug.LogError($"BaseList '{name}': list-items-container not found in UXML");
    }
    
    /// <summary>
    /// AIDEV-NOTE: Setup event listeners for UI interactions
    /// </summary>
    private void SetupEventListeners()
    {
        // List root for keyboard navigation
        if (_listRoot != null)
        {
            _listRoot.RegisterCallback<KeyDownEvent>(OnListKeyDown);
            _listRoot.focusable = true;
            _listRoot.tabIndex = 0;
        }
        
        // Header buttons
        if (_refreshButton != null)
            _refreshButton.clicked += OnRefreshButtonClicked;
        if (_settingsButton != null)
            _settingsButton.clicked += OnSettingsButtonClicked;
        
        // Search controls
        if (_searchField != null)
            _searchField.RegisterValueChangedCallback(OnSearchTextChanged);
        if (_clearSearchButton != null)
            _clearSearchButton.clicked += OnClearSearchClicked;
        
        // Sort controls
        if (_sortDropdown != null)
            _sortDropdown.RegisterValueChangedCallback(OnSortChanged);
        if (_sortDirectionButton != null)
            _sortDirectionButton.clicked += OnSortDirectionClicked;
        
        // Scroll view for virtualization
        if (_scrollView != null)
            _scrollView.RegisterCallback<WheelEvent>(OnScrollWheelChanged);
    }
    
    /// <summary>
    /// AIDEV-NOTE: Remove event listeners to prevent memory leaks
    /// </summary>
    private void RemoveEventListeners()
    {
        if (_listRoot != null)
            _listRoot.UnregisterCallback<KeyDownEvent>(OnListKeyDown);
        
        if (_refreshButton != null)
            _refreshButton.clicked -= OnRefreshButtonClicked;
        if (_settingsButton != null)
            _settingsButton.clicked -= OnSettingsButtonClicked;
        
        if (_searchField != null)
            _searchField.UnregisterValueChangedCallback(OnSearchTextChanged);
        if (_clearSearchButton != null)
            _clearSearchButton.clicked -= OnClearSearchClicked;
        
        if (_sortDropdown != null)
            _sortDropdown.UnregisterValueChangedCallback(OnSortChanged);
        if (_sortDirectionButton != null)
            _sortDirectionButton.clicked -= OnSortDirectionClicked;
        
        if (_scrollView != null)
            _scrollView.UnregisterCallback<WheelEvent>(OnScrollWheelChanged);
    }
    
    /// <summary>
    /// AIDEV-NOTE: Apply configuration settings to UI
    /// </summary>
    private void ApplyConfiguration()
    {
        if (_listHeader != null)
            _listHeader.style.display = _showHeader ? DisplayStyle.Flex : DisplayStyle.None;
        if (_searchContainer != null)
            _searchContainer.style.display = _showSearch ? DisplayStyle.Flex : DisplayStyle.None;
        if (_infoBar != null)
            _infoBar.style.display = _showInfoBar ? DisplayStyle.Flex : DisplayStyle.None;
        if (_footer != null)
            _footer.style.display = _showFooter ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    /// <summary>
    /// AIDEV-NOTE: Apply list-specific styles
    /// </summary>
    private void ApplyListStyles()
    {
        if (_listRoot == null) return;
        
        _listRoot.AddToClassList("base-list");
        
        if (_baseListStyles != null && !_uiDocument.rootVisualElement.styleSheets.Contains(_baseListStyles))
        {
            _uiDocument.rootVisualElement.styleSheets.Add(_baseListStyles);
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Setup sort dropdown with available options
    /// </summary>
    private void SetupSortDropdown()
    {
        if (_sortDropdown == null) return;
        
        var sortOptions = new List<string>
        {
            "None",
            "Title",
            "Name", 
            "Status",
            "Date",
            "Price"
        };
        
        _sortDropdown.choices = sortOptions;
        _sortDropdown.value = "None";
    }
    
    // Event Handlers
    
    private void OnFilteredItemsChanged(List<T> filteredItems, int originalCount)
    {
        _virtualization.SetTotalItemCount(filteredItems.Count);
        UpdateUI();
    }
    
    private void OnSearchCompleted(string searchText, int resultCount)
    {
        EventBus.Publish(new ListFilterChangedEvent(ListId, searchText, resultCount));
        UpdateUI();
    }
    
    private void OnSortChanged(ListSortCriteria criteria, bool ascending)
    {
        EventBus.Publish(new ListSortChangedEvent(ListId, criteria, ascending));
        UpdateSortDisplay();
    }
    
    private void OnScrollPositionChanged(int firstVisible, int lastVisible, float scrollPosition)
    {
        EventBus.Publish(new ListScrollChangedEvent(ListId, firstVisible, lastVisible, scrollPosition));
    }
    
    private void OnItemElementRequested(int index, VisualElement element)
    {
        var filteredItems = _filtering.GetFilteredItems();
        if (index >= 0 && index < filteredItems.Count)
        {
            var item = filteredItems[index];
            var card = GetOrCreateCard();
            
            if (card != null)
            {
                card.BindData(item);
                _visibleCards[index] = card;
                
                // Setup card event listeners
                card.OnCardClicked += OnCardClicked;
                card.OnCardDoubleClicked += OnCardDoubleClicked;
                card.OnCardRightClicked += OnCardRightClicked;
                card.OnPrimaryActionClicked += OnCardPrimaryAction;
                card.OnSecondaryActionClicked += OnCardSecondaryAction;
            }
        }
    }
    
    private void OnItemElementReleased(int index, VisualElement element)
    {
        if (_visibleCards.TryGetValue(index, out BaseCard card))
        {
            // Remove card event listeners
            card.OnCardClicked -= OnCardClicked;
            card.OnCardDoubleClicked -= OnCardDoubleClicked;
            card.OnCardRightClicked -= OnCardRightClicked;
            card.OnPrimaryActionClicked -= OnCardPrimaryAction;
            card.OnSecondaryActionClicked -= OnCardSecondaryAction;
            
            _visibleCards.Remove(index);
            RecycleCard(card);
        }
    }
    
    // UI Event Handlers
    
    private void OnListKeyDown(KeyDownEvent evt)
    {
        switch (evt.keyCode)
        {
            case KeyCode.F5:
                Refresh();
                evt.StopPropagation();
                break;
                
            case KeyCode.F:
                if (evt.ctrlKey && _searchField != null)
                {
                    _searchField.Focus();
                    evt.StopPropagation();
                }
                break;
                
            case KeyCode.A:
                if (evt.ctrlKey && _selectionMode == ListSelectionMode.Multiple)
                {
                    SelectAllItems();
                    evt.StopPropagation();
                }
                break;
                
            case KeyCode.Escape:
                ClearSelection();
                evt.StopPropagation();
                break;
        }
    }
    
    private void OnRefreshButtonClicked()
    {
        Refresh();
    }
    
    private void OnSettingsButtonClicked()
    {
        // AIDEV-TODO: Implement list settings dialog
        Debug.Log($"List settings for {ListId}");
    }
    
    private void OnSearchTextChanged(ChangeEvent<string> evt)
    {
        _lastSearchTime = Time.time;
        
        // Debounce search input
        this.StartCoroutine(DebounceSearch(evt.newValue, _lastSearchTime));
    }
    
    private System.Collections.IEnumerator DebounceSearch(string searchText, float searchTime)
    {
        yield return new WaitForSeconds(_searchDebounceTime);
        
        if (_lastSearchTime == searchTime) // Only apply if this is still the latest search
        {
            _filtering.SetSearchText(searchText);
        }
    }
    
    private void OnClearSearchClicked()
    {
        ClearSearch();
    }
    
    private void OnSortChanged(ChangeEvent<string> evt)
    {
        if (Enum.TryParse<ListSortCriteria>(evt.newValue, out var criteria))
        {
            _filtering.SetSort(criteria, _filtering.IsAscendingSort);
        }
    }
    
    private void OnSortDirectionClicked()
    {
        _filtering.ToggleSortDirection();
    }
    
    private void OnScrollWheelChanged(WheelEvent evt)
    {
        if (_scrollView != null)
        {
            _virtualization.OnScrollChanged(_scrollView.scrollOffset.y);
        }
    }
    
    // Card Event Handlers
    
    private void OnCardClicked(BaseCard card, ICardData data)
    {
        if (data is T item)
        {
            SelectItem(item, Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        }
    }
    
    private void OnCardDoubleClicked(BaseCard card, ICardData data)
    {
        if (data is T item)
        {
            OnItemDoubleClicked?.Invoke(item);
        }
    }
    
    private void OnCardRightClicked(BaseCard card, ICardData data, Vector2 position)
    {
        if (data is T item)
        {
            OnItemContextMenuRequested?.Invoke(item, position);
            EventBus.Publish(new ListContextMenuEvent(ListId, item.Id, position));
        }
    }
    
    private void OnCardPrimaryAction(BaseCard card, ICardData data)
    {
        if (data is T item)
        {
            OnItemActionTriggered?.Invoke(item, "Primary");
            
            var itemIndex = _items.IndexOf(item);
            EventBus.Publish(new ListItemActionEvent(ListId, item.Id, "Primary", itemIndex));
        }
    }
    
    private void OnCardSecondaryAction(BaseCard card, ICardData data)
    {
        if (data is T item)
        {
            OnItemActionTriggered?.Invoke(item, "Secondary");
            
            var itemIndex = _items.IndexOf(item);
            EventBus.Publish(new ListItemActionEvent(ListId, item.Id, "Secondary", itemIndex));
        }
    }
    
    // Card Management
    
    private BaseCard GetOrCreateCard()
    {
        if (_recycledCards.Count > 0)
        {
            return _recycledCards.Dequeue();
        }
        
        if (_cardPrefab != null)
        {
            var cardGameObject = Instantiate(_cardPrefab, this.transform);
            return cardGameObject.GetComponent<BaseCard>();
        }
        
        return null;
    }
    
    private void RecycleCard(BaseCard card)
    {
        if (card != null)
        {
            card.BindData(null); // Clear data
            _recycledCards.Enqueue(card);
        }
    }
    
    // UI Update Methods
    
    private void UpdateUI()
    {
        UpdateTitleDisplay();
        UpdateItemCountDisplay();
        UpdateSelectionDisplay();
        UpdateEmptyState();
        UpdateLoadingState();
    }
    
    private void UpdateTitleDisplay()
    {
        if (_listTitle != null)
            _listTitle.text = _currentTitle;
        if (_listSubtitle != null)
        {
            _listSubtitle.text = _currentSubtitle;
            _listSubtitle.style.display = string.IsNullOrEmpty(_currentSubtitle) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
    
    private void UpdateItemCountDisplay()
    {
        if (_itemCountLabel == null) return;
        
        var filteredCount = FilteredCount;
        var totalCount = ItemCount;
        
        if (filteredCount == totalCount)
        {
            _itemCountLabel.text = $"{totalCount} item{(totalCount != 1 ? "s" : "")}";
        }
        else
        {
            _itemCountLabel.text = $"{filteredCount} of {totalCount} item{(totalCount != 1 ? "s" : "")}";
        }
    }
    
    private void UpdateSelectionDisplay()
    {
        if (_selectionInfoLabel == null) return;
        
        var selectedCount = _selectedItems?.Count ?? 0;
        if (selectedCount > 0)
        {
            _selectionInfoLabel.text = $"{selectedCount} selected";
            _selectionInfoLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            _selectionInfoLabel.style.display = DisplayStyle.None;
        }
    }
    
    private void UpdateEmptyState()
    {
        if (_emptyState == null) return;
        
        bool shouldShowEmpty = !_isLoading && FilteredCount == 0;
        _emptyState.style.display = shouldShowEmpty ? DisplayStyle.Flex : DisplayStyle.None;
        
        if (_itemsContainer != null)
        {
            _itemsContainer.style.display = shouldShowEmpty ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
    
    private void UpdateLoadingState(string message = "Loading...")
    {
        if (_loadingState == null) return;
        
        _loadingState.style.display = _isLoading ? DisplayStyle.Flex : DisplayStyle.None;
        
        var loadingTitle = _loadingState.Q<Label>("list-loading-title");
        if (loadingTitle != null)
            loadingTitle.text = message;
    }
    
    private void UpdateSortDisplay()
    {
        if (_sortDropdown != null)
        {
            _sortDropdown.value = _filtering.SortCriteria.ToString();
        }
        
        if (_sortDirectionButton != null)
        {
            _sortDirectionButton.text = _filtering.IsAscendingSort ? "↑" : "↓";
        }
    }
    
    private void SelectAllItems()
    {
        if (_selectionMode != ListSelectionMode.Multiple) return;
        
        _selectedItems.Clear();
        _selectedItems.AddRange(_filtering.GetFilteredItems());
        UpdateSelectionDisplay();
        OnItemsSelected?.Invoke(_selectedItems);
        
        var itemIds = _selectedItems.Select(item => item.Id);
        var itemIndices = _selectedItems.Select(item => _items.IndexOf(item));
        EventBus.Publish(new ListItemsSelectedEvent(ListId, itemIds, itemIndices));
    }
}