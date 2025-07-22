using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AIDEV-NOTE: Card data adapter for ContractData to display contract information in BaseCard.
/// </summary>
public class ContractCardData : IDetailedCardData, IActionCardData
{
    private readonly ContractData _contractData;
    private readonly bool _isSelected;
    private readonly bool _isDisabled;

    public ContractCardData(ContractData contractData, bool isSelected = false, bool isDisabled = false)
    {
        _contractData = contractData;
        _isSelected = isSelected;
        _isDisabled = isDisabled;
    }

    // ICardData implementation
    public string Id => _contractData ? _contractData.name : "";
    
    public string Title => _contractData && _contractData.OriginCity && _contractData.DestinationCity
        ? $"{_contractData.OriginCity.CityName} → {_contractData.DestinationCity.CityName}"
        : "Unknown Route";
    
    public string Subtitle => _contractData && _contractData.CargoType
        ? $"{_contractData.CargoQuantity}x {_contractData.CargoType.GoodName}"
        : "No Cargo";
    
    public Sprite Icon => _contractData?.CargoType?.GoodIcon;
    
    public bool IsSelected => _isSelected;
    
    public bool IsDisabled => _isDisabled;
    
    public string BadgeText => GetContractBadge();

    // IDetailedCardData implementation
    public string Description => GetContractDescription();
    
    public Sprite PreviewImage => null; // Contracts don't typically have preview images
    
    public Dictionary<string, string> Details => GetContractDetails();

    // IActionCardData implementation
    public string PrimaryActionText => "Accept Contract";
    
    public bool IsPrimaryActionEnabled => !_isDisabled && CanAcceptContract();
    
    public string SecondaryActionText => "View Details";
    
    public bool IsSecondaryActionEnabled => true;

    /// <summary>
    /// Gets the appropriate badge text for the contract.
    /// AIDEV-NOTE: Shows urgency, special requirements, or status indicators.
    /// </summary>
    private string GetContractBadge()
    {
        if (_contractData == null) return "";

        // Check for special license requirements
        if (_contractData.MinimumLicense != LicenseType.Standard)
        {
            return _contractData.MinimumLicense.ToString();
        }

        // Check for urgency based on time limit
        if (_contractData.TimeLimit <= 6f) // Less than 6 hours
        {
            return "URGENT";
        }

        // Check for high value contracts
        float adjustedReward = _contractData.CalculateAdjustedReward();
        if (adjustedReward >= 5000f)
        {
            return "HIGH VALUE";
        }

        return "";
    }

    /// <summary>
    /// Gets a descriptive text for the contract.
    /// AIDEV-NOTE: Provides context about the contract requirements and conditions.
    /// </summary>
    private string GetContractDescription()
    {
        if (_contractData == null) return "No contract data available";

        string description = $"Transport {_contractData.CargoQuantity} units of {_contractData.CargoType?.GoodName ?? "unknown cargo"}";
        
        if (_contractData.RequiresInsurance)
        {
            description += " (Insurance Required)";
        }

        if (_contractData.PreferredVehicle != null)
        {
            description += $" - Preferred: {_contractData.PreferredVehicle.VehicleName}";
        }

        return description;
    }

    /// <summary>
    /// Gets detailed information about the contract as key-value pairs.
    /// AIDEV-NOTE: Creates structured data for the card details section.
    /// </summary>
    private Dictionary<string, string> GetContractDetails()
    {
        var details = new Dictionary<string, string>();

        if (_contractData == null) return details;

        // Basic contract information
        details["Reward"] = $"${_contractData.CalculateAdjustedReward():F0}";
        details["Time Limit"] = $"{_contractData.TimeLimit:F1} hours";
        
        // Cargo information
        details["Weight"] = $"{_contractData.TotalCargoWeight:F1} kg";
        details["Volume"] = $"{_contractData.TotalCargoVolume:F1} m³";
        
        if (_contractData.TotalCargoValue > 0)
        {
            details["Cargo Value"] = $"${_contractData.TotalCargoValue:F0}";
        }

        // Distance and estimated time
        float distance = _contractData.CalculateDistance();
        if (distance > 0)
        {
            details["Distance"] = $"{distance:F0} km";
        }

        // Requirements
        if (_contractData.MinimumLicense != LicenseType.Standard)
        {
            details["License Required"] = _contractData.MinimumLicense.ToString();
        }

        if (_contractData.RequiresInsurance)
        {
            details["Insurance"] = "Required";
        }

        // Penalty information
        if (_contractData.PenaltyRate > 0)
        {
            details["Late Penalty"] = $"{(_contractData.PenaltyRate * 100):F1}% per hour";
        }

        // Difficulty indicator
        if (_contractData.DifficultyMultiplier != 1f)
        {
            string difficultyText = _contractData.DifficultyMultiplier switch
            {
                <= 0.8f => "Easy",
                >= 2f => "Very Hard",
                >= 1.5f => "Hard",
                _ => "Normal"
            };
            details["Difficulty"] = difficultyText;
        }

        return details;
    }

