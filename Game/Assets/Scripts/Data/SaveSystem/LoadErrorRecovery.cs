using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using LogisticGame.Managers;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Comprehensive error recovery and backup restoration system for load operations
    public static class LoadErrorRecovery
    {
        // Recovery events
        public static event Action<string, RecoveryAttempt> OnRecoveryStarted;
        public static event Action<string, RecoveryResult> OnRecoveryCompleted;
        public static event Action<string, string> OnRecoveryFailed;
        public static event Action<string, float> OnRecoveryProgress;
        public static event Action<string, string> OnRecoveryStep;

        // Recovery configuration
        private const int MAX_RECOVERY_ATTEMPTS = 5;
        private const int MAX_BACKUP_AGE_DAYS = 30;
        private const int RECOVERY_DELAY_MS = 1000;
        private static readonly Dictionary<RecoveryStrategy, int> StrategyPriority = new Dictionary<RecoveryStrategy, int>
        {
            { RecoveryStrategy.RetryWithDelay, 1 },
            { RecoveryStrategy.RestoreFromBackup, 2 },
            { RecoveryStrategy.RepairSaveFile, 3 },
            { RecoveryStrategy.PartialDataRecovery, 4 },
            { RecoveryStrategy.FallbackToDefault, 5 }
        };

        // Recovery history
        private static Dictionary<string, List<RecoveryResult>> _recoveryHistory = new Dictionary<string, List<RecoveryResult>>();

        // Main recovery method
        public static async Task<SaveData> AttemptRecoveryAsync(string slotName, Exception originalException, LoadStrategy originalStrategy = LoadStrategy.Full)
        {
            var attempt = new RecoveryAttempt
            {
                SlotName = slotName,
                OriginalException = originalException,
                OriginalStrategy = originalStrategy,
                StartTime = DateTime.Now
            };

            OnRecoveryStarted?.Invoke(slotName, attempt);
            OnRecoveryStep?.Invoke(slotName, "Analyzing load failure...");

            try
            {
                // Analyze the failure and determine recovery strategies
                var strategies = AnalyzeFailureAndPlanRecovery(originalException, slotName);
                attempt.PlannedStrategies = strategies;

                if (strategies.Count == 0)
                {
                    throw new RecoveryException("No viable recovery strategies found", originalException);
                }

                OnRecoveryStep?.Invoke(slotName, $"Planned {strategies.Count} recovery strategies");
                OnRecoveryProgress?.Invoke(slotName, 0.1f);

                // Attempt recovery strategies in order of priority
                SaveData recoveredData = null;
                var results = new List<RecoveryResult>();

                for (int i = 0; i < strategies.Count && recoveredData == null; i++)
                {
                    var strategy = strategies[i];
                    float progressBase = 0.1f + (0.8f * i / strategies.Count);
                    float progressRange = 0.8f / strategies.Count;

                    OnRecoveryStep?.Invoke(slotName, $"Attempting strategy: {strategy}");
                    OnRecoveryProgress?.Invoke(slotName, progressBase);

                    var result = await ExecuteRecoveryStrategyAsync(slotName, strategy, originalException, progressBase, progressRange);
                    results.Add(result);
                    attempt.AttemptedStrategies.Add(strategy);

                    if (result.WasSuccessful && result.RecoveredData != null)
                    {
                        recoveredData = result.RecoveredData;
                        attempt.SuccessfulStrategy = strategy;
                        break;
                    }

                    // Wait between attempts to avoid overwhelming the system
                    if (i < strategies.Count - 1)
                    {
                        await Task.Delay(RECOVERY_DELAY_MS);
                    }
                }

                // Complete recovery
                attempt.EndTime = DateTime.Now;
                attempt.Duration = attempt.EndTime - attempt.StartTime;
                attempt.WasSuccessful = recoveredData != null;
                attempt.Results = results;

                // Store recovery history
                StoreRecoveryHistory(slotName, attempt);

                OnRecoveryProgress?.Invoke(slotName, 1.0f);

                if (recoveredData != null)
                {
                    var finalResult = new RecoveryResult
                    {
                        Strategy = attempt.SuccessfulStrategy.Value,
                        WasSuccessful = true,
                        RecoveredData = recoveredData,
                        Message = $"Recovery successful using {attempt.SuccessfulStrategy} strategy"
                    };

                    OnRecoveryCompleted?.Invoke(slotName, finalResult);
                    Debug.Log($"Load recovery successful for {slotName} using {attempt.SuccessfulStrategy} strategy in {attempt.Duration.TotalSeconds:F1}s");
                    
                    return recoveredData;
                }
                else
                {
                    var finalResult = new RecoveryResult
                    {
                        Strategy = RecoveryStrategy.None,
                        WasSuccessful = false,
                        Message = "All recovery strategies failed"
                    };

                    OnRecoveryFailed?.Invoke(slotName, "All recovery strategies exhausted");
                    throw new RecoveryException("All recovery strategies failed", originalException);
                }
            }
            catch (Exception ex)
            {
                attempt.EndTime = DateTime.Now;
                attempt.Duration = attempt.EndTime - attempt.StartTime;
                attempt.WasSuccessful = false;
                attempt.Exception = ex;

                OnRecoveryFailed?.Invoke(slotName, ex.Message);
                Debug.LogError($"Load recovery failed for {slotName}: {ex.Message}");
                
                throw new RecoveryException("Recovery process failed", originalException, ex);
            }
        }

        private static List<RecoveryStrategy> AnalyzeFailureAndPlanRecovery(Exception exception, string slotName)
        {
            var strategies = new List<RecoveryStrategy>();

            // Analyze exception type to determine appropriate strategies
            switch (exception)
            {
                case FileNotFoundException _:
                    strategies.Add(RecoveryStrategy.RestoreFromBackup);
                    strategies.Add(RecoveryStrategy.FallbackToDefault);
                    break;

                case UnauthorizedAccessException _:
                    strategies.Add(RecoveryStrategy.RetryWithDelay);
                    strategies.Add(RecoveryStrategy.RestoreFromBackup);
                    break;

                case ArgumentException _ when exception.Message.Contains("invalid data"):
                case ArgumentException _ when exception.Message.Contains("JSON"):
                case ArgumentException _:
                    strategies.Add(RecoveryStrategy.RepairSaveFile);
                    strategies.Add(RecoveryStrategy.RestoreFromBackup);
                    strategies.Add(RecoveryStrategy.PartialDataRecovery);
                    break;

                case OutOfMemoryException _:
                    strategies.Add(RecoveryStrategy.RetryWithDelay);
                    strategies.Add(RecoveryStrategy.PartialDataRecovery);
                    break;

                case IOException _:
                    strategies.Add(RecoveryStrategy.RetryWithDelay);
                    strategies.Add(RecoveryStrategy.RestoreFromBackup);
                    break;

                default:
                    // Generic recovery for unknown exceptions
                    strategies.Add(RecoveryStrategy.RetryWithDelay);
                    strategies.Add(RecoveryStrategy.RestoreFromBackup);
                    strategies.Add(RecoveryStrategy.RepairSaveFile);
                    break;
            }

            // Check what recovery options are available
            var availableBackups = SaveFileManager.GetAvailableBackups(slotName);
            if (availableBackups.Count == 0)
            {
                strategies.Remove(RecoveryStrategy.RestoreFromBackup);
            }

            // Check if save file exists for repair
            if (!SaveFileManager.SaveSlotExists(slotName))
            {
                strategies.Remove(RecoveryStrategy.RepairSaveFile);
                strategies.Remove(RecoveryStrategy.PartialDataRecovery);
            }

            // Sort strategies by priority
            strategies = strategies.OrderBy(s => StrategyPriority.ContainsKey(s) ? StrategyPriority[s] : 999).ToList();

            return strategies;
        }

        private static async Task<RecoveryResult> ExecuteRecoveryStrategyAsync(string slotName, RecoveryStrategy strategy, Exception originalException, float progressBase, float progressRange)
        {
            var result = new RecoveryResult
            {
                Strategy = strategy,
                StartTime = DateTime.Now
            };

            try
            {
                switch (strategy)
                {
                    case RecoveryStrategy.RetryWithDelay:
                        result.RecoveredData = await RetryWithDelayAsync(slotName, progressBase, progressRange);
                        break;

                    case RecoveryStrategy.RestoreFromBackup:
                        result.RecoveredData = await RestoreFromBackupAsync(slotName, progressBase, progressRange);
                        break;

                    case RecoveryStrategy.RepairSaveFile:
                        result.RecoveredData = await RepairSaveFileAsync(slotName, progressBase, progressRange);
                        break;

                    case RecoveryStrategy.PartialDataRecovery:
                        result.RecoveredData = await PartialDataRecoveryAsync(slotName, progressBase, progressRange);
                        break;

                    case RecoveryStrategy.FallbackToDefault:
                        result.RecoveredData = await FallbackToDefaultAsync(slotName, progressBase, progressRange);
                        break;

                    default:
                        throw new NotSupportedException($"Recovery strategy {strategy} is not implemented");
                }

                result.WasSuccessful = result.RecoveredData != null;
                result.Message = result.WasSuccessful ? $"{strategy} recovery successful" : $"{strategy} recovery failed";
            }
            catch (Exception ex)
            {
                result.WasSuccessful = false;
                result.Exception = ex;
                result.Message = $"{strategy} recovery failed: {ex.Message}";
            }
            finally
            {
                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime - result.StartTime;
            }

            return result;
        }

        private static async Task<SaveData> RetryWithDelayAsync(string slotName, float progressBase, float progressRange)
        {
            OnRecoveryStep?.Invoke(slotName, "Retrying load with delay...");
            
            // Progressive delay strategy
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                await Task.Delay(RECOVERY_DELAY_MS * attempt);
                
                float progress = progressBase + (progressRange * attempt / 3);
                OnRecoveryProgress?.Invoke(slotName, progress);

                try
                {
                    // Force memory cleanup before retry
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    
                    if (Application.isPlaying)
                    {
                        await Resources.UnloadUnusedAssets();
                    }

                    // Attempt standard load
                    return await LogisticGame.Managers.SaveManager.Instance.LoadGameAsync(slotName);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Retry attempt {attempt} failed: {ex.Message}");
                    
                    if (attempt == 3)
                    {
                        throw;
                    }
                }
            }

            return null;
        }

        private static async Task<SaveData> RestoreFromBackupAsync(string slotName, float progressBase, float progressRange)
        {
            OnRecoveryStep?.Invoke(slotName, "Attempting restore from backup...");
            OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.2f);

            var availableBackups = SaveFileManager.GetAvailableBackups(slotName);
            if (availableBackups.Count == 0)
            {
                throw new RecoveryException("No backups available for restoration");
            }

            // Try backups from newest to oldest
            var sortedBackups = availableBackups.OrderByDescending(b => b).ToList();
            
            for (int i = 0; i < sortedBackups.Count && i < 3; i++) // Try up to 3 most recent backups
            {
                var backup = sortedBackups[i];
                float backupProgress = progressBase + progressRange * (0.3f + 0.5f * i / Math.Min(sortedBackups.Count, 3));
                OnRecoveryProgress?.Invoke(slotName, backupProgress);

                try
                {
                    OnRecoveryStep?.Invoke(slotName, $"Restoring from backup: {backup}");
                    
                    bool restored = await SaveFileManager.RestoreFromBackupAsync(slotName, backup);
                    if (!restored)
                    {
                        continue;
                    }

                    // Verify the restored file
                    OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.9f);
                    
                    bool isValid = await SaveFileDiscovery.CheckSaveFileHealthAsync(slotName) != null;
                    if (!isValid)
                    {
                        Debug.LogWarning($"Restored backup {backup} is not valid, trying next backup");
                        continue;
                    }

                    // Load the restored save
                    return await LogisticGame.Managers.SaveManager.Instance.LoadGameAsync(slotName);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to restore from backup {backup}: {ex.Message}");
                }
            }

            throw new RecoveryException("All backup restoration attempts failed");
        }

        private static async Task<SaveData> RepairSaveFileAsync(string slotName, float progressBase, float progressRange)
        {
            OnRecoveryStep?.Invoke(slotName, "Attempting save file repair...");
            OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.2f);

            try
            {
                // Check if SaveFileDiscovery has a repair method
                bool repaired = await SaveFileDiscovery.RepairSaveFileAsync(slotName);
                OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.6f);

                if (!repaired)
                {
                    throw new RecoveryException("Save file repair was not successful");
                }

                OnRecoveryStep?.Invoke(slotName, "Save file repaired, attempting load...");
                OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.8f);

                // Attempt to load the repaired file
                return await LogisticGame.Managers.SaveManager.Instance.LoadGameAsync(slotName);
            }
            catch (Exception ex)
            {
                throw new RecoveryException("Save file repair failed", ex);
            }
        }

        private static async Task<SaveData> PartialDataRecoveryAsync(string slotName, float progressBase, float progressRange)
        {
            OnRecoveryStep?.Invoke(slotName, "Attempting partial data recovery...");
            OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.2f);

            try
            {
                // Try to load raw file content
                string saveContent = await SaveFileManager.LoadFileAsync(slotName);
                OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.4f);

                // Create a new SaveData and try to parse what we can
                SaveData partialSaveData = ScriptableObject.CreateInstance<SaveData>();
                
                // Try to parse core data first
                try
                {
                    JsonUtility.FromJsonOverwrite(saveContent, partialSaveData);
                }
                catch (Exception)
                {
                    // If full parsing fails, try to extract basic information
                    partialSaveData = await ExtractBasicSaveDataAsync(saveContent);
                }

                OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.7f);

                // Sanitize the partial data
                var sanitizedData = SaveValidator.SanitizeSaveData(partialSaveData);
                OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.9f);

                // Validate the sanitized data
                var validation = SaveValidator.ValidateSaveData(sanitizedData);
                if (validation.Severity == ValidationSeverity.Critical)
                {
                    throw new RecoveryException("Partial data recovery produced critically invalid data");
                }

                return sanitizedData;
            }
            catch (Exception ex)
            {
                throw new RecoveryException("Partial data recovery failed", ex);
            }
        }

        private static async Task<SaveData> ExtractBasicSaveDataAsync(string saveContent)
        {
            OnRecoveryStep?.Invoke("unknown", "Extracting basic save data...");
            
            // Create minimal save data with default values
            var basicSaveData = ScriptableObject.CreateInstance<SaveData>();
            
            // Try to extract basic information using string parsing
            try
            {
                // Extract save name
                var saveNameMatch = System.Text.RegularExpressions.Regex.Match(saveContent, @"""_saveName""\s*:\s*""([^""]+)""");
                if (saveNameMatch.Success)
                {
                    basicSaveData.SetField("_saveName", saveNameMatch.Groups[1].Value);
                }

                // Extract version
                var versionMatch = System.Text.RegularExpressions.Regex.Match(saveContent, @"""_saveVersion""\s*:\s*""([^""]+)""");
                if (versionMatch.Success)
                {
                    basicSaveData.SetField("_saveVersion", versionMatch.Groups[1].Value);
                }

                // Initialize with default values
                var gameState = ScriptableObject.CreateInstance<GameState>();
                var playerProgress = ScriptableObject.CreateInstance<PlayerProgress>();
                var settings = ScriptableObject.CreateInstance<SettingsData>();

                basicSaveData.Initialize(
                    basicSaveData.SaveName ?? "Recovered Save",
                    gameState,
                    playerProgress,
                    settings
                );
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to extract basic save data: {ex.Message}");
            }

            await Task.Delay(100); // Simulate processing time
            return basicSaveData;
        }

        private static async Task<SaveData> FallbackToDefaultAsync(string slotName, float progressBase, float progressRange)
        {
            OnRecoveryStep?.Invoke(slotName, "Creating fallback save data...");
            OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.5f);

            // Create a new default save with the same slot name
            var defaultSaveData = ScriptableObject.CreateInstance<SaveData>();
            
            // Create default game state
            var gameState = ScriptableObject.CreateInstance<GameState>();
            var companyData = ScriptableObject.CreateInstance<CompanyData>();
            var settingsData = ScriptableObject.CreateInstance<SettingsData>();
            var playerProgress = ScriptableObject.CreateInstance<PlayerProgress>();

            // Initialize with default values - assuming these methods exist
            // TODO: Implement proper initialization methods if they don't exist
            // companyData.Initialize("Recovered Company", 50000f);
            // gameState.Initialize(companyData, settingsData);
            
            defaultSaveData.Initialize($"Recovered {slotName}", gameState, playerProgress, settingsData);

            OnRecoveryProgress?.Invoke(slotName, progressBase + progressRange * 0.8f);

            // Save the fallback data
            try
            {
                string fallbackJson = JsonUtility.ToJson(defaultSaveData, true);
                await SaveFileManager.SaveFileAtomicAsync(slotName, fallbackJson);
                
                // Create metadata
                var metadata = new SaveMetadata
                {
                    SaveName = defaultSaveData.SaveName,
                    CreationDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    SaveVersion = Application.version ?? "1.0.0",
                    PlayTimeHours = 0f,
                    CurrentCredits = 50000f,
                    TotalContracts = 0,
                    Checksum = SaveValidator.CalculateChecksum(defaultSaveData)
                };

                await SaveFileManager.SaveMetadataAsync(slotName, metadata);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to save fallback data: {ex.Message}");
            }

            await Task.Delay(100);
            return defaultSaveData;
        }

        // History and analysis
        private static void StoreRecoveryHistory(string slotName, RecoveryAttempt attempt)
        {
            if (!_recoveryHistory.ContainsKey(slotName))
            {
                _recoveryHistory[slotName] = new List<RecoveryResult>();
            }

            // Convert attempt to result for history storage
            var historyResult = new RecoveryResult
            {
                Strategy = attempt.SuccessfulStrategy ?? RecoveryStrategy.None,
                WasSuccessful = attempt.WasSuccessful,
                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime,
                Duration = attempt.Duration,
                Message = attempt.WasSuccessful ? 
                    $"Recovery successful using {attempt.SuccessfulStrategy}" : 
                    "Recovery failed",
                RecoveredData = null // Don't store the data in history
            };

            _recoveryHistory[slotName].Add(historyResult);

            // Limit history size
            if (_recoveryHistory[slotName].Count > 10)
            {
                _recoveryHistory[slotName].RemoveAt(0);
            }
        }

        // Public utility methods
        public static List<RecoveryResult> GetRecoveryHistory(string slotName)
        {
            return _recoveryHistory.ContainsKey(slotName) ? 
                new List<RecoveryResult>(_recoveryHistory[slotName]) : 
                new List<RecoveryResult>();
        }

        public static RecoveryAnalysis AnalyzeRecoveryHistory()
        {
            var analysis = new RecoveryAnalysis();
            var allResults = _recoveryHistory.Values.SelectMany(r => r).ToList();

            analysis.TotalRecoveryAttempts = allResults.Count;
            analysis.SuccessfulRecoveries = allResults.Count(r => r.WasSuccessful);
            analysis.FailedRecoveries = analysis.TotalRecoveryAttempts - analysis.SuccessfulRecoveries;

            if (allResults.Count > 0)
            {
                analysis.SuccessRate = (double)analysis.SuccessfulRecoveries / analysis.TotalRecoveryAttempts;
                analysis.AverageRecoveryTime = allResults.Where(r => r.WasSuccessful).Average(r => r.Duration.TotalSeconds);
                
                analysis.MostSuccessfulStrategy = allResults
                    .Where(r => r.WasSuccessful)
                    .GroupBy(r => r.Strategy)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? RecoveryStrategy.None;
            }

            return analysis;
        }

        public static void ClearRecoveryHistory()
        {
            _recoveryHistory.Clear();
        }

        public static bool HasRecoveryHistory(string slotName)
        {
            return _recoveryHistory.ContainsKey(slotName) && _recoveryHistory[slotName].Count > 0;
        }
    }

    // AIDEV-NOTE: Data structures for error recovery
    [System.Serializable]
    public class RecoveryAttempt
    {
        public string SlotName;
        public Exception OriginalException;
        public LoadStrategy OriginalStrategy;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public bool WasSuccessful;
        public List<RecoveryStrategy> PlannedStrategies = new List<RecoveryStrategy>();
        public List<RecoveryStrategy> AttemptedStrategies = new List<RecoveryStrategy>();
        public RecoveryStrategy? SuccessfulStrategy;
        public List<RecoveryResult> Results = new List<RecoveryResult>();
        public Exception Exception;
    }

    [System.Serializable]
    public class RecoveryResult
    {
        public RecoveryStrategy Strategy;
        public bool WasSuccessful;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan Duration;
        public string Message;
        public SaveData RecoveredData;
        public Exception Exception;

        public string GetSummary()
        {
            return $"{Strategy}: {(WasSuccessful ? "SUCCESS" : "FAILED")} in {Duration.TotalSeconds:F1}s";
        }
    }

    [System.Serializable]
    public class RecoveryAnalysis
    {
        public int TotalRecoveryAttempts;
        public int SuccessfulRecoveries;
        public int FailedRecoveries;
        public double SuccessRate;
        public double AverageRecoveryTime;
        public RecoveryStrategy MostSuccessfulStrategy;

        public string GetReport()
        {
            return $"Recovery Analysis: {SuccessfulRecoveries}/{TotalRecoveryAttempts} successful " +
                   $"({SuccessRate:P1}), Average time: {AverageRecoveryTime:F1}s, " +
                   $"Best strategy: {MostSuccessfulStrategy}";
        }
    }

    // AIDEV-NOTE: Enumeration for recovery strategies
    public enum RecoveryStrategy
    {
        None,
        RetryWithDelay,
        RestoreFromBackup,
        RepairSaveFile,
        PartialDataRecovery,
        FallbackToDefault
    }

    // AIDEV-NOTE: Custom exception for recovery failures
    public class RecoveryException : Exception
    {
        public Exception OriginalException { get; }
        public Exception RecoveryException_Inner { get; }

        public RecoveryException(string message) : base(message) { }
        
        public RecoveryException(string message, Exception originalException) : base(message)
        {
            OriginalException = originalException;
        }
        
        public RecoveryException(string message, Exception originalException, Exception recoveryException) : base(message)
        {
            OriginalException = originalException;
            RecoveryException_Inner = recoveryException;
        }
    }
}