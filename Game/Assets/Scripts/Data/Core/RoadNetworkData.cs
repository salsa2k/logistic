using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// AIDEV-NOTE: ScriptableObject for road network data and pathfinding information
[CreateAssetMenu(fileName = "Road Network", menuName = "Logistics Game/Road Network Data")]
public class RoadNetworkData : ScriptableObject
{
    [Header("Network Information")]
    [SerializeField] private string _networkName;
    [SerializeField, TextArea(2, 4)] private string _description;
    [SerializeField] private List<CityData> _cities = new List<CityData>();
    
    [Header("Road Segments")]
    [SerializeField] private List<RoadSegment> _roadSegments = new List<RoadSegment>();
    [SerializeField] private List<RoadJunction> _junctions = new List<RoadJunction>();
    
    [Header("Network Settings")]
    [SerializeField] private float _defaultSpeedLimit = 90f; // km/h
    [SerializeField] private float _pathfindingPrecision = 1f; // km between waypoints
    [SerializeField] private bool _allowReverseTravel = true;
    
    [Header("Traffic Simulation")]
    [SerializeField] private float _baseTrafficDensity = 0.3f; // 0-1
    [SerializeField] private AnimationCurve _trafficVariation; // Traffic over time of day
    [SerializeField] private Dictionary<string, float> _cityTrafficMultipliers = new Dictionary<string, float>();
    
    // Properties
    public string NetworkName => _networkName;
    public string Description => _description;
    public List<CityData> Cities => _cities;
    public List<RoadSegment> RoadSegments => _roadSegments;
    public List<RoadJunction> Junctions => _junctions;
    public float DefaultSpeedLimit => _defaultSpeedLimit;
    public float PathfindingPrecision => _pathfindingPrecision;
    public bool AllowReverseTravel => _allowReverseTravel;
    public float BaseTrafficDensity => _baseTrafficDensity;
    public AnimationCurve TrafficVariation => _trafficVariation;
    
    // Cached data for performance
    private Dictionary<string, List<RoadSegment>> _cityConnections;
    private Dictionary<string, float> _precomputedDistances;
    private bool _cacheInitialized = false;
    
    // Events
    public static event Action<RoadNetworkData> OnNetworkChanged;
    
    // Initialization
    private void OnEnable()
    {
        InitializeCache();
    }
    
    private void InitializeCache()
    {
        if (_cacheInitialized) return;
        
        _cityConnections = new Dictionary<string, List<RoadSegment>>();
        _precomputedDistances = new Dictionary<string, float>();
        
        // Build city connections cache
        foreach (var city in _cities)
        {
            if (city == null) continue;
            
            string cityKey = city.CityName;
            _cityConnections[cityKey] = GetRoadSegmentsForCity(city);
        }
        
        // Precompute distances between all city pairs
        PrecomputeDistances();
        
        _cacheInitialized = true;
    }
    
    // Network building methods
    public void AddCity(CityData city)
    {
        if (city != null && !_cities.Contains(city))
        {
            _cities.Add(city);
            InvalidateCache();
        }
    }
    
    public void RemoveCity(CityData city)
    {
        if (_cities.Remove(city))
        {
            // Remove all road segments connected to this city
            _roadSegments.RemoveAll(segment => segment.StartCity == city || segment.EndCity == city);
            InvalidateCache();
        }
    }
    
    public RoadSegment AddRoadSegment(CityData startCity, CityData endCity, float speedLimit = 0f)
    {
        if (startCity == null || endCity == null || startCity == endCity) return null;
        
        // Check if segment already exists
        var existingSegment = GetRoadSegment(startCity, endCity);
        if (existingSegment != null) return existingSegment;
        
        var roadSegment = new RoadSegment(startCity, endCity, speedLimit > 0f ? speedLimit : _defaultSpeedLimit);
        _roadSegments.Add(roadSegment);
        
        InvalidateCache();
        return roadSegment;
    }
    
    public void RemoveRoadSegment(RoadSegment segment)
    {
        if (_roadSegments.Remove(segment))
        {
            InvalidateCache();
        }
    }
    
