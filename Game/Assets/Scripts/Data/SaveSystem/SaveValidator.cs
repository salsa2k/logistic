using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Handles data validation and integrity checking for save files
    public static class SaveValidator
    {
        // Validation configuration
        private const int MIN_CHECKSUM_LENGTH = 8;
        private const float MIN_CREDITS = 0f;
        private const float MAX_CREDITS = 999999999f;
        private const float MIN_PLAYTIME = 0f;
        private const float MAX_PLAYTIME = 100000f; // ~11 years
        private const int MAX_CONTRACTS = 100000;
        private const int MAX_VEHICLES = 1000;
        private const int MAX_CITIES = 10000;
        
        // Validation events
        public static event Action<string, ValidationResult> OnValidationCompleted;
        public static event Action<string, string> OnValidationError;
        
        // Main validation methods
        public static ValidationResult ValidateSaveData(SaveData saveData)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (saveData == null)
            {
                result.IsValid = false;
                result.Errors.Add("Save data is null");
                return result;
            }
            
            try
            {
                // Core data validation
                ValidateCoreData(saveData, result);
                
                // Financial validation
                ValidateFinancialData(saveData, result);
                
                // Instance data validation
                ValidateInstanceData(saveData, result);
                
                // Cross-reference validation
                ValidateCrossReferences(saveData, result);
                
                // Version and compatibility validation
                ValidateVersionCompatibility(saveData, result);
                
                // Checksum validation
                ValidateChecksum(saveData, result);
                
                // Calculate overall validity
                result.IsValid = result.Errors.Count == 0;
                result.Severity = CalculateValidationSeverity(result);
                
                OnValidationCompleted?.Invoke(saveData.SaveName, result);
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Validation exception: {ex.Message}");
                result.Severity = ValidationSeverity.Critical;
                
                OnValidationError?.Invoke(saveData.SaveName, ex.Message);
            }
            
            return result;
        }
        
        private static void ValidateCoreData(SaveData saveData, ValidationResult result)
        {
            // Basic required fields
            if (string.IsNullOrEmpty(saveData.SaveName))
            {
                result.Errors.Add("Save name is required");
            }
            
            if (string.IsNullOrEmpty(saveData.SaveVersion))
            {
                result.Errors.Add("Save version is required");
            }
            
            if (saveData.CreationDate == default(DateTime))
            {
                result.Errors.Add("Creation date is invalid");
            }
            
            if (saveData.CreationDate > DateTime.Now.AddDays(1))
            {
                result.Warnings.Add("Creation date is in the future");
            }
            
            if (saveData.LastModified < saveData.CreationDate)
            {
                result.Errors.Add("Last modified date cannot be before creation date");
            }
            
            // Game state validation
            if (saveData.GameState == null)
            {
                result.Errors.Add("Game state is required");
            }
            
            if (saveData.PlayerProgress == null)
            {
                result.Warnings.Add("Player progress is missing");
            }
            
            if (saveData.Settings == null)
            {
                result.Warnings.Add("Settings data is missing");
            }
        }
        
        private static void ValidateFinancialData(SaveData saveData, ValidationResult result)
        {
            if (saveData.GameState == null) return;
            
            float credits = saveData.GameState.CurrentCredits;
            
            // Credit bounds validation
            if (credits < MIN_CREDITS)
            {
                result.Errors.Add($"Credits cannot be negative: {credits}");
            }
            
            if (credits > MAX_CREDITS)
            {
                result.Warnings.Add($"Credits are unusually high: {credits:C0}");
            }
            
            // Financial consistency
            float totalEarnings = saveData.GameState.TotalEarnings;
            float totalExpenses = saveData.GameState.TotalExpenses;
            
            if (totalEarnings < 0)
            {
                result.Errors.Add($"Total earnings cannot be negative: {totalEarnings}");
            }
            
            if (totalExpenses < 0)
            {
                result.Errors.Add($"Total expenses cannot be negative: {totalExpenses}");
            }
            
            // Logical financial relationships
            float expectedCredits = totalEarnings - totalExpenses;
            float creditsDifference = Math.Abs(credits - expectedCredits);
            
            if (creditsDifference > 1000f) // Allow some tolerance for rounding
            {
                result.Warnings.Add($"Credits don't match earnings/expenses calculation. Expected: {expectedCredits:C0}, Actual: {credits:C0}");
            }
        }
        
        private static void ValidateInstanceData(SaveData saveData, ValidationResult result)
        {
            // Vehicle instance validation
            ValidateVehicleInstances(saveData.VehicleInstances, result);
            
            // Contract instance validation
            ValidateContractInstances(saveData.ContractInstances, result);
            
            // City instance validation
            ValidateCityInstances(saveData.CityInstances, result);
            
            // Collection size validation
            if (saveData.VehicleInstances.Count > MAX_VEHICLES)
            {
                result.Warnings.Add($"Unusually high vehicle count: {saveData.VehicleInstances.Count}");
            }
            
            if (saveData.ContractInstances.Count > MAX_CONTRACTS)
            {
                result.Warnings.Add($"Unusually high contract count: {saveData.ContractInstances.Count}");
            }
            
            if (saveData.CityInstances.Count > MAX_CITIES)
            {
                result.Warnings.Add($"Unusually high city count: {saveData.CityInstances.Count}");
            }
        }
        
        private static void ValidateVehicleInstances(List<VehicleInstanceData> vehicles, ValidationResult result)
        {
            var vehicleIds = new HashSet<string>();
            
            for (int i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                string prefix = $"Vehicle {i}";
                
                // Required fields
                if (string.IsNullOrEmpty(vehicle.InstanceId))
                {
                    result.Errors.Add($"{prefix}: Instance ID is required");
                    continue;
                }
                
                // Duplicate ID check
                if (vehicleIds.Contains(vehicle.InstanceId))
                {
                    result.Errors.Add($"{prefix}: Duplicate vehicle ID: {vehicle.InstanceId}");
                }
                else
                {
                    vehicleIds.Add(vehicle.InstanceId);
                }
                
                // Vehicle data reference
                if (vehicle.VehicleData == null)
                {
                    result.Errors.Add($"{prefix}: Vehicle data reference is required");
                    continue;
                }
                
                // Fuel validation
                if (vehicle.CurrentFuel < 0)
                {
                    result.Errors.Add($"{prefix}: Fuel cannot be negative: {vehicle.CurrentFuel}");
                }
                
                if (vehicle.CurrentFuel > vehicle.VehicleData.FuelCapacity * 1.1f) // Allow small tolerance
                {
                    result.Errors.Add($"{prefix}: Fuel exceeds capacity: {vehicle.CurrentFuel}/{vehicle.VehicleData.FuelCapacity}");
                }
                
                // Weight validation
                if (vehicle.CurrentWeight < 0)
                {
                    result.Errors.Add($"{prefix}: Weight cannot be negative: {vehicle.CurrentWeight}");
                }
                
                if (vehicle.CurrentWeight > vehicle.VehicleData.WeightCapacity * 1.05f) // Allow small tolerance
                {
                    result.Warnings.Add($"{prefix}: Weight exceeds capacity: {vehicle.CurrentWeight}/{vehicle.VehicleData.WeightCapacity}");
                }
                
                // Wear validation
                if (vehicle.WearLevel < 0 || vehicle.WearLevel > 1)
                {
                    result.Errors.Add($"{prefix}: Wear level must be 0-1: {vehicle.WearLevel}");
                }
                
                // Distance validation
                if (vehicle.TotalDistance < 0)
                {
                    result.Errors.Add($"{prefix}: Total distance cannot be negative: {vehicle.TotalDistance}");
                }
                
                // Financial validation
                if (vehicle.TotalRevenue < 0)
                {
                    result.Errors.Add($"{prefix}: Total revenue cannot be negative: {vehicle.TotalRevenue}");
                }
                
                if (vehicle.TotalExpenses < 0)
                {
                    result.Errors.Add($"{prefix}: Total expenses cannot be negative: {vehicle.TotalExpenses}");
                }
                
                // Date validation
                if (vehicle.PurchaseDate > DateTime.Now.AddDays(1))
                {
                    result.Warnings.Add($"{prefix}: Purchase date is in the future");
                }
                
                if (vehicle.LastMaintenance > DateTime.Now.AddDays(1))
                {
                    result.Warnings.Add($"{prefix}: Last maintenance date is in the future");
                }
            }
        }
        
        private static void ValidateContractInstances(List<ContractInstanceData> contracts, ValidationResult result)
        {
            var contractIds = new HashSet<string>();
            
            for (int i = 0; i < contracts.Count; i++)
            {
                var contract = contracts[i];
                string prefix = $"Contract {i}";
                
                // Required fields
                if (string.IsNullOrEmpty(contract.InstanceId))
                {
                    result.Errors.Add($"{prefix}: Instance ID is required");
                    continue;
                }
                
                // Duplicate ID check
                if (contractIds.Contains(contract.InstanceId))
                {
                    result.Errors.Add($"{prefix}: Duplicate contract ID: {contract.InstanceId}");
                }
                else
                {
                    contractIds.Add(contract.InstanceId);
                }
                
                // Contract data reference
                if (contract.ContractData == null)
                {
                    result.Errors.Add($"{prefix}: Contract data reference is required");
                    continue;
                }
                
                // Date validation
                if (contract.AcceptanceTime > DateTime.Now.AddDays(1))
                {
                    result.Warnings.Add($"{prefix}: Acceptance time is in the future");
                }
                
                if (contract.Deadline < contract.AcceptanceTime)
                {
                    result.Errors.Add($"{prefix}: Deadline cannot be before acceptance time");
                }
                
                // Progress validation
                if (contract.ProgressPercentage < 0 || contract.ProgressPercentage > 100)
                {
                    result.Errors.Add($"{prefix}: Progress percentage must be 0-100: {contract.ProgressPercentage}");
                }
                
                // Financial validation
                if (contract.AgreedReward < 0)
                {
                    result.Errors.Add($"{prefix}: Agreed reward cannot be negative: {contract.AgreedReward}");
                }
                
                if (contract.ActualReward < 0)
                {
                    result.Errors.Add($"{prefix}: Actual reward cannot be negative: {contract.ActualReward}");
                }
                
                if (contract.PenaltyAmount < 0)
                {
                    result.Errors.Add($"{prefix}: Penalty amount cannot be negative: {contract.PenaltyAmount}");
                }
                
                // Status consistency validation
                ValidateContractStatusConsistency(contract, result, prefix);
            }
        }
        
        private static void ValidateContractStatusConsistency(ContractInstanceData contract, ValidationResult result, string prefix)
        {
            switch (contract.Status)
            {
                case ContractStatus.Available:
                    if (!string.IsNullOrEmpty(contract.AssignedVehicleId))
                    {
                        result.Warnings.Add($"{prefix}: Available contract should not have assigned vehicle");
                    }
                    break;
                    
                case ContractStatus.Accepted:
                    if (string.IsNullOrEmpty(contract.AssignedVehicleId))
                    {
                        result.Warnings.Add($"{prefix}: Accepted contract should have assigned vehicle");
                    }
                    break;
                    
                case ContractStatus.Completed:
                    if (contract.ProgressPercentage < 100f)
                    {
                        result.Warnings.Add($"{prefix}: Completed contract should have 100% progress");
                    }
                    if (contract.ActualReward == 0f && contract.AgreedReward > 0f)
                    {
                        result.Warnings.Add($"{prefix}: Completed contract should have actual reward");
                    }
                    break;
                    
                case ContractStatus.Cancelled:
                case ContractStatus.Expired:
                    if (contract.ActualReward > 0f)
                    {
                        result.Warnings.Add($"{prefix}: Cancelled/expired contract should not have actual reward");
                    }
                    break;
            }
        }
        
        private static void ValidateCityInstances(List<CityInstanceData> cities, ValidationResult result)
        {
            var cityIds = new HashSet<string>();
            
            for (int i = 0; i < cities.Count; i++)
            {
                var city = cities[i];
                string prefix = $"City {i}";
                
                // Required fields
                if (string.IsNullOrEmpty(city.InstanceId))
                {
                    result.Errors.Add($"{prefix}: Instance ID is required");
                    continue;
                }
                
                // Duplicate ID check
                if (cityIds.Contains(city.InstanceId))
                {
                    result.Errors.Add($"{prefix}: Duplicate city ID: {city.InstanceId}");
                }
                else
                {
                    cityIds.Add(city.InstanceId);
                }
                
                // City data reference
                if (city.CityData == null)
                {
                    result.Errors.Add($"{prefix}: City data reference is required");
                    continue;
                }
                
                // Economic validation
                if (city.CurrentFuelPrice <= 0)
                {
                    result.Errors.Add($"{prefix}: Fuel price must be positive: {city.CurrentFuelPrice}");
                }
                
                if (city.EconomicMultiplier <= 0)
                {
                    result.Errors.Add($"{prefix}: Economic multiplier must be positive: {city.EconomicMultiplier}");
                }
                
                // Population validation
                if (city.CurrentPopulation < 0)
                {
                    result.Errors.Add($"{prefix}: Population cannot be negative: {city.CurrentPopulation}");
                }
                
                // Reputation validation
                if (city.PlayerReputation < 0)
                {
                    result.Errors.Add($"{prefix}: Player reputation cannot be negative: {city.PlayerReputation}");
                }
                
                // Traffic validation
                if (city.TrafficLevel < 0)
                {
                    result.Errors.Add($"{prefix}: Traffic level cannot be negative: {city.TrafficLevel}");
                }
                
                // Date validation
                if (city.IsDiscovered && city.DiscoveryDate > DateTime.Now.AddDays(1))
                {
                    result.Warnings.Add($"{prefix}: Discovery date is in the future");
                }
            }
        }
        
        private static void ValidateCrossReferences(SaveData saveData, ValidationResult result)
        {
            // Vehicle-Contract cross-references
            var vehicleIds = saveData.VehicleInstances.Select(v => v.InstanceId).ToHashSet();
            var contractIds = saveData.ContractInstances.Select(c => c.InstanceId).ToHashSet();
            
            // Check vehicle-contract assignments
            foreach (var contract in saveData.ContractInstances)
            {
                if (!string.IsNullOrEmpty(contract.AssignedVehicleId) && !vehicleIds.Contains(contract.AssignedVehicleId))
                {
                    result.Errors.Add($"Contract {contract.InstanceId} references non-existent vehicle: {contract.AssignedVehicleId}");
                }
            }
            
            // Check for orphaned assignments
            foreach (var vehicle in saveData.VehicleInstances)
            {
                if (!string.IsNullOrEmpty(vehicle.AssignedContractId) && !contractIds.Contains(vehicle.AssignedContractId))
                {
                    result.Warnings.Add($"Vehicle {vehicle.InstanceId} references non-existent contract: {vehicle.AssignedContractId}");
                }
            }
            
            // City-Vehicle location consistency
            var cityNames = saveData.CityInstances.Where(c => c.CityData != null).Select(c => c.CityData.name).ToHashSet();
            
            foreach (var vehicle in saveData.VehicleInstances)
            {
                if (!string.IsNullOrEmpty(vehicle.CurrentCityName) && !cityNames.Contains(vehicle.CurrentCityName))
                {
                    result.Warnings.Add($"Vehicle {vehicle.InstanceId} is in non-existent city: {vehicle.CurrentCityName}");
                }
                
                if (!string.IsNullOrEmpty(vehicle.DestinationCityName) && !cityNames.Contains(vehicle.DestinationCityName))
                {
                    result.Warnings.Add($"Vehicle {vehicle.InstanceId} has non-existent destination: {vehicle.DestinationCityName}");
                }
            }
        }
        
        private static void ValidateVersionCompatibility(SaveData saveData, ValidationResult result)
        {
            string currentVersion = Application.version ?? "1.0.0";
            string saveVersion = saveData.SaveVersion;
            
            if (string.IsNullOrEmpty(saveVersion))
            {
                result.Warnings.Add("Save version is missing");
                return;
            }
            
            // Simple version comparison (in a real implementation, this would be more sophisticated)
            if (saveVersion != currentVersion)
            {
                result.Warnings.Add($"Save version ({saveVersion}) differs from current version ({currentVersion})");
                
                // Check if migration is required
                if (RequiresMigration(saveVersion, currentVersion))
                {
                    result.RequiresMigration = true;
                    result.Warnings.Add("Save file requires migration to current version");
                }
            }
        }
        
        private static bool RequiresMigration(string fromVersion, string toVersion)
        {
            // Simple version comparison - in reality this would be more complex
            return fromVersion != toVersion;
        }
        
        private static void ValidateChecksum(SaveData saveData, ValidationResult result)
        {
            if (string.IsNullOrEmpty(saveData.Checksum))
            {
                result.Warnings.Add("Save data has no checksum");
                return;
            }
            
            if (saveData.Checksum.Length < MIN_CHECKSUM_LENGTH)
            {
                result.Warnings.Add("Checksum appears to be too short");
            }
            
            // Calculate expected checksum
            string expectedChecksum = CalculateChecksum(saveData);
            
            if (saveData.Checksum != expectedChecksum)
            {
                result.Errors.Add("Checksum validation failed - data may be corrupted");
            }
        }
        
        private static ValidationSeverity CalculateValidationSeverity(ValidationResult result)
        {
            if (result.Errors.Count > 0)
            {
                // Check for critical errors
                bool hasCriticalErrors = result.Errors.Any(error => 
                    error.Contains("null") || 
                    error.Contains("required") || 
                    error.Contains("corrupted") ||
                    error.Contains("checksum"));
                
                return hasCriticalErrors ? ValidationSeverity.Critical : ValidationSeverity.Error;
            }
            
            if (result.Warnings.Count > 5)
                return ValidationSeverity.Warning;
            
            if (result.Warnings.Count > 0)
                return ValidationSeverity.Info;
            
            return ValidationSeverity.Valid;
        }
        
        // Checksum calculation
        public static string CalculateChecksum(SaveData saveData)
        {
            try
            {
                var checksumData = new StringBuilder();
                
                // Include key data points in checksum
                checksumData.Append(saveData.SaveName ?? "");
                checksumData.Append(saveData.SaveVersion ?? "");
                checksumData.Append(saveData.CreationDate.Ticks);
                
                if (saveData.GameState != null)
                {
                    checksumData.Append(saveData.GameState.CurrentCredits);
                    checksumData.Append(saveData.GameState.TotalPlayTime);
                    checksumData.Append(saveData.GameState.TotalContracts);
                }
                
                checksumData.Append(saveData.VehicleInstances.Count);
                checksumData.Append(saveData.ContractInstances.Count);
                checksumData.Append(saveData.CityInstances.Count);
                
                // Generate hash
                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(checksumData.ToString()));
                    return Convert.ToBase64String(hashBytes).Substring(0, 16); // Use first 16 characters
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to calculate checksum: {ex.Message}");
                return "ERROR";
            }
        }
        
        // Data sanitization
        public static SaveData SanitizeSaveData(SaveData saveData)
        {
            if (saveData == null) return null;
            
            try
            {
                // Fix basic data issues
                if (string.IsNullOrEmpty(saveData.SaveName))
                    saveData.SetField("_saveName", $"Save Game {DateTime.Now:yyyy-MM-dd}");
                
                if (string.IsNullOrEmpty(saveData.SaveVersion))
                    saveData.SetField("_saveVersion", Application.version ?? "1.0.0");
                
                // Sanitize financial data
                if (saveData.GameState != null)
                {
                    var gameState = saveData.GameState;
                    if (gameState.CurrentCredits < 0)
                        gameState.SetField("_currentCredits", 0f);
                    
                    if (gameState.TotalPlayTime < 0)
                        gameState.SetField("_totalPlayTime", 0f);
                }
                
                // Sanitize vehicle instances
                for (int i = saveData.VehicleInstances.Count - 1; i >= 0; i--)
                {
                    var vehicle = saveData.VehicleInstances[i];
                    
                    if (string.IsNullOrEmpty(vehicle.InstanceId) || vehicle.VehicleData == null)
                    {
                        saveData.VehicleInstances.RemoveAt(i);
                        continue;
                    }
                    
                    if (vehicle.CurrentFuel < 0)
                        vehicle.CurrentFuel = 0f;
                    
                    if (vehicle.CurrentWeight < 0)
                        vehicle.CurrentWeight = 0f;
                    
                    if (vehicle.WearLevel < 0 || vehicle.WearLevel > 1)
                        vehicle.WearLevel = Mathf.Clamp01(vehicle.WearLevel);
                }
                
                // Sanitize contract instances
                for (int i = saveData.ContractInstances.Count - 1; i >= 0; i--)
                {
                    var contract = saveData.ContractInstances[i];
                    
                    if (string.IsNullOrEmpty(contract.InstanceId) || contract.ContractData == null)
                    {
                        saveData.ContractInstances.RemoveAt(i);
                        continue;
                    }
                    
                    if (contract.ProgressPercentage < 0 || contract.ProgressPercentage > 100)
                        contract.ProgressPercentage = Mathf.Clamp(contract.ProgressPercentage, 0f, 100f);
                }
                
                // Recalculate checksum after sanitization
                string newChecksum = CalculateChecksum(saveData);
                saveData.SetField("_checksum", newChecksum);
                
                return saveData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to sanitize save data: {ex.Message}");
                return saveData;
            }
        }
        
        // Version migration
        public static SaveData MigrateSaveData(SaveData saveData, string targetVersion)
        {
            if (saveData == null) return null;
            
            string currentVersion = saveData.SaveVersion;
            
            try
            {
                // Perform version-specific migrations
                // This would contain specific migration logic for each version
                
                if (currentVersion == "1.0.0" && targetVersion == "1.1.0")
                {
                    // Example migration from 1.0.0 to 1.1.0
                    MigrateFrom1_0_0To1_1_0(saveData);
                }
                
                // Update version
                saveData.SetField("_saveVersion", targetVersion);
                saveData.SetField("_requiresMigration", false);
                
                // Recalculate checksum
                string newChecksum = CalculateChecksum(saveData);
                saveData.SetField("_checksum", newChecksum);
                
                Debug.Log($"Successfully migrated save data from {currentVersion} to {targetVersion}");
                return saveData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to migrate save data from {currentVersion} to {targetVersion}: {ex.Message}");
                return saveData;
            }
        }
        
        private static void MigrateFrom1_0_0To1_1_0(SaveData saveData)
        {
            // Example migration logic
            // Add any new fields with default values
            // Convert old data formats to new formats
            // Remove deprecated fields
        }
    }
    
    // AIDEV-NOTE: Validation result data structure
    [System.Serializable]
    public class ValidationResult
    {
        public bool IsValid = true;
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        public ValidationSeverity Severity = ValidationSeverity.Valid;
        public bool RequiresMigration = false;
        public DateTime ValidationTime = DateTime.Now;
        
        public bool HasErrors => Errors.Count > 0;
        public bool HasWarnings => Warnings.Count > 0;
        public int TotalIssues => Errors.Count + Warnings.Count;
        
        public string GetSummary()
        {
            if (IsValid && !HasWarnings)
                return "Save data is valid";
            
            string summary = "";
            
            if (HasErrors)
                summary += $"{Errors.Count} error(s)";
            
            if (HasWarnings)
            {
                if (!string.IsNullOrEmpty(summary))
                    summary += ", ";
                summary += $"{Warnings.Count} warning(s)";
            }
            
            return summary;
        }
        
        public string GetDetailedReport()
        {
            var report = new StringBuilder();
            
            report.AppendLine($"Validation Report - {ValidationTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Status: {(IsValid ? "VALID" : "INVALID")} ({Severity})");
            report.AppendLine($"Issues: {TotalIssues} total");
            
            if (RequiresMigration)
                report.AppendLine("REQUIRES MIGRATION");
            
            if (HasErrors)
            {
                report.AppendLine();
                report.AppendLine("ERRORS:");
                foreach (var error in Errors)
                    report.AppendLine($"  - {error}");
            }
            
            if (HasWarnings)
            {
                report.AppendLine();
                report.AppendLine("WARNINGS:");
                foreach (var warning in Warnings)
                    report.AppendLine($"  - {warning}");
            }
            
            return report.ToString();
        }
    }
    
    // AIDEV-NOTE: Validation severity levels
    public enum ValidationSeverity
    {
        Valid,      // No issues
        Info,       // Minor informational warnings
        Warning,    // Issues that don't prevent loading
        Error,      // Issues that may cause problems
        Critical    // Issues that prevent loading
    }
}

// AIDEV-NOTE: Extension methods for reflection-based field setting (for sanitization)
public static class SaveDataExtensions
{
    public static void SetField(this object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(obj, value);
    }
}