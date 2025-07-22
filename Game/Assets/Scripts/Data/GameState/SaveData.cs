using UnityEngine;
using System;
using System.Collections.Generic;

// AIDEV-NOTE: ScriptableObject container for all persistent save data
[CreateAssetMenu(fileName = "Save Data", menuName = "Logistics Game/Save Data")]
public class SaveData : ScriptableObject
{
    [Header("Save Information")]
    [SerializeField] private string _saveName;
    [SerializeField] private string _saveVersion = "1.0.0";
    [SerializeField] private DateTime _creationDate;
    [SerializeField] private DateTime _lastModified;
    [SerializeField] private long _saveFileSize; // bytes
    [SerializeField] private string _saveFilePath;
    
    [Header("Game Data References")]
    [SerializeField] private GameState _gameState;
    [SerializeField] private PlayerProgress _playerProgress;
    [SerializeField] private SettingsData _settings;
    
    [Header("Runtime Instance Data")]
    [SerializeField] private List<VehicleInstanceData> _vehicleInstances = new List<VehicleInstanceData>();
    [SerializeField] private List<ContractInstanceData> _contractInstances = new List<ContractInstanceData>();
    [SerializeField] private List<CityInstanceData> _cityInstances = new List<CityInstanceData>();
    
    [Header("World State Data")]
    [SerializeField] private Dictionary<string, float> _fuelPrices = new Dictionary<string, float>();
    [SerializeField] private Dictionary<string, bool> _discoveryStates = new Dictionary<string, bool>();
    
    [Header("Save Validation")]
    [SerializeField] private string _checksum; // For save file integrity
    [SerializeField] private bool _isCorrupted = false;
    [SerializeField] private bool _requiresMigration = false;
    
    // Properties
    public string SaveName => _saveName;
    public string SaveVersion => _saveVersion;
    public DateTime CreationDate => _creationDate;
    public DateTime LastModified => _lastModified;
    public long SaveFileSize => _saveFileSize;
    public string SaveFilePath => _saveFilePath;
    public GameState GameState => _gameState;
    public PlayerProgress PlayerProgress => _playerProgress;
    public SettingsData Settings => _settings;
    public List<VehicleInstanceData> VehicleInstances => _vehicleInstances;
    public List<ContractInstanceData> ContractInstances => _contractInstances;
    public List<CityInstanceData> CityInstances => _cityInstances;
    public Dictionary<string, float> FuelPrices => _fuelPrices;
    public Dictionary<string, bool> DiscoveryStates => _discoveryStates;
    public string Checksum => _checksum;
    public bool IsCorrupted => _isCorrupted;
    public bool RequiresMigration => _requiresMigration;
    
    // Calculated properties
    public float PlayTimeHours => _gameState != null ? _gameState.TotalPlayTime : 0f;
    public int TotalContracts => _gameState != null ? _gameState.TotalContracts : 0;
    public float CurrentCredits => _gameState != null ? _gameState.CurrentCredits : 0f;
    
    // Events
    public static event Action<SaveData> OnSaveDataLoaded;
    public static event Action<SaveData> OnSaveDataCreated;
    public static event Action<string> OnSaveError;
    
    // Initialization
    public void Initialize(string saveName, GameState gameState, PlayerProgress playerProgress, SettingsData settings)
    {
        _saveName = saveName;
        _creationDate = DateTime.Now;
        _lastModified = _creationDate;
        _saveVersion = Application.version ?? "1.0.0";
        
        _gameState = gameState;
        _playerProgress = playerProgress;
        _settings = settings;
        
        // Initialize collections
        _vehicleInstances = new List<VehicleInstanceData>();
        _contractInstances = new List<ContractInstanceData>();
        _cityInstances = new List<CityInstanceData>();
        _fuelPrices = new Dictionary<string, float>();
        _discoveryStates = new Dictionary<string, bool>();
        
        _isCorrupted = false;
        _requiresMigration = false;
        
        GenerateChecksum();
        OnSaveDataCreated?.Invoke(this);
    }
    
