using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Structured version migration pipeline with validation and rollback support
    public static class VersionMigrationPipeline
    {
        // Migration events
        public static event Action<string, string, string> OnMigrationStarted; // slot, fromVersion, toVersion
        public static event Action<string, MigrationResult> OnMigrationCompleted;
        public static event Action<string, string> OnMigrationFailed;
        public static event Action<string, float> OnMigrationProgress;
        public static event Action<string, string> OnMigrationStep; // slot, stepDescription

        // Migration registry
        private static Dictionary<string, MigrationStep> _migrationSteps = new Dictionary<string, MigrationStep>();
        private static List<string> _migrationPath = new List<string>();
        private static bool _isInitialized = false;

        // Migration configuration
        private const int MAX_MIGRATION_ATTEMPTS = 3;
        private const bool ENABLE_MIGRATION_BACKUP = true;
        private const bool VALIDATE_AFTER_EACH_STEP = true;

        // Initialize the migration pipeline
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            // Register migration steps
            RegisterMigrationSteps();
            BuildMigrationPath();
            
            _isInitialized = true;
            Debug.Log($"Version migration pipeline initialized with {_migrationSteps.Count} steps");
        }

        private static void RegisterMigrationSteps()
        {
            // AIDEV-NOTE: Register all version migration steps
            // These would be expanded as the game develops through versions
            
            // Example migrations from different versions
            RegisterMigrationStep("1.0.0", "1.1.0", MigrateFrom1_0_0To1_1_0);
            RegisterMigrationStep("1.1.0", "1.2.0", MigrateFrom1_1_0To1_2_0);
            RegisterMigrationStep("1.2.0", "1.3.0", MigrateFrom1_2_0To1_3_0);
            RegisterMigrationStep("1.3.0", "1.4.0", MigrateFrom1_3_0To1_4_0);
            
            // Add more migration steps as versions are released
        }

        private static void RegisterMigrationStep(string fromVersion, string toVersion, Func<SaveData, Task<SaveData>> migrationFunc)
        {
            string key = $"{fromVersion}->{toVersion}";
            _migrationSteps[key] = new MigrationStep
            {
                FromVersion = fromVersion,
                ToVersion = toVersion,
                MigrationFunction = migrationFunc,
                Description = $"Migrate from {fromVersion} to {toVersion}"
            };
        }

        private static void BuildMigrationPath()
        {
            // Build a list of all known versions in order
            var versions = new HashSet<string>();
            
            foreach (var step in _migrationSteps.Values)
            {
                versions.Add(step.FromVersion);
                versions.Add(step.ToVersion);
            }

            // Sort versions (this is simplified - in reality you'd use proper version comparison)
            _migrationPath = versions.OrderBy(v => v).ToList();
        }

        // Main migration method
        public static async Task<MigrationResult> MigrateAsync(string slotName, string targetVersion)
        {
            if (!_isInitialized)
                Initialize();

            var result = new MigrationResult
            {
                SlotName = slotName,
                TargetVersion = targetVersion,
                StartTime = DateTime.Now,
                WasSuccessful = false
            };

            try
            {
                // Load current save data to check version
                var metadata = await SaveFileManager.LoadMetadataAsync(slotName);
                if (metadata == null)
                {
                    throw new InvalidOperationException("Cannot migrate: no metadata found");
                }

                string currentVersion = metadata.SaveVersion;
                result.SourceVersion = currentVersion;

                // Check if migration is needed
                if (currentVersion == targetVersion)
                {
                    result.WasSuccessful = true;
                    result.Message = "No migration needed - versions match";
                    return result;
                }

                OnMigrationStarted?.Invoke(slotName, currentVersion, targetVersion);

                // Create backup if enabled
                if (ENABLE_MIGRATION_BACKUP)
                {
                    OnMigrationStep?.Invoke(slotName, "Creating migration backup...");
                    bool backupCreated = await SaveFileManager.CreateBackupAsync(slotName);
                    if (!backupCreated)
                    {
                        Debug.LogWarning("Failed to create migration backup, continuing anyway");
                    }
                    result.BackupCreated = backupCreated;
                }

                // Plan migration path
                var migrationSteps = PlanMigrationPath(currentVersion, targetVersion);
                if (migrationSteps.Count == 0)
                {
                    throw new NotSupportedException($"No migration path found from {currentVersion} to {targetVersion}");
                }

                OnMigrationStep?.Invoke(slotName, $"Planned {migrationSteps.Count} migration steps");
                OnMigrationProgress?.Invoke(slotName, 0.1f);

                // Load save data for migration
                string saveContent = await SaveFileManager.LoadFileAsync(slotName);
                SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
                JsonUtility.FromJsonOverwrite(saveContent, saveData);

                // Execute migration steps
                SaveData migratedData = saveData;
                for (int i = 0; i < migrationSteps.Count; i++)
                {
                    var step = migrationSteps[i];
                    float stepProgress = 0.1f + (0.8f * (i + 1) / migrationSteps.Count);
                    
                    OnMigrationStep?.Invoke(slotName, $"Executing: {step.Description}");
                    OnMigrationProgress?.Invoke(slotName, stepProgress);

                    // Execute migration step with retry logic
                    bool stepSuccessful = false;
                    Exception lastException = null;
                    
                    for (int attempt = 1; attempt <= MAX_MIGRATION_ATTEMPTS; attempt++)
                    {
                        try
                        {
                            migratedData = await step.MigrationFunction(migratedData);
                            stepSuccessful = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            lastException = ex;
                            Debug.LogWarning($"Migration step {step.Description} failed (attempt {attempt}): {ex.Message}");
                            
                            if (attempt < MAX_MIGRATION_ATTEMPTS)
                            {
                                await Task.Delay(1000 * attempt); // Exponential backoff
                            }
                        }
                    }

                    if (!stepSuccessful)
                    {
                        throw new InvalidOperationException($"Migration step failed after {MAX_MIGRATION_ATTEMPTS} attempts: {step.Description}", lastException);
                    }

                    // Validate after each step if enabled
                    if (VALIDATE_AFTER_EACH_STEP)
                    {
                        var validationResult = SaveValidator.ValidateSaveData(migratedData);
                        if (!validationResult.IsValid && validationResult.Severity == ValidationSeverity.Critical)
                        {
                            throw new ArgumentException($"Migration step {step.Description} produced invalid data: {validationResult.GetSummary()}");
                        }
                    }

                    result.CompletedSteps.Add(step.Description);
                }

                // Final validation
                OnMigrationStep?.Invoke(slotName, "Performing final validation...");
                OnMigrationProgress?.Invoke(slotName, 0.95f);

                var finalValidation = SaveValidator.ValidateSaveData(migratedData);
                if (!finalValidation.IsValid && finalValidation.Severity == ValidationSeverity.Critical)
                {
                    throw new ArgumentException($"Final validation failed: {finalValidation.GetSummary()}");
                }

                // Save migrated data
                OnMigrationStep?.Invoke(slotName, "Saving migrated data...");
                string migratedJson = JsonUtility.ToJson(migratedData, true);
                bool saveSuccessful = await SaveFileManager.SaveFileAtomicAsync(slotName, migratedJson);
                
                if (!saveSuccessful)
                {
                    throw new InvalidOperationException("Failed to save migrated data");
                }

                // Update metadata
                metadata.SaveVersion = targetVersion;
                metadata.LastModified = DateTime.Now;
                await SaveFileManager.SaveMetadataAsync(slotName, metadata);

                OnMigrationProgress?.Invoke(slotName, 1.0f);

                // Complete migration
                result.WasSuccessful = true;
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
                result.Message = $"Successfully migrated from {currentVersion} to {targetVersion} in {result.Duration.TotalSeconds:F1}s";
                result.FinalValidation = finalValidation;

                OnMigrationCompleted?.Invoke(slotName, result);
                Debug.Log($"Migration completed: {result.Message}");

                return result;
            }
            catch (Exception ex)
            {
                result.WasSuccessful = false;
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
                result.Message = ex.Message;
                result.Exception = ex;

                OnMigrationFailed?.Invoke(slotName, ex.Message);
                Debug.LogError($"Migration failed for {slotName}: {ex.Message}");

                // Attempt rollback if backup was created
                if (result.BackupCreated && ENABLE_MIGRATION_BACKUP)
                {
                    await AttemptRollbackAsync(slotName, result);
                }

                return result;
            }
        }

        private static List<MigrationStep> PlanMigrationPath(string fromVersion, string toVersion)
        {
            var path = new List<MigrationStep>();
            string currentVersion = fromVersion;

            // Simple sequential migration (in reality this might be more complex)
            while (currentVersion != toVersion)
            {
                bool foundStep = false;
                
                foreach (var step in _migrationSteps.Values)
                {
                    if (step.FromVersion == currentVersion)
                    {
                        path.Add(step);
                        currentVersion = step.ToVersion;
                        foundStep = true;
                        break;
                    }
                }

                if (!foundStep)
                {
                    // No direct path found
                    break;
                }

                // Prevent infinite loops
                if (path.Count > 20)
                {
                    break;
                }
            }

            // Verify we reached the target
            if (currentVersion != toVersion)
            {
                return new List<MigrationStep>(); // No valid path
            }

            return path;
        }

        private static async Task AttemptRollbackAsync(string slotName, MigrationResult result)
        {
            try
            {
                var backups = SaveFileManager.GetAvailableBackups(slotName);
                if (backups.Count > 0)
                {
                    var latestBackup = backups.OrderByDescending(b => b).First();
                    bool rollbackSuccessful = await SaveFileManager.RestoreFromBackupAsync(slotName, latestBackup);
                    
                    if (rollbackSuccessful)
                    {
                        result.Message += " | Rollback successful - restored from backup";
                        Debug.Log($"Rollback successful for {slotName}");
                    }
                    else
                    {
                        result.Message += " | Rollback failed";
                        Debug.LogError($"Rollback failed for {slotName}");
                    }
                }
                else
                {
                    result.Message += " | No backup available for rollback";
                }
            }
            catch (Exception ex)
            {
                result.Message += $" | Rollback error: {ex.Message}";
                Debug.LogError($"Rollback error for {slotName}: {ex.Message}");
            }
        }

        // Individual migration functions
        private static async Task<SaveData> MigrateFrom1_0_0To1_1_0(SaveData saveData)
        {
            // Example migration from 1.0.0 to 1.1.0
            // This would contain actual migration logic for this version
            
            Debug.Log("Executing migration from 1.0.0 to 1.1.0");
            
            // Example: Add new vehicle properties that were introduced in 1.1.0
            foreach (var vehicle in saveData.VehicleInstances)
            {
                // Add default values for new properties
                if (vehicle.LastMaintenance == default(DateTime))
                {
                    vehicle.LastMaintenance = DateTime.Now.AddDays(-30); // Default to 30 days ago
                }
            }

            // Example: Migrate old contract status enum values
            foreach (var contract in saveData.ContractInstances)
            {
                // Handle any contract status changes that happened in 1.1.0
                if (contract.Status == ContractStatus.Accepted && contract.DeliveryProgress >= 1f)
                {
                    contract.Status = ContractStatus.Completed;
                }
            }

            // Update version
            saveData.SetField("_saveVersion", "1.1.0");
            
            await Task.Delay(100); // Simulate migration work
            return saveData;
        }

        private static async Task<SaveData> MigrateFrom1_1_0To1_2_0(SaveData saveData)
        {
            // Example migration from 1.1.0 to 1.2.0
            Debug.Log("Executing migration from 1.1.0 to 1.2.0");
            
            // Example: Add license system that was introduced in 1.2.0
            if (saveData.GameState != null && (saveData.GameState.OwnedLicenses == null || saveData.GameState.OwnedLicenses.Count == 0))
            {
                // Give player a basic license by default
                saveData.GameState.SetField("_ownedLicenses", new List<LicenseType> { LicenseType.Standard });
            }

            // Example: Update fuel price format
            var newFuelPrices = new Dictionary<string, float>();
            foreach (var kvp in saveData.FuelPrices)
            {
                // Convert old fuel price format to new format (example)
                newFuelPrices[kvp.Key] = kvp.Value * 1.1f; // Adjust for inflation
            }
            saveData.SetField("_fuelPrices", newFuelPrices);

            saveData.SetField("_saveVersion", "1.2.0");
            
            await Task.Delay(100);
            return saveData;
        }

        private static async Task<SaveData> MigrateFrom1_2_0To1_3_0(SaveData saveData)
        {
            // Example migration from 1.2.0 to 1.3.0
            Debug.Log("Executing migration from 1.2.0 to 1.3.0");
            
            // Example: Add city reputation system introduced in 1.3.0
            foreach (var city in saveData.CityInstances)
            {
                if (city.PlayerReputation <= 0)
                {
                    city.PlayerReputation = 50f; // Default reputation
                }
            }

            // Example: Update vehicle wear calculation
            foreach (var vehicle in saveData.VehicleInstances)
            {
                // Recalculate wear based on new formula in 1.3.0
                if (vehicle.TotalDistance > 0)
                {
                    vehicle.WearLevel = Mathf.Min(1f, vehicle.TotalDistance / 100000f); // New wear calculation
                }
            }

            saveData.SetField("_saveVersion", "1.3.0");
            
            await Task.Delay(100);
            return saveData;
        }

        private static async Task<SaveData> MigrateFrom1_3_0To1_4_0(SaveData saveData)
        {
            // Example migration from 1.3.0 to 1.4.0
            Debug.Log("Executing migration from 1.3.0 to 1.4.0");
            
            // Example: Add new economic features introduced in 1.4.0
            if (saveData.GameState != null)
            {
                // Add loan system
                if (saveData.GameState.OutstandingLoans < 0)
                {
                    saveData.GameState.SetField("_outstandingLoans", 0f);
                }

                // Add company reputation
                if (saveData.GameState.PlayerCompany != null && saveData.GameState.PlayerCompany.Reputation <= 0)
                {
                    saveData.GameState.PlayerCompany.SetField("_reputation", 100f);
                }
            }

            saveData.SetField("_saveVersion", "1.4.0");
            
            await Task.Delay(100);
            return saveData;
        }

        // Utility methods
        public static bool IsMigrationRequired(string currentVersion, string targetVersion)
        {
            if (!_isInitialized)
                Initialize();

            return currentVersion != targetVersion && CanMigrate(currentVersion, targetVersion);
        }

        public static bool CanMigrate(string fromVersion, string toVersion)
        {
            if (!_isInitialized)
                Initialize();

            var path = PlanMigrationPath(fromVersion, toVersion);
            return path.Count > 0;
        }

        public static List<string> GetMigrationPath(string fromVersion, string toVersion)
        {
            if (!_isInitialized)
                Initialize();

            var steps = PlanMigrationPath(fromVersion, toVersion);
            return steps.Select(s => s.Description).ToList();
        }

        public static List<string> GetSupportedVersions()
        {
            if (!_isInitialized)
                Initialize();

            return _migrationPath.ToList();
        }

        public static string GetLatestSupportedVersion()
        {
            if (!_isInitialized)
                Initialize();

            return _migrationPath.LastOrDefault() ?? Application.version ?? "1.0.0";
        }
    }

    // AIDEV-NOTE: Data structure for migration steps
    [System.Serializable]
    public class MigrationStep
    {
        public string FromVersion;
        public string ToVersion;
        public string Description;
        public Func<SaveData, Task<SaveData>> MigrationFunction;
    }

    // AIDEV-NOTE: Data structure for migration results
    [System.Serializable]
    public class MigrationResult
    {
        public string SlotName;
        public string SourceVersion;
        public string TargetVersion;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public bool WasSuccessful;
        public string Message;
        public bool BackupCreated;
        public List<string> CompletedSteps = new List<string>();
        public ValidationResult FinalValidation;
        public Exception Exception;

        public string GetSummary()
        {
            string status = WasSuccessful ? "SUCCESS" : "FAILED";
            return $"Migration {status}: {SourceVersion} → {TargetVersion} in {Duration.TotalSeconds:F1}s";
        }

        public string GetDetailedReport()
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine($"Migration Report for {SlotName}");
            report.AppendLine($"Source Version: {SourceVersion}");
            report.AppendLine($"Target Version: {TargetVersion}");
            report.AppendLine($"Status: {(WasSuccessful ? "SUCCESS" : "FAILED")}");
            report.AppendLine($"Start Time: {StartTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"End Time: {EndTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Duration: {Duration.TotalSeconds:F1} seconds");
            report.AppendLine($"Backup Created: {BackupCreated}");
            
            if (CompletedSteps.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("Completed Steps:");
                foreach (var step in CompletedSteps)
                {
                    report.AppendLine($"  - {step}");
                }
            }

            if (FinalValidation != null)
            {
                report.AppendLine();
                report.AppendLine($"Final Validation: {FinalValidation.GetSummary()}");
            }

            if (!string.IsNullOrEmpty(Message))
            {
                report.AppendLine();
                report.AppendLine($"Message: {Message}");
            }

            if (Exception != null)
            {
                report.AppendLine();
                report.AppendLine($"Exception: {Exception.Message}");
            }

            return report.ToString();
        }
    }
}