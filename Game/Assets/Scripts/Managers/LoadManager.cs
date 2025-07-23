using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using LogisticGame.SaveSystem;

namespace LogisticGame.Managers
{
    // AIDEV-NOTE: Dedicated manager for comprehensive load operations with progress tracking and error recovery
    public class LoadManager : MonoBehaviour
    {
        private static LoadManager _instance;
        public static LoadManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LoadManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(LoadManager));
                        _instance = go.AddComponent<LoadManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Load Configuration")]
        [SerializeField] private bool _enableProgressReporting = true;
        [SerializeField] private bool _enableErrorRecovery = true;
        [SerializeField] private bool _autoValidateBeforeLoad = true;
        #pragma warning disable 0414 // Field assigned but never used - planned for future progress updates
        [SerializeField] private float _progressUpdateInterval = 0.1f;
        #pragma warning restore 0414
        [SerializeField] private int _maxLoadRetries = 3;

        [Header("Load State")]
        [SerializeField] private bool _isLoading = false;
        [SerializeField] private string _currentLoadSlot;
        [SerializeField] private float _currentLoadProgress = 0f;
        [SerializeField] private LoadOperation _currentOperation;

        // Cached save slot information
        private Dictionary<string, SaveSlotInfo> _saveSlotCache = new Dictionary<string, SaveSlotInfo>();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan CacheValidityDuration = TimeSpan.FromMinutes(2);

        // Events for load operations
        public static event Action<string> OnLoadStarted;
        public static event Action<string, SaveData> OnLoadCompleted;
        public static event Action<string, string> OnLoadFailed;
        public static event Action<string, float> OnLoadProgress;
        public static event Action<string, LoadOperation> OnLoadOperationChanged;
        public static event Action<List<SaveSlotInfo>> OnSaveSlotDiscoveryCompleted;
        public static event Action<string, ValidationResult> OnSaveFileValidated;

        // Properties
        public bool IsLoading => _isLoading;
        public string CurrentLoadSlot => _currentLoadSlot;
        public float CurrentLoadProgress => _currentLoadProgress;
        public LoadOperation CurrentOperation => _currentOperation;
        public bool EnableProgressReporting => _enableProgressReporting;
        public bool EnableErrorRecovery => _enableErrorRecovery;

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
            
