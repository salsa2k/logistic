# Phase 9D - Fuel Consumption System

## Overview
Implement realistic fuel consumption calculations based on vehicle specifications, cargo weight, distance traveled, and driving conditions with save system integration.

## Tasks

### Fuel Consumption Algorithm
- Calculate fuel usage based on vehicle type and specifications
- Factor in cargo weight effects on fuel consumption
- Consider road conditions and driving efficiency
- Implement realistic consumption rates in metric units (liters)

### Vehicle-Specific Consumption
- Different consumption rates for each vehicle type
- Base consumption rates from vehicle specifications
- Efficiency factors based on vehicle condition and maintenance
- Speed-dependent consumption curves for realistic behavior

### Dynamic Consumption Factors
- Cargo weight impact on fuel efficiency
- Road grade and condition effects on consumption
- Speed optimization curves for fuel efficiency
- Traffic and driving pattern influences

### Real-Time Fuel Tracking
- Continuous fuel level updates during vehicle movement
- Fuel gauge updates for visual feedback
- Low fuel warnings and critical alerts
- Integration with vehicle status and movement systems

### Fuel Cost Calculations
- Calculate fuel costs based on consumption and current prices
- Track fuel expenses for contract profitability analysis
- Integration with credit system for fuel purchases
- Cost estimation for route planning and budgeting

### Emergency Fuel Management
- Out-of-fuel detection and vehicle stopping
- Emergency refuel system activation
- Fuel delivery and emergency service costs
- Vehicle stranding prevention and recovery

### Save System Integration
- Persistent fuel level storage for all vehicles
- Fuel consumption history tracking
- Cost tracking and financial impact recording
- State restoration for vehicles with varying fuel levels

## Acceptance Criteria

### Consumption Accuracy
- ✅ Fuel consumption rates are realistic and balanced
- ✅ Vehicle specifications accurately influence consumption
- ✅ Cargo weight effects are proportional and logical
- ✅ All calculations use metric units consistently

### Real-Time Updates
- ✅ Fuel levels update smoothly during vehicle movement
- ✅ Consumption calculations reflect actual distance traveled
- ✅ Low fuel warnings trigger at appropriate levels
- ✅ Emergency situations are detected and handled correctly

### System Integration
- ✅ Fuel system integrates properly with movement logic
- ✅ Cost calculations affect credit balance appropriately
- ✅ Vehicle status reflects fuel level accurately
- ✅ Save system maintains fuel state correctly

### User Experience
- ✅ Fuel information is clearly displayed and accessible
- ✅ Warning systems provide adequate notice of issues
- ✅ Emergency procedures are clear and effective
- ✅ Fuel management enhances strategic gameplay

## Technical Notes

### Component Structure
```
FuelConsumption/
├── FuelConsumptionEngine.cs (core consumption logic)
├── VehicleFuelManager.cs (per-vehicle fuel tracking)
├── ConsumptionCalculator.cs (consumption rate calculation)
├── FuelCostManager.cs (cost tracking and billing)
└── EmergencyFuelHandler.cs (emergency situations)
```

### Fuel Consumption Formula
```
FuelConsumption = BaseRate * DistanceFactor * WeightFactor * ConditionFactor
Where:
- BaseRate: Vehicle-specific consumption (L/100km)
- DistanceFactor: Distance traveled segment
- WeightFactor: 1.0 + (CargoWeight / MaxCapacity) * 0.3
- ConditionFactor: Road and driving condition multiplier
```

### Vehicle Consumption Rates
```
VehicleConsumptionRates:
- Cargo Van: 8 L/100km base consumption
- Delivery Truck: 12 L/100km base consumption
- Heavy Lorry: 18 L/100km base consumption
- Refrigerated Truck: 15 L/100km base consumption
- Flatbed Truck: 20 L/100km base consumption
- Tanker Truck: 22 L/100km base consumption
```

### Dynamic Factors
```
ConsumptionFactors:
- SpeedEfficiency: Optimal at 60-80 km/h, increases at extremes
- CargoWeight: Linear increase up to 30% at full capacity
- RoadConditions: 0.9 (good) to 1.3 (poor) multiplier
- TrafficConditions: 1.0 (free flow) to 1.5 (heavy traffic)
- VehicleCondition: 0.95 (excellent) to 1.2 (poor maintenance)
```

## Dependencies
- Requires Phase 9C (Vehicle Movement Logic) for distance tracking
- Requires vehicle specifications from data structures
- Requires credit system for fuel cost management

## Integration Points
- Movement system provides distance and speed data
- Vehicle management provides vehicle specifications
- Credit system handles fuel cost transactions
- Gas station system triggers fuel purchases

## Notes

### Consumption Calculation Process
1. Monitor vehicle movement and distance traveled
2. Calculate base consumption rate for vehicle type
3. Apply cargo weight multiplier to base rate
4. Apply road and driving condition factors
5. Deduct calculated fuel from vehicle tank
6. Update fuel level and trigger warnings if needed
7. Calculate fuel costs and update financial records

### Fuel Warning System
- **Full Tank**: 100% - Normal operation
- **Half Tank**: 50% - No warnings, normal status
- **Low Fuel**: 25% - Yellow warning, plan refueling
- **Critical**: 10% - Red alert, urgent refueling needed
- **Empty**: 0% - Vehicle stops, emergency refuel required

### Emergency Fuel Procedures
1. Vehicle runs out of fuel and stops immediately
2. Emergency refuel modal appears with options
3. Player can purchase emergency fuel at premium cost
4. Emergency fuel provides 20% tank capacity
5. Vehicle can resume movement after emergency refuel
6. Cost is 20% premium over nearest gas station price

### Cost Tracking and Analysis
- Track fuel costs per vehicle for profitability analysis
- Calculate fuel efficiency metrics for fleet optimization
- Provide fuel cost estimates for route planning
- Generate fuel expense reports for business analysis

### Fuel Efficiency Optimization
- Speed recommendations for optimal fuel efficiency
- Route planning consideration of fuel consumption
- Cargo loading optimization for fuel economy
- Vehicle selection guidance based on fuel efficiency

### Performance Considerations
- Efficient real-time consumption calculations
- Optimized fuel level updates without frame drops
- Memory-efficient tracking for large vehicle fleets
- Cached calculation results for repeated operations

### Testing Requirements
- Test consumption accuracy with various vehicle/cargo combinations
- Verify fuel level persistence through save/load cycles
- Test emergency fuel scenarios and recovery procedures
- Validate cost calculations and credit system integration
- Performance test with multiple vehicles consuming fuel simultaneously