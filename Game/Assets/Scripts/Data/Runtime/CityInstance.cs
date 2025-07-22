using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// AIDEV-NOTE: Runtime instance data for cities with dynamic state tracking
[System.Serializable]
public class CityInstance
{
    [Header("Identity")]
    [SerializeField] private string _instanceId;
    [SerializeField] private CityData _cityData;
    [SerializeField] private bool _isDiscovered = false;
    [SerializeField] private DateTime _firstVisitTime;
    [SerializeField] private int _totalVisits = 0;
    
    [Header("Current State")]
    [SerializeField] private List<VehicleInstance> _stationedVehicles = new List<VehicleInstance>();
    [SerializeField] private List<ContractInstance> _availableContracts = new List<ContractInstance>();
    [SerializeField] private List<ContractInstance> _activeContracts = new List<ContractInstance>();
    [SerializeField] private bool _hasGasStation = true;
    [SerializeField] private bool _hasMaintenanceShop = true;
    
    [Header("Economic State")]
    [SerializeField] private float _fuelPrice = 1.5f; // per liter
    [SerializeField] private Dictionary<GoodData, float> _goodsPrices = new Dictionary<GoodData, float>();
    [SerializeField] private Dictionary<GoodData, int> _goodsAvailability = new Dictionary<GoodData, int>();
    [SerializeField] private Dictionary<GoodData, float> _demandMultipliers = new Dictionary<GoodData, float>();
    
    [Header("Infrastructure")]
    [SerializeField] private float _maintenanceQuality = 1f; // 0-1, affects cost and effectiveness
    [SerializeField] private int _populationSize = 50000;
    [SerializeField] private float _economicActivity = 1f; // 0-2, affects contract generation
    [SerializeField] private WeatherCondition _currentWeather = WeatherCondition.Clear;
    
    [Header("Statistics")]
    [SerializeField] private int _contractsGenerated = 0;
    [SerializeField] private int _contractsCompleted = 0;
    [SerializeField] private float _totalRevenueGenerated = 0f;
    [SerializeField] private DateTime _lastContractGeneration;
    [SerializeField] private float _playerReputation = 50f; // 0-100
    
    [Header("Dynamic Events")]
    [SerializeField] private List<CityEvent> _activeEvents = new List<CityEvent>();
    [SerializeField] private DateTime _lastEventCheck;
    [SerializeField] private bool _hasSpecialEvent = false;
    
    // Events for city state changes
    public static event Action<CityInstance> OnCityDiscovered;
    public static event Action<CityInstance, VehicleInstance> OnVehicleArrived;
    public static event Action<CityInstance, VehicleInstance> OnVehicleDeparted;
    public static event Action<CityInstance, ContractInstance> OnContractGenerated;
    public static event Action<CityInstance, CityEvent> OnEventStarted;
    public static event Action<CityInstance, CityEvent> OnEventEnded;
    
    // Properties
    public string InstanceId => _instanceId;
    public CityData CityData => _cityData;
    public bool IsDiscovered => _isDiscovered;
    public DateTime FirstVisitTime => _firstVisitTime;
    public int TotalVisits => _totalVisits;
    public List<VehicleInstance> StationedVehicles => _stationedVehicles;
    public List<ContractInstance> AvailableContracts => _availableContracts;
    public List<ContractInstance> ActiveContracts => _activeContracts;
    public bool HasGasStation => _hasGasStation;
    public bool HasMaintenanceShop => _hasMaintenanceShop;
    public float FuelPrice => _fuelPrice;
    public Dictionary<GoodData, float> GoodsPrices => _goodsPrices;
    public Dictionary<GoodData, int> GoodsAvailability => _goodsAvailability;
    public Dictionary<GoodData, float> DemandMultipliers => _demandMultipliers;
    public float MaintenanceQuality => _maintenanceQuality;
    public int PopulationSize => _populationSize;
    public float EconomicActivity => _economicActivity;
    public WeatherCondition CurrentWeather => _currentWeather;
    public int ContractsGenerated => _contractsGenerated;
    public int ContractsCompleted => _contractsCompleted;
    public float TotalRevenueGenerated => _totalRevenueGenerated;
    public DateTime LastContractGeneration => _lastContractGeneration;
    public float PlayerReputation => _playerReputation;
    public List<CityEvent> ActiveEvents => _activeEvents;
    public bool HasSpecialEvent => _hasSpecialEvent;
    