            InitializeLoadSystem();
        }

        private void InitializeLoadSystem()
        {
            // AIDEV-NOTE: Initialize load system and subscribe to SaveManager events
            Debug.Log("LoadManager initialized");
            
            // Subscribe to SaveManager events for coordination
            if (SaveManager.Instance != null)
            {
                SaveManager.OnLoadStarted += OnSaveManagerLoadStarted;
                SaveManager.OnLoadCompleted += OnSaveManagerLoadCompleted;
                SaveManager.OnLoadError += OnSaveManagerLoadError;
                SaveManager.OnLoadProgress += OnSaveManagerLoadProgress;
            }

            // Subscribe to SaveFileManager events
            SaveFileManager.OnSaveFileCreated += OnSaveFileChanged;
            SaveFileManager.OnSaveFileDeleted += OnSaveFileChanged;
            SaveFileManager.OnSaveFileError += OnSaveFileError;

            // Subscribe to SaveValidator events
            SaveValidator.OnValidationCompleted += OnSaveValidationCompleted;
            SaveValidator.OnValidationError += OnSaveValidationError;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (SaveManager.Instance != null)
            {
                SaveManager.OnLoadStarted -= OnSaveManagerLoadStarted;
                SaveManager.OnLoadCompleted -= OnSaveManagerLoadCompleted;
                SaveManager.OnLoadError -= OnSaveManagerLoadError;
                SaveManager.OnLoadProgress -= OnSaveManagerLoadProgress;
            }

            SaveFileManager.OnSaveFileCreated -= OnSaveFileChanged;
            SaveFileManager.OnSaveFileDeleted -= OnSaveFileChanged;
            SaveFileManager.OnSaveFileError -= OnSaveFileError;

            SaveValidator.OnValidationCompleted -= OnSaveValidationCompleted;
            SaveValidator.OnValidationError -= OnSaveValidationError;
        }

        // Main load operations
        public async Task<SaveData> LoadGameAsync(string slotName, LoadStrategy strategy = LoadStrategy.Full)
        {
            if (_isLoading)
            {
                Debug.LogWarning("Load operation already in progress");
                return null;
            }

            if (string.IsNullOrEmpty(slotName))
            {
                Debug.LogError("Slot name cannot be null or empty");
                return null;
            }

            _isLoading = true;
            _currentLoadSlot = slotName;
            _currentLoadProgress = 0f;
            _currentOperation = LoadOperation.Initializing;

            OnLoadStarted?.Invoke(slotName);
            OnLoadOperationChanged?.Invoke(slotName, _currentOperation);

            try
            {
                SaveData loadedData = null;
                int retryCount = 0;

                while (retryCount <= _maxLoadRetries && loadedData == null)
                {
                    try
                    {
                        loadedData = await PerformLoadOperationAsync(slotName, strategy);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        Debug.LogWarning($"Load attempt {retryCount} failed for {slotName}: {ex.Message}");

                        if (retryCount <= _maxLoadRetries)
                        {
                            await Task.Delay(1000 * retryCount); // Exponential backoff
                            
                            // Try to recover from backup if available
                            if (_enableErrorRecovery && retryCount == 1)
                            {
                                bool recovered = await TryRecoverFromBackupAsync(slotName);
                                if (recovered)
                                {
                                    Debug.Log($"Recovered {slotName} from backup, retrying load");
                                }
                            }
                        }
                        else
                        {
                            throw; // Re-throw on final failure
                        }
                    }
                }

                if (loadedData != null)
                {
                    // Apply loaded data to game systems
                    await ApplyLoadedDataAsync(loadedData);
                    
                    UpdateProgress(1.0f, LoadOperation.Completed);
                    OnLoadCompleted?.Invoke(slotName, loadedData);
                    
                    Debug.Log($"Successfully loaded game from slot: {slotName}");
                    return loadedData;
                }
                else
                {
                    throw new InvalidOperationException("Load operation failed after all retry attempts");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Load failed for slot {slotName}: {ex.Message}");
                OnLoadFailed?.Invoke(slotName, ex.Message);
                return null;
            }
            finally
            {
                _isLoading = false;
                _currentLoadSlot = null;
                _currentLoadProgress = 0f;
                _currentOperation = LoadOperation.None;
                OnLoadOperationChanged?.Invoke(slotName, _currentOperation);
            }
        }

        private async Task<SaveData> PerformLoadOperationAsync(string slotName, LoadStrategy strategy)
        {
            // Phase 1: Validate save file
            UpdateProgress(0.1f, LoadOperation.Validating);
            
            if (_autoValidateBeforeLoad)
            {
                bool isValid = await ValidateSaveFileAsync(slotName);
                if (!isValid)
                {
                    throw new InvalidDataException($"Save file validation failed for slot: {slotName}");
                }
            }

            // Phase 2: Check version compatibility
            UpdateProgress(0.2f, LoadOperation.CheckingCompatibility);
            
            var metadata = await SaveFileManager.LoadMetadataAsync(slotName);
            if (metadata != null && RequiresVersionMigration(metadata.SaveVersion))
            {
                UpdateProgress(0.3f, LoadOperation.MigratingVersion);
                await PerformVersionMigrationAsync(slotName, metadata);
            }

            // Phase 3: Load save data based on strategy
            UpdateProgress(0.4f, LoadOperation.LoadingData);
            
            SaveData saveData = await LoadSaveDataByStrategyAsync(slotName, strategy);
            
            if (saveData == null)
            {
                throw new InvalidDataException($"Failed to load save data from slot: {slotName}");
            }

            // Phase 4: Validate loaded data
            UpdateProgress(0.7f, LoadOperation.ValidatingData);
            
            var validationResult = SaveValidator.ValidateSaveData(saveData);
            if (!validationResult.IsValid && validationResult.Severity == ValidationSeverity.Critical)
            {
                throw new InvalidDataException($"Loaded data validation failed: {validationResult.GetSummary()}");
            }

            // Phase 5: Sanitize data if needed
            if (validationResult.HasErrors || validationResult.HasWarnings)
            {
                UpdateProgress(0.8f, LoadOperation.SanitizingData);
                saveData = SaveValidator.SanitizeSaveData(saveData);
            }

            UpdateProgress(0.9f, LoadOperation.FinalValidation);
            
            // Final validation after sanitization
            var finalValidation = SaveValidator.ValidateSaveData(saveData);
            if (!finalValidation.IsValid)
            {
                Debug.LogWarning($"Save data still has issues after sanitization: {finalValidation.GetSummary()}");
            }

            return saveData;
        }

        private async Task<SaveData> LoadSaveDataByStrategyAsync(string slotName, LoadStrategy strategy)
        {
            switch (strategy)
            {
                case LoadStrategy.Full:
                    return await LoadFullSaveDataAsync(slotName);
                    
                case LoadStrategy.Lazy:
                    return await LoadLazySaveDataAsync(slotName);
                    
                case LoadStrategy.Streaming:
                    return await LoadStreamingSaveDataAsync(slotName);
                    
                default:
                    return await LoadFullSaveDataAsync(slotName);
            }
        }

        private async Task<SaveData> LoadFullSaveDataAsync(string slotName)
        {
            // Use SaveManager's existing load functionality
            return await SaveManager.Instance.LoadGameAsync(slotName);
        }

        private async Task<SaveData> LoadLazySaveDataAsync(string slotName)
        {
            // Load core data immediately, defer instance data loading
            UpdateProgress(0.45f, LoadOperation.LoadingCoreData);
            
            string saveContent = await SaveFileManager.LoadFileAsync(slotName);
            SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
            JsonUtility.FromJsonOverwrite(saveContent, saveData);
            
            // Clear instance collections for lazy loading
            saveData.VehicleInstances.Clear();
            saveData.ContractInstances.Clear();
            saveData.CityInstances.Clear();
            
            UpdateProgress(0.6f, LoadOperation.LoadingData);
            
            // Instance data will be loaded on-demand by other systems
            return saveData;
        }

        private async Task<SaveData> LoadStreamingSaveDataAsync(string slotName)
        {
            // Load data in chunks to prevent memory spikes
            UpdateProgress(0.45f, LoadOperation.LoadingCoreData);
            
            // For now, use full loading (streaming would require more complex serialization)
            // This is a placeholder for future streaming implementation
            await Task.Delay(100); // Simulate streaming delay
            
            UpdateProgress(0.6f, LoadOperation.LoadingData);
            
            return await LoadFullSaveDataAsync(slotName);
        }

        private async Task ApplyLoadedDataAsync(SaveData saveData)
        {
            UpdateProgress(0.95f, LoadOperation.ApplyingData);
            
            // Notify GameManager to apply the loaded data
            if (GameManager.Instance != null)
            {
                // GameManager.Instance.ApplySaveData(saveData) - this method exists in GameManager
                // For now, we'll let the SaveManager handle this through its existing mechanism
            }
            
            // Notify other systems that may need to respond to loaded data
            // This can be expanded as more systems are implemented
            
            await Task.Delay(50); // Brief delay to ensure all systems process the data
        }

        // Save file discovery and management
        public async Task<List<SaveSlotInfo>> DiscoverSaveFilesAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && IsCacheValid())
            {
                return _saveSlotCache.Values.ToList();
            }

            try
            {
                var saveSlots = new List<SaveSlotInfo>();
                var availableSlots = SaveFileManager.GetAvailableSaveSlots();

                foreach (string slotName in availableSlots)
                {
                    var slotInfo = await BuildSaveSlotInfoAsync(slotName);
                    if (slotInfo != null)
                    {
                        saveSlots.Add(slotInfo);
                        _saveSlotCache[slotName] = slotInfo;
                    }
                }

                // Sort by last modified date (newest first)
                saveSlots.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
                
                _lastCacheUpdate = DateTime.Now;
                OnSaveSlotDiscoveryCompleted?.Invoke(saveSlots);
                
                return saveSlots;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to discover save files: {ex.Message}");
                return new List<SaveSlotInfo>();
            }
        }

        private async Task<SaveSlotInfo> BuildSaveSlotInfoAsync(string slotName)
        {
            try
            {
                // Get file information
                var fileInfo = SaveFileManager.GetSaveFileInfo(slotName);
                if (fileInfo == null || !fileInfo.IsValid)
                {
                    return null;
                }

                // Get metadata
                var metadata = await SaveFileManager.LoadMetadataAsync(slotName);
                if (metadata == null)
                {
                    // Create basic slot info without metadata
                    return new SaveSlotInfo
                    {
                        SlotName = slotName,
                        DisplayName = slotName,
                        CreationDate = fileInfo.CreationTime,
                        LastModified = fileInfo.LastWriteTime,
                        PlayTimeHours = 0f,
                        CurrentCredits = 0f,
                        TotalContracts = 0,
                        SaveVersion = "Unknown",
                        IsValid = fileInfo.IsValid
                    };
                }

                return new SaveSlotInfo
                {
                    SlotName = slotName,
                    DisplayName = metadata.SaveName,
                    CreationDate = metadata.CreationDate,
                    LastModified = metadata.LastModified,
                    PlayTimeHours = metadata.PlayTimeHours,
                    CurrentCredits = metadata.CurrentCredits,
                    TotalContracts = metadata.TotalContracts,
                    SaveVersion = metadata.SaveVersion,
                    IsValid = fileInfo.IsValid && !string.IsNullOrEmpty(metadata.Checksum)
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to build save slot info for {slotName}: {ex.Message}");
                return null;
            }
        }

        // Save file validation
        public async Task<bool> ValidateSaveFileAsync(string slotName)
        {
            try
            {
                // Basic file integrity check
                if (!SaveFileManager.ValidateFileIntegrity(slotName))
                {
                    return false;
                }

                // Load and validate save data
                string saveContent = await SaveFileManager.LoadFileAsync(slotName);
                SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
                JsonUtility.FromJsonOverwrite(saveContent, saveData);

                var validationResult = SaveValidator.ValidateSaveData(saveData);
                OnSaveFileValidated?.Invoke(slotName, validationResult);

                return validationResult.IsValid || validationResult.Severity != ValidationSeverity.Critical;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save file validation failed for {slotName}: {ex.Message}");
                return false;
            }
        }

        // Version migration
        private bool RequiresVersionMigration(string saveVersion)
        {
            string currentVersion = Application.version ?? "1.0.0";
            return saveVersion != currentVersion;
        }

        private async Task PerformVersionMigrationAsync(string slotName, SaveMetadata metadata)
        {
            try
            {
                string currentVersion = Application.version ?? "1.0.0";
                
                // Load save data for migration
                string saveContent = await SaveFileManager.LoadFileAsync(slotName);
                SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
                JsonUtility.FromJsonOverwrite(saveContent, saveData);

                // Perform migration
                SaveData migratedData = SaveValidator.MigrateSaveData(saveData, currentVersion);
                
                if (migratedData != null)
                {
                    // Save migrated data back
                    string migratedJson = JsonUtility.ToJson(migratedData, true);
                    await SaveFileManager.SaveFileAtomicAsync(slotName, migratedJson);
                    
                    // Update metadata
                    metadata.SaveVersion = currentVersion;
                    metadata.LastModified = DateTime.Now;
                    await SaveFileManager.SaveMetadataAsync(slotName, metadata);
                    
                    Debug.Log($"Successfully migrated save {slotName} to version {currentVersion}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Version migration failed for {slotName}: {ex.Message}");
                throw;
            }
        }

        // Error recovery
        private async Task<bool> TryRecoverFromBackupAsync(string slotName)
        {
            try
            {
                var availableBackups = SaveFileManager.GetAvailableBackups(slotName);
                if (availableBackups.Count == 0)
                {
                    return false;
                }

                // Try to restore from the most recent backup
                string latestBackup = availableBackups.OrderByDescending(b => b).FirstOrDefault();
                if (string.IsNullOrEmpty(latestBackup))
                {
                    return false;
                }

                bool restored = await SaveFileManager.RestoreFromBackupAsync(slotName, latestBackup);
                if (restored)
                {
                    Debug.Log($"Successfully recovered {slotName} from backup: {latestBackup}");
                    InvalidateSlotCache(slotName);
                }

                return restored;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Backup recovery failed for {slotName}: {ex.Message}");
                return false;
            }
        }

        // Progress reporting
        private void UpdateProgress(float progress, LoadOperation operation)
        {
            _currentLoadProgress = Mathf.Clamp01(progress);
            _currentOperation = operation;

            if (_enableProgressReporting)
            {
                OnLoadProgress?.Invoke(_currentLoadSlot, _currentLoadProgress);
                OnLoadOperationChanged?.Invoke(_currentLoadSlot, _currentOperation);
            }
        }

        // Cache management
        private bool IsCacheValid()
        {
            return DateTime.Now - _lastCacheUpdate < CacheValidityDuration && _saveSlotCache.Count > 0;
        }

        private void InvalidateSlotCache(string slotName = null)
        {
            if (string.IsNullOrEmpty(slotName))
            {
                _saveSlotCache.Clear();
                _lastCacheUpdate = DateTime.MinValue;
            }
            else
            {
                _saveSlotCache.Remove(slotName);
            }
        }

        // Event handlers
        private void OnSaveManagerLoadStarted(string slotName)
        {
            if (!_isLoading) // Only if not already managed by LoadManager
            {
                OnLoadStarted?.Invoke(slotName);
            }
        }

        private void OnSaveManagerLoadCompleted(SaveData saveData)
        {
            if (!_isLoading) // Only if not already managed by LoadManager
            {
                OnLoadCompleted?.Invoke(saveData.SaveName, saveData);
            }
        }

        private void OnSaveManagerLoadError(string slotName, string error)
        {
            if (!_isLoading) // Only if not already managed by LoadManager
            {
                OnLoadFailed?.Invoke(slotName, error);
            }
        }

        private void OnSaveManagerLoadProgress(float progress)
        {
            if (_isLoading && _enableProgressReporting)
            {
                // Integrate SaveManager progress with LoadManager progress
                float adjustedProgress = Mathf.Lerp(0.4f, 0.7f, progress); // Map to load data phase
                UpdateProgress(adjustedProgress, LoadOperation.LoadingData);
            }
        }

        private void OnSaveFileChanged(string slotName)
        {
            InvalidateSlotCache(slotName);
        }

        private void OnSaveFileError(string slotName, string error)
        {
            Debug.LogError($"Save file error for {slotName}: {error}");
            InvalidateSlotCache(slotName);
        }

        private void OnSaveValidationCompleted(string slotName, ValidationResult result)
        {
            OnSaveFileValidated?.Invoke(slotName, result);
        }

        private void OnSaveValidationError(string slotName, string error)
        {
            Debug.LogError($"Save validation error for {slotName}: {error}");
        }

        // Public utility methods
        public SaveSlotInfo GetCachedSlotInfo(string slotName)
        {
            return _saveSlotCache.ContainsKey(slotName) ? _saveSlotCache[slotName] : null;
        }

        public async Task<SaveSlotInfo> RefreshSlotInfoAsync(string slotName)
        {
            var slotInfo = await BuildSaveSlotInfoAsync(slotName);
            if (slotInfo != null)
            {
                _saveSlotCache[slotName] = slotInfo;
            }
            return slotInfo;
        }

        public void ClearCache()
        {
            InvalidateSlotCache();
        }

        public bool CanLoad(string slotName)
        {
            return !_isLoading && SaveFileManager.SaveSlotExists(slotName);
        }

        public void CancelLoad()
        {
            if (_isLoading)
            {
                Debug.Log("Load operation cancelled");
                _isLoading = false;
                _currentLoadSlot = null;
                _currentLoadProgress = 0f;
                _currentOperation = LoadOperation.Cancelled;
                
                OnLoadFailed?.Invoke(_currentLoadSlot ?? "Unknown", "Load operation was cancelled");
                OnLoadOperationChanged?.Invoke(_currentLoadSlot ?? "Unknown", _currentOperation);
            }
        }
    }

    // AIDEV-NOTE: Enumeration for different load strategies
    public enum LoadStrategy
    {
        Full,       // Load all data immediately
        Lazy,       // Load core data immediately, defer instance data
        Streaming   // Load data in chunks (future implementation)
    }

    // AIDEV-NOTE: Enumeration for load operation phases
    public enum LoadOperation
    {
        None,
        Initializing,
        Validating,
        CheckingCompatibility,
        MigratingVersion,
        LoadingData,
        LoadingCoreData,
        ValidatingData,
        SanitizingData,
        FinalValidation,
        ApplyingData,
        Completed,
        Cancelled
    }
}