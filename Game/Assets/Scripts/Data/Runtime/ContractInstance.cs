using UnityEngine;
using System;
using System.Collections.Generic;

// AIDEV-NOTE: Runtime instance data for contracts with progress tracking
[System.Serializable]
public class ContractInstance
{
    [Header("Identity")]
    [SerializeField] private string _instanceId;
    [SerializeField] private ContractData _contractData;
    [SerializeField] private DateTime _generationTime;
    [SerializeField] private string _contractTitle;
    
    [Header("Contract Status")]
    [SerializeField] private ContractStatus _status = ContractStatus.Available;
    [SerializeField] private DateTime _acceptanceTime;
    [SerializeField] private DateTime _completionTime;
    [SerializeField] private DateTime _expirationTime;
    
    [Header("Assignment")]
    [SerializeField] private VehicleInstance _assignedVehicle;
    [SerializeField] private string _assignedVehicleId; // For serialization
    [SerializeField] private bool _isExpedited = false; // Rush delivery bonus
    
    [Header("Progress Tracking")]
    [SerializeField] private bool _cargoLoaded = false;
    [SerializeField] private DateTime _loadingTime;
    [SerializeField] private DateTime _departureTime;
    [SerializeField] private float _deliveryProgress = 0f; // 0-1
    [SerializeField] private List<string> _progressMilestones = new List<string>();
    
    [Header("Financial Information")]
    [SerializeField] private float _originalReward;
    [SerializeField] private float _currentReward;
    [SerializeField] private float _penaltyAmount = 0f;
    [SerializeField] private float _bonusAmount = 0f;
    [SerializeField] private bool _insuranceClaimed = false;
    
    [Header("Performance Metrics")]
    [SerializeField] private float _expectedDuration; // hours
    [SerializeField] private float _actualDuration; // hours
    [SerializeField] private float _fuelEfficiency; // km per liter
    [SerializeField] private bool _onTimeDelivery = false;
    [SerializeField] private float _customerSatisfaction = 1f; // 0-1
    
    // Events for contract state changes
    public static event Action<ContractInstance, ContractStatus> OnStatusChanged;
    public static event Action<ContractInstance, VehicleInstance> OnVehicleAssigned;
    public static event Action<ContractInstance> OnContractCompleted;
    public static event Action<ContractInstance> OnContractExpired;
    
    // Properties
    public string InstanceId => _instanceId;
    public ContractData ContractData => _contractData;
    public DateTime GenerationTime => _generationTime;
    public string ContractTitle => _contractTitle;
    public ContractStatus Status => _status;
    public DateTime AcceptanceTime => _acceptanceTime;
    public DateTime CompletionTime => _completionTime;
    public DateTime ExpirationTime => _expirationTime;
    public VehicleInstance AssignedVehicle => _assignedVehicle;
    public string AssignedVehicleId => _assignedVehicleId;
    public bool IsExpedited => _isExpedited;
    public bool CargoLoaded => _cargoLoaded;
    public DateTime LoadingTime => _loadingTime;
    public DateTime DepartureTime => _departureTime;
    public float DeliveryProgress => _deliveryProgress;
    public List<string> ProgressMilestones => _progressMilestones;
    public float OriginalReward => _originalReward;
    public float CurrentReward => _currentReward;
    public float PenaltyAmount => _penaltyAmount;
    public float BonusAmount => _bonusAmount;
    public bool InsuranceClaimed => _insuranceClaimed;
    public float ExpectedDuration => _expectedDuration;
    public float ActualDuration => _actualDuration;
    public float FuelEfficiency => _fuelEfficiency;
    public bool OnTimeDelivery => _onTimeDelivery;
    public float CustomerSatisfaction => _customerSatisfaction;
    
    // Calculated properties
    public bool IsExpired => DateTime.Now > _expirationTime && _status != ContractStatus.Completed;
    public bool IsLate => DateTime.Now > _expirationTime && _status == ContractStatus.InProgress;
    public TimeSpan TimeRemaining => _expirationTime - DateTime.Now;
    public TimeSpan TimeSinceAcceptance => DateTime.Now - _acceptanceTime;
    public float CompletionPercentage => _status == ContractStatus.Completed ? 100f : _deliveryProgress * 100f;
    public float TotalDistance => _contractData?.CalculateDistance() ?? 0f;
    public float TotalCargoWeight => _contractData?.TotalCargoWeight ?? 0f;
    public float TotalCargoVolume => _contractData?.TotalCargoVolume ?? 0f;
    public bool RequiresSpecialHandling => _contractData?.CargoType?.RequiresSpecialVehicle ?? false;
    