    // Calculated properties
    public string CityName => _cityData?.CityName ?? "Unknown City";
    public Vector2 Position => _cityData?.Position ?? Vector2.zero;
    public int VehicleCount => _stationedVehicles?.Count ?? 0;
    public int AvailableContractCount => _availableContracts?.Count ?? 0;
    public float ContractSuccessRate => _contractsGenerated > 0 ? (float)_contractsCompleted / _contractsGenerated * 100f : 0f;
    public bool IsActive => _isDiscovered && (_stationedVehicles.Count > 0 || _availableContracts.Count > 0);
    public float ModifiedFuelPrice => _fuelPrice * GetEventMultiplier("fuel_price");
    
    // Constructor
    public CityInstance(CityData cityData)
    {
        _instanceId = System.Guid.NewGuid().ToString();
        _cityData = cityData;
        _isDiscovered = false;
        _totalVisits = 0;
        
        // Initialize collections
        _stationedVehicles = new List<VehicleInstance>();
        _availableContracts = new List<ContractInstance>();
        _activeContracts = new List<ContractInstance>();
        _goodsPrices = new Dictionary<GoodData, float>();
        _goodsAvailability = new Dictionary<GoodData, int>();
        _demandMultipliers = new Dictionary<GoodData, float>();
        _activeEvents = new List<CityEvent>();
        
        // Initialize economic state
        InitializeEconomicState();
        
        // Initialize infrastructure based on city size
        InitializeInfrastructure();
        
        _lastEventCheck = DateTime.Now;
        _lastContractGeneration = DateTime.Now;
        _playerReputation = 50f; // Neutral starting reputation
    }
    
    // Discovery and visits
    public void DiscoverCity()
    {
        if (_isDiscovered) return;
        
        _isDiscovered = true;
        _firstVisitTime = DateTime.Now;
        _totalVisits = 1;
        
        // Generate initial contracts when discovered
        GenerateInitialContracts();
        
        OnCityDiscovered?.Invoke(this);
    }
    
    public void VisitCity(VehicleInstance vehicle)
    {
        if (vehicle == null) return;
        
        if (!_isDiscovered)
        {
            DiscoverCity();
        }
        else
        {
            _totalVisits++;
        }
        
        // Add vehicle to stationed vehicles if not already present
        if (!_stationedVehicles.Contains(vehicle))
        {
            _stationedVehicles.Add(vehicle);
            OnVehicleArrived?.Invoke(this, vehicle);
        }
        
        // Update player reputation based on successful deliveries
        if (vehicle.AssignedContract?.Status == ContractStatus.Completed)
        {
            UpdatePlayerReputation(5f); // Small reputation boost for successful delivery
        }
    }
    
    public void DepartVehicle(VehicleInstance vehicle)
    {
        if (vehicle == null || !_stationedVehicles.Contains(vehicle)) return;
        
        _stationedVehicles.Remove(vehicle);
        OnVehicleDeparted?.Invoke(this, vehicle);
    }
    
    // Economic management
    private void InitializeEconomicState()
    {
        if (_cityData?.AvailableGoods == null) return;
        
        // Initialize prices and availability for available goods
        foreach (var good in _cityData.AvailableGoods)
        {
            if (good == null) continue;
            
            // Set base price with city multiplier
            float basePrice = good.BaseValue * _cityData.PriceMultiplier;
            _goodsPrices[good] = basePrice;
            
            // Set initial availability based on city size and good type
            int availability = CalculateInitialAvailability(good);
            _goodsAvailability[good] = availability;
            
            // Set demand multiplier
            _demandMultipliers[good] = UnityEngine.Random.Range(0.8f, 1.2f);
        }
        
        // Set fuel price based on city location and size
        _fuelPrice = CalculateBaseFuelPrice();
    }
    
