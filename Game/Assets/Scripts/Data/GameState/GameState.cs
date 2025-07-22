using UnityEngine;
using System;
using System.Collections.Generic;

// AIDEV-NOTE: ScriptableObject for overall game state and progression tracking
[CreateAssetMenu(fileName = "Game State", menuName = "Logistics Game/Game State")]
public class GameState : ScriptableObject
{
    [Header("Game Information")]
    [SerializeField] private string _saveName;
    [SerializeField] private DateTime _gameStartTime;
    [SerializeField] private DateTime _lastSaveTime;
    [SerializeField] private float _totalPlayTime; // In hours
    
    [Header("Time Management")]
    [SerializeField] private DateTime _currentGameTime;
    [SerializeField, Range(0.1f, 10f)] private float _timeScale = 1f;
    [SerializeField] private bool _isPaused = false;
    
    [Header("Economic State")]
    [SerializeField] private float _currentCredits;
    [SerializeField] private float _totalEarnings;
    [SerializeField] private float _totalExpenses;
    [SerializeField] private float _outstandingLoans;
    
    [Header("Company Information")]
    [SerializeField] private CompanyData _playerCompany;
    [SerializeField] private List<VehicleData> _ownedVehicles = new List<VehicleData>();
    [SerializeField] private List<LicenseType> _ownedLicenses = new List<LicenseType>();
    
    [Header("World State")]
    [SerializeField] private List<CityData> _discoveredCities = new List<CityData>();
    [SerializeField] private List<ContractData> _availableContracts = new List<ContractData>();
    [SerializeField] private List<ContractData> _activeContracts = new List<ContractData>();
    [SerializeField] private List<ContractData> _completedContracts = new List<ContractData>();
    
    [Header("Settings")]
    [SerializeField] private SettingsData _gameSettings;
    
    // Events for state changes
    public static event Action<float> OnCreditsChanged;
    public static event Action<ContractData> OnContractAccepted;
    public static event Action<ContractData> OnContractCompleted;
    public static event Action<VehicleData> OnVehiclePurchased;
    public static event Action<LicenseType> OnLicensePurchased;
    
    // Properties
    public string SaveName => _saveName;
    public DateTime GameStartTime => _gameStartTime;
    public DateTime LastSaveTime => _lastSaveTime;
    public float TotalPlayTime => _totalPlayTime;
    public DateTime CurrentGameTime => _currentGameTime;
    public float TimeScale => _timeScale;
    public bool IsPaused => _isPaused;
    public float CurrentCredits => _currentCredits;
    public float TotalEarnings => _totalEarnings;
    public float TotalExpenses => _totalExpenses;
    public float OutstandingLoans => _outstandingLoans;
    public CompanyData PlayerCompany => _playerCompany;
    public List<VehicleData> OwnedVehicles => _ownedVehicles;
    public List<LicenseType> OwnedLicenses => _ownedLicenses;
    public List<CityData> DiscoveredCities => _discoveredCities;
    public List<ContractData> AvailableContracts => _availableContracts;
    public List<ContractData> ActiveContracts => _activeContracts;
    public List<ContractData> CompletedContracts => _completedContracts;
    public SettingsData GameSettings => _gameSettings;
    
    // Calculated properties
    public float NetWorth => _currentCredits + CalculateVehicleValue() - _outstandingLoans;
    public int TotalContracts => _completedContracts.Count + _activeContracts.Count;
    
    // Initialization
    public void Initialize(CompanyData company, SettingsData settings)
    {
        _playerCompany = company;
        _gameSettings = settings;
        _gameStartTime = DateTime.Now;
        _currentGameTime = _gameStartTime;
        _lastSaveTime = _gameStartTime;
        _totalPlayTime = 0f;
        _timeScale = 1f;
        _isPaused = false;
        
        // Initialize financial state
        _currentCredits = company != null ? company.InitialCredits : 50000f;
        _totalEarnings = 0f;
        _totalExpenses = 0f;
        _outstandingLoans = 0f;
        
        // Initialize collections
        _ownedVehicles = new List<VehicleData>();
        _ownedLicenses = new List<LicenseType> { LicenseType.Standard }; // Start with basic license
        _discoveredCities = new List<CityData>();
        _availableContracts = new List<ContractData>();
        _activeContracts = new List<ContractData>();
        _completedContracts = new List<ContractData>();
        
        if (string.IsNullOrEmpty(_saveName))
        {
            _saveName = $"{company?.CompanyName ?? "New Company"} - {_gameStartTime:yyyy-MM-dd}";
        }
    }
    