    // Data management
    public void UpdateSaveInfo(string filePath, long fileSize)
    {
        _saveFilePath = filePath;
        _saveFileSize = fileSize;
        _lastModified = DateTime.Now;
    }
    
    public void UpdateLastModified()
    {
        _lastModified = DateTime.Now;
        GenerateChecksum();
    }
    
    // Vehicle instance management
    public void AddVehicleInstance(VehicleInstanceData vehicleInstance)
    {
        if (vehicleInstance != null && !_vehicleInstances.Contains(vehicleInstance))
        {
            _vehicleInstances.Add(vehicleInstance);
            UpdateLastModified();
        }
    }
    
    public void RemoveVehicleInstance(VehicleInstanceData vehicleInstance)
    {
        if (_vehicleInstances.Remove(vehicleInstance))
        {
            UpdateLastModified();
        }
    }
    
    public VehicleInstanceData GetVehicleInstance(string vehicleId)
    {
        return _vehicleInstances.Find(v => v.InstanceId == vehicleId);
    }
    
    // Contract instance management
    public void AddContractInstance(ContractInstanceData contractInstance)
    {
        if (contractInstance != null && !_contractInstances.Contains(contractInstance))
        {
            _contractInstances.Add(contractInstance);
            UpdateLastModified();
        }
    }
    
    public void RemoveContractInstance(ContractInstanceData contractInstance)
    {
        if (_contractInstances.Remove(contractInstance))
        {
            UpdateLastModified();
        }
    }
    
    public ContractInstanceData GetContractInstance(string contractId)
    {
        return _contractInstances.Find(c => c.InstanceId == contractId);
    }
    
    // City instance management
    public void AddCityInstance(CityInstanceData cityInstance)
    {
        if (cityInstance != null && !_cityInstances.Contains(cityInstance))
        {
            _cityInstances.Add(cityInstance);
            UpdateLastModified();
        }
    }
    
    public CityInstanceData GetCityInstance(string cityId)
    {
        return _cityInstances.Find(c => c.CityData != null && c.CityData.name == cityId);
    }
    
    // World state management
    public void SetFuelPrice(string cityName, float price)
    {
        _fuelPrices[cityName] = price;
        UpdateLastModified();
    }
    
    public float GetFuelPrice(string cityName)
    {
        return _fuelPrices.ContainsKey(cityName) ? _fuelPrices[cityName] : 1.5f; // Default fuel price
    }
    
    public void SetDiscoveryState(string entityId, bool discovered)
    {
        _discoveryStates[entityId] = discovered;
        UpdateLastModified();
    }
    
    public bool GetDiscoveryState(string entityId)
    {
        return _discoveryStates.ContainsKey(entityId) && _discoveryStates[entityId];
    }
    
    // Save validation
    private void GenerateChecksum()
    {
        // Generate a simple checksum based on key data
        // In a real implementation, this would be more sophisticated
        string dataString = $"{_saveName}{_saveVersion}{_creationDate}{PlayTimeHours}{CurrentCredits}{TotalContracts}";
        _checksum = dataString.GetHashCode().ToString("X");
    }
    
    public bool ValidateChecksum()
    {
        string currentChecksum = _checksum;
        GenerateChecksum();
        bool isValid = currentChecksum == _checksum;
        
        if (!isValid)
        {
            _isCorrupted = true;
            OnSaveError?.Invoke($"Save file checksum mismatch for {_saveName}");
        }
        
        return isValid;
    }
    