    private void InitializeInfrastructure()
    {
        // Larger cities have better infrastructure
        float cityScale = Mathf.Clamp01(_populationSize / 100000f);
        
        _hasGasStation = _populationSize >= 10000 || UnityEngine.Random.value < 0.8f;
        _hasMaintenanceShop = _populationSize >= 25000 || UnityEngine.Random.value < 0.6f;
        _maintenanceQuality = Mathf.Lerp(0.6f, 1f, cityScale);
        
        // Economic activity based on population and available goods
        int goodsCount = _cityData?.AvailableGoods?.Count ?? 1;
        _economicActivity = Mathf.Lerp(0.5f, 2f, cityScale) * Mathf.Lerp(0.8f, 1.2f, goodsCount / 5f);
    }
    
    private int CalculateInitialAvailability(GoodData good)
    {
        // Base availability on population size and good type
        int baseAvailability = Mathf.RoundToInt(_populationSize / 1000f);
        
        // Adjust for good type
        if (good.IsPerishable)
            baseAvailability = Mathf.RoundToInt(baseAvailability * 0.7f); // Less available
        else if (good.IsHazardous)
            baseAvailability = Mathf.RoundToInt(baseAvailability * 0.3f); // Much less available
        
        return Mathf.Max(5, baseAvailability); // Minimum 5 units
    }
    
    private float CalculateBaseFuelPrice()
    {
        // Base fuel price with some variation
        float basePrice = 1.5f;
        
        // Remote cities have higher fuel prices
        float remoteness = Vector2.Distance(Position, Vector2.zero) / 1000f; // Assume center is 0,0
        basePrice += remoteness * 0.1f;
        
        // Smaller cities have higher prices
        if (_populationSize < 20000)
            basePrice += 0.2f;
        
        return Mathf.Clamp(basePrice, 1.2f, 2.5f);
    }
    
    // Contract management
    public void UpdateContracts(float deltaTime)
    {
        // Check for expired contracts
        for (int i = _availableContracts.Count - 1; i >= 0; i--)
        {
            if (_availableContracts[i].IsExpired)
            {
                _availableContracts[i].ExpireContract();
                _availableContracts.RemoveAt(i);
            }
        }
        
        // Generate new contracts periodically
        if ((DateTime.Now - _lastContractGeneration).TotalHours >= GetContractGenerationInterval())
        {
            GenerateNewContracts();
            _lastContractGeneration = DateTime.Now;
        }
    }
    
    private void GenerateInitialContracts()
    {
        // Generate 2-4 initial contracts when city is discovered
        int contractCount = UnityEngine.Random.Range(2, 5);
        for (int i = 0; i < contractCount; i++)
        {
            GenerateRandomContract();
        }
    }
    
    private void GenerateNewContracts()
    {
        // Number of contracts based on economic activity and available slots
        int maxContracts = Mathf.RoundToInt(5 * _economicActivity);
        int currentContracts = _availableContracts.Count;
        
        if (currentContracts < maxContracts)
        {
            int contractsToGenerate = UnityEngine.Random.Range(1, maxContracts - currentContracts + 1);
            for (int i = 0; i < contractsToGenerate; i++)
            {
                GenerateRandomContract();
            }
        }
    }
    
    private void GenerateRandomContract()
    {
        // This would integrate with a contract generation system
        // For now, we'll create a placeholder
        
        // Find available goods in this city
        if (_cityData?.AvailableGoods == null || _cityData.AvailableGoods.Count == 0) return;
        
        var availableGood = _cityData.AvailableGoods[UnityEngine.Random.Range(0, _cityData.AvailableGoods.Count)];
        if (availableGood == null) return;
        
        // This would normally use a contract template or generation system
        // For now, we'll just track that a contract was generated
        _contractsGenerated++;
        
        // Note: Actual contract generation would require access to other cities
        // and contract templates, which would be handled by a contract manager
    }
    
