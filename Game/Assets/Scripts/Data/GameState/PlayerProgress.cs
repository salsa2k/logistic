using UnityEngine;
using System;
using System.Collections.Generic;

// AIDEV-NOTE: ScriptableObject for tracking player achievements and progression
[CreateAssetMenu(fileName = "Player Progress", menuName = "Logistics Game/Player Progress")]
public class PlayerProgress : ScriptableObject
{
    [Header("Travel Statistics")]
    [SerializeField] private List<CityData> _visitedCities = new List<CityData>();
    [SerializeField] private float _totalDistanceTraveled; // km
    [SerializeField] private float _totalFuelConsumed; // liters
    [SerializeField] private int _totalTrips;
    
    [Header("Contract Statistics")]
    [SerializeField] private int _contractsCompleted;
    [SerializeField] private int _contractsFailed;
    [SerializeField] private float _totalRevenueEarned; // credits
    [SerializeField] private ContractData _largestContract; // By value
    [SerializeField] private float _longestDistance; // km
    
    [Header("Vehicle Statistics")]
    [SerializeField] private List<VehicleData> _ownedVehicles = new List<VehicleData>();
    [SerializeField] private VehicleData _favoriteVehicle; // Most used
    [SerializeField] private Dictionary<VehicleData, float> _vehicleDistances = new Dictionary<VehicleData, float>();
    [SerializeField] private Dictionary<VehicleData, int> _vehicleTrips = new Dictionary<VehicleData, int>();
    
    [Header("Goods Statistics")]
    [SerializeField] private Dictionary<GoodData, int> _goodsTransported = new Dictionary<GoodData, int>();
    [SerializeField] private Dictionary<GoodData, float> _goodsRevenue = new Dictionary<GoodData, float>();
    [SerializeField] private GoodData _mostTransportedGood;
    [SerializeField] private GoodData _mostProfitableGood;
    
    [Header("Achievements")]
    [SerializeField] private List<Achievement> _unlockedAchievements = new List<Achievement>();
    [SerializeField] private DateTime _firstContractDate;
    [SerializeField] private DateTime _lastPlayDate;
    
    [Header("Milestones")]
    [SerializeField] private bool _visitedAllCities;
    [SerializeField] private bool _ownedAllVehicleTypes;
    [SerializeField] private bool _unlockedAllLicenses;
    [SerializeField] private int _consecutiveSuccessfulContracts;
    [SerializeField] private int _maxConsecutiveSuccessfulContracts;
    
    // Events for progression tracking
    public static event Action<CityData> OnCityVisited;
    public static event Action<Achievement> OnAchievementUnlocked;
    public static event Action<int> OnMilestoneReached;
    
    // Properties
    public List<CityData> VisitedCities => _visitedCities;
    public float TotalDistanceTraveled => _totalDistanceTraveled;
    public float TotalFuelConsumed => _totalFuelConsumed;
    public int TotalTrips => _totalTrips;
    public int ContractsCompleted => _contractsCompleted;
    public int ContractsFailed => _contractsFailed;
    public float TotalRevenueEarned => _totalRevenueEarned;
    public ContractData LargestContract => _largestContract;
    public float LongestDistance => _longestDistance;
    public List<VehicleData> OwnedVehicles => _ownedVehicles;
    public VehicleData FavoriteVehicle => _favoriteVehicle;
    public List<Achievement> UnlockedAchievements => _unlockedAchievements;
    public bool VisitedAllCities => _visitedAllCities;
    public bool OwnedAllVehicleTypes => _ownedAllVehicleTypes;
    public bool UnlockedAllLicenses => _unlockedAllLicenses;
    public int ConsecutiveSuccessfulContracts => _consecutiveSuccessfulContracts;
    public int MaxConsecutiveSuccessfulContracts => _maxConsecutiveSuccessfulContracts;
    
    // Calculated properties
    public float SuccessRate => (_contractsCompleted + _contractsFailed) > 0 ? 
        (float)_contractsCompleted / (_contractsCompleted + _contractsFailed) * 100f : 0f;
    public float AverageDistancePerTrip => _totalTrips > 0 ? _totalDistanceTraveled / _totalTrips : 0f;
    public float AverageRevenuePerContract => _contractsCompleted > 0 ? _totalRevenueEarned / _contractsCompleted : 0f;
    