    public RoadJunction AddJunction(Vector2 position, string junctionName = "")
    {
        var junction = new RoadJunction(position, junctionName);
        _junctions.Add(junction);
        return junction;
    }
    
    // Pathfinding methods
    public List<Vector2> FindPath(CityData startCity, CityData endCity)
    {
        if (startCity == null || endCity == null || startCity == endCity)
            return new List<Vector2>();
        
        InitializeCache();
        
        // Use A* pathfinding algorithm
        var path = FindPathAStar(startCity, endCity);
        
        if (path == null || path.Count == 0)
        {
            // Fallback to direct path if no road connection exists
            Debug.LogWarning($"No road path found from {startCity.CityName} to {endCity.CityName}, using direct route");
            return new List<Vector2> { startCity.Position, endCity.Position };
        }
        
        return path;
    }
    
    private List<Vector2> FindPathAStar(CityData startCity, CityData endCity)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<string>();
        var cameFrom = new Dictionary<string, PathNode>();
        
        var startNode = new PathNode(startCity, 0f, Vector2.Distance(startCity.Position, endCity.Position));
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            // Get node with lowest F score
            var currentNode = openSet.OrderBy(node => node.FScore).First();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.City.CityName);
            
            // Check if we reached the destination
            if (currentNode.City == endCity)
            {
                return ReconstructPath(cameFrom, currentNode);
            }
            
            // Check neighbors
            foreach (var segment in GetRoadSegmentsForCity(currentNode.City))
            {
                var neighbor = segment.GetOtherCity(currentNode.City);
                if (neighbor == null || closedSet.Contains(neighbor.CityName))
                    continue;
                
                float tentativeGScore = currentNode.GScore + segment.Distance;
                
                var existingNode = openSet.FirstOrDefault(node => node.City == neighbor);
                if (existingNode != null && tentativeGScore >= existingNode.GScore)
                    continue;
                
                // This path to neighbor is better than any previous one
                var neighborNode = new PathNode(neighbor, tentativeGScore, Vector2.Distance(neighbor.Position, endCity.Position));
                
                if (existingNode != null)
                {
                    openSet.Remove(existingNode);
                }
                
                openSet.Add(neighborNode);
                cameFrom[neighbor.CityName] = currentNode;
            }
        }
        
        return new List<Vector2>(); // No path found
    }
    
    private List<Vector2> ReconstructPath(Dictionary<string, PathNode> cameFrom, PathNode endNode)
    {
        var path = new List<Vector2>();
        var current = endNode;
        
        while (current != null)
        {
            path.Insert(0, current.City.Position);
            
            if (cameFrom.ContainsKey(current.City.CityName))
            {
                current = cameFrom[current.City.CityName];
            }
            else
            {
                break;
            }
        }
        
        // Add waypoints along road segments for more realistic travel
        return GenerateWaypoints(path);
    }
    
    private List<Vector2> GenerateWaypoints(List<Vector2> cityPath)
    {
        if (cityPath.Count < 2) return cityPath;
        
        var waypoints = new List<Vector2>();
        
        for (int i = 0; i < cityPath.Count - 1; i++)
        {
            Vector2 start = cityPath[i];
            Vector2 end = cityPath[i + 1];
            
            // Add start point
            waypoints.Add(start);
            
            // Add intermediate waypoints based on precision setting
            float distance = Vector2.Distance(start, end);
            int waypointCount = Mathf.FloorToInt(distance / _pathfindingPrecision);
            
            for (int j = 1; j < waypointCount; j++)
            {
                float t = (float)j / waypointCount;
                Vector2 waypoint = Vector2.Lerp(start, end, t);
                
                // Add some variation to make roads feel more natural
                waypoint += GetRoadVariation(waypoint, distance) * 0.1f;
                waypoints.Add(waypoint);
            }
        }
        
        // Add final destination
        waypoints.Add(cityPath[cityPath.Count - 1]);
        
        return waypoints;
    }
    
    private Vector2 GetRoadVariation(Vector2 position, float segmentDistance)
    {
        // Generate slight curves in roads based on position
        float noise = Mathf.PerlinNoise(position.x * 0.01f, position.y * 0.01f);
        float angle = noise * Mathf.PI * 2f;
        float magnitude = Mathf.Min(segmentDistance * 0.05f, 2f); // Max 2km deviation
        
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * magnitude;
    }
    
    // Helper methods
    private List<RoadSegment> GetRoadSegmentsForCity(CityData city)
    {
        if (_cityConnections != null && _cityConnections.ContainsKey(city.CityName))
        {
            return _cityConnections[city.CityName];
        }
        
        return _roadSegments.Where(segment => segment.StartCity == city || segment.EndCity == city).ToList();
    }
    
    public RoadSegment GetRoadSegment(CityData startCity, CityData endCity)
    {
        return _roadSegments.FirstOrDefault(segment => 
            (segment.StartCity == startCity && segment.EndCity == endCity) ||
            (_allowReverseTravel && segment.StartCity == endCity && segment.EndCity == startCity));
    }
    
    public float GetDistance(CityData startCity, CityData endCity)
    {
        if (startCity == null || endCity == null) return float.MaxValue;
        if (startCity == endCity) return 0f;
        
        InitializeCache();
        
        string key = $"{startCity.CityName}-{endCity.CityName}";
        if (_precomputedDistances.ContainsKey(key))
        {
            return _precomputedDistances[key];
        }
        
        // Fallback to direct distance
        return Vector2.Distance(startCity.Position, endCity.Position);
    }
    
    private void PrecomputeDistances()
    {
        // Precompute shortest distances between all city pairs using Floyd-Warshall
        var cityCount = _cities.Count;
        var distances = new float[cityCount, cityCount];
        var cityIndexMap = new Dictionary<string, int>();
        
        // Initialize city index mapping
        for (int i = 0; i < cityCount; i++)
        {
            cityIndexMap[_cities[i].CityName] = i;
        }
        
        // Initialize distance matrix
        for (int i = 0; i < cityCount; i++)
        {
            for (int j = 0; j < cityCount; j++)
            {
                if (i == j)
                {
                    distances[i, j] = 0f;
                }
                else
                {
                    var segment = GetRoadSegment(_cities[i], _cities[j]);
                    distances[i, j] = segment?.Distance ?? float.MaxValue;
                }
            }
        }
        
        // Floyd-Warshall algorithm
        for (int k = 0; k < cityCount; k++)
        {
            for (int i = 0; i < cityCount; i++)
            {
                for (int j = 0; j < cityCount; j++)
                {
                    if (distances[i, k] != float.MaxValue && distances[k, j] != float.MaxValue)
                    {
                        float newDistance = distances[i, k] + distances[k, j];
                        if (newDistance < distances[i, j])
                        {
                            distances[i, j] = newDistance;
                        }
                    }
                }
            }
        }
        
        // Store results in cache
        for (int i = 0; i < cityCount; i++)
        {
            for (int j = 0; j < cityCount; j++)
            {
                if (i != j)
                {
                    string key = $"{_cities[i].CityName}-{_cities[j].CityName}";
                    _precomputedDistances[key] = distances[i, j];
                }
            }
        }
    }
    
    // Traffic simulation
    public float GetTrafficMultiplier(Vector2 position, float timeOfDay)
    {
        float baseTraffic = _baseTrafficDensity;
        
        // Apply time-of-day variation
        if (_trafficVariation != null)
        {
            float normalizedTime = timeOfDay / 24f; // 0-1
            baseTraffic *= _trafficVariation.Evaluate(normalizedTime);
        }
        
        // Apply city-specific multipliers
        var nearestCity = GetNearestCity(position);
        if (nearestCity != null && _cityTrafficMultipliers.ContainsKey(nearestCity.CityName))
        {
            baseTraffic *= _cityTrafficMultipliers[nearestCity.CityName];
        }
        
        return Mathf.Clamp(baseTraffic, 0.1f, 2f); // Traffic can slow speed by up to 90% or increase by up to 100%
    }
    
    public CityData GetNearestCity(Vector2 position)
    {
        CityData nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var city in _cities)
        {
            if (city == null) continue;
            
            float distance = Vector2.Distance(position, city.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = city;
            }
        }
        
        return nearest;
    }
    
    public void SetCityTrafficMultiplier(string cityName, float multiplier)
    {
        _cityTrafficMultipliers[cityName] = Mathf.Clamp(multiplier, 0.1f, 3f);
    }
    
    // Network validation and utilities
    public bool ValidateNetwork()
    {
        var errors = new List<string>();
        
        // Check for disconnected cities
        foreach (var city in _cities)
        {
            if (GetRoadSegmentsForCity(city).Count == 0)
            {
                errors.Add($"City {city.CityName} has no road connections");
            }
        }
        
        // Check for invalid road segments
        foreach (var segment in _roadSegments)
        {
            if (segment.StartCity == null || segment.EndCity == null)
            {
                errors.Add("Road segment with null city reference");
            }
            else if (segment.StartCity == segment.EndCity)
            {
                errors.Add($"Road segment connects {segment.StartCity.CityName} to itself");
            }
        }
        
        if (errors.Count > 0)
        {
            Debug.LogWarning($"Road network validation failed:\n{string.Join("\n", errors)}");
            return false;
        }
        
        return true;
    }
    
    public void GenerateFullyConnectedNetwork()
    {
        // Connect every city to every other city with direct roads
        for (int i = 0; i < _cities.Count; i++)
        {
            for (int j = i + 1; j < _cities.Count; j++)
            {
                AddRoadSegment(_cities[i], _cities[j]);
            }
        }
    }
    
    public void GenerateMinimumSpanningTree()
    {
        // Generate a minimum spanning tree to ensure all cities are connected
        // with the minimum total road distance
        
        if (_cities.Count < 2) return;
        
        var edges = new List<RoadEdge>();
        
        // Create all possible edges
        for (int i = 0; i < _cities.Count; i++)
        {
            for (int j = i + 1; j < _cities.Count; j++)
            {
                float distance = Vector2.Distance(_cities[i].Position, _cities[j].Position);
                edges.Add(new RoadEdge(_cities[i], _cities[j], distance));
            }
        }
        
        // Sort edges by distance
        edges.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        
        // Use Kruskal's algorithm to find minimum spanning tree
        var parent = new Dictionary<CityData, CityData>();
        foreach (var city in _cities)
        {
            parent[city] = city;
        }
        
        _roadSegments.Clear();
        
        foreach (var edge in edges)
        {
            var rootA = FindRoot(parent, edge.CityA);
            var rootB = FindRoot(parent, edge.CityB);
            
            if (rootA != rootB)
            {
                AddRoadSegment(edge.CityA, edge.CityB);
                parent[rootA] = rootB;
            }
        }
    }
    
    private CityData FindRoot(Dictionary<CityData, CityData> parent, CityData city)
    {
        if (parent[city] != city)
        {
            parent[city] = FindRoot(parent, parent[city]);
        }
        return parent[city];
    }
    
    private void InvalidateCache()
    {
        _cacheInitialized = false;
        _cityConnections?.Clear();
        _precomputedDistances?.Clear();
        OnNetworkChanged?.Invoke(this);
    }
    
    // Validation
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_networkName))
        {
            _networkName = "Road Network";
        }
        
        _defaultSpeedLimit = Mathf.Clamp(_defaultSpeedLimit, 20f, 150f);
        _pathfindingPrecision = Mathf.Clamp(_pathfindingPrecision, 0.1f, 10f);
        _baseTrafficDensity = Mathf.Clamp01(_baseTrafficDensity);
        
        // Initialize traffic variation curve if not set
        if (_trafficVariation == null || _trafficVariation.keys.Length == 0)
        {
            _trafficVariation = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0.5f);
            // Add typical traffic pattern (higher during day, lower at night)
            _trafficVariation = new AnimationCurve(
                new Keyframe(0f, 0.3f),      // Midnight - low traffic
                new Keyframe(0.25f, 0.5f),   // 6 AM - increasing
                new Keyframe(0.375f, 1.2f),  // 9 AM - rush hour
                new Keyframe(0.5f, 0.8f),    // Noon - moderate
                new Keyframe(0.75f, 1.1f),   // 6 PM - evening rush
                new Keyframe(1f, 0.3f)       // Midnight - low traffic
            );
        }
        
        InvalidateCache();
    }
}

