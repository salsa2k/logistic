# Phase 1C - Data Structures

## Overview
Create ScriptableObject-based data structures for all game entities including Cities, Vehicles, Goods, Contracts, and Company data.

## Tasks

### Core Data ScriptableObjects
- Create CityData ScriptableObject with name, description, position, available goods
- Create VehicleData ScriptableObject with specifications (speed, fuel, weight capacity, price)
- Create GoodData ScriptableObject with name, description, icon, weight properties
- Create ContractData ScriptableObject with origin, destination, cargo, reward details
- Create CompanyData ScriptableObject with name, logo, credits, licenses

### Game State Data Structures
- Create GameState ScriptableObject for overall game progress
- Create PlayerProgress ScriptableObject for visited cities, owned vehicles
- Create SettingsData ScriptableObject for game preferences
- Create SaveData ScriptableObject as container for all persistent data

### Vehicle System Data
- Define VehicleInstance class for runtime vehicle state
- Include current position, fuel level, current weight, assigned contract
- Track vehicle status (moving, stopped, waiting)
- Implement vehicle movement state and pathfinding data

### Contract System Data
- Define ContractInstance class for runtime contract state
- Track acceptance status, completion status, assigned vehicle
- Include generation timestamp and expiration logic
- Store progress tracking and completion validation

### City and Road Network Data
- Create CityInstance class for runtime city state
- Track visited status, available contracts, stationed vehicles
- Define RoadNetwork data structure for pathfinding
- Store road segments with speed limits and distances

## Acceptance Criteria

### ScriptableObject Structure
- ✅ All base data types implemented as ScriptableObjects
- ✅ Proper serialization attributes for all fields
- ✅ Editor-friendly organization and naming
- ✅ Asset creation workflows established

### Data Validation
- ✅ Input validation for all data fields
- ✅ Logical constraints enforced (positive values, valid ranges)
- ✅ Cross-reference validation between related data
- ✅ Editor warnings for invalid data configurations

### Runtime Data Classes
- ✅ Instance classes for all runtime game entities
- ✅ Proper state tracking for game progress
- ✅ Efficient data access patterns
- ✅ Memory-efficient data structures

### Integration Readiness
- ✅ Data structures support save/load operations
- ✅ Clear interfaces for game systems to interact with data
- ✅ Event-driven data change notifications
- ✅ Modular design for easy extension

## Technical Notes

### Data Organization
- Store all ScriptableObjects in Assets/Data/ScriptableObjects/
- Use meaningful folder structure: Cities/, Vehicles/, Goods/, etc.
- Implement consistent naming conventions for asset files
- Create asset creation menus for easy data authoring

### Metric System Implementation
- All distances in kilometers
- All weights in kilograms  
- All fuel in liters
- All speeds in km/h
- Document unit expectations in code comments

### Performance Considerations
- Use structs for small data containers when appropriate
- Implement efficient lookup mechanisms for large data sets
- Consider memory allocation patterns for runtime instances
- Plan for data streaming if datasets become large

### Serialization Strategy
- Use Unity's built-in serialization system
- Ensure all data is properly serializable
- Handle version compatibility for save files
- Implement data migration strategies

## Dependencies
- Requires Phase 1A (Basic Unity Setup) for project structure
- Provides foundation for save/load systems

## Integration Points
- Save/Load systems will serialize these data structures
- All game systems will use these as data sources
- Asset Manager will provide access to ScriptableObject assets
- UI systems will display data from these structures

## Notes

### Business Logic Integration
- Cities must have different available goods for contract generation
- Vehicle specifications must be realistic and balanced
- Contract rewards should scale with distance and cargo weight
- Company data must support the license system for goods transport

### Predefined Data Requirements
From the original plan, implement these specific datasets:
- 10 cities: Port Vireo, Brunholt, Calderique, Nordhagen, Arelmoor, Sundale Ridge, Veltrona, Duskwell, New Halvern, Eastmere Bay
- 6 vehicle types: Cargo Van, Delivery Truck, Heavy Lorry, Refrigerated Truck, Flatbed Truck, Tanker Truck
- 5 goods types: Shopping Goods, Perishable Goods, Fragile Goods, Heavy Goods, Hazardous Materials
- License system for specialized goods transport

### Validation Rules
- Origin and destination cities must be different for contracts
- Vehicle weight capacity must accommodate contract cargo
- Fuel consumption must be realistic based on vehicle specifications
- Speed limits must be within realistic ranges (80-110 km/h for roads)