    // Initialization
    public void Initialize()
    {
        _visitedCities = new List<CityData>();
        _ownedVehicles = new List<VehicleData>();
        _unlockedAchievements = new List<Achievement>();
        _vehicleDistances = new Dictionary<VehicleData, float>();
        _vehicleTrips = new Dictionary<VehicleData, int>();
        _goodsTransported = new Dictionary<GoodData, int>();
        _goodsRevenue = new Dictionary<GoodData, float>();
        
        _totalDistanceTraveled = 0f;
        _totalFuelConsumed = 0f;
        _totalTrips = 0;
        _contractsCompleted = 0;
        _contractsFailed = 0;
        _totalRevenueEarned = 0f;
        _longestDistance = 0f;
        _consecutiveSuccessfulContracts = 0;
        _maxConsecutiveSuccessfulContracts = 0;
        
        _firstContractDate = DateTime.MinValue;
        _lastPlayDate = DateTime.Now;
    }
    
    // Travel tracking
    public void RecordTrip(CityData fromCity, CityData toCity, VehicleData vehicle, float distance, float fuelUsed)
    {
        // Update basic statistics
        _totalDistanceTraveled += distance;
        _totalFuelConsumed += fuelUsed;
        _totalTrips++;
        
        // Track longest distance
        if (distance > _longestDistance)
        {
            _longestDistance = distance;
        }
        
        // Update city visits
        if (toCity != null && !_visitedCities.Contains(toCity))
        {
            _visitedCities.Add(toCity);
            OnCityVisited?.Invoke(toCity);
            CheckCityMilestones();
        }
        
        // Update vehicle statistics
        if (vehicle != null)
        {
            if (!_vehicleDistances.ContainsKey(vehicle))
            {
                _vehicleDistances[vehicle] = 0f;
                _vehicleTrips[vehicle] = 0;
            }
            
            _vehicleDistances[vehicle] += distance;
            _vehicleTrips[vehicle]++;
            
            UpdateFavoriteVehicle();
        }
        
        _lastPlayDate = DateTime.Now;
    }
    
    // Contract tracking
    public void RecordContractCompletion(ContractData contract, bool successful, float revenue = 0f)
    {
        if (contract == null) return;
        
        if (successful)
        {
            _contractsCompleted++;
            _consecutiveSuccessfulContracts++;
            _totalRevenueEarned += revenue;
            
            // Track largest contract
            if (_largestContract == null || revenue > _largestContract.CalculateAdjustedReward())
            {
                _largestContract = contract;
            }
            
            // Update max consecutive successful contracts
            if (_consecutiveSuccessfulContracts > _maxConsecutiveSuccessfulContracts)
            {
                _maxConsecutiveSuccessfulContracts = _consecutiveSuccessfulContracts;
            }
            
            // Track goods statistics
            if (contract.CargoType != null)
            {
                if (!_goodsTransported.ContainsKey(contract.CargoType))
                {
                    _goodsTransported[contract.CargoType] = 0;
                    _goodsRevenue[contract.CargoType] = 0f;
                }
                
                _goodsTransported[contract.CargoType] += contract.CargoQuantity;
                _goodsRevenue[contract.CargoType] += revenue;
                
                UpdateGoodsStatistics();
            }
        }
        else
        {
            _contractsFailed++;
            _consecutiveSuccessfulContracts = 0;
        }
        
        // Set first contract date if this is the first contract
        if (_firstContractDate == DateTime.MinValue)
        {
            _firstContractDate = DateTime.Now;
        }
        
        _lastPlayDate = DateTime.Now;
        CheckAchievements();
    }
    
    // Vehicle management
    public void AddVehicle(VehicleData vehicle)
    {
        if (vehicle != null && !_ownedVehicles.Contains(vehicle))
        {
            _ownedVehicles.Add(vehicle);
            CheckVehicleMilestones();
        }
    }
    
    public void RemoveVehicle(VehicleData vehicle)
    {
        if (vehicle != null)
        {
            _ownedVehicles.Remove(vehicle);
            _vehicleDistances.Remove(vehicle);
            _vehicleTrips.Remove(vehicle);
            UpdateFavoriteVehicle();
        }
    }
    
    private void UpdateFavoriteVehicle()
    {
        VehicleData mostUsed = null;
        float maxDistance = 0f;
        
        foreach (var kvp in _vehicleDistances)
        {
            if (kvp.Value > maxDistance)
            {
                maxDistance = kvp.Value;
                mostUsed = kvp.Key;
            }
        }
        
        _favoriteVehicle = mostUsed;
    }
    
