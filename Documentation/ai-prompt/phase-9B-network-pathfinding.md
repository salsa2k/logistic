# Phase 9B - Network Pathfinding

## Overview
Implement advanced pathfinding system using the connected road network to calculate optimal routes between cities with multiple route options and real-time recalculation.

## Tasks

### Pathfinding Algorithm Implementation
- Implement A* pathfinding algorithm optimized for road networks
- Support multiple route calculation criteria (fastest, shortest, economical)
- Handle dynamic route recalculation for changing conditions
- Optimize pathfinding performance for real-time operations

### Route Optimization Strategies
- Fastest route: Minimize travel time considering speed limits
- Shortest route: Minimize total distance traveled
- Economical route: Minimize fuel consumption and costs
- Balanced route: Optimize multiple factors simultaneously

### Dynamic Route Calculation
- Real-time route recalculation when conditions change
- Alternative route generation for traffic or road conditions
- Route validation and fallback for blocked or invalid paths
- Adaptive routing based on vehicle capabilities and restrictions

### Route Caching and Optimization
- Cache frequently calculated routes for performance
- Pre-calculate common city-pair routes during initialization
- Optimize pathfinding data structures for fast access
- Implement hierarchical pathfinding for long-distance routes

### Vehicle-Specific Routing
- Route calculation based on vehicle type and capabilities
- Consider vehicle speed limitations in route planning
- Factor in fuel capacity and consumption for route viability
- Handle specialized vehicle requirements and restrictions

### Route Quality Assessment
- Route scoring system for comparing alternative paths
- Traffic congestion and road condition factors
- Route difficulty assessment for different vehicle types
- Time-of-day and seasonal route adjustments

### Integration with Game Systems
- Contract route planning for delivery estimates
- Vehicle movement system integration for navigation
- Fuel consumption calculation for route planning
- Save system integration for route caching

## Acceptance Criteria

### Algorithm Performance
- ✅ Pathfinding calculates routes quickly for all city pairs
- ✅ Multiple route options provide meaningful alternatives
- ✅ Route quality reflects realistic travel considerations
- ✅ Real-time recalculation maintains smooth gameplay

### Route Accuracy
- ✅ Calculated routes follow valid road network connections
- ✅ Route distances and times are accurate and consistent
- ✅ Vehicle-specific routing considers appropriate constraints
- ✅ Route optimization produces sensible results

### System Integration
- ✅ Pathfinding integrates seamlessly with vehicle movement
- ✅ Route data supports fuel consumption calculations
- ✅ Contract planning uses accurate route estimates
- ✅ Save system maintains route cache efficiently

### Performance Optimization
- ✅ Route calculation doesn't cause gameplay delays
- ✅ Caching system improves performance for repeated calculations
- ✅ Memory usage is optimized for pathfinding operations
- ✅ Large road networks don't impact performance significantly

## Technical Notes

### Component Structure
```
NetworkPathfinding/
├── PathfindingEngine.cs (core algorithm implementation)
├── RouteCalculator.cs (route optimization strategies)
├── RouteCache.cs (caching and performance optimization)
├── RouteValidator.cs (route verification and fallback)
└── PathfindingData.cs (optimized data structures)
```

### Pathfinding Algorithm
```
AStarImplementation:
- OpenSet: Nodes to be evaluated
- ClosedSet: Nodes already evaluated
- GScore: Cost from start to current node
- FScore: GScore + heuristic estimate to goal
- Heuristic: Euclidean distance with speed factor
- PathReconstruction: Backtrack optimal path
```

### Route Calculation Types
```
RouteTypes:
- Fastest: Minimize (Distance / SpeedLimit) + TrafficDelay
- Shortest: Minimize Distance
- Economical: Minimize FuelConsumption * FuelPrice
- Balanced: Weighted combination of time, distance, cost
```

### Route Data Structure
```
CalculatedRoute:
- RouteId: Unique route identifier
- OriginCity: Starting city
- DestinationCity: Target city
- RouteSegments: Ordered list of road segments
- TotalDistance: Route distance in kilometers
- EstimatedTime: Travel time considering speed limits
- FuelRequired: Estimated fuel consumption
- RouteDifficulty: Complexity rating for route
```

### Caching Strategy
- Route cache with LRU eviction policy
- Pre-cached routes for common city pairs
- Invalidation triggers for network changes
- Persistent cache storage for session continuity

## Dependencies
- Requires Phase 8B (Connected Road Network) for road data
- Requires Phase 8A (2D Map Foundation) for city positions
- Will integrate with vehicle movement and fuel systems

## Integration Points
- Road network provides pathfinding graph data
- Vehicle movement system executes calculated routes
- Fuel consumption system uses route data for estimates
- Contract system uses routes for delivery planning

## Notes

### Algorithm Optimization
- A* with Euclidean distance heuristic
- Early termination for obviously suboptimal paths
- Bidirectional search for long-distance routes
- Hierarchical pathfinding for complex networks

### Route Quality Factors
- **Distance**: Shorter routes generally preferred
- **Speed Limits**: Higher speed limits reduce travel time
- **Road Conditions**: Better roads improve efficiency
- **Traffic Density**: Congestion affects travel time
- **Vehicle Compatibility**: Route suitability for vehicle type

### Route Caching Strategy
- Cache routes for all city pairs at startup
- Update cache when road conditions change
- Store cache data with save files for persistence
- Memory-efficient cache size management

### Vehicle-Specific Considerations
- **Speed Limitations**: Vehicle max speed vs road speed limits
- **Fuel Capacity**: Route viability based on fuel tank size
- **Cargo Restrictions**: Hazardous materials route limitations
- **Vehicle Size**: Bridge and road capacity restrictions

### Dynamic Route Updates
- Traffic condition changes affecting route timing
- Road closures or construction affecting availability
- Weather conditions impacting route safety
- Emergency situations requiring route alternatives

### Performance Optimization
- Optimized graph data structures for fast traversal
- Parallel processing for multiple route calculations
- Memory pooling for pathfinding operations
- Incremental updates for network changes

### Testing Requirements
- Test pathfinding accuracy for all city combinations
- Verify route optimization produces expected results
- Test performance with large road networks
- Validate cache efficiency and persistence
- Test dynamic route recalculation scenarios