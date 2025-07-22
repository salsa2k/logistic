using UnityEngine;
using System;

// AIDEV-NOTE: ScriptableObject template for contract generation and specifications
[CreateAssetMenu(fileName = "New Contract Template", menuName = "Logistics Game/Contract Data")]
public class ContractData : ScriptableObject
{
    [Header("Route Information")]
    [SerializeField] private CityData _originCity;
    [SerializeField] private CityData _destinationCity;
    
    [Header("Cargo Details")]
    [SerializeField] private GoodData _cargoType;
    [SerializeField, Range(1, 1000)] private int _cargoQuantity = 10;
    
    [Header("Contract Terms")]
    [SerializeField, Range(100f, 100000f)] private float _baseReward = 1000f; // credits
    [SerializeField, Range(1f, 168f)] private float _timeLimit = 24f; // hours
    [SerializeField, Range(0f, 0.5f)] private float _penaltyRate = 0.1f; // penalty per hour late
    
    [Header("Requirements")]
    [SerializeField] private VehicleData _preferredVehicle; // Optional preferred vehicle
    [SerializeField] private bool _requiresInsurance = false;
    [SerializeField] private LicenseType _minimumLicense = LicenseType.Standard;
    
    [Header("Generation Settings")]
    [SerializeField, Range(0.1f, 5.0f)] private float _difficultyMultiplier = 1f;
    [SerializeField] private bool _isRecurring = false;
    [SerializeField, Range(1, 30)] private int _recurringDays = 7; // Days between recurring contracts
    
    // Properties
    public CityData OriginCity => _originCity;
    public CityData DestinationCity => _destinationCity;
    public GoodData CargoType => _cargoType;
    public int CargoQuantity => _cargoQuantity;
    public float BaseReward => _baseReward;
    public float TimeLimit => _timeLimit; // hours
    public float PenaltyRate => _penaltyRate;
    public VehicleData PreferredVehicle => _preferredVehicle;
    public bool RequiresInsurance => _requiresInsurance;
    public LicenseType MinimumLicense => _minimumLicense;
    public float DifficultyMultiplier => _difficultyMultiplier;
    public bool IsRecurring => _isRecurring;
    public int RecurringDays => _recurringDays;
    
    // Calculated properties
    public float TotalCargoWeight => _cargoType != null ? _cargoType.CalculateTotalWeight(_cargoQuantity) : 0f;
    public float TotalCargoVolume => _cargoType != null ? _cargoType.CalculateTotalVolume(_cargoQuantity) : 0f;
    public float TotalCargoValue => _cargoType != null ? _cargoType.CalculateTotalValue(_cargoQuantity) : 0f;
    
    // Validation
    private void OnValidate()
    {
        // Ensure origin and destination are different
        if (_originCity != null && _destinationCity != null && _originCity == _destinationCity)
        {
            Debug.LogWarning($"Contract {name}: Origin and destination cities cannot be the same!");
            _destinationCity = null;
        }
        
        // Ensure realistic values
        _cargoQuantity = Mathf.Max(1, _cargoQuantity);
        _baseReward = Mathf.Max(50f, _baseReward);
        _timeLimit = Mathf.Clamp(_timeLimit, 0.5f, 720f); // 30 minutes to 30 days
        _penaltyRate = Mathf.Clamp(_penaltyRate, 0f, 1f);
        _difficultyMultiplier = Mathf.Clamp(_difficultyMultiplier, 0.1f, 10f);
        
        // Validate cargo availability in origin city
        if (_originCity != null && _cargoType != null && !_originCity.HasGood(_cargoType))
        {
            Debug.LogWarning($"Contract {name}: Origin city {_originCity.CityName} does not have {_cargoType.GoodName} available!");
        }
    }
    
    // Helper methods
    public float CalculateDistance()
    {
        if (_originCity == null || _destinationCity == null) return 0f;
        return Vector2.Distance(_originCity.Position, _destinationCity.Position);
    }
    
    public float CalculateAdjustedReward()
    {
        float distance = CalculateDistance();
        float weightFactor = TotalCargoWeight / 1000f; // Weight in tons
        float difficultyFactor = _difficultyMultiplier;
        
        // Apply city price multipliers
        float originMultiplier = _originCity != null ? _originCity.PriceMultiplier : 1f;
        float destinationMultiplier = _destinationCity != null ? _destinationCity.PriceMultiplier : 1f;
        float cityMultiplier = (originMultiplier + destinationMultiplier) / 2f;
        
        return _baseReward * (1f + distance / 100f) * (1f + weightFactor) * difficultyFactor * cityMultiplier;
    }
    
    public bool IsValidForVehicle(VehicleData vehicle)
    {
        if (vehicle == null) return false;
        
        // Check weight and volume capacity
        if (!vehicle.CanCarry(TotalCargoWeight, TotalCargoVolume))
            return false;
        
        // Check if vehicle can transport this cargo type
        if (_cargoType != null && !vehicle.CanTransport(_cargoType))
            return false;
        
        return true;
    }
    
    public float CalculateEstimatedTime(VehicleData vehicle)
    {
        if (vehicle == null) return float.MaxValue;
        
        float distance = CalculateDistance();
        float averageSpeed = vehicle.MaxSpeed * 0.8f; // Assume 80% of max speed
        
        return distance / averageSpeed; // hours
    }
}