    public bool ValidateData()
    {
        List<string> errors = new List<string>();
        
        // Validate core data
        if (_gameState == null)
            errors.Add("Missing GameState");
        
        if (_playerProgress == null)
            errors.Add("Missing PlayerProgress");
        
        if (_settings == null)
            errors.Add("Missing SettingsData");
        
        // Validate data consistency
        if (_gameState != null && _playerProgress != null)
        {
            if (_gameState.CurrentCredits < 0)
                errors.Add("Negative credits detected");
            
            if (_gameState.TotalPlayTime < 0)
                errors.Add("Negative play time detected");
        }
        
        // Validate vehicle instances
        foreach (var vehicle in _vehicleInstances)
        {
            if (vehicle.CurrentFuel < 0 || vehicle.CurrentFuel > vehicle.VehicleData.FuelCapacity)
                errors.Add($"Invalid fuel level for vehicle {vehicle.InstanceId}");
            
            if (vehicle.CurrentWeight < 0 || vehicle.CurrentWeight > vehicle.VehicleData.WeightCapacity)
                errors.Add($"Invalid weight for vehicle {vehicle.InstanceId}");
        }
        
        // Log errors and mark as corrupted if any found
        if (errors.Count > 0)
        {
            _isCorrupted = true;
            string errorMessage = $"Save data validation failed for {_saveName}:\n{string.Join("\n", errors)}";
            OnSaveError?.Invoke(errorMessage);
            Debug.LogError(errorMessage);
        }
        
        return errors.Count == 0;
    }
    
    // Version migration
    public bool RequiresVersionMigration(string currentVersion)
    {
        // Simple version comparison - in reality this would be more sophisticated
        _requiresMigration = _saveVersion != currentVersion;
        return _requiresMigration;
    }
    
    public void MigrateToVersion(string newVersion)
    {
        string oldVersion = _saveVersion;
        _saveVersion = newVersion;
        
        // Perform migration logic based on version differences
        // This would contain specific migration code for each version
        
        Debug.Log($"Migrated save data from version {oldVersion} to {newVersion}");
        UpdateLastModified();
        _requiresMigration = false;
    }
    
    // Save/Load operations
    public void PrepareForSave()
    {
        // Ensure all data is up to date before saving
        if (_gameState != null)
        {
            _gameState.UpdateLastSaveTime();
        }
        
        // Clean up any invalid references
        _vehicleInstances.RemoveAll(v => v == null || v.VehicleData == null);
        _contractInstances.RemoveAll(c => c == null || c.ContractData == null);
        _cityInstances.RemoveAll(c => c == null || c.CityData == null);
        
        UpdateLastModified();
        GenerateChecksum();
    }
    
    public void OnLoadComplete()
    {
        // Validate data after loading
        if (!ValidateChecksum() || !ValidateData())
        {
            Debug.LogWarning($"Save data validation failed for {_saveName}");
        }
        
        OnSaveDataLoaded?.Invoke(this);
    }
    
    // Helper methods
    public string GetDisplayName()
    {
        if (_gameState?.PlayerCompany != null)
        {
            return $"{_gameState.PlayerCompany.CompanyName} - {_creationDate:MM/dd/yyyy}";
        }
        
        return _saveName ?? "Unnamed Save";
    }
    
    public string GetSaveInfo()
    {
        return $"Save: {GetDisplayName()}\n" +
               $"Created: {_creationDate:yyyy-MM-dd HH:mm}\n" +
               $"Last Modified: {_lastModified:yyyy-MM-dd HH:mm}\n" +
               $"Play Time: {PlayTimeHours:F1} hours\n" +
               $"Credits: {CurrentCredits:C0}\n" +
               $"Contracts: {TotalContracts}\n" +
               $"Version: {_saveVersion}";
    }
    
    public long GetEstimatedSize()
    {
        // Rough estimation of save data size
        long size = 1024; // Base size
        size += _vehicleInstances.Count * 512;
        size += _contractInstances.Count * 256;
        size += _cityInstances.Count * 128;
        size += _fuelPrices.Count * 32;
        size += _discoveryStates.Count * 16;
        
        return size;
    }
    
    // Validation
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_saveName))
        {
            _saveName = $"Save Game {DateTime.Now:yyyy-MM-dd}";
        }
        
        if (string.IsNullOrEmpty(_saveVersion))
        {
            _saveVersion = "1.0.0";
        }
        
        _saveFileSize = System.Math.Max(0L, _saveFileSize);
    }
}

// AIDEV-NOTE: Simplified data structures for runtime instances will be created separately
// These are placeholder references for the actual runtime instance classes