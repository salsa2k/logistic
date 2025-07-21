# Phase 9E - Speed Calculation System

## Overview
Implement dynamic speed calculation system that determines vehicle speed based on road conditions, vehicle specifications, cargo weight, and traffic factors with police enforcement integration.

## Tasks

### Speed Calculation Engine
- Dynamic speed calculation based on multiple factors
- Vehicle maximum speed vs road speed limit enforcement
- Real-time speed adjustment for changing conditions
- Integration with police checkpoint system for violation detection

### Vehicle Speed Specifications
- Individual maximum speed for each vehicle type
- Vehicle-specific acceleration and deceleration rates
- Speed limitations based on cargo type and weight
- Maintenance and condition effects on maximum speed

### Road-Based Speed Factors
- Road speed limit enforcement (80-110 km/h range)
- Road condition impact on safe travel speeds
- Weather and visibility effects on speed limits
- Traffic density influence on actual travel speeds

### Cargo Weight Impact
- Speed reduction based on vehicle loading
- Different cargo types affecting speed capabilities
- Dynamic adjustment as cargo is loaded/unloaded
- Safety speed limits for hazardous materials

### Speed Monitoring and Enforcement
- Real-time speed tracking for police checkpoint system
- Speeding violation detection and recording
- Speed warning system for approaching checkpoints
- Integration with fine system for violations

### Traffic and Condition Modifiers
- Traffic congestion speed reduction
- Road construction and temporary speed limits
- Weather condition speed adjustments
- Time-of-day traffic pattern effects

### Save System Integration
- Current speed and target speed persistence
- Speed violation history tracking
- Traffic condition state storage
- Speed-related preferences and settings

## Acceptance Criteria

### Speed Accuracy
- ✅ Speed calculations reflect realistic vehicle behavior
- ✅ Road speed limits are enforced appropriately
- ✅ Cargo weight impact is proportional and logical
- ✅ All speed values use metric units (km/h) consistently

### Dynamic Adjustment
- ✅ Speed adjusts appropriately for changing conditions
- ✅ Real-time updates maintain smooth vehicle movement
- ✅ Speed limits are respected across different road types
- ✅ Emergency speed reductions work correctly

### Police Integration
- ✅ Speed monitoring integrates with checkpoint system
- ✅ Violation detection is accurate and fair
- ✅ Speed warnings help prevent violations
- ✅ Fine calculations reflect actual speed violations

### Performance
- ✅ Speed calculations maintain smooth frame rates
- ✅ Real-time updates don't impact game performance
- ✅ Multiple vehicle speed tracking is efficient
- ✅ Memory usage is optimized for speed operations

## Technical Notes

### Component Structure
```
SpeedCalculation/
├── SpeedCalculationEngine.cs (core speed logic)
├── VehicleSpeedManager.cs (per-vehicle speed tracking)
├── RoadSpeedEnforcer.cs (speed limit enforcement)
├── CargoSpeedImpact.cs (weight-based adjustments)
└── SpeedViolationTracker.cs (police integration)
```

### Speed Calculation Formula
```
EffectiveSpeed = Min(VehicleMaxSpeed, RoadSpeedLimit) * CargoFactor * ConditionFactor
Where:
- VehicleMaxSpeed: Vehicle specification maximum
- RoadSpeedLimit: Current road segment limit
- CargoFactor: Weight-based speed reduction (0.7-1.0)
- ConditionFactor: Road/weather/traffic modifier (0.5-1.0)
```

### Vehicle Speed Specifications
```
VehicleMaxSpeeds:
- Cargo Van: 100 km/h maximum speed
- Delivery Truck: 90 km/h maximum speed
- Heavy Lorry: 80 km/h maximum speed
- Refrigerated Truck: 85 km/h maximum speed
- Flatbed Truck: 75 km/h maximum speed
- Tanker Truck: 70 km/h maximum speed
```

### Speed Reduction Factors
```
SpeedFactors:
- CargoWeight: 1.0 (empty) to 0.7 (full capacity)
- RoadConditions: 1.0 (perfect) to 0.6 (poor)
- Weather: 1.0 (clear) to 0.5 (severe)
- Traffic: 1.0 (free flow) to 0.3 (gridlock)
- Visibility: 1.0 (day) to 0.7 (night/fog)
```

## Dependencies
- Requires Phase 8B (Connected Road Network) for speed limits
- Requires Phase 9C (Vehicle Movement Logic) for speed application
- Will integrate with police checkpoint system

## Integration Points
- Road network provides speed limit data for segments
- Vehicle movement applies calculated speeds to movement
- Police checkpoints monitor vehicle speeds for violations
- Cargo system provides weight data for speed calculations

## Notes

### Speed Limit Enforcement
- Road segments have defined speed limits (80-110 km/h)
- Vehicle specifications limit maximum achievable speed
- Effective speed is minimum of vehicle max and road limit
- Special speed zones around cities and hazards

### Cargo Impact on Speed
- Empty vehicles can achieve maximum rated speed
- Full capacity reduces speed by up to 30%
- Hazardous materials may have additional speed restrictions
- Fragile cargo requires reduced speeds for safety

### Police Speed Monitoring
- Continuous speed tracking for moving vehicles
- Violation detection when exceeding speed limits
- Grace period or tolerance for minor violations
- Speed warning system for approaching checkpoints

### Dynamic Speed Adjustment
1. Calculate base vehicle maximum speed
2. Apply road speed limit constraints
3. Calculate cargo weight impact factor
4. Apply road and weather condition modifiers
5. Apply traffic density speed reduction
6. Update vehicle target speed for movement system
7. Monitor for speed violations and warnings

### Speed Violation System
- Violation threshold: 5+ km/h over speed limit
- Fine calculation based on violation severity
- Warning system for vehicles approaching checkpoints
- Violation history tracking for repeat offenders

### Performance Optimization
- Cached speed calculations for identical conditions
- Efficient speed monitoring for multiple vehicles
- Optimized condition checking for speed factors
- Smooth speed transitions without sudden changes

### Testing Requirements
- Test speed calculations with various vehicle/cargo combinations
- Verify speed limit enforcement accuracy
- Test police integration and violation detection
- Validate performance with multiple vehicles
- Test dynamic speed adjustment for changing conditions