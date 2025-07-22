using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using LogisticGame.Managers;
using LogisticGame.Events;
using System.Threading.Tasks;

namespace LogisticGame.UI.SaveLoad
{
    // AIDEV-NOTE: UI controller for save slot management and save/load operations
    public class SaveSlotController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset _saveSlotTemplate;
        
        [Header("Configuration")]
        [SerializeField] private int _maxSaveSlots = 10;
        [SerializeField] private bool _showQuickSave = true;
        [SerializeField] private bool _showAutoSave = true;
        
        // UI Elements
        private VisualElement _root;
        private ListView _saveSlotsList;
        private Button _newGameButton;
        private Button _saveGameButton;
        private Button _loadGameButton;
        private Button _deleteSlotButton;
        private Button _refreshButton;
        private Label _selectedSlotInfo;
        private ProgressBar _operationProgress;
        private Label _statusLabel;
        
        // State
        private List<SaveSlotData> _saveSlots = new List<SaveSlotData>();
        private SaveSlotData _selectedSlot;
        private bool _isOperationInProgress = false;
        
        // Events
        public static System.Action<string> OnSaveSlotSelected;
        public static System.Action<string, bool> OnSaveOperationCompleted;
        public static System.Action<string, bool> OnLoadOperationCompleted;
        
        private void OnEnable()
        {
            InitializeUI();
            SubscribeToEvents();
            RefreshSaveSlots();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeUI()
        {
            if (_uiDocument == null) return;
            
            _root = _uiDocument.rootVisualElement;
            
            // Get UI elements
            _saveSlotsList = _root.Q<ListView>("save-slots-list");
            _newGameButton = _root.Q<Button>("new-game-button");
            _saveGameButton = _root.Q<Button>("save-game-button");
            _loadGameButton = _root.Q<Button>("load-game-button");
            _deleteSlotButton = _root.Q<Button>("delete-slot-button");
            _refreshButton = _root.Q<Button>("refresh-button");
            _selectedSlotInfo = _root.Q<Label>("selected-slot-info");
            _operationProgress = _root.Q<ProgressBar>("operation-progress");
            _statusLabel = _root.Q<Label>("status-label");
            
            // Configure ListView
            ConfigureSaveSlotsList();
            
            // Setup button callbacks
            SetupButtonCallbacks();
            
            // Initial state
            UpdateUIState();
        }
        
        private void ConfigureSaveSlotsList()
        {
            if (_saveSlotsList == null) return;
            
            _saveSlotsList.itemsSource = _saveSlots;
            _saveSlotsList.makeItem = CreateSaveSlotItem;
            _saveSlotsList.bindItem = BindSaveSlotItem;
            _saveSlotsList.selectionChanged += OnSaveSlotSelectionChanged;
        }
        
        private VisualElement CreateSaveSlotItem()
        {
            if (_saveSlotTemplate != null)
            {
                return _saveSlotTemplate.Instantiate();
            }
            
            // Fallback: create basic item
            var item = new VisualElement();
            item.AddToClassList("save-slot-item");
            
            var nameLabel = new Label();
            nameLabel.name = "slot-name";
            nameLabel.AddToClassList("slot-name");
            
            var infoLabel = new Label();
            infoLabel.name = "slot-info";
            infoLabel.AddToClassList("slot-info");
            
            var dateLabel = new Label();
            dateLabel.name = "slot-date";
            dateLabel.AddToClassList("slot-date");
            
            item.Add(nameLabel);
            item.Add(infoLabel);
            item.Add(dateLabel);
            
            return item;
        }
        
        private void BindSaveSlotItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _saveSlots.Count) return;
            
            var slotData = _saveSlots[index];
            
            var nameLabel = element.Q<Label>("slot-name");
            var infoLabel = element.Q<Label>("slot-info");
            var dateLabel = element.Q<Label>("slot-date");
            
            if (nameLabel != null)
                nameLabel.text = slotData.DisplayName;
            
            if (infoLabel != null)
                infoLabel.text = slotData.GetInfoText();
            
            if (dateLabel != null)
                dateLabel.text = slotData.GetDateText();
            
            // Add visual states
            element.ClearClassList();
            element.AddToClassList("save-slot-item");
            
