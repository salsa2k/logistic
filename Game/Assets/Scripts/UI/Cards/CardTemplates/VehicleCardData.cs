using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AIDEV-NOTE: Card data adapter for VehicleData to display vehicle information in BaseCard.
/// </summary>
public class VehicleCardData : IDetailedCardData, IActionCardData
{
    private readonly VehicleData _vehicleData;
    private readonly bool _isSelected;
    private readonly bool _isDisabled;
    private readonly bool _isOwned;
    private readonly VehicleInstance _vehicleInstance;

    public VehicleCardData(VehicleData vehicleData, bool isSelected = false, bool isDisabled = false, bool isOwned = false, VehicleInstance vehicleInstance = null)
    {
        _vehicleData = vehicleData;
        _isSelected = isSelected;
        _isDisabled = isDisabled;
        _isOwned = isOwned;
        _vehicleInstance = vehicleInstance;
    }

    // ICardData implementation
    public string Id => _vehicleData ? _vehicleData.name : "";
    
    public string Title => _vehicleData?.VehicleName ?? "Unknown Vehicle";
    
    public string Subtitle => GetVehicleSubtitle();
    
    public Sprite Icon => _vehicleData?.VehicleIcon;
    
    public bool IsSelected => _isSelected;
    
    public bool IsDisabled => _isDisabled;
    
    public string BadgeText => GetVehicleBadge();

    // IDetailedCardData implementation
    public string Description => _vehicleData?.Description ?? "No description available";
    
    public Sprite PreviewImage => _vehicleData?.VehicleIcon; // Use icon as preview for vehicles
    
    public Dictionary<string, string> Details => GetVehicleDetails();

    // IActionCardData implementation
    public string PrimaryActionText => GetPrimaryActionText();
    
    public bool IsPrimaryActionEnabled => !_isDisabled && CanPerformPrimaryAction();
    
    public string SecondaryActionText => "View Details";
    
    public bool IsSecondaryActionEnabled => true;

    /// <summary>
    /// Gets the vehicle subtitle based on ownership and key specifications.
    /// AIDEV-NOTE: Shows capacity and status information in a compact format.
    /// </summary>
    private string GetVehicleSubtitle()
    {
        if (_vehicleData == null) return "No data";

        if (_isOwned && _vehicleInstance != null)
        {
            return $"{_vehicleInstance.CurrentFuel:F0}/{_vehicleData.FuelCapacity:F0}L • {GetConditionText()}";
        }

        return $"{_vehicleData.WeightCapacity:F0}kg • {_vehicleData.CargoVolume:F1}m³";
    }

    /// <summary>
    /// Gets the appropriate badge text for the vehicle.
    /// AIDEV-NOTE: Shows ownership status, special capabilities, or alerts.
    /// </summary>
    private string GetVehicleBadge()
    {
        if (_vehicleData == null) return "";

        // Show ownership status
        if (_isOwned)
        {
            if (_vehicleInstance != null)
            {
                // Show vehicle condition or status
                if (_vehicleInstance.RequiresMaintenance)
                    return "REPAIR";
                
                if (_vehicleInstance.CurrentFuel <= (_vehicleData.FuelCapacity * 0.2f))
                    return "LOW FUEL";
                
                if (!_vehicleInstance.IsAvailable)
                    return "IN USE";
            }
            
            return "OWNED";
        }

        // Show special requirements or capabilities
        if (_vehicleData.RequiresSpecialLicense)
        {
            return "SPECIAL";
        }

        // Show if it's a high-capacity vehicle
        if (_vehicleData.WeightCapacity >= 20000f) // 20+ tons
        {
            return "HEAVY";
        }

        // Show if it's fuel-efficient
        if (_vehicleData.FuelConsumption <= 8f) // Low consumption
        {
            return "EFFICIENT";
        }

        return "";
    }