    // Constructor
    public ContractInstance(ContractData contractData)
    {
        _instanceId = System.Guid.NewGuid().ToString();
        _contractData = contractData;
        _generationTime = DateTime.Now;
        _contractTitle = GenerateContractTitle();
        
        // Set expiration time
        _expirationTime = _generationTime.AddHours(contractData?.TimeLimit ?? 24f);
        
        // Initialize status
        _status = ContractStatus.Available;
        
        // Initialize financial data
        _originalReward = contractData?.CalculateAdjustedReward() ?? 0f;
        _currentReward = _originalReward;
        
        // Initialize progress tracking
        _cargoLoaded = false;
        _deliveryProgress = 0f;
        _progressMilestones = new List<string>();
        
        // Initialize performance metrics
        _expectedDuration = contractData?.CalculateEstimatedTime(null) ?? 0f;
        _customerSatisfaction = 1f;
        
        AddProgressMilestone("Contract generated");
    }
    
    // Contract lifecycle methods
    public bool AcceptContract(VehicleInstance vehicle)
    {
        if (_status != ContractStatus.Available || vehicle == null) return false;
        if (IsExpired) return false;
        
        // Check if vehicle can handle this contract
        if (!vehicle.CanAcceptContract(this)) return false;
        
        _status = ContractStatus.Accepted;
        _acceptanceTime = DateTime.Now;
        AssignVehicle(vehicle);
        
        AddProgressMilestone($"Contract accepted by {vehicle.CustomName}");
        OnStatusChanged?.Invoke(this, _status);
        
        return true;
    }
    
    public void StartDelivery()
    {
        if (_status != ContractStatus.Accepted) return;
        
        _status = ContractStatus.InProgress;
        _departureTime = DateTime.Now;
        
        // Recalculate expected duration based on assigned vehicle
        if (_assignedVehicle != null)
        {
            _expectedDuration = _contractData.CalculateEstimatedTime(_assignedVehicle.VehicleData);
        }
        
        AddProgressMilestone("Delivery started");
        OnStatusChanged?.Invoke(this, _status);
    }
    
    public void LoadCargo()
    {
        if (_status != ContractStatus.Accepted && _status != ContractStatus.InProgress) return;
        
        _cargoLoaded = true;
        _loadingTime = DateTime.Now;
        
        // Create cargo item for the vehicle
        if (_assignedVehicle != null && _contractData != null)
        {
            var cargoItem = new CargoItem(
                _contractData.CargoType,
                _contractData.CargoQuantity,
                _contractData.OriginCity.CityName,
                _contractData.DestinationCity.CityName
            );
            
            _assignedVehicle.LoadCargo(cargoItem);
        }
        
        AddProgressMilestone("Cargo loaded");
    }
    
    public void UpdateProgress(float progress)
    {
        if (_status != ContractStatus.InProgress) return;
        
        float previousProgress = _deliveryProgress;
        _deliveryProgress = Mathf.Clamp01(progress);
        
        // Check for milestone achievements
        if (previousProgress < 0.25f && _deliveryProgress >= 0.25f)
        {
            AddProgressMilestone("25% of journey completed");
        }
        else if (previousProgress < 0.5f && _deliveryProgress >= 0.5f)
        {
            AddProgressMilestone("Halfway point reached");
        }
        else if (previousProgress < 0.75f && _deliveryProgress >= 0.75f)
        {
            AddProgressMilestone("75% of journey completed");
        }
        
        // Check if delivery is complete
        if (_deliveryProgress >= 1f && _assignedVehicle?.CurrentCity == _contractData?.DestinationCity)
        {
            CompleteDelivery();
        }
    }
    
    public void CompleteDelivery()
    {
        if (_status != ContractStatus.InProgress) return;
        
        _status = ContractStatus.Completed;
        _completionTime = DateTime.Now;
        _actualDuration = (float)(_completionTime - _acceptanceTime).TotalHours;
        _onTimeDelivery = _completionTime <= _expirationTime;
        
        // Calculate final reward with bonuses/penalties
        CalculateFinalReward();
        
        // Update customer satisfaction
        CalculateCustomerSatisfaction();
        
        // Unload cargo from vehicle
        if (_assignedVehicle != null)
        {
            _assignedVehicle.UnloadAllCargo();
            _assignedVehicle.AssignContract(null);
        }
        
        AddProgressMilestone($"Delivery completed {(_onTimeDelivery ? "on time" : "late")}");
        OnContractCompleted?.Invoke(this);
        OnStatusChanged?.Invoke(this, _status);
    }
    
    public void CancelContract(string reason = "")
    {
        if (_status == ContractStatus.Completed || _status == ContractStatus.Cancelled) return;
        
        _status = ContractStatus.Cancelled;
        _completionTime = DateTime.Now;
        
        // Apply cancellation penalty
        _penaltyAmount = _originalReward * 0.2f; // 20% penalty
        _currentReward = 0f;
        
        // Unload cargo and release vehicle
        if (_assignedVehicle != null)
        {
            _assignedVehicle.UnloadAllCargo();
            _assignedVehicle.AssignContract(null);
        }
        
        AddProgressMilestone($"Contract cancelled: {reason}");
        OnStatusChanged?.Invoke(this, _status);
    }
    
