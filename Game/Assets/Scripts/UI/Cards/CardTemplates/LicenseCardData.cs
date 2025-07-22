using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AIDEV-NOTE: Card data adapter for license information to display in BaseCard.
/// Handles both owned and available licenses for purchase.
/// </summary>
public class LicenseCardData : IDetailedCardData, IActionCardData
{
    private readonly LicenseType _licenseType;
    private readonly bool _isSelected;
    private readonly bool _isDisabled;
    private readonly bool _isOwned;
    private readonly float _purchaseCost;
    private readonly CompanyData _companyData;

    public LicenseCardData(LicenseType licenseType, bool isSelected = false, bool isDisabled = false, bool isOwned = false, float purchaseCost = 0f, CompanyData companyData = null)
    {
        _licenseType = licenseType;
        _isSelected = isSelected;
        _isDisabled = isDisabled;
        _isOwned = isOwned;
        _purchaseCost = purchaseCost;
        _companyData = companyData;
    }

    // ICardData implementation
    public string Id => _licenseType.ToString();
    
    public string Title => GetLicenseTitle();
    
    public string Subtitle => GetLicenseSubtitle();
    
    public Sprite Icon => GetLicenseIcon();
    
    public bool IsSelected => _isSelected;
    
    public bool IsDisabled => _isDisabled;
    
    public string BadgeText => GetLicenseBadge();

    // IDetailedCardData implementation
    public string Description => GetLicenseDescription();
    
    public Sprite PreviewImage => null; // Licenses don't typically have preview images
    
    public Dictionary<string, string> Details => GetLicenseDetails();

    // IActionCardData implementation
    public string PrimaryActionText => GetPrimaryActionText();
    
    public bool IsPrimaryActionEnabled => !_isDisabled && CanPerformPrimaryAction();
    
    public string SecondaryActionText => "Learn More";
    
    public bool IsSecondaryActionEnabled => true;

    /// <summary>
    /// Gets the display title for the license type.
    /// AIDEV-NOTE: Converts enum to readable license names.
    /// </summary>
    private string GetLicenseTitle()
    {
        return _licenseType switch
        {
            LicenseType.Standard => "Standard License",
            LicenseType.Perishable => "Perishable Goods License",
            LicenseType.Fragile => "Fragile Handling License",
            LicenseType.Heavy => "Heavy Vehicle License",
            LicenseType.Hazardous => "Hazardous Materials License",
            _ => "Unknown License"
        };
    }

    /// <summary>
    /// Gets the license subtitle based on ownership and requirements.
    /// AIDEV-NOTE: Shows cost, prerequisites, or current status.
    /// </summary>
    private string GetLicenseSubtitle()
    {
        if (_isOwned)
        {
            return "Licensed & Active";
        }

        if (_purchaseCost > 0)
        {
            return $"${_purchaseCost:F0}";
        }

        // Get cost from company data if available
        if (_companyData != null)
        {
            float cost = _companyData.GetLicenseCost(_licenseType);
            if (cost > 0)
            {
                return $"${cost:F0}";
            }
        }

        return "Available";
    }

    /// <summary>
    /// Gets the appropriate badge text for the license.
    /// AIDEV-NOTE: Shows ownership status, requirements level, or special conditions.
    /// </summary>
    private string GetLicenseBadge()
    {
        if (_isOwned)
        {
            return "OWNED";
        }

        // Show difficulty/requirement level
        return _licenseType switch
        {
            LicenseType.Standard => "", // No badge for basic license
            LicenseType.Perishable => "CERTIFIED",
            LicenseType.Fragile => "SKILLED",
            LicenseType.Heavy => "ADVANCED",
            LicenseType.Hazardous => "EXPERT",
            _ => ""
        };
    }