    /// <summary>
    /// Determines if the contract can be accepted based on current game state.
    /// AIDEV-NOTE: Checks player resources, licenses, and vehicle availability.
    /// </summary>
    private bool CanAcceptContract()
    {
        if (_contractData == null) return false;

        // Check if player has required license
        // AIDEV-NOTE: GameState doesn't have Instance - would need to be accessed differently
        // For now, assume player can accept it unless explicitly disabled
        // var gameState = FindObjectOfType<GameState>();
        // if (gameState != null && !gameState.OwnedLicenses.Contains(_contractData.MinimumLicense))
        // {
        //     return false;
        // }

        // AIDEV-TODO: Add additional checks for vehicle availability, credits, etc.
        // This would need to integrate with the game's current state management

        return true;
    }

    /// <summary>
    /// Gets the estimated completion time for this contract with the player's best vehicle.
    /// AIDEV-NOTE: Useful for displaying time estimates in the UI.
    /// </summary>
    public float GetEstimatedCompletionTime()
    {
        if (_contractData == null) return float.MaxValue;

        // AIDEV-TODO: Integrate with player's vehicle inventory to get best available vehicle
        // For now, assume a standard vehicle with average performance
        float distance = _contractData.CalculateDistance();
        float averageSpeed = 80f; // km/h - average truck speed
        
        return distance / averageSpeed; // hours
    }

    /// <summary>
    /// Gets the profitability rating of this contract.
    /// AIDEV-NOTE: Calculates profit per hour ratio for contract comparison.
    /// </summary>
    public float GetProfitabilityRating()
    {
        if (_contractData == null) return 0f;

        float reward = _contractData.CalculateAdjustedReward();
        float estimatedTime = GetEstimatedCompletionTime();
        
        if (estimatedTime <= 0) return 0f;
        
        return reward / estimatedTime; // Credits per hour
    }

    /// <summary>
    /// Gets a risk assessment for this contract.
    /// AIDEV-NOTE: Combines time pressure, cargo value, and requirements into risk level.
    /// </summary>
    public string GetRiskAssessment()
    {
        if (_contractData == null) return "Unknown";

        int riskScore = 0;

        // Time pressure risk
        if (_contractData.TimeLimit <= 4f) riskScore += 3;
        else if (_contractData.TimeLimit <= 8f) riskScore += 2;
        else if (_contractData.TimeLimit <= 16f) riskScore += 1;

        // Cargo value risk
        if (_contractData.TotalCargoValue >= 10000f) riskScore += 2;
        else if (_contractData.TotalCargoValue >= 5000f) riskScore += 1;

        // Special requirements risk
        if (_contractData.MinimumLicense != LicenseType.Standard) riskScore += 1;
        if (_contractData.RequiresInsurance) riskScore += 1;
        if (_contractData.PreferredVehicle != null) riskScore += 1;

        // High penalty risk
        if (_contractData.PenaltyRate >= 0.2f) riskScore += 2;
        else if (_contractData.PenaltyRate >= 0.1f) riskScore += 1;

        return riskScore switch
        {
            <= 2 => "Low",
            <= 4 => "Medium", 
            <= 6 => "High",
            _ => "Very High"
        };
    }
}