    public void ExpireContract()
    {
        if (_status == ContractStatus.Completed || _status == ContractStatus.Cancelled) return;
        
        _status = ContractStatus.Expired;
        _completionTime = DateTime.Now;
        _currentReward = 0f;
        
        // Release vehicle if assigned
        if (_assignedVehicle != null)
        {
            _assignedVehicle.UnloadAllCargo();
            _assignedVehicle.AssignContract(null);
        }
        
        AddProgressMilestone("Contract expired");
        OnContractExpired?.Invoke(this);
        OnStatusChanged?.Invoke(this, _status);
    }
    
    // Vehicle assignment
    private void AssignVehicle(VehicleInstance vehicle)
    {
        _assignedVehicle = vehicle;
        _assignedVehicleId = vehicle?.InstanceId;
        
        if (vehicle != null)
        {
            vehicle.AssignContract(this);
            OnVehicleAssigned?.Invoke(this, vehicle);
        }
    }
    
    public void UnassignVehicle()
    {
        if (_assignedVehicle != null)
        {
            _assignedVehicle.AssignContract(null);
        }
        
        _assignedVehicle = null;
        _assignedVehicleId = null;
    }
    
    // Financial calculations
    private void CalculateFinalReward()
    {
        float finalReward = _originalReward;
        
        // Time bonus/penalty
        if (_onTimeDelivery)
        {
            // Early delivery bonus (up to 10%)
            float earlyBonus = Mathf.Max(0f, (_expirationTime - _completionTime).Hours) / _expectedDuration;
            _bonusAmount = _originalReward * Mathf.Min(0.1f, earlyBonus * 0.05f);
            finalReward += _bonusAmount;
        }
        else
        {
            // Late delivery penalty
            float hoursLate = (float)(_completionTime - _expirationTime).TotalHours;
            _penaltyAmount = _originalReward * _contractData.PenaltyRate * hoursLate;
            finalReward -= _penaltyAmount;
        }
        
        // Expedited delivery bonus
        if (_isExpedited)
        {
            _bonusAmount += _originalReward * 0.15f; // 15% bonus for rush delivery
            finalReward += _originalReward * 0.15f;
        }
        
        // Cargo condition bonus (based on cargo type and vehicle condition)
        if (_contractData?.CargoType?.IsFragile == true)
        {
            float conditionBonus = _customerSatisfaction * _originalReward * 0.05f; // Up to 5% bonus
            _bonusAmount += conditionBonus;
            finalReward += conditionBonus;
        }
        
        _currentReward = Mathf.Max(0f, finalReward);
    }
    
    private void CalculateCustomerSatisfaction()
    {
        float satisfaction = 1f;
        
        // Time factor
        if (_onTimeDelivery)
        {
            float timeRatio = _actualDuration / _expectedDuration;
            satisfaction *= Mathf.Lerp(1.2f, 1f, timeRatio); // Bonus for fast delivery
        }
        else
        {
            float latePenalty = (float)(_completionTime - _expirationTime).TotalHours / _expectedDuration;
            satisfaction *= Mathf.Max(0.1f, 1f - latePenalty);
        }
        
        // Vehicle condition factor
        if (_assignedVehicle != null)
        {
            float vehicleCondition = 1f - _assignedVehicle.WearLevel;
            satisfaction *= Mathf.Lerp(0.8f, 1f, vehicleCondition);
        }
        
        // Cargo type factor
        if (_contractData?.CargoType != null)
        {
            if (_contractData.CargoType.IsFragile && _assignedVehicle?.WearLevel > 0.5f)
            {
                satisfaction *= 0.9f; // Penalty for fragile goods in worn vehicle
            }
            
            if (_contractData.CargoType.IsPerishable && !_onTimeDelivery)
            {
                satisfaction *= 0.7f; // Heavy penalty for late perishable delivery
            }
        }
        
        _customerSatisfaction = Mathf.Clamp01(satisfaction);
    }
    
    // Utility methods
    private string GenerateContractTitle()
    {
        if (_contractData == null) return "Unknown Contract";
        
        string origin = _contractData.OriginCity?.CityName ?? "Unknown";
        string destination = _contractData.DestinationCity?.CityName ?? "Unknown";
        string cargo = _contractData.CargoType?.GoodName ?? "Unknown Cargo";
        
        return $"{cargo} delivery from {origin} to {destination}";
    }
    
    private void AddProgressMilestone(string milestone)
    {
        string timestampedMilestone = $"{DateTime.Now:HH:mm} - {milestone}";
        _progressMilestones.Add(timestampedMilestone);
    }
    