    /// <summary>
    /// Gets detailed information about the vehicle as key-value pairs.
    /// AIDEV-NOTE: Creates structured data for the card details section.
    /// </summary>
    private Dictionary<string, string> GetVehicleDetails()
    {
        var details = new Dictionary<string, string>();

        if (_vehicleData == null) return details;

        // Performance specifications
        details["Max Speed"] = $"{_vehicleData.MaxSpeed:F0} km/h";
        details["Fuel Tank"] = $"{_vehicleData.FuelCapacity:F0}L";
        details["Consumption"] = $"{_vehicleData.FuelConsumption:F1}L/100km";
        details["Range"] = $"{_vehicleData.RangeKm:F0} km";

        // Cargo specifications
        details["Weight Cap."] = $"{_vehicleData.WeightCapacity:F0} kg";
        details["Cargo Volume"] = $"{_vehicleData.CargoVolume:F1} m³";

        // Economic information
        if (!_isOwned)
        {
            details["Price"] = $"${_vehicleData.PurchasePrice:F0}";
        }
        
        details["Maintenance"] = $"${_vehicleData.MaintenanceCost:F0}/1000km";

        // Current status if owned
        if (_isOwned && _vehicleInstance != null)
        {
            details["Current Fuel"] = $"{_vehicleInstance.CurrentFuel:F0}L";
            details["Odometer"] = $"{_vehicleInstance.TotalDistance:F0} km";
            details["Condition"] = GetConditionText();
            
            if (!_vehicleInstance.IsAvailable)
            {
                details["Status"] = "In Use";
            }
            else if (_vehicleInstance.RequiresMaintenance)
            {
                details["Status"] = "Needs Repair";
            }
            else
            {
                details["Status"] = "Available";
            }
        }

        // Special capabilities
        if (_vehicleData.RequiresSpecialLicense)
        {
            details["License"] = "Special Required";
        }

        if (_vehicleData.AuthorizedGoods != null && _vehicleData.AuthorizedGoods.Count > 0)
        {
            details["Authorized For"] = GetAuthorizedGoodsText();
        }

        return details;
    }

    /// <summary>
    /// Gets the primary action text based on vehicle state.
    /// AIDEV-NOTE: Changes based on ownership and availability.
    /// </summary>
    private string GetPrimaryActionText()
    {
        if (_vehicleData == null) return "Unavailable";

        if (_isOwned)
        {
            if (_vehicleInstance != null)
            {
                if (_vehicleInstance.RequiresMaintenance)
                    return "Repair";
                
                if (!_vehicleInstance.IsAvailable)
                    return "View Jobs";
                
                if (_vehicleInstance.CurrentFuel <= (_vehicleData.FuelCapacity * 0.2f))
                    return "Refuel";
                    
                return "Use Vehicle";
            }
            
            return "Manage";
        }

        return "Purchase";
    }

    /// <summary>
    /// Determines if the primary action can be performed.
    /// AIDEV-NOTE: Checks player resources, licenses, and vehicle state.
    /// </summary>
    private bool CanPerformPrimaryAction()
    {
        if (_vehicleData == null) return false;

        if (_isOwned)
        {
            // For owned vehicles, most actions are available unless disabled
            return true;
        }

        // For purchase, check if player can afford it
        // AIDEV-NOTE: GameState doesn't have Instance - would need to be accessed differently
        // For now, assume player can afford it unless explicitly disabled
        // var gameState = FindObjectOfType<GameState>();
        // if (gameState != null)
        // {
        //     // Check if player has enough credits
        //     if (gameState.Credits < _vehicleData.PurchasePrice)
        //         return false;

        //     // Check if player has required license
        //     if (_vehicleData.RequiresSpecialLicense)
        //     {
        //         // AIDEV-TODO: Add specific license checks based on vehicle requirements
        //         // For now, assume special license means Heavy license
        //         if (!gameState.OwnedLicenses.Contains(LicenseType.Heavy))
        //             return false;
        //     }
        // }

        return true;
    }

