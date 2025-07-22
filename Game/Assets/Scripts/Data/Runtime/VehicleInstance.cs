using UnityEngine;
using System;
using System.Collections.Generic;

// AIDEV-NOTE: Runtime instance data for vehicles with current state tracking
[System.Serializable]
public class VehicleInstance
{
    [Header("Identity")]
    [SerializeField] private string _instanceId;
    [SerializeField] private VehicleData _vehicleData;
    [SerializeField] private string _customName;
    [SerializeField] private DateTime _purchaseDate;
    
    [Header("Current Position")]
    [SerializeField] private Vector2 _currentPosition; // World coordinates in km
    [SerializeField] private CityData _currentCity; // Null if on road
    [SerializeField] private CityData _destinationCity; // Null if stationary
    
    [Header("Movement State")]
    [SerializeField] private VehicleStatus _status = VehicleStatus.Stopped;
    [SerializeField] private float _currentSpeed; // km/h
    [SerializeField] private float _travelProgress; // 0-1 between current and destination
    [SerializeField] private List<Vector2> _currentRoute = new List<Vector2>(); // Waypoints in km
    [SerializeField] private DateTime _departureTime;
    [SerializeField] private DateTime _estimatedArrival;
    
    [Header("Vehicle Condition")]
    [SerializeField] private float _currentFuel; // liters
    [SerializeField] private float _currentWeight; // kg (including cargo)
    [SerializeField] private float _wearLevel; // 0-1, affects performance and maintenance
    [SerializeField] private float _totalDistance; // km driven
    [SerializeField] private DateTime _lastMaintenance;
    
    [Header("Cargo Information")]
    [SerializeField] private List<CargoItem> _currentCargo = new List<CargoItem>();
    [SerializeField] private ContractInstance _assignedContract; // Current active contract
    [SerializeField] private float _cargoValue; // Total value of current cargo
    
    [Header("Economic Data")]
    [SerializeField] private float _totalRevenue; // Total earnings from this vehicle
    [SerializeField] private float _totalExpenses; // Total costs (fuel, maintenance, etc.)
    [SerializeField] private float _maintenanceCosts; // Accumulated maintenance costs
    [SerializeField] private int _completedDeliveries;
    
    // Events for vehicle state changes
    public static event Action<VehicleInstance, VehicleStatus> OnStatusChanged;
    public static event Action<VehicleInstance, float> OnFuelChanged;
    public static event Action<VehicleInstance, CityData> OnCityArrived;
    public static event Action<VehicleInstance, ContractInstance> OnContractAssigned;
    
    // Properties
    public string InstanceId => _instanceId;
    public VehicleData VehicleData => _vehicleData;
    public string CustomName => string.IsNullOrEmpty(_customName) ? _vehicleData?.VehicleName : _customName;
    public DateTime PurchaseDate => _purchaseDate;
    public Vector2 CurrentPosition => _currentPosition;
    public CityData CurrentCity => _currentCity;
    public CityData DestinationCity => _destinationCity;
    public VehicleStatus Status => _status;
    public float CurrentSpeed => _currentSpeed;
    public float TravelProgress => _travelProgress;
    public List<Vector2> CurrentRoute => _currentRoute;
    public DateTime DepartureTime => _departureTime;
    public DateTime EstimatedArrival => _estimatedArrival;
    public float CurrentFuel => _currentFuel;
    public float CurrentWeight => _currentWeight;
    public float WearLevel => _wearLevel;
    public float TotalDistance => _totalDistance;
    public DateTime LastMaintenance => _lastMaintenance;
    public List<CargoItem> CurrentCargo => _currentCargo;
    public ContractInstance AssignedContract => _assignedContract;
    public float CargoValue => _cargoValue;
    public float TotalRevenue => _totalRevenue;
    public float TotalExpenses => _totalExpenses;
    public float MaintenanceCosts => _maintenanceCosts;
    public int CompletedDeliveries => _completedDeliveries;
    
    // Calculated properties
    public float FuelPercentage => _vehicleData != null ? (_currentFuel / _vehicleData.FuelCapacity) * 100f : 0f;
    public float WeightPercentage => _vehicleData != null ? (_currentWeight / _vehicleData.WeightCapacity) * 100f : 0f;
    public float RemainingRange => _vehicleData != null ? (_currentFuel / _vehicleData.FuelConsumption) * 100f : 0f; // km
    public float NetProfit => _totalRevenue - _totalExpenses;
    public bool IsMoving => _status == VehicleStatus.Moving;
    public bool IsAvailable => _status == VehicleStatus.Stopped && _assignedContract == null;
    public bool RequiresMaintenance => _wearLevel > 0.8f;
    public bool IsLowOnFuel => FuelPercentage < 20f;
    public bool IsOverloaded => WeightPercentage > 100f;
    