    public void SetExpedited(bool expedited)
    {
        _isExpedited = expedited;
        if (expedited)
        {
            AddProgressMilestone("Marked as expedited delivery");
        }
    }
    
    public float GetEstimatedArrivalTime()
    {
        if (_assignedVehicle == null || _status != ContractStatus.InProgress) return 0f;
        
        float remainingDistance = TotalDistance * (1f - _deliveryProgress);
        float averageSpeed = _assignedVehicle.CurrentSpeed * 0.8f; // Conservative estimate
        
        return remainingDistance / averageSpeed; // Hours
    }
    
    public bool IsUrgent()
    {
        if (_status != ContractStatus.Available && _status != ContractStatus.Accepted) return false;
        
        return TimeRemaining.TotalHours <= 6f; // Urgent if less than 6 hours remaining
    }
    
    public bool RequiresInsurance()
    {
        return _contractData?.RequiresInsurance == true || TotalCargoWeight > 10000f || _currentReward > 50000f;
    }
    
    public string GetStatusDescription()
    {
        return _status switch
        {
            ContractStatus.Available => $"Available until {_expirationTime:MMM dd, HH:mm}",
            ContractStatus.Accepted => $"Assigned to {_assignedVehicle?.CustomName ?? "Unknown Vehicle"}",
            ContractStatus.InProgress => $"In progress - {CompletionPercentage:F0}% complete",
            ContractStatus.Completed => $"Completed {(_onTimeDelivery ? "on time" : "late")} on {_completionTime:MMM dd, HH:mm}",
            ContractStatus.Cancelled => $"Cancelled on {_completionTime:MMM dd, HH:mm}",
            ContractStatus.Expired => $"Expired on {_expirationTime:MMM dd, HH:mm}",
            _ => "Unknown status"
        };
    }
    
    public Dictionary<string, object> GetPerformanceMetrics()
    {
        return new Dictionary<string, object>
        {
            { "OnTimeDelivery", _onTimeDelivery },
            { "ActualDuration", _actualDuration },
            { "ExpectedDuration", _expectedDuration },
            { "CustomerSatisfaction", _customerSatisfaction },
            { "FinalReward", _currentReward },
            { "BonusAmount", _bonusAmount },
            { "PenaltyAmount", _penaltyAmount },
            { "CompletionPercentage", CompletionPercentage }
        };
    }
}

// AIDEV-NOTE: Enum for contract status states
public enum ContractStatus
{
    Available,      // Available for acceptance
    Accepted,       // Accepted but not yet started
    InProgress,     // Currently being delivered
    Completed,      // Successfully completed
    Cancelled,      // Cancelled by player or system
    Expired         // Expired due to time limit
}

// AIDEV-NOTE: Serializable data structure for save/load operations
[System.Serializable]
public class ContractInstanceData
{
    public string InstanceId;
    public ContractData ContractData;
    public DateTime GenerationTime;
    public string ContractTitle;
    public ContractStatus Status;
    public DateTime AcceptanceTime;
    public DateTime CompletionTime;
    public DateTime ExpirationTime;
    public string AssignedVehicleId;
    public bool IsExpedited;
    public bool CargoLoaded;
    public DateTime LoadingTime;
    public DateTime DepartureTime;
    public float DeliveryProgress;
    public List<string> ProgressMilestones;
    public float OriginalReward;
    public float CurrentReward;
    public float PenaltyAmount;
    public float BonusAmount;
    public float ExpectedDuration;
    public float ActualDuration;
    public bool OnTimeDelivery;
    public float CustomerSatisfaction;
    
    public ContractInstanceData(ContractInstance instance)
    {
        InstanceId = instance.InstanceId;
        ContractData = instance.ContractData;
        GenerationTime = instance.GenerationTime;
        ContractTitle = instance.ContractTitle;
        Status = instance.Status;
        AcceptanceTime = instance.AcceptanceTime;
        CompletionTime = instance.CompletionTime;
        ExpirationTime = instance.ExpirationTime;
        AssignedVehicleId = instance.AssignedVehicleId;
        IsExpedited = instance.IsExpedited;
        CargoLoaded = instance.CargoLoaded;
        LoadingTime = instance.LoadingTime;
        DepartureTime = instance.DepartureTime;
        DeliveryProgress = instance.DeliveryProgress;
        ProgressMilestones = new List<string>(instance.ProgressMilestones);
        OriginalReward = instance.OriginalReward;
        CurrentReward = instance.CurrentReward;
        PenaltyAmount = instance.PenaltyAmount;
        BonusAmount = instance.BonusAmount;
        ExpectedDuration = instance.ExpectedDuration;
        ActualDuration = instance.ActualDuration;
        OnTimeDelivery = instance.OnTimeDelivery;
        CustomerSatisfaction = instance.CustomerSatisfaction;
    }
}