    /// <summary>
    /// Gets the vehicle's condition as text.
    /// AIDEV-NOTE: Converts numerical condition to readable status.
    /// </summary>
    private string GetConditionText()
    {
        if (_vehicleInstance == null) return "Unknown";

        // Use WearLevel to determine condition (lower wear = better condition)
        float condition = 1f - _vehicleInstance.WearLevel;
        
        return condition switch
        {
            >= 0.9f => "Excellent",
            >= 0.8f => "Good",
            >= 0.6f => "Fair",
            >= 0.4f => "Poor",
            >= 0.2f => "Bad",
            _ => "Critical"
        };
    }

    /// <summary>
    /// Gets a summary of goods this vehicle is authorized to carry.
    /// AIDEV-NOTE: Creates readable list of cargo types.
    /// </summary>
    private string GetAuthorizedGoodsText()
    {
        if (_vehicleData.AuthorizedGoods == null || _vehicleData.AuthorizedGoods.Count == 0)
            return "All Standard";

        if (_vehicleData.AuthorizedGoods.Count == 1)
            return _vehicleData.AuthorizedGoods[0].GoodName;

        if (_vehicleData.AuthorizedGoods.Count <= 3)
        {
            string result = "";
            for (int i = 0; i < _vehicleData.AuthorizedGoods.Count; i++)
            {
                if (i > 0) result += ", ";
                result += _vehicleData.AuthorizedGoods[i].GoodName;
            }
            return result;
        }

        return $"{_vehicleData.AuthorizedGoods[0].GoodName} +{_vehicleData.AuthorizedGoods.Count - 1} more";
    }

    /// <summary>
    /// Calculates the vehicle's efficiency rating.
    /// AIDEV-NOTE: Combines fuel efficiency, capacity, and speed into single metric.
    /// </summary>
    public float GetEfficiencyRating()
    {
        if (_vehicleData == null) return 0f;

        // Calculate efficiency score based on multiple factors
        float fuelScore = Mathf.Clamp01((15f - _vehicleData.FuelConsumption) / 10f); // Lower consumption is better
        float capacityScore = Mathf.Clamp01(_vehicleData.WeightCapacity / 40000f); // Higher capacity is better
        float speedScore = Mathf.Clamp01(_vehicleData.MaxSpeed / 120f); // Higher speed is better
        float rangeScore = Mathf.Clamp01(_vehicleData.RangeKm / 2000f); // Better range is better

        return (fuelScore + capacityScore + speedScore + rangeScore) / 4f;
    }

    /// <summary>
    /// Gets the vehicle's specialization type.
    /// AIDEV-NOTE: Categorizes vehicle based on its capabilities and design.
    /// </summary>
    public string GetSpecializationType()
    {
        if (_vehicleData == null) return "Unknown";

        // Heavy cargo specialist
        if (_vehicleData.WeightCapacity >= 25000f)
            return "Heavy Hauler";

        // High volume specialist  
        if (_vehicleData.CargoVolume >= 50f)
            return "Volume Specialist";

        // Speed specialist
        if (_vehicleData.MaxSpeed >= 100f && _vehicleData.WeightCapacity <= 10000f)
            return "Express Delivery";

        // Fuel efficiency specialist
        if (_vehicleData.FuelConsumption <= 7f)
            return "Eco-Friendly";

        // Long range specialist
        if (_vehicleData.RangeKm >= 1500f)
            return "Long Haul";

        // Special cargo specialist
        if (_vehicleData.AuthorizedGoods != null && _vehicleData.AuthorizedGoods.Count > 0)
            return "Specialized";

        return "General Purpose";
    }

    /// <summary>
    /// Calculates daily operational cost estimate.
    /// AIDEV-NOTE: Helps players understand ongoing vehicle costs.
    /// </summary>
    public float GetDailyOperationalCost()
    {
        if (_vehicleData == null) return 0f;

        // Assume average daily usage of 500km
        float dailyDistance = 500f;
        float fuelCostPerLiter = 1.5f; // Example fuel price
        
        float dailyFuelCost = (_vehicleData.FuelConsumption / 100f) * dailyDistance * fuelCostPerLiter;
        float dailyMaintenanceCost = (_vehicleData.MaintenanceCost / 1000f) * dailyDistance;
        
        return dailyFuelCost + dailyMaintenanceCost;
    }
}