    /// <summary>
    /// Gets detailed description of what the license enables.
    /// AIDEV-NOTE: Explains the practical benefits and requirements of each license.
    /// </summary>
    private string GetLicenseDescription()
    {
        return _licenseType switch
        {
            LicenseType.Standard => "Basic commercial driving license allowing transport of standard goods. Required for all commercial vehicle operations.",
            
            LicenseType.Perishable => "Specialized certification for transporting temperature-sensitive goods. Requires knowledge of refrigeration systems and cold chain management.",
            
            LicenseType.Fragile => "Professional certification for handling delicate and breakable items. Includes training in proper loading techniques and vibration control.",
            
            LicenseType.Heavy => "Advanced license for operating heavy-duty vehicles and transporting oversized cargo. Requires additional safety training and vehicle handling expertise.",
            
            LicenseType.Hazardous => "Expert certification for transporting dangerous materials. Requires extensive safety training, emergency procedures, and compliance knowledge.",
            
            _ => "License information not available."
        };
    }

    /// <summary>
    /// Gets the license icon based on type.
    /// AIDEV-NOTE: Uses AssetManager to load consistent license icons.
    /// </summary>
    private Sprite GetLicenseIcon()
    {
        // AIDEV-TODO: Integrate with AssetManager to load license icons
        // For now, return null - icons can be assigned through the inspector or AssetManager
        return null;
    }

    /// <summary>
    /// Gets detailed information about the license as key-value pairs.
    /// AIDEV-NOTE: Creates structured data for the card details section.
    /// </summary>
    private Dictionary<string, string> GetLicenseDetails()
    {
        var details = new Dictionary<string, string>();

        // Cost information
        if (!_isOwned)
        {
            float cost = _purchaseCost;
            if (cost <= 0 && _companyData != null)
            {
                cost = _companyData.GetLicenseCost(_licenseType);
            }
            
            if (cost > 0)
            {
                details["Cost"] = $"${cost:F0}";
            }
            else if (_licenseType == LicenseType.Standard)
            {
                details["Cost"] = "Free";
            }
        }

        // Requirements and benefits
        switch (_licenseType)
        {
            case LicenseType.Standard:
                details["Allows"] = "Standard goods transport";
                details["Vehicles"] = "All basic commercial vehicles";
                if (!_isOwned)
                {
                    details["Requirement"] = "None - Basic license";
                }
                break;

            case LicenseType.Perishable:
                details["Allows"] = "Refrigerated goods transport";
                details["Vehicles"] = "Refrigerated trucks";
                details["Training"] = "Cold chain management";
                if (!_isOwned)
                {
                    details["Requirement"] = "Standard license";
                }
                break;

            case LicenseType.Fragile:
                details["Allows"] = "Delicate items transport";
                details["Vehicles"] = "Specialized handling equipment";
                details["Training"] = "Proper loading techniques";
                if (!_isOwned)
                {
                    details["Requirement"] = "Standard license";
                }
                break;

            case LicenseType.Heavy:
                details["Allows"] = "Heavy cargo transport";
                details["Vehicles"] = "Heavy-duty trucks, trailers";
                details["Training"] = "Advanced vehicle operation";
                if (!_isOwned)
                {
                    details["Requirement"] = "2+ years experience";
                }
                break;

            case LicenseType.Hazardous:
                details["Allows"] = "Dangerous materials transport";
                details["Vehicles"] = "Hazmat-certified vehicles";
                details["Training"] = "Safety & emergency procedures";
                if (!_isOwned)
                {
                    details["Requirement"] = "All other licenses";
                }
                break;
        }

        // Additional information
        if (_isOwned)
        {
            details["Status"] = "Active";
            // AIDEV-TODO: Add expiration date if licenses have renewal periods
        }
        else
        {
            details["Duration"] = "Permanent";
            details["Processing"] = "Instant";
        }

        return details;
    }