    // Constructor
    public VehicleInstance(VehicleData vehicleData, CityData startingCity = null)
    {
        _instanceId = System.Guid.NewGuid().ToString();
        _vehicleData = vehicleData;
        _customName = vehicleData?.VehicleName;
        _purchaseDate = DateTime.Now;
        
        // Initialize position
        _currentCity = startingCity;
        _currentPosition = startingCity?.Position ?? Vector2.zero;
        _destinationCity = null;
        
        // Initialize state
        _status = VehicleStatus.Stopped;
        _currentSpeed = 0f;
        _travelProgress = 0f;
        _currentRoute = new List<Vector2>();
        
        // Initialize condition
        _currentFuel = vehicleData?.FuelCapacity ?? 100f;
        _currentWeight = 0f; // Empty vehicle
        _wearLevel = 0f; // New vehicle
        _totalDistance = 0f;
        _lastMaintenance = DateTime.Now;
        
        // Initialize cargo
        _currentCargo = new List<CargoItem>();
        _assignedContract = null;
        _cargoValue = 0f;
        
        // Initialize economics
        _totalRevenue = 0f;
        _totalExpenses = 0f;
        _maintenanceCosts = 0f;
        _completedDeliveries = 0;
    }
    
    // Movement methods
    public bool StartTrip(CityData destination, List<Vector2> route)
    {
        if (destination == null || _status != VehicleStatus.Stopped) return false;
        if (_currentFuel <= 0f) return false;
        
        _destinationCity = destination;
        _currentRoute = new List<Vector2>(route);
        _status = VehicleStatus.Moving;
        _travelProgress = 0f;
        _departureTime = DateTime.Now;
        _estimatedArrival = CalculateArrivalTime(route);
        
        // Clear current city when departing
        _currentCity = null;
        
        OnStatusChanged?.Invoke(this, _status);
        return true;
    }
    
    public void UpdateMovement(float deltaTime)
    {
        if (_status != VehicleStatus.Moving || _currentRoute.Count == 0) return;
        
        // Calculate movement based on current speed and conditions
        float effectiveSpeed = CalculateEffectiveSpeed();
        float distanceThisFrame = (effectiveSpeed * deltaTime) / 3600f; // Convert from km/h to km per second
        
        // Consume fuel
        float fuelConsumption = (_vehicleData.FuelConsumption / 100f) * distanceThisFrame;
        ConsumeFuel(fuelConsumption);
        
        // If out of fuel, stop the vehicle
        if (_currentFuel <= 0f)
        {
            _status = VehicleStatus.OutOfFuel;
            _currentSpeed = 0f;
            OnStatusChanged?.Invoke(this, _status);
            return;
        }
        
        // Update position along route
        UpdatePositionOnRoute(distanceThisFrame);
        _totalDistance += distanceThisFrame;
        
        // Update wear based on distance and conditions
        UpdateWear(distanceThisFrame);
        
        // Check if arrived at destination
        if (_travelProgress >= 1f)
        {
            ArriveAtDestination();
        }
    }
    
    private void UpdatePositionOnRoute(float distance)
    {
        if (_currentRoute.Count < 2) return;
        
        float remainingDistance = distance;
        int currentSegment = Mathf.FloorToInt(_travelProgress * (_currentRoute.Count - 1));
        
        while (remainingDistance > 0f && currentSegment < _currentRoute.Count - 1)
        {
            Vector2 segmentStart = _currentRoute[currentSegment];
            Vector2 segmentEnd = _currentRoute[currentSegment + 1];
            float segmentLength = Vector2.Distance(segmentStart, segmentEnd);
            
            float segmentProgress = _travelProgress * (_currentRoute.Count - 1) - currentSegment;
            float remainingSegmentDistance = segmentLength * (1f - segmentProgress);
            
            if (remainingDistance >= remainingSegmentDistance)
            {
                // Move to next segment
                remainingDistance -= remainingSegmentDistance;
                currentSegment++;
                _travelProgress = (float)currentSegment / (_currentRoute.Count - 1);
            }
            else
            {
                // Move within current segment
                float segmentMovement = remainingDistance / segmentLength;
                _travelProgress += segmentMovement / (_currentRoute.Count - 1);
                remainingDistance = 0f;
            }
        }
        
        // Update current position
        if (currentSegment < _currentRoute.Count - 1)
        {
            Vector2 segmentStart = _currentRoute[currentSegment];
            Vector2 segmentEnd = _currentRoute[currentSegment + 1];
            float segmentProgress = (_travelProgress * (_currentRoute.Count - 1)) - currentSegment;
            _currentPosition = Vector2.Lerp(segmentStart, segmentEnd, segmentProgress);
        }
        else
        {
            _currentPosition = _currentRoute[_currentRoute.Count - 1];
        }
    }
    
