using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Comprehensive save file discovery and health checking system
    public static class SaveFileDiscovery
    {
        // Discovery events
        public static event Action<List<SaveFileHealth>> OnHealthCheckCompleted;
        public static event Action<string, SaveFileHealth> OnSaveFileHealthChecked;
        public static event Action<string, string> OnDiscoveryError;

        // Health check configuration
        private const long MAX_SAVE_FILE_SIZE = 100 * 1024 * 1024; // 100MB
        private const long MIN_SAVE_FILE_SIZE = 100; // 100 bytes
        private const int MAX_SAVE_FILES = 1000;
        private static readonly TimeSpan MAX_SAVE_AGE = TimeSpan.FromDays(365 * 5); // 5 years

        // Cache for health check results
        private static Dictionary<string, SaveFileHealth> _healthCache = new Dictionary<string, SaveFileHealth>();
        private static DateTime _lastHealthCheck = DateTime.MinValue;
        private static readonly TimeSpan HealthCacheValidityDuration = TimeSpan.FromMinutes(10);

        // Main discovery methods
        public static async Task<List<SaveFileHealth>> PerformComprehensiveDiscoveryAsync(bool forceRefresh = false)
        {
            try
            {
                // Check cache validity
                if (!forceRefresh && IsHealthCacheValid())
                {
                    return _healthCache.Values.ToList();
                }

                var healthResults = new List<SaveFileHealth>();

                // Phase 1: Discover all save slots
                var saveSlots = SaveFileManager.GetAvailableSaveSlots();
                
                if (saveSlots.Count > MAX_SAVE_FILES)
                {
                    Debug.LogWarning($"Found {saveSlots.Count} save files, which exceeds the maximum of {MAX_SAVE_FILES}");
                }

                // Phase 2: Health check each save file
                var healthTasks = saveSlots.Select(slot => CheckSaveFileHealthAsync(slot)).ToArray();
                var healthChecks = await Task.WhenAll(healthTasks);

                foreach (var health in healthChecks)
                {
                    if (health != null)
                    {
                        healthResults.Add(health);
                        _healthCache[health.SlotName] = health;
                    }
                }

                // Phase 3: Identify orphaned files and corrupted data
                await IdentifyOrphanedFilesAsync(healthResults);

                // Phase 4: Generate discovery summary
                var summary = GenerateDiscoverySummary(healthResults);
                Debug.Log($"Save file discovery completed: {summary}");

                _lastHealthCheck = DateTime.Now;
                OnHealthCheckCompleted?.Invoke(healthResults);

                return healthResults;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Comprehensive discovery failed: {ex.Message}");
                OnDiscoveryError?.Invoke("System", ex.Message);
                return new List<SaveFileHealth>();
            }
        }

        public static async Task<SaveFileHealth> CheckSaveFileHealthAsync(string slotName)
        {
            try
            {
                var health = new SaveFileHealth { SlotName = slotName };

                // Phase 1: Basic file existence and accessibility
                if (!SaveFileManager.SaveSlotExists(slotName))
                {
                    health.OverallHealth = SaveHealth.Missing;
                    health.Issues.Add("Save file does not exist");
                    return health;
                }

                // Phase 2: File system level checks
                await PerformFileSystemChecksAsync(health);

                // Phase 3: File integrity checks
                await PerformIntegrityChecksAsync(health);

                // Phase 4: Data structure validation
                await PerformDataValidationAsync(health);

                // Phase 5: Metadata consistency checks
                await PerformMetadataChecksAsync(health);

                // Phase 6: Backup availability check
                await CheckBackupAvailabilityAsync(health);

                // Phase 7: Calculate overall health
                health.OverallHealth = CalculateOverallHealth(health);
                health.LastChecked = DateTime.Now;

                OnSaveFileHealthChecked?.Invoke(slotName, health);
                return health;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Health check failed for {slotName}: {ex.Message}");
                return new SaveFileHealth
                {
                    SlotName = slotName,
                    OverallHealth = SaveHealth.Critical,
                    Issues = new List<string> { $"Health check failed: {ex.Message}" },
                    LastChecked = DateTime.Now
                };
            }
        }

        private static async Task PerformFileSystemChecksAsync(SaveFileHealth health)
        {
            try
            {
                var fileInfo = SaveFileManager.GetSaveFileInfo(health.SlotName);
                if (fileInfo == null)
                {
                    health.Issues.Add("Unable to access file information");
                    return;
                }

                health.FileSize = fileInfo.FileSize;
                health.CreationTime = fileInfo.CreationTime;
                health.LastModified = fileInfo.LastWriteTime;

                // Size validation
                if (fileInfo.FileSize < MIN_SAVE_FILE_SIZE)
                {
                    health.Issues.Add($"File size too small: {fileInfo.FileSize} bytes");
                }
                else if (fileInfo.FileSize > MAX_SAVE_FILE_SIZE)
                {
                    health.Issues.Add($"File size unusually large: {fileInfo.GetSizeDisplay()}");
                }

                // Age validation
                var fileAge = DateTime.Now - fileInfo.CreationTime;
                if (fileAge > MAX_SAVE_AGE)
                {
                    health.Warnings.Add($"Save file is very old: {fileAge.TotalDays:F0} days");
                }

                // Modification time consistency
                if (fileInfo.LastWriteTime < fileInfo.CreationTime)
                {
                    health.Issues.Add("Last modified time is before creation time");
                }

                // File permissions check (Windows-specific)
                if (Application.platform == RuntimePlatform.WindowsPlayer || 
                    Application.platform == RuntimePlatform.WindowsEditor)
                {
                    await CheckFilePermissionsAsync(health, fileInfo.FilePath);
                }
            }
            catch (Exception ex)
            {
                health.Issues.Add($"File system check failed: {ex.Message}");
            }
        }

        private static async Task CheckFilePermissionsAsync(SaveFileHealth health, string filePath)
        {
            try
            {
                // Test read access
                using (var stream = File.OpenRead(filePath))
                {
                    await stream.ReadAsync(new byte[1], 0, 1);
                }

                // Test write access by touching the file
                File.SetLastWriteTime(filePath, File.GetLastWriteTime(filePath));
            }
            catch (UnauthorizedAccessException)
            {
                health.Issues.Add("Insufficient file permissions");
            }
            catch (Exception ex)
            {
                health.Warnings.Add($"Permission check failed: {ex.Message}");
            }
        }

        private static async Task PerformIntegrityChecksAsync(SaveFileHealth health)
        {
            try
            {
                // Basic file integrity
                bool isIntegrityValid = SaveFileManager.ValidateFileIntegrity(health.SlotName);
                health.HasIntegrity = isIntegrityValid;

                if (!isIntegrityValid)
                {
                    health.Issues.Add("File integrity validation failed");
                    return;
                }

                // JSON structure validation
                string content = await SaveFileManager.LoadFileAsync(health.SlotName);
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    health.Issues.Add("Save file is empty or contains only whitespace");
                    return;
                }

                // Check for common corruption indicators
                if (content.Contains("\0"))
                {
                    health.Issues.Add("File contains null characters (possible corruption)");
                }

                if (!content.TrimStart().StartsWith("{"))
                {
                    health.Issues.Add("File does not start with JSON object marker");
                }

                if (!content.TrimEnd().EndsWith("}"))
                {
                    health.Issues.Add("File does not end with JSON object marker");
                }

                // Check for balanced braces
                int braceBalance = 0;
                foreach (char c in content)
                {
                    if (c == '{') braceBalance++;
                    else if (c == '}') braceBalance--;
                }

                if (braceBalance != 0)
                {
                    health.Issues.Add($"Unbalanced braces in JSON: {braceBalance}");
                }
            }
            catch (Exception ex)
            {
                health.Issues.Add($"Integrity check failed: {ex.Message}");
            }
        }

        private static async Task PerformDataValidationAsync(SaveFileHealth health)
        {
            try
            {
                // Load and validate save data structure
                string content = await SaveFileManager.LoadFileAsync(health.SlotName);
                SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
                JsonUtility.FromJsonOverwrite(content, saveData);

                // Use SaveValidator for comprehensive validation
                var validationResult = SaveValidator.ValidateSaveData(saveData);
                health.ValidationResult = validationResult;
                health.IsDataValid = validationResult.IsValid;

                // Add validation issues to health report
                foreach (var error in validationResult.Errors)
                {
                    health.Issues.Add($"Validation Error: {error}");
                }

                foreach (var warning in validationResult.Warnings)
                {
                    health.Warnings.Add($"Validation Warning: {warning}");
                }

                // Check for migration requirements
                if (validationResult.RequiresMigration)
                {
                    health.RequiresMigration = true;
                    health.Warnings.Add("Save file requires version migration");
                }

                // Additional data consistency checks
                await PerformDataConsistencyChecksAsync(health, saveData);
            }
            catch (Exception ex)
            {
                health.Issues.Add($"Data validation failed: {ex.Message}");
                health.IsDataValid = false;
            }
        }

        private static async Task PerformDataConsistencyChecksAsync(SaveFileHealth health, SaveData saveData)
        {
            try
            {
                // Check for logical inconsistencies beyond basic validation
                
                // Vehicle-Contract consistency
                var vehicleIds = saveData.VehicleInstances.Select(v => v.InstanceId).ToHashSet();
                var contractIds = saveData.ContractInstances.Select(c => c.InstanceId).ToHashSet();

                foreach (var contract in saveData.ContractInstances)
                {
                    if (!string.IsNullOrEmpty(contract.AssignedVehicleId) && !vehicleIds.Contains(contract.AssignedVehicleId))
                    {
                        health.Issues.Add($"Contract {contract.InstanceId} references missing vehicle {contract.AssignedVehicleId}");
                    }
                }

                // Financial consistency
                if (saveData.GameState != null)
                {
                    float totalVehicleValue = saveData.VehicleInstances.Sum(v => v.VehicleData?.PurchasePrice ?? 0f);
                    if (totalVehicleValue > saveData.GameState.CurrentCredits * 100) // Arbitrary large multiple
                    {
                        health.Warnings.Add("Vehicle value seems disproportionately high compared to current credits");
                    }
                }

                // Temporal consistency
                var now = DateTime.Now;
                if (saveData.CreationDate > now.AddDays(1))
                {
                    health.Issues.Add("Save creation date is in the future");
                }

                if (saveData.LastModified > now.AddDays(1))
                {
                    health.Issues.Add("Save modification date is in the future");
                }

                await Task.Delay(1); // Ensure async nature
            }
            catch (Exception ex)
            {
                health.Warnings.Add($"Data consistency check failed: {ex.Message}");
            }
        }

        private static async Task PerformMetadataChecksAsync(SaveFileHealth health)
        {
            try
            {
                var metadata = await SaveFileManager.LoadMetadataAsync(health.SlotName);
                health.HasMetadata = metadata != null;

                if (metadata == null)
                {
                    health.Warnings.Add("No metadata file found");
                    return;
                }

                // Metadata validation
                if (string.IsNullOrEmpty(metadata.SaveName))
                {
                    health.Warnings.Add("Metadata has no save name");
                }

                if (string.IsNullOrEmpty(metadata.SaveVersion))
                {
                    health.Warnings.Add("Metadata has no version information");
                }

                if (metadata.PlayTimeHours < 0)
                {
                    health.Issues.Add("Metadata has negative play time");
                }

                if (metadata.CurrentCredits < 0)
                {
                    health.Issues.Add("Metadata has negative credits");
                }

                // Check metadata-file consistency
                var fileInfo = SaveFileManager.GetSaveFileInfo(health.SlotName);
                if (fileInfo != null)
                {
                    var timeDifference = Math.Abs((metadata.LastModified - fileInfo.LastWriteTime).TotalMinutes);
                    if (timeDifference > 5) // Allow 5 minute tolerance
                    {
                        health.Warnings.Add("Metadata modification time doesn't match file modification time");
                    }
                }
            }
            catch (Exception ex)
            {
                health.Warnings.Add($"Metadata check failed: {ex.Message}");
            }
        }

        private static async Task CheckBackupAvailabilityAsync(SaveFileHealth health)
        {
            try
            {
                var backups = SaveFileManager.GetAvailableBackups(health.SlotName);
                health.BackupCount = backups.Count;
                health.HasBackups = backups.Count > 0;

                if (backups.Count == 0)
                {
                    health.Warnings.Add("No backup files available");
                }
                else if (backups.Count > 10)
                {
                    health.Warnings.Add($"Large number of backup files: {backups.Count}");
                }

                await Task.Delay(1); // Ensure async nature
            }
            catch (Exception ex)
            {
                health.Warnings.Add($"Backup check failed: {ex.Message}");
            }
        }

        private static async Task IdentifyOrphanedFilesAsync(List<SaveFileHealth> healthResults)
        {
            try
            {
                // Look for orphaned metadata files
                var saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
                if (!Directory.Exists(saveDirectory))
                    return;

                var validSlots = healthResults.Select(h => h.SlotName).ToHashSet();
                var allFiles = Directory.GetFiles(saveDirectory, "*", SearchOption.TopDirectoryOnly);

                foreach (var file in allFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    
                    // Check for orphaned metadata files
                    if (fileName.EndsWith("_metadata") && !file.EndsWith(".json"))
                    {
                        var slotName = fileName.Replace("_metadata", "");
                        if (!validSlots.Contains(slotName))
                        {
                            Debug.LogWarning($"Found orphaned metadata file: {file}");
                        }
                    }
                }

                await Task.Delay(1); // Ensure async nature
            }
            catch (Exception ex)
            {
                Debug.LogError($"Orphaned file identification failed: {ex.Message}");
            }
        }

        private static SaveHealth CalculateOverallHealth(SaveFileHealth health)
        {
            // Critical issues = Critical health
            if (health.Issues.Any(issue => 
                issue.Contains("corruption") || 
                issue.Contains("missing") || 
                issue.Contains("integrity") ||
                issue.Contains("permissions")))
            {
                return SaveHealth.Critical;
            }

            // Multiple issues = Poor health
            if (health.Issues.Count >= 3)
            {
                return SaveHealth.Poor;
            }

            // Any validation errors = Degraded health
            if (health.Issues.Count > 0 || !health.IsDataValid)
            {
                return SaveHealth.Degraded;
            }

            // Many warnings = Fair health
            if (health.Warnings.Count >= 5)
            {
                return SaveHealth.Fair;
            }

            // Few warnings = Good health
            if (health.Warnings.Count > 0)
            {
                return SaveHealth.Good;
            }

            // No issues = Excellent health
            return SaveHealth.Excellent;
        }

        private static string GenerateDiscoverySummary(List<SaveFileHealth> healthResults)
        {
            var excellentCount = healthResults.Count(h => h.OverallHealth == SaveHealth.Excellent);
            var goodCount = healthResults.Count(h => h.OverallHealth == SaveHealth.Good);
            var fairCount = healthResults.Count(h => h.OverallHealth == SaveHealth.Fair);
            var degradedCount = healthResults.Count(h => h.OverallHealth == SaveHealth.Degraded);
            var poorCount = healthResults.Count(h => h.OverallHealth == SaveHealth.Poor);
            var criticalCount = healthResults.Count(h => h.OverallHealth == SaveHealth.Critical);
            var missingCount = healthResults.Count(h => h.OverallHealth == SaveHealth.Missing);

            return $"Found {healthResults.Count} save files - " +
                   $"Excellent: {excellentCount}, Good: {goodCount}, Fair: {fairCount}, " +
                   $"Degraded: {degradedCount}, Poor: {poorCount}, Critical: {criticalCount}, Missing: {missingCount}";
        }

        // Cache management
        private static bool IsHealthCacheValid()
        {
            return DateTime.Now - _lastHealthCheck < HealthCacheValidityDuration && _healthCache.Count > 0;
        }

        public static void InvalidateHealthCache(string slotName = null)
        {
            if (string.IsNullOrEmpty(slotName))
            {
                _healthCache.Clear();
                _lastHealthCheck = DateTime.MinValue;
            }
            else
            {
                _healthCache.Remove(slotName);
            }
        }

        public static SaveFileHealth GetCachedHealth(string slotName)
        {
            return _healthCache.ContainsKey(slotName) ? _healthCache[slotName] : null;
        }

        // Utility methods
        public static List<SaveFileHealth> GetHealthByStatus(SaveHealth status)
        {
            return _healthCache.Values.Where(h => h.OverallHealth == status).ToList();
        }

        public static List<SaveFileHealth> GetFilesRequiringAttention()
        {
            return _healthCache.Values.Where(h => 
                h.OverallHealth == SaveHealth.Critical || 
                h.OverallHealth == SaveHealth.Poor ||
                h.RequiresMigration).ToList();
        }

        public static async Task<bool> RepairSaveFileAsync(string slotName)
        {
            try
            {
                var health = await CheckSaveFileHealthAsync(slotName);
                
                if (health.OverallHealth == SaveHealth.Critical)
                {
                    // Try to restore from backup
                    var backups = SaveFileManager.GetAvailableBackups(slotName);
                    if (backups.Count > 0)
                    {
                        var latestBackup = backups.OrderByDescending(b => b).First();
                        return await SaveFileManager.RestoreFromBackupAsync(slotName, latestBackup);
                    }
                }
                else if (health.Issues.Count > 0)
                {
                    // Try to sanitize the save data
                    string content = await SaveFileManager.LoadFileAsync(slotName);
                    SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
                    JsonUtility.FromJsonOverwrite(content, saveData);

                    SaveData sanitizedData = SaveValidator.SanitizeSaveData(saveData);
                    if (sanitizedData != null)
                    {
                        string sanitizedJson = JsonUtility.ToJson(sanitizedData, true);
                        return await SaveFileManager.SaveFileAtomicAsync(slotName, sanitizedJson);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save file repair failed for {slotName}: {ex.Message}");
                return false;
            }
        }
    }

    // AIDEV-NOTE: Data structure for comprehensive save file health information
    [System.Serializable]
    public class SaveFileHealth
    {
        public string SlotName;
        public SaveHealth OverallHealth = SaveHealth.Unknown;
        public List<string> Issues = new List<string>();
        public List<string> Warnings = new List<string>();
        
        // File system properties
        public long FileSize;
        public DateTime CreationTime;
        public DateTime LastModified;
        public DateTime LastChecked;
        
        // Validation properties
        public bool HasIntegrity = false;
        public bool IsDataValid = false;
        public bool HasMetadata = false;
        public bool HasBackups = false;
        public int BackupCount = 0;
        public bool RequiresMigration = false;
        
        // Validation result reference
        public ValidationResult ValidationResult;
        
        // Summary properties
        public bool IsHealthy => OverallHealth == SaveHealth.Excellent || OverallHealth == SaveHealth.Good;
        public bool RequiresAttention => OverallHealth == SaveHealth.Critical || OverallHealth == SaveHealth.Poor || RequiresMigration;
        public int TotalIssues => Issues.Count + Warnings.Count;
        
        public string GetHealthSummary()
        {
            return $"{OverallHealth} - {Issues.Count} issues, {Warnings.Count} warnings";
        }
        
        public string GetDetailedReport()
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine($"Health Report for {SlotName}");
            report.AppendLine($"Overall Health: {OverallHealth}");
            report.AppendLine($"Last Checked: {LastChecked:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"File Size: {GetFileSizeDisplay()}");
            report.AppendLine($"Created: {CreationTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Modified: {LastModified:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Has Integrity: {HasIntegrity}");
            report.AppendLine($"Data Valid: {IsDataValid}");
            report.AppendLine($"Has Metadata: {HasMetadata}");
            report.AppendLine($"Backup Count: {BackupCount}");
            
            if (RequiresMigration)
                report.AppendLine("REQUIRES MIGRATION");
            
            if (Issues.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("ISSUES:");
                foreach (var issue in Issues)
                    report.AppendLine($"  - {issue}");
            }
            
            if (Warnings.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("WARNINGS:");
                foreach (var warning in Warnings)
                    report.AppendLine($"  - {warning}");
            }
            
            return report.ToString();
        }
        
        private string GetFileSizeDisplay()
        {
            if (FileSize < 1024)
                return $"{FileSize} B";
            else if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024f:F1} KB";
            else
                return $"{FileSize / (1024f * 1024f):F1} MB";
        }
    }

    // AIDEV-NOTE: Enumeration for save file health status
    public enum SaveHealth
    {
        Unknown,    // Not yet checked
        Missing,    // File doesn't exist
        Critical,   // Cannot be loaded, corrupted
        Poor,       // Major issues, may fail to load
        Degraded,   // Has errors but probably loadable
        Fair,       // Minor issues, should load fine
        Good,       // Few warnings, loads reliably
        Excellent   // No issues found
    }
}