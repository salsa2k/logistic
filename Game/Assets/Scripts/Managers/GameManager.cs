using UnityEngine;
using LogisticGame.Events;
using System.Threading.Tasks;

namespace LogisticGame.Managers
{
    public class GameManager : MonoBehaviour
    {
        // AIDEV-NOTE: Singleton instance for global game state management
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(GameManager));
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Game State")]
        [SerializeField] private bool _isGamePaused;
        [SerializeField] private bool _isGameStarted;

        [Header("Player Data")]
        [SerializeField] private float _playerCredits = 10000f;
        [SerializeField] private string _companyName = "Player Company";

        [Header("Save System")]
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private string _currentSaveSlot = "quicksave";

        // Events for game state changes
        public static System.Action<bool> OnGamePausedChanged;
        public static System.Action<bool> OnGameStartedChanged;
        public static System.Action<float> OnPlayerCreditsChanged;

        // Properties
        public bool IsGamePaused => _isGamePaused;
        public bool IsGameStarted => _isGameStarted;
        public float PlayerCredits => _playerCredits;
        public string CompanyName => _companyName;
        public bool AutoSaveEnabled => _autoSaveEnabled;
        public string CurrentSaveSlot => _currentSaveSlot;

        private void Awake()
        {
            // AIDEV-NOTE: Ensure only one instance exists
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            // AIDEV-NOTE: Initialize game systems and default values
            Debug.Log($"GameManager initialized. Company: {_companyName}, Credits: {_playerCredits:C}");
            
            // Subscribe to auto-save events
            SubscribeToAutoSaveEvents();
        }

        private void SubscribeToAutoSaveEvents()
        {
            // Subscribe to events that should trigger auto-saves
            EventBus.Subscribe<CreditsChangedEvent>(OnCreditsChanged);
            
            // Contract and vehicle events would be added here when those systems are implemented
            // EventBus.Subscribe<ContractCompletedEvent>(OnContractCompleted);
            // EventBus.Subscribe<VehiclePurchasedEvent>(OnVehiclePurchased);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<CreditsChangedEvent>(OnCreditsChanged);
        }

        private void OnCreditsChanged(CreditsChangedEvent creditEvent)
        {
            if (_autoSaveEnabled && Mathf.Abs(creditEvent.Change) > 1000f) // Auto-save for significant credit changes
            {
                TriggerAutoSave("Credits changed significantly");
            }
        }

        public void SetGamePaused(bool paused)
        {
            if (_isGamePaused != paused)
            {
                _isGamePaused = paused;
                Time.timeScale = paused ? 0f : 1f;
                OnGamePausedChanged?.Invoke(_isGamePaused);
            }
        }

        public void StartGame()
        {
            if (!_isGameStarted)
            {
                _isGameStarted = true;
                OnGameStartedChanged?.Invoke(_isGameStarted);
                Debug.Log("Game started!");
            }
        }

        public void EndGame()
        {
            if (_isGameStarted)
            {
                _isGameStarted = false;
                OnGameStartedChanged?.Invoke(_isGameStarted);
                Debug.Log("Game ended!");
            }
        }

        public bool TrySpendCredits(float amount)
        {
            if (_playerCredits >= amount)
            {
                float oldCredits = _playerCredits;
                _playerCredits -= amount;
                OnPlayerCreditsChanged?.Invoke(_playerCredits);
                
                // Publish credit change event
                EventBus.Publish(new CreditsChangedEvent(_playerCredits, -amount));
                return true;
            }
            return false;
        }

        public void AddCredits(float amount)
        {
            float oldCredits = _playerCredits;
            _playerCredits += amount;
            OnPlayerCreditsChanged?.Invoke(_playerCredits);
            
            // Publish credit change event
            EventBus.Publish(new CreditsChangedEvent(_playerCredits, amount));
        }

        public void SetCompanyName(string companyName)
        {
            _companyName = companyName;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (_isGameStarted)
            {
                SetGamePaused(pauseStatus);
                
                if (pauseStatus && _autoSaveEnabled)
                {
                    TriggerAutoSave("Application paused");
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_isGameStarted && !hasFocus)
            {
                SetGamePaused(true);
                
                // Auto-save when losing focus
                if (_autoSaveEnabled)
                {
                    TriggerAutoSave("Application focus lost");
                }
            }
        }

        // Save system integration
        public void SetAutoSaveEnabled(bool enabled)
        {
            _autoSaveEnabled = enabled;
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.AutoSaveEnabled = enabled;
            }
        }

        public void SetCurrentSaveSlot(string slotName)
        {
            _currentSaveSlot = slotName;
        }

        public async Task<bool> SaveGameAsync(string slotName = null)
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("SaveManager not available");
                return false;
            }

            string saveSlot = slotName ?? _currentSaveSlot;
            
            // Create save data from current game state
            SaveData saveData = CreateSaveData(saveSlot);
            
            return await SaveManager.Instance.SaveGameAsync(saveSlot, saveData);
        }

        public async Task<bool> LoadGameAsync(string slotName = null)
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("SaveManager not available");
                return false;
            }

            string saveSlot = slotName ?? _currentSaveSlot;
            
            SaveData loadedData = await SaveManager.Instance.LoadGameAsync(saveSlot);
            
            if (loadedData != null)
            {
                ApplySaveData(loadedData);
                _currentSaveSlot = saveSlot;
                return true;
            }
            
            return false;
        }

        public void TriggerAutoSave(string reason)
        {
            if (_autoSaveEnabled && SaveManager.Instance != null)
            {
                SaveManager.Instance.TriggerAutoSave(reason);
            }
        }

        private SaveData CreateSaveData(string saveName)
        {
            // Create a new SaveData ScriptableObject instance
            SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
            
            // Create and populate GameState
            GameState gameState = ScriptableObject.CreateInstance<GameState>();
            // Initialize gameState with current values
            // This would need to be expanded based on the actual GameState structure
            
            // Create and populate PlayerProgress
            PlayerProgress playerProgress = ScriptableObject.CreateInstance<PlayerProgress>();
            // Initialize playerProgress with current values
            
            // Create and populate SettingsData
            SettingsData settings = ScriptableObject.CreateInstance<SettingsData>();
            // Initialize settings with current values
            
            // Initialize the save data
            saveData.Initialize(saveName, gameState, playerProgress, settings);
            
            return saveData;
        }

        private void ApplySaveData(SaveData saveData)
        {
            if (saveData.GameState != null)
            {
                _playerCredits = saveData.GameState.CurrentCredits;
                // Apply other game state values
            }
            
            if (saveData.PlayerProgress != null)
            {
                // Apply player progress values
            }
            
            if (saveData.Settings != null)
            {
                // Apply settings values
            }
            
            // Notify systems of the loaded data
            OnPlayerCreditsChanged?.Invoke(_playerCredits);
            
            Debug.Log($"Game loaded from save: {saveData.SaveName}");
        }

    }
}