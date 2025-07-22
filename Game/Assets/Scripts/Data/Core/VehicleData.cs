using UnityEngine;
using System.Collections.Generic;

// AIDEV-NOTE: ScriptableObject for vehicle specifications and capabilities
[CreateAssetMenu(fileName = "New Vehicle", menuName = "Logistics Game/Vehicle Data")]
public class VehicleData : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string _vehicleName;
    [SerializeField, TextArea(2, 4)] private string _description;
    [SerializeField] private Sprite _vehicleIcon;
    
    [Header("Performance Specifications")]
    [SerializeField, Range(60f, 120f)] private float _maxSpeed = 90f; // km/h
    [SerializeField, Range(50f, 500f)] private float _fuelCapacity = 100f; // liters
    [SerializeField, Range(5f, 25f)] private float _fuelConsumption = 10f; // liters per 100km
    
    [Header("Cargo Specifications")]
    [SerializeField, Range(500f, 40000f)] private float _weightCapacity = 3500f; // kg
    [SerializeField, Range(5f, 100f)] private float _cargoVolume = 15f; // cubic meters
    
    [Header("Economic Data")]
    [SerializeField, Range(10000f, 500000f)] private float _purchasePrice = 50000f; // credits
    [SerializeField, Range(100f, 5000f)] private float _maintenanceCost = 500f; // credits per 1000km
    
    [Header("License Requirements")]
    [SerializeField] private List<GoodData> _authorizedGoods = new List<GoodData>();
    [SerializeField] private bool _requiresSpecialLicense = false;
    
    // Properties
    public string VehicleName => _vehicleName;
    public string Description => _description;
    public Sprite VehicleIcon => _vehicleIcon;
    public float MaxSpeed => _maxSpeed; // km/h
    public float FuelCapacity => _fuelCapacity; // liters
    public float FuelConsumption => _fuelConsumption; // liters per 100km
    public float WeightCapacity => _weightCapacity; // kg
    public float CargoVolume => _cargoVolume; // cubic meters
    public float PurchasePrice => _purchasePrice; // credits
    public float MaintenanceCost => _maintenanceCost; // credits per 1000km
    public List<GoodData> AuthorizedGoods => _authorizedGoods;
    public bool RequiresSpecialLicense => _requiresSpecialLicense;
    
    // Calculated properties
    public float RangeKm => (_fuelCapacity / _fuelConsumption) * 100f; // Maximum range in km
    
    // Validation
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_vehicleName))
        {
            _vehicleName = name;
        }
        
        // Ensure realistic values
        _maxSpeed = Mathf.Clamp(_maxSpeed, 40f, 150f);
        _fuelCapacity = Mathf.Clamp(_fuelCapacity, 30f, 1000f);
        _fuelConsumption = Mathf.Clamp(_fuelConsumption, 5f, 50f);
        _weightCapacity = Mathf.Clamp(_weightCapacity, 500f, 80000f);
        _cargoVolume = Mathf.Clamp(_cargoVolume, 2f, 200f);
    }
    
    // Helper methods
    public bool CanTransport(GoodData good)
    {
        if (good == null) return false;
        
        // If no specific authorization is required, can transport any good
        if (_authorizedGoods == null || _authorizedGoods.Count == 0)
        {
            return !good.RequiresSpecialVehicle || !_requiresSpecialLicense;
        }
        
        return _authorizedGoods.Contains(good);
    }
    
    public bool CanCarry(float totalWeight, float totalVolume)
    {
        return totalWeight <= _weightCapacity && totalVolume <= _cargoVolume;
    }
    
    public float CalculateFuelCost(float distanceKm, float fuelPricePerLiter)
    {
        float fuelNeeded = (distanceKm / 100f) * _fuelConsumption;
        return fuelNeeded * fuelPricePerLiter;
    }
    
    public float CalculateMaintenanceCost(float distanceKm)
    {
        return (distanceKm / 1000f) * _maintenanceCost;
    }
}