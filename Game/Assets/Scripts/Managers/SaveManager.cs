using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LogisticGame.Managers
{
    // AIDEV-NOTE: Singleton manager for centralized save/load operations with async support
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SaveManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(SaveManager));
                        _instance = go.AddComponent<SaveManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Save Configuration")]
        [SerializeField] private int _maxSaveSlots = 10;
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool _createBackups = true;
        [SerializeField] private int _maxBackups = 3;

        [Header("Save State")]
        [SerializeField] private SaveData _currentSaveData;
        [SerializeField] private bool _isSaving = false;
        [SerializeField] private bool _isLoading = false;
        [SerializeField] private string _currentSaveSlot;

        // Save directory paths
        private string _saveDirectory;
        private string _backupDirectory;

        // Auto-save timer
        private float _autoSaveTimer;

        // Events for save/load operations
        public static event Action<SaveData> OnSaveCompleted;
        public static event Action<SaveData> OnLoadCompleted;
        public static event Action<string> OnSaveStarted;
        public static event Action<string> OnLoadStarted;
        public static event Action<string, string> OnSaveError;
        public static event Action<string, string> OnLoadError;
        public static event Action<float> OnSaveProgress;
        public static event Action<float> OnLoadProgress;

        // Properties
        public SaveData CurrentSaveData => _currentSaveData;
        public bool IsSaving => _isSaving;
        public bool IsLoading => _isLoading;
        public bool IsOperating => _isSaving || _isLoading;
        public string CurrentSaveSlot => _currentSaveSlot;
        public bool AutoSaveEnabled 
        { 
            get => _autoSaveEnabled; 
            set => _autoSaveEnabled = value; 
        }

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
            
            InitializeSaveSystem();
        }

        private void Update()
        {
            // Auto-save timer
            if (_autoSaveEnabled && !IsOperating && _currentSaveData != null)
            {
                _autoSaveTimer += Time.unscaledDeltaTime;
                if (_autoSaveTimer >= _autoSaveInterval)
                {
                    _autoSaveTimer = 0f;
                    _ = QuickSaveAsync("Auto-save");
                }
            }
        }

        private void InitializeSaveSystem()
        {
            // AIDEV-NOTE: Set up save directories in persistent data path
            _saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
            _backupDirectory = Path.Combine(_saveDirectory, "Backups");

            // Create directories if they don't exist
            try
            {
                if (!Directory.Exists(_saveDirectory))
                    Directory.CreateDirectory(_saveDirectory);

                if (!Directory.Exists(_backupDirectory))
                    Directory.CreateDirectory(_backupDirectory);

                Debug.Log($"Save system initialized. Save directory: {_saveDirectory}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize save directories: {ex.Message}");
                OnSaveError?.Invoke("System", $"Failed to initialize save system: {ex.Message}");
            }
        }

        // Main save/load operations
        public async Task<bool> SaveGameAsync(string slotName, SaveData saveData = null)
        {
            if (_isSaving)
            {
                Debug.LogWarning("Save operation already in progress");
                return false;
            }

            _isSaving = true;
            OnSaveStarted?.Invoke(slotName);

            try
            {
                SaveData dataToSave = saveData ?? _currentSaveData;
                if (dataToSave == null)
                {
                    throw new InvalidOperationException("No save data available");
                }

                // Prepare save data
                dataToSave.PrepareForSave();
                OnSaveProgress?.Invoke(0.2f);

                // Create slot directory
                string slotDirectory = Path.Combine(_saveDirectory, slotName);
                if (!Directory.Exists(slotDirectory))
                    Directory.CreateDirectory(slotDirectory);

                OnSaveProgress?.Invoke(0.4f);

                // Create backup if enabled
                if (_createBackups && File.Exists(GetSaveFilePath(slotName)))
                {
                    await CreateBackupAsync(slotName);
                }

                OnSaveProgress?.Invoke(0.6f);

                // Save main data files
                await SaveDataFilesAsync(slotDirectory, dataToSave);
                OnSaveProgress?.Invoke(0.9f);

                // Update save metadata
                string saveFilePath = GetSaveFilePath(slotName);
                FileInfo fileInfo = new FileInfo(saveFilePath);
                dataToSave.UpdateSaveInfo(saveFilePath, fileInfo.Length);

                _currentSaveData = dataToSave;
                _currentSaveSlot = slotName;

                OnSaveProgress?.Invoke(1.0f);
                OnSaveCompleted?.Invoke(dataToSave);
                
                Debug.Log($"Game saved successfully to slot: {slotName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save failed for slot {slotName}: {ex.Message}");
                OnSaveError?.Invoke(slotName, ex.Message);
                return false;
            }
            finally
            {
                _isSaving = false;
                OnSaveProgress?.Invoke(0f);
            }
        }

        public async Task<SaveData> LoadGameAsync(string slotName)
        {
            if (_isLoading)
            {
                Debug.LogWarning("Load operation already in progress");
                return null;
            }

            _isLoading = true;
            OnLoadStarted?.Invoke(slotName);

            try
            {
                string saveFilePath = GetSaveFilePath(slotName);
                if (!File.Exists(saveFilePath))
                {
                    throw new FileNotFoundException($"Save file not found for slot: {slotName}");
                }

                OnLoadProgress?.Invoke(0.2f);

                // Load save data
                SaveData loadedData = await LoadDataFilesAsync(slotName);
                OnLoadProgress?.Invoke(0.6f);

                if (loadedData == null)
                {
                    throw new InvalidDataException("Failed to load save data");
                }

                // Validate loaded data
                if (!loadedData.ValidateChecksum() || !loadedData.ValidateData())
                {
                    throw new InvalidDataException("Save data validation failed");
                }

                OnLoadProgress?.Invoke(0.9f);

                // Check for version migration
                string currentVersion = Application.version ?? "1.0.0";
                if (loadedData.RequiresVersionMigration(currentVersion))
                {
                    Debug.Log($"Migrating save data from version {loadedData.SaveVersion} to {currentVersion}");
                    loadedData.MigrateToVersion(currentVersion);
                }

                _currentSaveData = loadedData;
                _currentSaveSlot = slotName;

                loadedData.OnLoadComplete();
                OnLoadProgress?.Invoke(1.0f);
                OnLoadCompleted?.Invoke(loadedData);

                Debug.Log($"Game loaded successfully from slot: {slotName}");
                return loadedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Load failed for slot {slotName}: {ex.Message}");
                OnLoadError?.Invoke(slotName, ex.Message);
                return null;
            }
            finally
            {
                _isLoading = false;
                OnLoadProgress?.Invoke(0f);
            }
        }

        // Quick save/load operations
        public async Task<bool> QuickSaveAsync(string reason = "Quick Save")
        {
            if (string.IsNullOrEmpty(_currentSaveSlot))
            {
                Debug.LogWarning("No current save slot for quick save");
                return false;
            }

            Debug.Log($"Quick save triggered: {reason}");
            return await SaveGameAsync(_currentSaveSlot);
        }

        public async Task<SaveData> QuickLoadAsync()
        {
            if (string.IsNullOrEmpty(_currentSaveSlot))
            {
                Debug.LogWarning("No current save slot for quick load");
                return null;
            }

            return await LoadGameAsync(_currentSaveSlot);
        }

        // File operations
        private async Task SaveDataFilesAsync(string slotDirectory, SaveData saveData)
        {
            // Save main save data as JSON
            string saveJson = JsonUtility.ToJson(saveData, true);
            string saveFilePath = Path.Combine(slotDirectory, "GameSave.json");
            await File.WriteAllTextAsync(saveFilePath, saveJson);

            // Save metadata separately
            var metadata = new SaveMetadata
            {
                SaveName = saveData.SaveName,
                CreationDate = saveData.CreationDate,
                LastModified = saveData.LastModified,
                PlayTimeHours = saveData.PlayTimeHours,
                CurrentCredits = saveData.CurrentCredits,
                TotalContracts = saveData.TotalContracts,
                SaveVersion = saveData.SaveVersion,
                Checksum = saveData.Checksum
            };

            string metadataJson = JsonUtility.ToJson(metadata, true);
            string metadataPath = Path.Combine(slotDirectory, "Metadata.json");
            await File.WriteAllTextAsync(metadataPath, metadataJson);
        }

        private async Task<SaveData> LoadDataFilesAsync(string slotName)
        {
            string slotDirectory = Path.Combine(_saveDirectory, slotName);
            string saveFilePath = Path.Combine(slotDirectory, "GameSave.json");

            if (!File.Exists(saveFilePath))
            {
                throw new FileNotFoundException($"Save file not found: {saveFilePath}");
            }

            string saveJson = await File.ReadAllTextAsync(saveFilePath);
            SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
            JsonUtility.FromJsonOverwrite(saveJson, saveData);

            return saveData;
        }

        // Backup operations
        private async Task CreateBackupAsync(string slotName)
        {
            try
            {
                string sourceFile = GetSaveFilePath(slotName);
                if (!File.Exists(sourceFile))
                    return;

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupFileName = $"{slotName}_backup_{timestamp}.json";
                string backupPath = Path.Combine(_backupDirectory, backupFileName);

                await Task.Run(() => File.Copy(sourceFile, backupPath));

                // Clean up old backups
                CleanupOldBackups(slotName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create backup for {slotName}: {ex.Message}");
            }
        }

        private void CleanupOldBackups(string slotName)
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupDirectory, $"{slotName}_backup_*.json");
                if (backupFiles.Length > _maxBackups)
                {
                    Array.Sort(backupFiles);
                    for (int i = 0; i < backupFiles.Length - _maxBackups; i++)
                    {
                        File.Delete(backupFiles[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to cleanup old backups: {ex.Message}");
            }
        }

        // Save slot management
        public List<SaveSlotInfo> GetSaveSlots()
        {
            List<SaveSlotInfo> slots = new List<SaveSlotInfo>();

            try
            {
                if (!Directory.Exists(_saveDirectory))
                    return slots;

                string[] slotDirectories = Directory.GetDirectories(_saveDirectory);
                foreach (string slotDir in slotDirectories)
                {
                    string slotName = Path.GetFileName(slotDir);
                    if (slotName == "Backups") continue;

                    SaveSlotInfo slotInfo = GetSaveSlotInfo(slotName);
                    if (slotInfo != null)
                        slots.Add(slotInfo);
                }

                slots.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get save slots: {ex.Message}");
            }

            return slots;
        }

        public SaveSlotInfo GetSaveSlotInfo(string slotName)
        {
            try
            {
                string metadataPath = Path.Combine(_saveDirectory, slotName, "Metadata.json");
                if (!File.Exists(metadataPath))
                    return null;

                string metadataJson = File.ReadAllText(metadataPath);
                SaveMetadata metadata = JsonUtility.FromJson<SaveMetadata>(metadataJson);

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
                    IsValid = !string.IsNullOrEmpty(metadata.Checksum)
                };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to read save slot info for {slotName}: {ex.Message}");
                return null;
            }
        }

        public bool DeleteSaveSlot(string slotName)
        {
            try
            {
                string slotDirectory = Path.Combine(_saveDirectory, slotName);
                if (Directory.Exists(slotDirectory))
                {
                    Directory.Delete(slotDirectory, true);
                    Debug.Log($"Deleted save slot: {slotName}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save slot {slotName}: {ex.Message}");
                OnSaveError?.Invoke(slotName, $"Failed to delete save: {ex.Message}");
            }

            return false;
        }

        // Utility methods
        private string GetSaveFilePath(string slotName)
        {
            return Path.Combine(_saveDirectory, slotName, "GameSave.json");
        }

        public bool HasSaveData(string slotName)
        {
            return File.Exists(GetSaveFilePath(slotName));
        }

        public long GetSaveFileSize(string slotName)
        {
            string saveFile = GetSaveFilePath(slotName);
            if (File.Exists(saveFile))
            {
                return new FileInfo(saveFile).Length;
            }
            return 0;
        }

        // Auto-save triggers
        public void TriggerAutoSave(string reason)
        {
            if (_autoSaveEnabled && !IsOperating)
            {
                _ = QuickSaveAsync($"Auto-save: {reason}");
            }
        }

        public void ResetAutoSaveTimer()
        {
            _autoSaveTimer = 0f;
        }

        // Application lifecycle events
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _autoSaveEnabled)
            {
                TriggerAutoSave("Application pause");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _autoSaveEnabled)
            {
                TriggerAutoSave("Application focus lost");
            }
        }

        private void OnDestroy()
        {
            if (_autoSaveEnabled && _currentSaveData != null)
            {
                // Synchronous save on application quit
                try
                {
                    SaveGameAsync(_currentSaveSlot).Wait(5000); // 5 second timeout
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to save on quit: {ex.Message}");
                }
            }
        }
    }

    // AIDEV-NOTE: Data structures for save slot management
    [System.Serializable]
    public class SaveSlotInfo
    {
        public string SlotName;
        public string DisplayName;
        public DateTime CreationDate;
        public DateTime LastModified;
        public float PlayTimeHours;
        public float CurrentCredits;
        public int TotalContracts;
        public string SaveVersion;
        public bool IsValid;

        public string GetDisplayInfo()
        {
            return $"{DisplayName}\n" +
                   $"Last Played: {LastModified:MM/dd/yyyy HH:mm}\n" +
                   $"Play Time: {PlayTimeHours:F1}h\n" +
                   $"Credits: {CurrentCredits:C0}";
        }
    }

    [System.Serializable]
    public class SaveMetadata
    {
        public string SaveName;
        public DateTime CreationDate;
        public DateTime LastModified;
        public float PlayTimeHours;
        public float CurrentCredits;
        public int TotalContracts;
        public string SaveVersion;
        public string Checksum;
    }
}