    // Time management
    public void UpdateGameTime(float deltaTime)
    {
        if (!_isPaused)
        {
            _totalPlayTime += deltaTime / 3600f; // Convert to hours
            _currentGameTime = _currentGameTime.AddSeconds(deltaTime * _timeScale);
        }
    }
    
    public void SetTimeScale(float newTimeScale)
    {
        _timeScale = Mathf.Clamp(newTimeScale, 0.1f, 10f);
    }
    
    public void SetPaused(bool paused)
    {
        _isPaused = paused;
    }
    
    // Financial operations
    public bool SpendCredits(float amount, string reason = "")
    {
        if (amount <= 0f) return false;
        if (_currentCredits < amount) return false;
        
        _currentCredits -= amount;
        _totalExpenses += amount;
        
        OnCreditsChanged?.Invoke(_currentCredits);
        Debug.Log($"Spent {amount:C} credits for {reason}. Remaining: {_currentCredits:C}");
        return true;
    }
    
    public void AddCredits(float amount, string reason = "")
    {
        if (amount <= 0f) return;
        
        _currentCredits += amount;
        _totalEarnings += amount;
        
        OnCreditsChanged?.Invoke(_currentCredits);
        Debug.Log($"Earned {amount:C} credits from {reason}. Total: {_currentCredits:C}");
    }
    
    // Vehicle management
    public bool PurchaseVehicle(VehicleData vehicle)
    {
        if (vehicle == null) return false;
        if (!SpendCredits(vehicle.PurchasePrice, $"vehicle purchase: {vehicle.VehicleName}"))
            return false;
        
        _ownedVehicles.Add(vehicle);
        OnVehiclePurchased?.Invoke(vehicle);
        return true;
    }
    
    public bool SellVehicle(VehicleData vehicle, float sellPrice)
    {
        if (vehicle == null || !_ownedVehicles.Contains(vehicle)) return false;
        
        _ownedVehicles.Remove(vehicle);
        AddCredits(sellPrice, $"vehicle sale: {vehicle.VehicleName}");
        return true;
    }
    
    public float CalculateVehicleValue()
    {
        float totalValue = 0f;
        foreach (var vehicle in _ownedVehicles)
        {
            // Assume 80% of purchase price as current value
            totalValue += vehicle.PurchasePrice * 0.8f;
        }
        return totalValue;
    }
    
    // License management
    public bool PurchaseLicense(LicenseType licenseType)
    {
        if (_ownedLicenses.Contains(licenseType)) return false;
        
        float cost = _playerCompany?.GetLicenseCost(licenseType) ?? 0f;
        if (!SpendCredits(cost, $"license purchase: {licenseType}"))
            return false;
        
        _ownedLicenses.Add(licenseType);
        _playerCompany?.AddLicense(licenseType);
        OnLicensePurchased?.Invoke(licenseType);
        return true;
    }
    
    // Contract management
    public bool AcceptContract(ContractData contract)
    {
        if (contract == null || _activeContracts.Contains(contract)) return false;
        if (!_availableContracts.Contains(contract)) return false;
        
        // Check if player has required license
        if (!_ownedLicenses.Contains(contract.MinimumLicense)) return false;
        
        _availableContracts.Remove(contract);
        _activeContracts.Add(contract);
        OnContractAccepted?.Invoke(contract);
        return true;
    }
    
    public void CompleteContract(ContractData contract, bool successful)
    {
        if (contract == null || !_activeContracts.Contains(contract)) return;
        
        _activeContracts.Remove(contract);
        _completedContracts.Add(contract);
        
        // Update company reputation
        _playerCompany?.UpdateReputation(successful, contract.CalculateAdjustedReward());
        
        if (successful)
        {
            AddCredits(contract.CalculateAdjustedReward(), $"contract completion: {contract.name}");
        }
        
        OnContractCompleted?.Invoke(contract);
    }
    
    // City discovery
    public void DiscoverCity(CityData city)
    {
        if (city != null && !_discoveredCities.Contains(city))
        {
            _discoveredCities.Add(city);
        }
    }
    
    // Save state
    public void UpdateLastSaveTime()
    {
        _lastSaveTime = DateTime.Now;
    }
    
    // Validation
    private void OnValidate()
    {
        _currentCredits = Mathf.Max(0f, _currentCredits);
        _totalEarnings = Mathf.Max(0f, _totalEarnings);
        _totalExpenses = Mathf.Max(0f, _totalExpenses);
        _outstandingLoans = Mathf.Max(0f, _outstandingLoans);
        _totalPlayTime = Mathf.Max(0f, _totalPlayTime);
        _timeScale = Mathf.Clamp(_timeScale, 0.1f, 10f);
    }
}