    private float GetContractGenerationInterval()
    {
        // More active cities generate contracts more frequently
        return Mathf.Lerp(24f, 4f, _economicActivity); // 4-24 hours
    }
    
    public void AddContract(ContractInstance contract)
    {
        if (contract != null && !_availableContracts.Contains(contract))
        {
            _availableContracts.Add(contract);
            OnContractGenerated?.Invoke(this, contract);
        }
    }
    
    public void RemoveContract(ContractInstance contract)
    {
        _availableContracts.Remove(contract);
        
        if (contract?.Status == ContractStatus.Completed)
        {
            _contractsCompleted++;
            _totalRevenueGenerated += contract.CurrentReward;
        }
    }
    
    // Services
    public bool RefuelVehicle(VehicleInstance vehicle, float amount)
    {
        if (!_hasGasStation || vehicle == null || amount <= 0f) return false;
        
        float cost = amount * ModifiedFuelPrice;
        // Cost would be deducted from player credits in actual implementation
        
        return vehicle.Refuel(amount);
    }
    
    public bool PerformMaintenance(VehicleInstance vehicle)
    {
        if (!_hasMaintenanceShop || vehicle == null) return false;
        
        vehicle.PerformMaintenance();
        
        // Improve player reputation for using local services
        UpdatePlayerReputation(1f);
        
        return true;
    }
    
    public float GetMaintenanceCost(VehicleInstance vehicle)
    {
        if (!_hasMaintenanceShop || vehicle == null) return 0f;
        
        float baseCost = vehicle.VehicleData.MaintenanceCost;
        float wearMultiplier = 1f + vehicle.WearLevel;
        float qualityMultiplier = 2f - _maintenanceQuality; // Lower quality = higher cost
        
        return baseCost * wearMultiplier * qualityMultiplier;
    }
    
    // Reputation and relationships
    public void UpdatePlayerReputation(float change)
    {
        _playerReputation = Mathf.Clamp(_playerReputation + change, 0f, 100f);
        
        // Reputation affects prices and contract availability
        UpdatePricesBasedOnReputation();
    }
    
    private void UpdatePricesBasedOnReputation()
    {
        float reputationMultiplier = Mathf.Lerp(1.1f, 0.9f, _playerReputation / 100f);
        
        // Better reputation = lower fuel prices
        _fuelPrice = CalculateBaseFuelPrice() * reputationMultiplier;
    }
    
    // Events and weather
    public void UpdateEvents(float deltaTime)
    {
        // Check for event expiration
        for (int i = _activeEvents.Count - 1; i >= 0; i--)
        {
            var cityEvent = _activeEvents[i];
            if (cityEvent.IsExpired)
            {
                EndEvent(cityEvent);
            }
        }
        
        // Chance to start new events
        if ((DateTime.Now - _lastEventCheck).TotalHours >= 6f) // Check every 6 hours
        {
            TryStartRandomEvent();
            _lastEventCheck = DateTime.Now;
        }
        
        // Update weather periodically
        if (UnityEngine.Random.value < 0.01f) // Small chance each update
        {
            UpdateWeather();
        }
    }
    
