# Phase 10B - Gas Station Interaction

## Overview
Implement gas station interaction system allowing vehicles to refuel during transit with automatic detection, refuel selection, and credit processing integration.

## Tasks

### Gas Station Detection System
- Automatic detection when vehicles approach gas stations
- Vehicle proximity calculation for refuel opportunity
- Gas station availability and service status checking
- Integration with vehicle movement and route systems

### Refuel Selection Interface
- Modal dialog for gas station refuel options
- Display gas station brand, fuel price, and services
- Refuel amount selection (partial or full tank)
- Cost calculation and credit validation before refuel

### Automatic Refuel Processing
- Vehicles marked for refuel automatically stop at gas stations
- Refuel checkbox system for pre-selecting refuel stops
- Automatic credit deduction when vehicle passes marked station
- Fuel tank filling and status updates during refuel

### Refuel Animation and Timing
- Vehicle stop animation at gas station
- Refuel progress indicator during fuel addition
- Realistic refuel timing (few seconds pause)
- Vehicle resume movement after refuel completion

### Credit Integration
- Real-time credit validation before refuel operations
- Automatic credit deduction during refuel process
- Negative credit handling for insufficient funds
- Transaction logging for fuel purchase tracking

### Gas Station Service Levels
- Different service levels by brand affecting refuel speed
- Brand loyalty programs and discount systems
- Premium services with additional benefits
- Emergency refuel services with premium pricing

### Save System Integration
- Refuel status and selection persistence
- Transaction history storage for analysis
- Gas station usage tracking and preferences
- Vehicle fuel level persistence across sessions

## Acceptance Criteria

### Detection and Selection
- ✅ Gas stations are detected accurately when vehicles approach
- ✅ Refuel interface displays correct information and pricing
- ✅ Refuel selection system is intuitive and responsive
- ✅ Credit validation prevents invalid refuel attempts

### Refuel Process
- ✅ Automatic refuel works seamlessly during vehicle movement
- ✅ Manual refuel provides immediate fuel and cost feedback
- ✅ Vehicle stops and resumes movement smoothly during refuel
- ✅ Fuel levels update accurately after refuel operations

### Financial Integration
- ✅ Credit deduction processes correctly for all refuel types
- ✅ Pricing calculations are accurate for different brands
- ✅ Negative credit scenarios are handled appropriately
- ✅ Transaction records are maintained for analysis

### User Experience
- ✅ Refuel process feels realistic and immersive
- ✅ Gas station choices provide meaningful gameplay decisions
- ✅ Refuel interface is accessible and clear
- ✅ Automatic systems reduce micromanagement burden

## Technical Notes

### Component Structure
```
GasStationInteraction/
├── StationDetector.cs (proximity detection)
├── RefuelInterface.cs (refuel selection UI)
├── RefuelProcessor.cs (refuel operation handling)
├── RefuelAnimator.cs (visual effects and timing)
└── RefuelTransaction.cs (credit processing)
```

### Refuel Detection System
```
RefuelDetection:
- ProximityRadius: 50 units from gas station
- VehicleSpeed: Detection when speed < 5 km/h near station
- RouteAlignment: Vehicle must be on road segment with station
- RefuelMarked: Vehicle has refuel checkbox selected
- AutoTrigger: Automatic refuel when all conditions met
```

### Refuel Interface Data
```
RefuelInterfaceData:
- StationBrand: Gas station brand identifier
- FuelPrice: Price per liter for current brand
- CurrentFuel: Vehicle's current fuel level
- TankCapacity: Vehicle's maximum fuel capacity
- RefuelAmount: Selected amount to refuel
- TotalCost: Calculated cost for selected refuel
- PlayerCredits: Current player credit balance
```

### Refuel Process Flow
1. Vehicle approaches gas station along route
2. System detects proximity and refuel eligibility
3. If marked for refuel, automatic process begins
4. If manual, refuel interface modal appears
5. Player selects refuel amount and confirms
6. Credit validation and deduction processing
7. Vehicle stops with refuel animation
8. Fuel level updated and transaction logged
9. Vehicle resumes movement after delay

### Brand Service Differences
```
BrandServices:
- QuickFuel: Fast refuel (2 seconds), basic service
- RoadMaster: Standard refuel (3 seconds), loyalty points
- Premium Plus: Slow refuel (4 seconds), premium amenities
```

## Dependencies
- Requires Phase 10A (Gas Stations Placement) for station data
- Requires Phase 9C (Vehicle Movement Logic) for movement integration
- Requires Phase 4C (Credits Display) for credit processing

## Integration Points
- Gas station placement provides station locations and data
- Vehicle movement system handles stops and route following
- Credit system processes fuel purchase transactions
- Vehicle fuel system updates fuel levels after refuel

## Notes

### Refuel Detection Logic
- Vehicle must be within proximity radius of gas station
- Vehicle must be traveling slowly (approaching or stopped)
- Vehicle must be on the road segment containing the station
- Vehicle must have refuel marked or manual trigger activated

### Automatic Refuel Workflow
- Player marks vehicle for refuel using checkbox
- Vehicle automatically stops at next encountered gas station
- Refuel amount determined by tank capacity and current fuel
- Credit deduction processed automatically
- Vehicle resumes journey after refuel completion

### Manual Refuel Workflow
- Player clicks on gas station marker during vehicle movement
- Refuel interface modal opens with options
- Player selects partial or full refuel amount
- Credit validation and cost confirmation
- Refuel processing with visual feedback

### Refuel Timing and Animation
- Vehicle decelerates when approaching marked gas station
- Vehicle stops at gas station position for refuel
- Refuel progress indicator shows fuel being added
- Realistic timing based on brand service level
- Vehicle accelerates and resumes route after completion

### Credit Processing
- Real-time credit balance checking before refuel
- Immediate credit deduction upon refuel confirmation
- Negative credit allowance with appropriate warnings
- Transaction logging with timestamp and station details

### Error Handling
- **Insufficient Credits**: Partial refuel or cancel with notification
- **Station Unavailable**: Skip station and continue to next
- **Vehicle Full**: Skip refuel and continue journey
- **System Errors**: Graceful fallback with user notification

### Testing Requirements
- Test automatic refuel with various vehicle and fuel combinations
- Verify manual refuel interface and selection process
- Test credit integration and transaction processing
- Validate refuel animation and timing accuracy
- Performance test with multiple vehicles refueling simultaneously