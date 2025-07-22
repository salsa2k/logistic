using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogisticGame.Managers;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Handles file I/O operations and save file metadata management
    public static class SaveFileManager
    {
        // Save file extensions and naming
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string BACKUP_EXTENSION = ".bak";
        private const string TEMP_EXTENSION = ".tmp";
        private const string METADATA_FILENAME = "metadata.json";
        
        // Directory structure
        private static readonly string SaveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
        private static readonly string BackupDirectory = Path.Combine(SaveDirectory, "Backups");
        private static readonly string TempDirectory = Path.Combine(SaveDirectory, "Temp");
        
        // Cache for improved performance
        private static Dictionary<string, SaveMetadata> _metadataCache = new Dictionary<string, SaveMetadata>();
        private static DateTime _lastCacheUpdate = DateTime.MinValue;
        private static readonly TimeSpan CacheValidityDuration = TimeSpan.FromMinutes(5);
        
        // Events
        public static event Action<string> OnSaveFileCreated;
        public static event Action<string> OnSaveFileDeleted;
        public static event Action<string, string> OnSaveFileError;
        
        static SaveFileManager()
        {
            InitializeDirectories();
        }
        
        // Directory initialization
        private static void InitializeDirectories()
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                Directory.CreateDirectory(BackupDirectory);
                Directory.CreateDirectory(TempDirectory);
                
                Debug.Log($"Save directories initialized at: {SaveDirectory}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize save directories: {ex.Message}");
                OnSaveFileError?.Invoke("System", $"Directory initialization failed: {ex.Message}");
            }
        }
        
        // File path utilities
        public static string GetSaveFilePath(string slotName)
        {
            return Path.Combine(SaveDirectory, $"{slotName}{SAVE_FILE_EXTENSION}");
        }
        
        public static string GetBackupFilePath(string slotName, DateTime timestamp)
        {
            string timestampStr = timestamp.ToString("yyyy-MM-dd_HH-mm-ss");
            return Path.Combine(BackupDirectory, $"{slotName}_{timestampStr}{BACKUP_EXTENSION}");
        }
        
        public static string GetTempFilePath(string slotName)
        {
            return Path.Combine(TempDirectory, $"{slotName}_{Guid.NewGuid()}{TEMP_EXTENSION}");
        }
        
        public static string GetMetadataFilePath(string slotName)
        {
            return Path.Combine(SaveDirectory, $"{slotName}_{METADATA_FILENAME}");
        }
        
        // Atomic file operations
        public static async Task<bool> SaveFileAtomicAsync(string slotName, string content)
        {
            string finalPath = GetSaveFilePath(slotName);
            string tempPath = GetTempFilePath(slotName);
            
            try
            {
                // Write to temporary file first
                await File.WriteAllTextAsync(tempPath, content);
                
                // Verify the temporary file
                if (!File.Exists(tempPath))
                {
                    throw new IOException("Temporary file was not created successfully");
                }
                
                // Create backup if original exists
                if (File.Exists(finalPath))
                {
                    await CreateBackupAsync(slotName);
                }
                
                // Atomically replace the original file
                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }
                File.Move(tempPath, finalPath);
                
                // Update metadata cache
                InvalidateMetadataCache(slotName);
                
                OnSaveFileCreated?.Invoke(slotName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Atomic save failed for {slotName}: {ex.Message}");
                OnSaveFileError?.Invoke(slotName, ex.Message);
                
                // Clean up temporary file
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch (Exception cleanupEx)
                {
                    Debug.LogWarning($"Failed to clean up temp file: {cleanupEx.Message}");
                }
                
                return false;
            }
        }
        
        public static async Task<string> LoadFileAsync(string slotName)
        {
            string filePath = GetSaveFilePath(slotName);
            
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Save file not found: {slotName}");
                }
                
                string content = await File.ReadAllTextAsync(filePath);
                
                if (string.IsNullOrEmpty(content))
                {
                    throw new InvalidDataException($"Save file is empty: {slotName}");
                }
                
                return content;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save file {slotName}: {ex.Message}");
                OnSaveFileError?.Invoke(slotName, ex.Message);
                throw;
            }
        }
        
        // Backup management
        public static async Task<bool> CreateBackupAsync(string slotName)
        {
            string originalPath = GetSaveFilePath(slotName);
            
            if (!File.Exists(originalPath))
                return false;
            
            try
            {
                DateTime timestamp = DateTime.Now;
                string backupPath = GetBackupFilePath(slotName, timestamp);
                
                await Task.Run(() => File.Copy(originalPath, backupPath, true));
                
                // Clean up old backups
                await CleanupOldBackupsAsync(slotName);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create backup for {slotName}: {ex.Message}");
                OnSaveFileError?.Invoke(slotName, $"Backup creation failed: {ex.Message}");
                return false;
            }
        }
        
        public static async Task CleanupOldBackupsAsync(string slotName, int maxBackups = 5)
        {
            try
            {
                await Task.Run(() =>
                {
                    string searchPattern = $"{slotName}_*{BACKUP_EXTENSION}";
                    var backupFiles = Directory.GetFiles(BackupDirectory, searchPattern);
                    
                    if (backupFiles.Length <= maxBackups)
                        return;
                    
                    // Sort by creation time, oldest first
                    Array.Sort(backupFiles, (x, y) => File.GetCreationTime(x).CompareTo(File.GetCreationTime(y)));
                    
                    // Delete oldest files
                    int filesToDelete = backupFiles.Length - maxBackups;
                    for (int i = 0; i < filesToDelete; i++)
                    {
                        File.Delete(backupFiles[i]);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to cleanup old backups for {slotName}: {ex.Message}");
            }
        }
        
        public static List<string> GetAvailableBackups(string slotName)
        {
            try
            {
                string searchPattern = $"{slotName}_*{BACKUP_EXTENSION}";
                var backupFiles = Directory.GetFiles(BackupDirectory, searchPattern);
                
                var backupList = new List<string>();
                foreach (var file in backupFiles)
                {
                    backupList.Add(Path.GetFileNameWithoutExtension(file));
                }
                
                return backupList;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get backups for {slotName}: {ex.Message}");
                return new List<string>();
            }
        }
        
        public static async Task<bool> RestoreFromBackupAsync(string slotName, string backupName)
        {
            try
            {
                string backupPath = Path.Combine(BackupDirectory, $"{backupName}{BACKUP_EXTENSION}");
                string savePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException($"Backup not found: {backupName}");
                }
                
                await Task.Run(() => File.Copy(backupPath, savePath, true));
                
                InvalidateMetadataCache(slotName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to restore backup {backupName} for {slotName}: {ex.Message}");
                OnSaveFileError?.Invoke(slotName, $"Backup restoration failed: {ex.Message}");
                return false;
            }
        }
        
        // Save slot management
        public static List<string> GetAvailableSaveSlots()
        {
            try
            {
                var saveFiles = Directory.GetFiles(SaveDirectory, $"*{SAVE_FILE_EXTENSION}");
                var slots = new List<string>();
                
                foreach (var file in saveFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    // Skip metadata files
                    if (!fileName.EndsWith($"_{Path.GetFileNameWithoutExtension(METADATA_FILENAME)}"))
                    {
                        slots.Add(fileName);
                    }
                }
                
                return slots;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get available save slots: {ex.Message}");
                return new List<string>();
            }
        }
        
        public static bool SaveSlotExists(string slotName)
        {
            return File.Exists(GetSaveFilePath(slotName));
        }
        
        public static bool DeleteSaveSlot(string slotName)
        {
            try
            {
                string savePath = GetSaveFilePath(slotName);
                string metadataPath = GetMetadataFilePath(slotName);
                
                if (File.Exists(savePath))
                    File.Delete(savePath);
                
                if (File.Exists(metadataPath))
                    File.Delete(metadataPath);
                
                InvalidateMetadataCache(slotName);
                OnSaveFileDeleted?.Invoke(slotName);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save slot {slotName}: {ex.Message}");
                OnSaveFileError?.Invoke(slotName, $"Deletion failed: {ex.Message}");
                return false;
            }
        }
        
        // Metadata management
        public static async Task<bool> SaveMetadataAsync(string slotName, SaveMetadata metadata)
        {
            try
            {
                string metadataPath = GetMetadataFilePath(slotName);
                string json = JsonUtility.ToJson(metadata, true);
                
                await File.WriteAllTextAsync(metadataPath, json);
                
                // Update cache
                _metadataCache[slotName] = metadata;
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save metadata for {slotName}: {ex.Message}");
                OnSaveFileError?.Invoke(slotName, $"Metadata save failed: {ex.Message}");
                return false;
            }
        }
        
        public static async Task<SaveMetadata> LoadMetadataAsync(string slotName)
        {
            try
            {
                // Check cache first
                if (IsMetadataCacheValid() && _metadataCache.ContainsKey(slotName))
                {
                    return _metadataCache[slotName];
                }
                
                string metadataPath = GetMetadataFilePath(slotName);
                
                if (!File.Exists(metadataPath))
                {
                    // Generate metadata from save file if it exists
                    if (SaveSlotExists(slotName))
                    {
                        return await GenerateMetadataFromSaveFile(slotName);
                    }
                    
                    return null;
                }
                
                string json = await File.ReadAllTextAsync(metadataPath);
                var metadata = JsonUtility.FromJson<SaveMetadata>(json);
                
                // Update cache
                _metadataCache[slotName] = metadata;
                
                return metadata;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load metadata for {slotName}: {ex.Message}");
                return null;
            }
        }
        
        private static async Task<SaveMetadata> GenerateMetadataFromSaveFile(string slotName)
        {
            try
            {
                string savePath = GetSaveFilePath(slotName);
                var fileInfo = new FileInfo(savePath);
                
                var metadata = new SaveMetadata
                {
                    SaveName = slotName,
                    CreationDate = fileInfo.CreationTime,
                    LastModified = fileInfo.LastWriteTime,
                    SaveVersion = Application.version ?? "1.0.0",
                    PlayTimeHours = 0f,
                    CurrentCredits = 0f,
                    TotalContracts = 0,
                    Checksum = "generated"
                };
                
                // Save the generated metadata
                await SaveMetadataAsync(slotName, metadata);
                
                return metadata;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate metadata for {slotName}: {ex.Message}");
                return null;
            }
        }
        
        // Cache management
        private static bool IsMetadataCacheValid()
        {
            return DateTime.Now - _lastCacheUpdate < CacheValidityDuration;
        }
        
        private static void InvalidateMetadataCache(string slotName = null)
        {
            if (string.IsNullOrEmpty(slotName))
            {
                _metadataCache.Clear();
                _lastCacheUpdate = DateTime.MinValue;
            }
            else
            {
                _metadataCache.Remove(slotName);
            }
        }
        
        public static void RefreshMetadataCache()
        {
            _metadataCache.Clear();
            _lastCacheUpdate = DateTime.Now;
        }
        
        // File validation
        public static bool ValidateFileIntegrity(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(filePath))
                    return false;
                
                // Basic validation - check if file is readable JSON
                string content = File.ReadAllText(filePath);
                
                if (string.IsNullOrWhiteSpace(content))
                    return false;
                
                // Try to parse as JSON (basic structure validation)
                try
                {
                    JsonUtility.FromJson<object>(content);
                    return true;
                }
                catch (ArgumentException)
                {
                    // Not valid JSON
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"File integrity validation failed for {slotName}: {ex.Message}");
                return false;
            }
        }
        
        // File information
        public static SaveFileInfo GetSaveFileInfo(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(filePath))
                    return null;
                
                var fileInfo = new FileInfo(filePath);
                
                return new SaveFileInfo
                {
                    SlotName = slotName,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    CreationTime = fileInfo.CreationTime,
                    LastWriteTime = fileInfo.LastWriteTime,
                    IsValid = ValidateFileIntegrity(slotName)
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get file info for {slotName}: {ex.Message}");
                return null;
            }
        }
        
        public static long GetTotalSaveDataSize()
        {
            try
            {
                long totalSize = 0;
                
                // Main save files
                foreach (var file in Directory.GetFiles(SaveDirectory, $"*{SAVE_FILE_EXTENSION}"))
                {
                    totalSize += new FileInfo(file).Length;
                }
                
                // Metadata files
                foreach (var file in Directory.GetFiles(SaveDirectory, $"*{METADATA_FILENAME}"))
                {
                    totalSize += new FileInfo(file).Length;
                }
                
                // Backup files
                foreach (var file in Directory.GetFiles(BackupDirectory))
                {
                    totalSize += new FileInfo(file).Length;
                }
                
                return totalSize;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to calculate total save data size: {ex.Message}");
                return 0;
            }
        }
        
        // Cleanup operations
        public static async Task CleanupTempFilesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var tempFiles = Directory.GetFiles(TempDirectory);
                    foreach (var file in tempFiles)
                    {
                        try
                        {
                            // Delete temp files older than 1 hour
                            if (DateTime.Now - File.GetCreationTime(file) > TimeSpan.FromHours(1))
                            {
                                File.Delete(file);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Failed to delete temp file {file}: {ex.Message}");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to cleanup temp files: {ex.Message}");
            }
        }
        
        public static async Task<bool> CompactSaveDataAsync()
        {
            try
            {
                // Remove old backups beyond the limit
                var saveSlots = GetAvailableSaveSlots();
                foreach (var slot in saveSlots)
                {
                    await CleanupOldBackupsAsync(slot, 3); // Keep only 3 backups per slot
                }
                
                // Clean up temp files
                await CleanupTempFilesAsync();
                
                // Refresh metadata cache
                RefreshMetadataCache();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to compact save data: {ex.Message}");
                return false;
            }
        }
    }
    
    // AIDEV-NOTE: Data structure for save file information
    [System.Serializable]
    public class SaveFileInfo
    {
        public string SlotName;
        public string FilePath;
        public long FileSize;
        public DateTime CreationTime;
        public DateTime LastWriteTime;
        public bool IsValid;
        
        public string GetSizeDisplay()
        {
            if (FileSize < 1024)
                return $"{FileSize} B";
            else if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024f:F1} KB";
            else
                return $"{FileSize / (1024f * 1024f):F1} MB";
        }
        
        public string GetLastModifiedDisplay()
        {
            var timeSpan = DateTime.Now - LastWriteTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            else if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            else if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} hours ago";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            else
                return LastWriteTime.ToString("MMM dd, yyyy");
        }
    }
}