    private void TryStartRandomEvent()
    {
        if (_activeEvents.Count >= 2) return; // Max 2 simultaneous events
        
        if (UnityEngine.Random.value < 0.1f) // 10% chance
        {
            var eventType = (CityEventType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(CityEventType)).Length);
            StartEvent(new CityEvent(eventType));
        }
    }
    
    public void StartEvent(CityEvent cityEvent)
    {
        if (cityEvent == null || _activeEvents.Contains(cityEvent)) return;
        
        _activeEvents.Add(cityEvent);
        _hasSpecialEvent = true;
        
        ApplyEventEffects(cityEvent);
        OnEventStarted?.Invoke(this, cityEvent);
    }
    
    public void EndEvent(CityEvent cityEvent)
    {
        if (cityEvent == null || !_activeEvents.Contains(cityEvent)) return;
        
        _activeEvents.Remove(cityEvent);
        _hasSpecialEvent = _activeEvents.Count > 0;
        
        RemoveEventEffects(cityEvent);
        OnEventEnded?.Invoke(this, cityEvent);
    }
    
    private void ApplyEventEffects(CityEvent cityEvent)
    {
        // Apply event-specific effects to city state
        switch (cityEvent.EventType)
        {
            case CityEventType.FuelShortage:
                _fuelPrice *= 1.5f;
                break;
            case CityEventType.EconomicBoom:
                _economicActivity *= 1.3f;
                break;
            case CityEventType.Strike:
                _economicActivity *= 0.7f;
                break;
            case CityEventType.Festival:
                UpdatePlayerReputation(10f);
                break;
        }
    }
    
    private void RemoveEventEffects(CityEvent cityEvent)
    {
        // Remove event-specific effects
        switch (cityEvent.EventType)
        {
            case CityEventType.FuelShortage:
                _fuelPrice = CalculateBaseFuelPrice();
                break;
            case CityEventType.EconomicBoom:
                _economicActivity = Mathf.Clamp01(_economicActivity / 1.3f);
                break;
            case CityEventType.Strike:
                _economicActivity = Mathf.Min(2f, _economicActivity / 0.7f);
                break;
        }
    }
    
    private void UpdateWeather()
    {
        var weatherTypes = System.Enum.GetValues(typeof(WeatherCondition));
        _currentWeather = (WeatherCondition)weatherTypes.GetValue(UnityEngine.Random.Range(0, weatherTypes.Length));
    }
    
    private float GetEventMultiplier(string effectType)
    {
        float multiplier = 1f;
        
        foreach (var cityEvent in _activeEvents)
        {
            multiplier *= cityEvent.GetEffectMultiplier(effectType);
        }
        
        return multiplier;
    }
    
    // Utility methods
    public List<VehicleInstance> GetAvailableVehicles()
    {
        return _stationedVehicles.Where(v => v.IsAvailable).ToList();
    }
    
    public List<ContractInstance> GetContractsForGood(GoodData good)
    {
        return _availableContracts.Where(c => c.ContractData?.CargoType == good).ToList();
    }
    
    public bool HasGood(GoodData good)
    {
        return _goodsAvailability.ContainsKey(good) && _goodsAvailability[good] > 0;
    }
    
    public int GetGoodAvailability(GoodData good)
    {
        return _goodsAvailability.ContainsKey(good) ? _goodsAvailability[good] : 0;
    }
    
    public float GetGoodPrice(GoodData good)
    {
        return _goodsPrices.ContainsKey(good) ? _goodsPrices[good] : good?.BaseValue ?? 0f;
    }
    
    public string GetCityInfo()
    {
        return $"{CityName}\n" +
               $"Population: {_populationSize:N0}\n" +
               $"Vehicles: {VehicleCount}\n" +
               $"Contracts: {AvailableContractCount}\n" +
               $"Reputation: {_playerReputation:F0}/100\n" +
               $"Fuel Price: {ModifiedFuelPrice:C2}/L\n" +
               $"Weather: {_currentWeather}";
    }
}

// AIDEV-NOTE: Enum for weather conditions affecting gameplay
public enum WeatherCondition
{
    Clear,
    Cloudy,
    Rain,
    Snow,
    Fog,
    Storm
}

// AIDEV-NOTE: Data structure for city events that affect gameplay
[System.Serializable]
public class CityEvent
{
    [SerializeField] private CityEventType _eventType;
    [SerializeField] private string _eventName;
    [SerializeField] private string _description;
    [SerializeField] private DateTime _startTime;
    [SerializeField] private DateTime _endTime;
    [SerializeField] private Dictionary<string, float> _effectMultipliers;
    