    private void ArriveAtDestination()
    {
        _currentCity = _destinationCity;
        _currentPosition = _destinationCity.Position;
        _destinationCity = null;
        _status = VehicleStatus.Stopped;
        _currentSpeed = 0f;
        _travelProgress = 0f;
        _currentRoute.Clear();
        
        OnCityArrived?.Invoke(this, _currentCity);
        OnStatusChanged?.Invoke(this, _status);
        
        // Check if this completes a contract
        if (_assignedContract != null && _assignedContract.ContractData?.DestinationCity == _currentCity)
        {
            CompleteDelivery();
        }
    }
    
    private float CalculateEffectiveSpeed()
    {
        if (_vehicleData == null) return 0f;
        
        float baseSpeed = _vehicleData.MaxSpeed;
        
        // Reduce speed based on cargo weight
        float weightFactor = 1f - (_currentWeight / _vehicleData.WeightCapacity) * 0.3f; // Max 30% reduction
        
        // Reduce speed based on wear
        float wearFactor = 1f - _wearLevel * 0.2f; // Max 20% reduction
        
        // Apply fuel efficiency (slight speed reduction when low on fuel)
        float fuelFactor = FuelPercentage < 10f ? 0.8f : 1f;
        
        _currentSpeed = baseSpeed * weightFactor * wearFactor * fuelFactor;
        return _currentSpeed;
    }
    
    private DateTime CalculateArrivalTime(List<Vector2> route)
    {
        if (route.Count < 2) return DateTime.Now;
        
        float totalDistance = 0f;
        for (int i = 0; i < route.Count - 1; i++)
        {
            totalDistance += Vector2.Distance(route[i], route[i + 1]);
        }
        
        float averageSpeed = CalculateEffectiveSpeed() * 0.8f; // Account for variations
        float travelTimeHours = totalDistance / averageSpeed;
        
        return DateTime.Now.AddHours(travelTimeHours);
    }
    
    // Fuel and maintenance
    public bool Refuel(float amount)
    {
        if (amount <= 0f || _vehicleData == null) return false;
        
        float maxRefuel = _vehicleData.FuelCapacity - _currentFuel;
        float actualRefuel = Mathf.Min(amount, maxRefuel);
        
        _currentFuel += actualRefuel;
        OnFuelChanged?.Invoke(this, _currentFuel);
        
        // If vehicle was out of fuel, it can now move again
        if (_status == VehicleStatus.OutOfFuel && _currentFuel > 0f)
        {
            _status = VehicleStatus.Stopped;
            OnStatusChanged?.Invoke(this, _status);
        }
        
        return actualRefuel > 0f;
    }
    
    public void ConsumeFuel(float amount)
    {
        _currentFuel = Mathf.Max(0f, _currentFuel - amount);
        OnFuelChanged?.Invoke(this, _currentFuel);
    }
    
    public void PerformMaintenance()
    {
        float maintenanceCost = _vehicleData.MaintenanceCost * _wearLevel;
        _maintenanceCosts += maintenanceCost;
        _totalExpenses += maintenanceCost;
        
        _wearLevel = 0f; // Reset wear
        _lastMaintenance = DateTime.Now;
    }
    
    private void UpdateWear(float distance)
    {
        // Wear increases based on distance and conditions
        float baseWear = distance / 10000f; // 1% wear per 1000km
        float weightWear = (_currentWeight / _vehicleData.WeightCapacity) * baseWear * 0.5f;
        
        _wearLevel = Mathf.Min(1f, _wearLevel + baseWear + weightWear);
    }
    
    // Cargo management
    public bool LoadCargo(CargoItem cargo)
    {
        if (cargo == null) return false;
        
        float newWeight = _currentWeight + cargo.TotalWeight;
        float newVolume = GetCurrentVolume() + cargo.TotalVolume;
        
        if (newWeight > _vehicleData.WeightCapacity || newVolume > _vehicleData.CargoVolume)
            return false;
        
        _currentCargo.Add(cargo);
        _currentWeight = newWeight;
        _cargoValue += cargo.TotalValue;
        
        return true;
    }
    
    public bool UnloadCargo(CargoItem cargo)
    {
        if (cargo == null || !_currentCargo.Contains(cargo)) return false;
        
        _currentCargo.Remove(cargo);
        _currentWeight -= cargo.TotalWeight;
        _cargoValue -= cargo.TotalValue;
        
        return true;
    }
    
    public void UnloadAllCargo()
    {
        _currentCargo.Clear();
        _currentWeight = 0f;
        _cargoValue = 0f;
    }
    