    private void UpdateGoodsStatistics()
    {
        // Find most transported good by quantity
        GoodData mostTransported = null;
        int maxQuantity = 0;
        
        foreach (var kvp in _goodsTransported)
        {
            if (kvp.Value > maxQuantity)
            {
                maxQuantity = kvp.Value;
                mostTransported = kvp.Key;
            }
        }
        _mostTransportedGood = mostTransported;
        
        // Find most profitable good by revenue
        GoodData mostProfitable = null;
        float maxRevenue = 0f;
        
        foreach (var kvp in _goodsRevenue)
        {
            if (kvp.Value > maxRevenue)
            {
                maxRevenue = kvp.Value;
                mostProfitable = kvp.Key;
            }
        }
        _mostProfitableGood = mostProfitable;
    }
    
    // Achievement system
    public void UnlockAchievement(Achievement achievement)
    {
        if (achievement != null && !_unlockedAchievements.Contains(achievement))
        {
            _unlockedAchievements.Add(achievement);
            OnAchievementUnlocked?.Invoke(achievement);
        }
    }
    
    private void CheckAchievements()
    {
        // Check for various achievements based on statistics
        // This would be expanded with actual achievement logic
        
        // Example: First contract achievement
        if (_contractsCompleted == 1 && !HasAchievement("First Delivery"))
        {
            // Would unlock "First Delivery" achievement
        }
        
        // Example: Distance milestone
        if (_totalDistanceTraveled >= 10000f && !HasAchievement("Long Hauler"))
        {
            // Would unlock "Long Hauler" achievement
        }
    }
    
    private void CheckCityMilestones()
    {
        // This would check against a list of all available cities
        // For now, assume there are 10 cities total
        if (_visitedCities.Count >= 10)
        {
            _visitedAllCities = true;
            OnMilestoneReached?.Invoke(1); // City explorer milestone
        }
    }
    
    private void CheckVehicleMilestones()
    {
        // This would check against all available vehicle types
        // Implementation would depend on actual vehicle type enum/system
    }
    
    public bool HasAchievement(string achievementName)
    {
        return _unlockedAchievements.Exists(a => a.Name == achievementName);
    }
    
    // Statistics helpers
    public float GetVehicleDistance(VehicleData vehicle)
    {
        return _vehicleDistances.ContainsKey(vehicle) ? _vehicleDistances[vehicle] : 0f;
    }
    
    public int GetVehicleTrips(VehicleData vehicle)
    {
        return _vehicleTrips.ContainsKey(vehicle) ? _vehicleTrips[vehicle] : 0;
    }
    
    public int GetGoodsQuantity(GoodData good)
    {
        return _goodsTransported.ContainsKey(good) ? _goodsTransported[good] : 0;
    }
    
    public float GetGoodsRevenue(GoodData good)
    {
        return _goodsRevenue.ContainsKey(good) ? _goodsRevenue[good] : 0f;
    }
    
    // Validation
    private void OnValidate()
    {
        _totalDistanceTraveled = Mathf.Max(0f, _totalDistanceTraveled);
        _totalFuelConsumed = Mathf.Max(0f, _totalFuelConsumed);
        _totalTrips = Mathf.Max(0, _totalTrips);
        _contractsCompleted = Mathf.Max(0, _contractsCompleted);
        _contractsFailed = Mathf.Max(0, _contractsFailed);
        _totalRevenueEarned = Mathf.Max(0f, _totalRevenueEarned);
        _longestDistance = Mathf.Max(0f, _longestDistance);
        _consecutiveSuccessfulContracts = Mathf.Max(0, _consecutiveSuccessfulContracts);
        _maxConsecutiveSuccessfulContracts = Mathf.Max(0, _maxConsecutiveSuccessfulContracts);
    }
}

// AIDEV-NOTE: Simple achievement data structure
[System.Serializable]
public class Achievement
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private DateTime _unlockedDate;
    [SerializeField] private bool _isSecret;
    
    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;
    public DateTime UnlockedDate => _unlockedDate;
    public bool IsSecret => _isSecret;
    
    public Achievement(string name, string description, Sprite icon = null, bool isSecret = false)
    {
        _name = name;
        _description = description;
        _icon = icon;
        _isSecret = isSecret;
        _unlockedDate = DateTime.Now;
    }
}