    /// <summary>
    /// Gets the primary action text based on license state.
    /// AIDEV-NOTE: Changes based on ownership and availability.
    /// </summary>
    private string GetPrimaryActionText()
    {
        if (_isOwned)
        {
            return "View Certificate";
        }

        if (!CanAffordLicense())
        {
            return "Insufficient Credits";
        }

        if (!MeetsPrerequisites())
        {
            return "Prerequisites Required";
        }

        return "Purchase License";
    }

    /// <summary>
    /// Determines if the primary action can be performed.
    /// AIDEV-NOTE: Checks player resources, prerequisites, and license state.
    /// </summary>
    private bool CanPerformPrimaryAction()
    {
        if (_isOwned)
        {
            // Can always view owned licenses
            return true;
        }

        // Check if player can purchase the license
        return CanAffordLicense() && MeetsPrerequisites();
    }

    /// <summary>
    /// Checks if the player can afford this license.
    /// AIDEV-NOTE: Compares license cost with player's current credits.
    /// </summary>
    private bool CanAffordLicense()
    {
        // AIDEV-NOTE: GameState doesn't have Instance - would need to be accessed differently
        // For now, assume player can afford it unless explicitly disabled
        // var gameState = FindObjectOfType<GameState>();
        // if (gameState == null) return false;

        float cost = _purchaseCost;
        if (cost <= 0 && _companyData != null)
        {
            cost = _companyData.GetLicenseCost(_licenseType);
        }

        // For now, just check if cost is reasonable (not free licenses like Standard)
        return cost <= 20000f; // Assume reasonable limit
    }

    /// <summary>
    /// Checks if the player meets the prerequisites for this license.
    /// AIDEV-NOTE: Validates required licenses and experience levels.
    /// </summary>
    private bool MeetsPrerequisites()
    {
        // AIDEV-NOTE: GameState doesn't have Instance - would need to be accessed differently
        // For now, simplified logic based on license type
        // var gameState = FindObjectOfType<GameState>();
        // if (gameState == null) return false;
        // var ownedLicenses = gameState.OwnedLicenses;

        switch (_licenseType)
        {
            case LicenseType.Standard:
                // No prerequisites for standard license
                return true;

            case LicenseType.Perishable:
            case LicenseType.Fragile:
            case LicenseType.Heavy:
                // For testing, assume these are available if not owned
                return !_isOwned;

            case LicenseType.Hazardous:
                // Hardest license - assume more restrictive
                return !_isOwned && _purchaseCost >= 10000f;

            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the difficulty level of obtaining this license.
    /// AIDEV-NOTE: Helps players understand the progression path.
    /// </summary>
    public string GetDifficultyLevel()
    {
        return _licenseType switch
        {
            LicenseType.Standard => "Beginner",
            LicenseType.Perishable => "Intermediate",
            LicenseType.Fragile => "Intermediate", 
            LicenseType.Heavy => "Advanced",
            LicenseType.Hazardous => "Expert",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the estimated earning potential increase from this license.
    /// AIDEV-NOTE: Helps players understand the investment value.
    /// </summary>
    public string GetEarningPotential()
    {
        return _licenseType switch
        {
            LicenseType.Standard => "Base earning potential",
            LicenseType.Perishable => "+25% on refrigerated contracts",
            LicenseType.Fragile => "+20% on delicate cargo contracts",
            LicenseType.Heavy => "+40% on heavy haul contracts",
            LicenseType.Hazardous => "+60% on hazardous material contracts",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the job availability increase from this license.
    /// AIDEV-NOTE: Shows how many more contracts become available.
    /// </summary>
    public string GetJobAvailabilityIncrease()
    {
        return _licenseType switch
        {
            LicenseType.Standard => "All basic transport jobs",
            LicenseType.Perishable => "+15% more available contracts",
            LicenseType.Fragile => "+12% more available contracts",
            LicenseType.Heavy => "+20% more available contracts",
            LicenseType.Hazardous => "+8% more available contracts",
            _ => "Unknown impact"
        };
    }
}