    private float GetCurrentVolume()
    {
        float totalVolume = 0f;
        foreach (var cargo in _currentCargo)
        {
            totalVolume += cargo.TotalVolume;
        }
        return totalVolume;
    }
    
    // Contract management
    public void AssignContract(ContractInstance contract)
    {
        _assignedContract = contract;
        OnContractAssigned?.Invoke(this, contract);
    }
    
    public void CompleteDelivery()
    {
        if (_assignedContract != null)
        {
            _completedDeliveries++;
            _totalRevenue += _assignedContract.ContractData.CalculateAdjustedReward();
            
            // Unload cargo
            UnloadAllCargo();
            
            _assignedContract = null;
        }
    }
    
    // Utility methods
    public void SetCustomName(string name)
    {
        _customName = string.IsNullOrEmpty(name) ? _vehicleData?.VehicleName : name;
    }
    
    public bool CanAcceptContract(ContractInstance contract)
    {
        if (contract == null || _assignedContract != null) return false;
        if (_status != VehicleStatus.Stopped) return false;
        
        // Check if vehicle can handle the cargo
        return _vehicleData.CanCarry(contract.TotalCargoWeight, contract.TotalCargoVolume) &&
               _vehicleData.CanTransport(contract.ContractData.CargoType);
    }
    
    public float EstimateTripCost(float distance)
    {
        if (_vehicleData == null) return 0f;
        
        float fuelCost = _vehicleData.CalculateFuelCost(distance, 1.5f); // Assume $1.50 per liter
        float maintenanceCost = _vehicleData.CalculateMaintenanceCost(distance);
        
        return fuelCost + maintenanceCost;
    }
}

// AIDEV-NOTE: Enum for vehicle status states
public enum VehicleStatus
{
    Stopped,        // Stationary in a city
    Moving,         // Traveling between cities
    Loading,        // Loading/unloading cargo
    Maintenance,    // Under maintenance
    OutOfFuel,      // Out of fuel and stranded
    Broken          // Broken down and needs repair
}

// AIDEV-NOTE: Data structure for cargo items
[System.Serializable]
public class CargoItem
{
    [SerializeField] private GoodData _goodData;
    [SerializeField] private int _quantity;
    [SerializeField] private string _origin;
    [SerializeField] private string _destination;
    
    public GoodData GoodData => _goodData;
    public int Quantity => _quantity;
    public string Origin => _origin;
    public string Destination => _destination;
    
    public float TotalWeight => _goodData != null ? _goodData.CalculateTotalWeight(_quantity) : 0f;
    public float TotalVolume => _goodData != null ? _goodData.CalculateTotalVolume(_quantity) : 0f;
    public float TotalValue => _goodData != null ? _goodData.CalculateTotalValue(_quantity) : 0f;
    
    public CargoItem(GoodData goodData, int quantity, string origin, string destination)
    {
        _goodData = goodData;
        _quantity = quantity;
        _origin = origin;
        _destination = destination;
    }
}

// AIDEV-NOTE: Serializable data structure for save/load operations
[System.Serializable]
public class VehicleInstanceData
{
    public string InstanceId;
    public VehicleData VehicleData;
    public string CustomName;
    public DateTime PurchaseDate;
    public Vector2 CurrentPosition;
    public string CurrentCityName;
    public string DestinationCityName;
    public VehicleStatus Status;
    public float CurrentFuel;
    public float CurrentWeight;
    public float WearLevel;
    public float TotalDistance;
    public DateTime LastMaintenance;
    public List<CargoItem> CurrentCargo;
    public string AssignedContractId;
    public float TotalRevenue;
    public float TotalExpenses;
    public int CompletedDeliveries;
    
    public VehicleInstanceData(VehicleInstance instance)
    {
        InstanceId = instance.InstanceId;
        VehicleData = instance.VehicleData;
        CustomName = instance.CustomName;
        PurchaseDate = instance.PurchaseDate;
        CurrentPosition = instance.CurrentPosition;
        CurrentCityName = instance.CurrentCity?.name;
        DestinationCityName = instance.DestinationCity?.name;
        Status = instance.Status;
        CurrentFuel = instance.CurrentFuel;
        CurrentWeight = instance.CurrentWeight;
        WearLevel = instance.WearLevel;
        TotalDistance = instance.TotalDistance;
        LastMaintenance = instance.LastMaintenance;
        CurrentCargo = new List<CargoItem>(instance.CurrentCargo);
        AssignedContractId = instance.AssignedContract?.InstanceId;
        TotalRevenue = instance.TotalRevenue;
        TotalExpenses = instance.TotalExpenses;
        CompletedDeliveries = instance.CompletedDeliveries;
    }
}