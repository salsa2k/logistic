using UnityEngine;

// AIDEV-NOTE: ScriptableObject for goods/cargo type definitions and properties
[CreateAssetMenu(fileName = "New Good", menuName = "Logistics Game/Good Data")]
public class GoodData : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string _goodName;
    [SerializeField, TextArea(2, 4)] private string _description;
    [SerializeField] private Sprite _goodIcon;
    
    [Header("Physical Properties")]
    [SerializeField, Range(0.1f, 1000f)] private float _weightPerUnit = 1f; // kg per unit
    [SerializeField, Range(0.001f, 10f)] private float _volumePerUnit = 0.1f; // cubic meters per unit
    
    [Header("Economic Properties")]
    [SerializeField, Range(1f, 10000f)] private float _baseValue = 100f; // credits per unit
    [SerializeField, Range(0.5f, 3.0f)] private float _demandMultiplier = 1f; // Price modifier based on demand
    
    [Header("Transport Requirements")]
    [SerializeField] private bool _requiresSpecialVehicle = false;
    [SerializeField] private bool _isPerishable = false;
    [SerializeField] private bool _isFragile = false;
    [SerializeField] private bool _isHazardous = false;
    
    [Header("License Requirements")]
    [SerializeField] private LicenseType _requiredLicense = LicenseType.Standard;
    
    // Properties
    public string GoodName => _goodName;
    public string Description => _description;
    public Sprite GoodIcon => _goodIcon;
    public float WeightPerUnit => _weightPerUnit; // kg
    public float VolumePerUnit => _volumePerUnit; // cubic meters
    public float BaseValue => _baseValue; // credits per unit
    public float DemandMultiplier => _demandMultiplier;
    public bool RequiresSpecialVehicle => _requiresSpecialVehicle;
    public bool IsPerishable => _isPerishable;
    public bool IsFragile => _isFragile;
    public bool IsHazardous => _isHazardous;
    public LicenseType RequiredLicense => _requiredLicense;
    
    // Calculated properties
    public float CurrentValue => _baseValue * _demandMultiplier;
    
    // Validation
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_goodName))
        {
            _goodName = name;
        }
        
        // Ensure realistic values
        _weightPerUnit = Mathf.Max(0.001f, _weightPerUnit);
        _volumePerUnit = Mathf.Max(0.0001f, _volumePerUnit);
        _baseValue = Mathf.Max(0.1f, _baseValue);
        _demandMultiplier = Mathf.Clamp(_demandMultiplier, 0.1f, 10f);
    }
    
    // Helper methods
    public float CalculateTotalWeight(int units)
    {
        return units * _weightPerUnit;
    }
    
    public float CalculateTotalVolume(int units)
    {
        return units * _volumePerUnit;
    }
    
    public float CalculateTotalValue(int units)
    {
        return units * CurrentValue;
    }
    
    public bool IsCompatibleWithVehicle(VehicleData vehicle)
    {
        if (vehicle == null) return false;
        
        // Check if vehicle can handle this type of good
        if (_requiresSpecialVehicle && !vehicle.CanTransport(this))
        {
            return false;
        }
        
        return true;
    }
}

// AIDEV-NOTE: Enum for different license types required for transporting goods
public enum LicenseType
{
    Standard,           // Basic goods, no special license required
    Perishable,         // Refrigerated goods, requires special training
    Fragile,            // Delicate items, requires careful handling certification
    Heavy,              // Oversized/overweight cargo, requires heavy vehicle license
    Hazardous           // Dangerous materials, requires hazmat certification
}