// AIDEV-NOTE: Data structure for road segments connecting cities
[System.Serializable]
public class RoadSegment
{
    [SerializeField] private CityData _startCity;
    [SerializeField] private CityData _endCity;
    [SerializeField] private float _speedLimit; // km/h
    [SerializeField] private float _distance; // km
    [SerializeField] private RoadType _roadType = RoadType.Highway;
    [SerializeField] private List<Vector2> _waypoints = new List<Vector2>();
    [SerializeField] private bool _isTollRoad = false;
    [SerializeField] private float _tollCost = 0f;
    
    public CityData StartCity => _startCity;
    public CityData EndCity => _endCity;
    public float SpeedLimit => _speedLimit;
    public float Distance => _distance;
    public RoadType RoadType => _roadType;
    public List<Vector2> Waypoints => _waypoints;
    public bool IsTollRoad => _isTollRoad;
    public float TollCost => _tollCost;
    
    public RoadSegment(CityData startCity, CityData endCity, float speedLimit)
    {
        _startCity = startCity;
        _endCity = endCity;
        _speedLimit = speedLimit;
        _distance = Vector2.Distance(startCity.Position, endCity.Position);
        _waypoints = new List<Vector2> { startCity.Position, endCity.Position };
    }
    
    public CityData GetOtherCity(CityData city)
    {
        if (city == _startCity) return _endCity;
        if (city == _endCity) return _startCity;
        return null;
    }
    
