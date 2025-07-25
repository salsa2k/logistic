using UnityEngine;
using System.Collections.Generic;
using System;

// AIDEV-NOTE: ScriptableObject for company/player data and progression
[CreateAssetMenu(fileName = "New Company", menuName = "Logistics Game/Company Data")]
public class CompanyData : ScriptableObject
{
    [Header("Company Information")]
    [SerializeField] private string _companyName;
    [SerializeField, TextArea(2, 4)] private string _companyDescription;
    [SerializeField] private Sprite _companyLogo;
    
    [Header("Financial Data")]
    [SerializeField] private float _initialCredits = 50000f;
    [SerializeField] private float _creditLine = 0f; // Available loan amount
    [SerializeField, Range(0.01f, 0.2f)] private float _loanInterestRate = 0.05f; // Monthly interest rate
    
    [Header("License Information")]
    [SerializeField] private List<LicenseType> _ownedLicenses = new List<LicenseType>();
    [SerializeField] private Dictionary<LicenseType, float> _licenseCosts = new Dictionary<LicenseType, float>();
    
    [Header("Reputation System")]
    [SerializeField, Range(0f, 100f)] private float _reputation = 50f;
    [SerializeField] private int _contractsCompleted = 0;
    [SerializeField] private int _contractsFailed = 0;
    
    [Header("Company Settings")]
    [SerializeField] private DateTime _foundedDate;
    [SerializeField] private bool _isPlayerCompany = true;
    
    // Properties
    public string CompanyName => _companyName;
    public string CompanyDescription => _companyDescription;
    public Sprite CompanyLogo => _companyLogo;
    public float InitialCredits => _initialCredits;
    public float CreditLine => _creditLine;
    public float LoanInterestRate => _loanInterestRate;
    public List<LicenseType> OwnedLicenses => _ownedLicenses;
    public float Reputation => _reputation;
    public int ContractsCompleted => _contractsCompleted;
    public int ContractsFailed => _contractsFailed;
    public DateTime FoundedDate => _foundedDate;
    public bool IsPlayerCompany => _isPlayerCompany;
    
    // Calculated properties
    public float SuccessRate => (_contractsCompleted + _contractsFailed) > 0 ? 
        (float)_contractsCompleted / (_contractsCompleted + _contractsFailed) * 100f : 0f;
    
    // Validation
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_companyName))
        {
            _companyName = "New Logistics Company";
        }
        
        // Ensure realistic values
        _initialCredits = Mathf.Max(0f, _initialCredits);
        _creditLine = Mathf.Max(0f, _creditLine);
        _loanInterestRate = Mathf.Clamp(_loanInterestRate, 0.001f, 1f);
        _reputation = Mathf.Clamp(_reputation, 0f, 100f);
        _contractsCompleted = Mathf.Max(0, _contractsCompleted);
        _contractsFailed = Mathf.Max(0, _contractsFailed);
        
        // Initialize license costs if empty
        InitializeLicenseCosts();
    }
    
    private void InitializeLicenseCosts()
    {
        if (_licenseCosts == null)
        {
            _licenseCosts = new Dictionary<LicenseType, float>();
        }
        
        // Set default license costs if not already set
        var defaultCosts = new Dictionary<LicenseType, float>
        {
            { LicenseType.Standard, 0f },      // Free basic license
            { LicenseType.Perishable, 5000f }, // Refrigerated goods certification
            { LicenseType.Fragile, 3000f },    // Careful handling certification
            { LicenseType.Heavy, 8000f },      // Heavy vehicle license
            { LicenseType.Hazardous, 15000f }  // Hazmat certification
        };
        
        foreach (var licenseType in System.Enum.GetValues(typeof(LicenseType)))
        {
            var license = (LicenseType)licenseType;
            if (!_licenseCosts.ContainsKey(license))
            {
                _licenseCosts[license] = defaultCosts[license];
            }
        }
    }
    
    // Helper methods
    public bool HasLicense(LicenseType licenseType)
    {
        return _ownedLicenses != null && _ownedLicenses.Contains(licenseType);
    }
    
    public bool CanPurchaseLicense(LicenseType licenseType, float currentCredits)
    {
        if (HasLicense(licenseType)) return false;
        
        float cost = GetLicenseCost(licenseType);
        return currentCredits >= cost;
    }
    
    public float GetLicenseCost(LicenseType licenseType)
    {
        InitializeLicenseCosts();
        return _licenseCosts.ContainsKey(licenseType) ? _licenseCosts[licenseType] : 0f;
    }
    
    public void AddLicense(LicenseType licenseType)
    {
        if (_ownedLicenses == null)
        {
            _ownedLicenses = new List<LicenseType>();
        }
        
        if (!HasLicense(licenseType))
        {
            _ownedLicenses.Add(licenseType);
        }
    }
    
    public void UpdateReputation(bool contractSuccessful, float contractValue = 0f)
    {
        if (contractSuccessful)
        {
            _contractsCompleted++;
            // Increase reputation based on contract value and current reputation
            float reputationGain = Mathf.Lerp(0.5f, 2f, contractValue / 10000f);
            _reputation = Mathf.Min(100f, _reputation + reputationGain);
        }
        else
        {
            _contractsFailed++;
            // Decrease reputation
            float reputationLoss = Mathf.Lerp(1f, 5f, (100f - _reputation) / 100f);
            _reputation = Mathf.Max(0f, _reputation - reputationLoss);
        }
    }
    
    public float GetReputationMultiplier()
    {
        // Convert reputation (0-100) to multiplier (0.5x to 1.5x)
        return Mathf.Lerp(0.5f, 1.5f, _reputation / 100f);
    }
    
    public bool CanAcceptContract(ContractData contract)
    {
        if (contract == null) return false;
        
        // Check if company has required license
        return HasLicense(contract.MinimumLicense);
    }
    
    /// <summary>
    /// AIDEV-NOTE: Initializes company data for new company creation.
    /// Used by GameManager when creating a new game from NewGameModal setup.
    /// </summary>
    /// <param name="companyName">Name of the company</param>
    /// <param name="companyLogo">Company logo sprite</param>
    /// <param name="initialCredits">Starting credits amount</param>
    /// <param name="foundedDate">Company foundation date</param>
    public void InitializeNewCompany(string companyName, Sprite companyLogo, float initialCredits, DateTime foundedDate)
    {
        _companyName = companyName;
        _companyLogo = companyLogo;
        _initialCredits = initialCredits;
        _foundedDate = foundedDate;
        
        // Set default values for new company
        _companyDescription = $"A logistics company founded on {foundedDate:MMMM dd, yyyy}.";
        _creditLine = 0f;
        _loanInterestRate = 0.05f;
        _reputation = 50f; // Start with neutral reputation
        _contractsCompleted = 0;
        _contractsFailed = 0;
        _isPlayerCompany = true;
        
        // Initialize with standard license
        _ownedLicenses = new List<LicenseType> { LicenseType.Standard };
        
        // Initialize license costs
        InitializeLicenseCosts();
        
        // Mark as dirty for Unity serialization
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// AIDEV-NOTE: Updates company description after initialization.
    /// </summary>
    /// <param name="description">New company description</param>
    public void SetCompanyDescription(string description)
    {
        _companyDescription = description;
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}