    public CityEventType EventType => _eventType;
    public string EventName => _eventName;
    public string Description => _description;
    public DateTime StartTime => _startTime;
    public DateTime EndTime => _endTime;
    public bool IsActive => DateTime.Now >= _startTime && DateTime.Now <= _endTime;
    public bool IsExpired => DateTime.Now > _endTime;
    
    public CityEvent(CityEventType eventType)
    {
        _eventType = eventType;
        _startTime = DateTime.Now;
        _endTime = _startTime.AddHours(UnityEngine.Random.Range(6f, 48f)); // 6-48 hour events
        _effectMultipliers = new Dictionary<string, float>();
        
        InitializeEvent();
    }
    
    private void InitializeEvent()
    {
        switch (_eventType)
        {
            case CityEventType.FuelShortage:
                _eventName = "Fuel Shortage";
                _description = "Limited fuel supply increases prices";
                _effectMultipliers["fuel_price"] = 1.5f;
                break;
            case CityEventType.EconomicBoom:
                _eventName = "Economic Boom";
                _description = "Increased business activity and contract generation";
                _effectMultipliers["contract_generation"] = 1.3f;
                _effectMultipliers["contract_rewards"] = 1.2f;
                break;
            case CityEventType.Strike:
                _eventName = "Worker Strike";
                _description = "Reduced economic activity affects contract availability";
                _effectMultipliers["contract_generation"] = 0.7f;
                break;
            case CityEventType.Festival:
                _eventName = "City Festival";
                _description = "Increased reputation gain and special contracts";
                _effectMultipliers["reputation_gain"] = 1.5f;
                break;
            case CityEventType.RoadConstruction:
                _eventName = "Road Construction";
                _description = "Slower travel speeds on routes from this city";
                _effectMultipliers["travel_speed"] = 0.8f;
                break;
        }
    }
    
    public float GetEffectMultiplier(string effectType)
    {
        return _effectMultipliers.ContainsKey(effectType) ? _effectMultipliers[effectType] : 1f;
    }
}

// AIDEV-NOTE: Enum for different types of city events
public enum CityEventType
{
    FuelShortage,
    EconomicBoom,
    Strike,
    Festival,
    RoadConstruction,
    WeatherDelay,
    SupplyShortage,
    TrafficJam
}

// AIDEV-NOTE: Serializable data structure for save/load operations
[System.Serializable]
public class CityInstanceData
{
    public string InstanceId;
    public CityData CityData;
    public bool IsDiscovered;
    public DateTime FirstVisitTime;
    public int TotalVisits;
    public List<string> StationedVehicleIds;
    public List<string> AvailableContractIds;
    public bool HasGasStation;
    public bool HasMaintenanceShop;
    public float FuelPrice;
    public float MaintenanceQuality;
    public WeatherCondition CurrentWeather;
    public float PlayerReputation;
    public int ContractsGenerated;
    public int ContractsCompleted;
    public float TotalRevenueGenerated;
    public List<CityEvent> ActiveEvents;
    
    public CityInstanceData(CityInstance instance)
    {
        InstanceId = instance.InstanceId;
        CityData = instance.CityData;
        IsDiscovered = instance.IsDiscovered;
        FirstVisitTime = instance.FirstVisitTime;
        TotalVisits = instance.TotalVisits;
        StationedVehicleIds = instance.StationedVehicles.Select(v => v.InstanceId).ToList();
        AvailableContractIds = instance.AvailableContracts.Select(c => c.InstanceId).ToList();
        HasGasStation = instance.HasGasStation;
        HasMaintenanceShop = instance.HasMaintenanceShop;
        FuelPrice = instance.FuelPrice;
        MaintenanceQuality = instance.MaintenanceQuality;
        CurrentWeather = instance.CurrentWeather;
        PlayerReputation = instance.PlayerReputation;
        ContractsGenerated = instance.ContractsGenerated;
        ContractsCompleted = instance.ContractsCompleted;
        TotalRevenueGenerated = instance.TotalRevenueGenerated;
        ActiveEvents = new List<CityEvent>(instance.ActiveEvents);
    }
}