    public float GetEffectiveSpeedLimit(float trafficMultiplier = 1f)
    {
        return _speedLimit * trafficMultiplier;
    }
}

// AIDEV-NOTE: Data structure for road junctions/intersections
[System.Serializable]
public class RoadJunction
{
    [SerializeField] private Vector2 _position;
    [SerializeField] private string _junctionName;
    [SerializeField] private List<RoadSegment> _connectedSegments = new List<RoadSegment>();
    [SerializeField] private float _trafficDelayMultiplier = 1f;
    
    public Vector2 Position => _position;
    public string JunctionName => _junctionName;
    public List<RoadSegment> ConnectedSegments => _connectedSegments;
    public float TrafficDelayMultiplier => _trafficDelayMultiplier;
    
    public RoadJunction(Vector2 position, string name = "")
    {
        _position = position;
        _junctionName = string.IsNullOrEmpty(name) ? $"Junction {position.x:F0},{position.y:F0}" : name;
    }
}

// AIDEV-NOTE: Helper classes for pathfinding
public class PathNode
{
    public CityData City { get; }
    public float GScore { get; } // Cost from start
    public float HScore { get; } // Heuristic cost to end
    public float FScore => GScore + HScore; // Total cost
    
    public PathNode(CityData city, float gScore, float hScore)
    {
        City = city;
        GScore = gScore;
        HScore = hScore;
    }
}

public class RoadEdge
{
    public CityData CityA { get; }
    public CityData CityB { get; }
    public float Distance { get; }
    
    public RoadEdge(CityData cityA, CityData cityB, float distance)
    {
        CityA = cityA;
        CityB = cityB;
        Distance = distance;
    }
}

// AIDEV-NOTE: Enum for different types of roads
public enum RoadType
{
    Highway,        // High speed, good condition
    MainRoad,       // Medium speed, good condition
    SecondaryRoad,  // Lower speed, moderate condition
    UrbanStreet,    // Low speed, frequent stops
    DirtRoad        // Very low speed, poor condition
}