            if (!slotData.IsValid)
                element.AddToClassList("invalid-slot");
            
            if (slotData.IsEmpty)
                element.AddToClassList("empty-slot");
            
            if (slotData.SlotName == GameManager.Instance?.CurrentSaveSlot)
                element.AddToClassList("current-slot");
        }
        
        private void SetupButtonCallbacks()
        {
            if (_newGameButton != null)
                _newGameButton.clicked += OnNewGameClicked;
            
            if (_saveGameButton != null)
                _saveGameButton.clicked += OnSaveGameClicked;
            
            if (_loadGameButton != null)
                _loadGameButton.clicked += OnLoadGameClicked;
            
            if (_deleteSlotButton != null)
                _deleteSlotButton.clicked += OnDeleteSlotClicked;
            
            if (_refreshButton != null)
                _refreshButton.clicked += OnRefreshClicked;
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to save manager events
            if (SaveManager.Instance != null)
            {
                SaveManager.OnSaveCompleted += OnSaveCompleted;
                SaveManager.OnLoadCompleted += OnLoadCompleted;
                SaveManager.OnSaveError += OnSaveError;
                SaveManager.OnLoadError += OnLoadError;
                SaveManager.OnSaveProgress += OnSaveProgress;
                SaveManager.OnLoadProgress += OnLoadProgress;
            }
            
            // Subscribe to game events
            EventBus.Subscribe<GameSavedEvent>(OnGameSaved);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }
        
        private void UnsubscribeFromEvents()
        {
            // Unsubscribe from save manager events
            if (SaveManager.Instance != null)
            {
                SaveManager.OnSaveCompleted -= OnSaveCompleted;
                SaveManager.OnLoadCompleted -= OnLoadCompleted;
                SaveManager.OnSaveError -= OnSaveError;
                SaveManager.OnLoadError -= OnLoadError;
                SaveManager.OnSaveProgress -= OnSaveProgress;
                SaveManager.OnLoadProgress -= OnLoadProgress;
            }
            
            // Unsubscribe from game events
            EventBus.Unsubscribe<GameSavedEvent>(OnGameSaved);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }
        
        // Save slot management
        private async void RefreshSaveSlots()
        {
            if (SaveManager.Instance == null) return;
            
            SetStatus("Loading save slots...");
            
            _saveSlots.Clear();
            
            // Add special slots if enabled
            if (_showQuickSave)
            {
                _saveSlots.Add(await CreateSaveSlotData("quicksave", "Quick Save"));
            }
            
            if (_showAutoSave)
            {
                _saveSlots.Add(await CreateSaveSlotData("autosave", "Auto Save"));
            }
            
            // Get available save slots
            var availableSlots = SaveManager.Instance.GetSaveSlots();
            foreach (var slotInfo in availableSlots)
            {
                if (slotInfo.SlotName != "quicksave" && slotInfo.SlotName != "autosave")
                {
                    _saveSlots.Add(await CreateSaveSlotData(slotInfo.SlotName, slotInfo.DisplayName, slotInfo));
                }
            }
            
            // Add empty slots
            for (int i = _saveSlots.Count; i < _maxSaveSlots; i++)
            {
                _saveSlots.Add(new SaveSlotData($"slot_{i + 1}", $"Empty Slot {i + 1}", true));
            }
            
            // Refresh UI
            _saveSlotsList?.RefreshItems();
            UpdateUIState();
            SetStatus("Save slots loaded");
        }
        
        private async Task<SaveSlotData> CreateSaveSlotData(string slotName, string displayName, SaveSlotInfo slotInfo = null)
        {
            if (slotInfo == null && SaveManager.Instance.HasSaveData(slotName))
            {
                slotInfo = SaveManager.Instance.GetSaveSlotInfo(slotName);
            }
            
            if (slotInfo != null)
            {
                return new SaveSlotData(slotName, displayName, false)
                {
                    IsValid = slotInfo.IsValid,
                    LastModified = slotInfo.LastModified,
                    PlayTimeHours = slotInfo.PlayTimeHours,
                    CurrentCredits = slotInfo.CurrentCredits,
                    TotalContracts = slotInfo.TotalContracts,
                    SaveVersion = slotInfo.SaveVersion
                };
            }
            
            return new SaveSlotData(slotName, displayName, true);
        }
        
        // Event handlers
        private void OnSaveSlotSelectionChanged(IEnumerable<object> selectedItems)
        {
            _selectedSlot = null;
            
            foreach (var item in selectedItems)
            {
                if (item is SaveSlotData slotData)
                {
                    _selectedSlot = slotData;
                    break;
                }
            }
            
            UpdateSelectedSlotInfo();
            UpdateUIState();
            
            OnSaveSlotSelected?.Invoke(_selectedSlot?.SlotName);
        }
        
        private void UpdateSelectedSlotInfo()
        {
            if (_selectedSlotInfo == null) return;
            
            if (_selectedSlot == null)
            {
                _selectedSlotInfo.text = "No slot selected";
                return;
            }
            
            if (_selectedSlot.IsEmpty)
            {
                _selectedSlotInfo.text = $"Empty Slot: {_selectedSlot.DisplayName}";
                return;
            }
            
            string info = $"<b>{_selectedSlot.DisplayName}</b>\n";
            info += $"Last Modified: {_selectedSlot.LastModified:MMM dd, yyyy HH:mm}\n";
            info += $"Play Time: {_selectedSlot.PlayTimeHours:F1} hours\n";
            info += $"Credits: {_selectedSlot.CurrentCredits:C0}\n";
            info += $"Contracts: {_selectedSlot.TotalContracts}\n";
            info += $"Version: {_selectedSlot.SaveVersion}";
            
            if (!_selectedSlot.IsValid)
            {
                info += "\n<color=red>⚠ Invalid Save File</color>";
            }
            
            _selectedSlotInfo.text = info;
        }
        
        private void UpdateUIState()
        {
            bool hasSelection = _selectedSlot != null;
            bool canSave = hasSelection && GameManager.Instance != null && GameManager.Instance.IsGameStarted;
            bool canLoad = hasSelection && !_selectedSlot.IsEmpty && _selectedSlot.IsValid;
            bool canDelete = hasSelection && !_selectedSlot.IsEmpty;
            
            if (_saveGameButton != null)
                _saveGameButton.SetEnabled(canSave && !_isOperationInProgress);
            
            if (_loadGameButton != null)
                _loadGameButton.SetEnabled(canLoad && !_isOperationInProgress);
            
            if (_deleteSlotButton != null)
                _deleteSlotButton.SetEnabled(canDelete && !_isOperationInProgress);
            
            if (_refreshButton != null)
                _refreshButton.SetEnabled(!_isOperationInProgress);
        }
        
        // Button event handlers
        private void OnNewGameClicked()
        {
            // This would typically open a new game dialog
            Debug.Log("New Game requested");
        }
        
        private async void OnSaveGameClicked()
        {
            if (_selectedSlot == null || GameManager.Instance == null) return;
            
            SetOperationInProgress(true);
            SetStatus($"Saving to {_selectedSlot.DisplayName}...");
            
            bool success = await GameManager.Instance.SaveGameAsync(_selectedSlot.SlotName);
            
            SetOperationInProgress(false);
            
            if (success)
            {
                SetStatus($"Game saved to {_selectedSlot.DisplayName}");
                RefreshSaveSlots(); // Refresh to update slot info
            }
            else
            {
                SetStatus($"Failed to save to {_selectedSlot.DisplayName}");
            }
        }
        
        private async void OnLoadGameClicked()
        {
            if (_selectedSlot == null || GameManager.Instance == null) return;
            
            SetOperationInProgress(true);
            SetStatus($"Loading from {_selectedSlot.DisplayName}...");
            
            bool success = await GameManager.Instance.LoadGameAsync(_selectedSlot.SlotName);
            
            SetOperationInProgress(false);
            
            if (success)
            {
                SetStatus($"Game loaded from {_selectedSlot.DisplayName}");
            }
            else
            {
                SetStatus($"Failed to load from {_selectedSlot.DisplayName}");
            }
        }
        
        private void OnDeleteSlotClicked()
        {
            if (_selectedSlot == null || _selectedSlot.IsEmpty) return;
            
            // Show confirmation dialog (simplified for now)
            bool confirmed = true; // In reality, this would show a proper confirmation dialog
            
            if (confirmed && SaveManager.Instance != null)
            {
                bool success = SaveManager.Instance.DeleteSaveSlot(_selectedSlot.SlotName);
                
                if (success)
                {
                    SetStatus($"Deleted {_selectedSlot.DisplayName}");
                    RefreshSaveSlots();
                }
                else
                {
                    SetStatus($"Failed to delete {_selectedSlot.DisplayName}");
                }
            }
        }
        
        private void OnRefreshClicked()
        {
            RefreshSaveSlots();
        }
        
        // Save manager event handlers
        private void OnSaveCompleted(SaveData saveData)
        {
            SetOperationInProgress(false);
            OnSaveOperationCompleted?.Invoke(saveData.SaveName, true);
        }
        
        private void OnLoadCompleted(SaveData saveData)
        {
            SetOperationInProgress(false);
            OnLoadOperationCompleted?.Invoke(saveData.SaveName, true);
        }
        
        private void OnSaveError(string slotName, string error)
        {
            SetOperationInProgress(false);
            SetStatus($"Save error: {error}");
            OnSaveOperationCompleted?.Invoke(slotName, false);
        }
        
        private void OnLoadError(string slotName, string error)
        {
            SetOperationInProgress(false);
            SetStatus($"Load error: {error}");
            OnLoadOperationCompleted?.Invoke(slotName, false);
        }
        
        private void OnSaveProgress(float progress)
        {
            if (_operationProgress != null)
            {
                _operationProgress.value = progress * 100f;
                _operationProgress.style.display = progress > 0f ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        private void OnLoadProgress(float progress)
        {
            OnSaveProgress(progress); // Same progress handling
        }
        
        // Game event handlers
        private void OnGameSaved(GameSavedEvent gameSavedEvent)
        {
            if (gameSavedEvent.Success)
            {
                RefreshSaveSlots();
            }
        }
        
        private void OnGameLoaded(GameLoadedEvent gameLoadedEvent)
        {
            if (gameLoadedEvent.Success)
            {
                RefreshSaveSlots();
            }
        }
        
        // UI utility methods
        private void SetOperationInProgress(bool inProgress)
        {
            _isOperationInProgress = inProgress;
            UpdateUIState();
        }
        
        private void SetStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }
            
            Debug.Log($"SaveSlotController: {message}");
        }
        
        // Public API
        public void SelectSlot(string slotName)
        {
            for (int i = 0; i < _saveSlots.Count; i++)
            {
                if (_saveSlots[i].SlotName == slotName)
                {
                    _saveSlotsList?.SetSelection(i);
                    break;
                }
            }
        }
        
        public void CreateNewSaveSlot(string slotName, string displayName)
        {
            var newSlot = new SaveSlotData(slotName, displayName, true);
            _saveSlots.Add(newSlot);
            _saveSlotsList?.RefreshItems();
        }
    }
    
    // AIDEV-NOTE: Data structure for save slot information in the UI
    [System.Serializable]
    public class SaveSlotData
    {
        public string SlotName;
        public string DisplayName;
        public bool IsEmpty;
        public bool IsValid = true;
        public System.DateTime LastModified;
        public float PlayTimeHours;
        public float CurrentCredits;
        public int TotalContracts;
        public string SaveVersion;
        
        public SaveSlotData(string slotName, string displayName, bool isEmpty)
        {
            SlotName = slotName;
            DisplayName = displayName;
            IsEmpty = isEmpty;
            LastModified = System.DateTime.Now;
        }
        
        public string GetInfoText()
        {
            if (IsEmpty)
                return "Empty Slot";
            
            if (!IsValid)
                return "Invalid Save";
            
            return $"{PlayTimeHours:F1}h | {CurrentCredits:C0} | {TotalContracts} contracts";
        }
        
        public string GetDateText()
        {
            if (IsEmpty)
                return "";
            
            var timeSpan = System.DateTime.Now - LastModified;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            else if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            else if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            else
                return LastModified.ToString("MMM dd");
        }
    }
}