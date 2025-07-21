# Phase 7C - Move Vehicle Modal

## Overview
Create the vehicle movement modal that allows players to relocate vehicles between visited cities with fuel consumption calculation and route planning.

## Tasks

### Move Vehicle Modal Design
- Design modal using BaseModal component for consistent styling
- Create city selection interface for destination choice
- Implement route preview and fuel consumption estimation
- Set up movement confirmation with cost breakdown

### City Selection System
- Display list of visited cities as movement destinations
- Filter out current vehicle location from options
- Show city information and distance from current location
- Provide search and filtering for large city lists

### Route Calculation
- Calculate optimal route between current and destination cities
- Estimate travel time based on vehicle specifications
- Calculate fuel consumption for the journey
- Display route information and potential hazards

### Cost Estimation
- Calculate fuel consumption based on vehicle type and distance
- Show estimated fuel cost if refueling needed
- Display any route tolls or fees (if applicable)
- Provide total movement cost breakdown

### Movement Confirmation
- Display complete movement details before confirmation
- Show route, time, fuel consumption, and costs
- Confirm vehicle will be unavailable during movement
- Provide clear cancellation option

### Movement Execution
- Process vehicle movement with progress tracking
- Update vehicle status to "Moving" during transit
- Handle fuel consumption and vehicle state changes
- Complete movement and update vehicle location

### Integration with Game Systems
- Coordinate with fuel consumption system
- Integrate with vehicle status management
- Update save system with movement data
- Trigger appropriate notifications

## Acceptance Criteria

### User Interface
- ✅ Modal uses dark graphite theme consistently
- ✅ City selection interface is intuitive and responsive
- ✅ Route information is clear and comprehensive
- ✅ Cost breakdown is accurate and detailed

### Functionality
- ✅ City filtering shows only valid destinations
- ✅ Route calculation provides accurate estimates
- ✅ Fuel consumption calculation is correct
- ✅ Movement execution works reliably

### User Experience
- ✅ Movement process feels informed and controlled
- ✅ Cost estimation prevents surprises
- ✅ Progress tracking provides clear feedback
- ✅ Error handling guides user appropriately

### System Integration
- ✅ Vehicle status updates correctly during movement
- ✅ Fuel consumption affects vehicle state appropriately
- ✅ Save system persists movement data
- ✅ Game systems coordinate properly

## Technical Notes

### Component Structure
```
MoveVehicleModal/
├── MoveVehicleController.cs (modal behavior)
├── MoveVehicleDocument.uxml (modal layout)
├── MoveVehicleStyles.uss (modal styling)
├── CitySelector.cs (destination selection)
├── RouteCalculator.cs (route planning)
└── MovementExecutor.cs (movement processing)
```

### Movement Calculation
```
MovementCalculation:
- Distance: Kilometers between cities
- TravelTime: Distance / VehicleSpeed
- FuelConsumption: Distance * VehicleFuelRate
- FuelCost: FuelNeeded * CurrentFuelPrice
- TotalCost: FuelCost + RouteFees
```

### Movement States
- **Planning**: Selecting destination and reviewing costs
- **Confirmed**: Movement approved and processing
- **InTransit**: Vehicle traveling to destination
- **Arrived**: Movement completed, vehicle at destination
- **Failed**: Movement interrupted or cancelled

## Dependencies
- Requires Phase 2C (Base Modal Component) for foundation
- Requires city and route data from map system
- Integrates with fuel consumption and vehicle management

## Integration Points
- Vehicle management provides vehicle data
- Map system provides route and distance calculations
- Fuel system handles consumption during movement
- Save system persists movement state

## Notes

### City Selection Rules
- Only cities marked as visited are available
- Current vehicle location excluded from options
- Cities displayed with distance and route difficulty
- Invalid selections prevented with clear messaging

### Fuel Consumption Logic
- Base consumption rate per vehicle type
- Distance-based fuel calculation
- Route difficulty modifiers (hills, traffic)
- Emergency refuel handling for insufficient fuel

### Movement Timeline
1. Player selects "Move Vehicle" from vehicle card
2. Modal opens with city selection interface
3. Player selects destination city
4. System calculates route and costs
5. Player reviews and confirms movement
6. Vehicle status changes to "Moving"
7. Progress tracking shows movement status
8. Vehicle arrives at destination
9. Status updates to available at new location

### Error Handling
- **Insufficient Fuel**: Offer emergency refuel or route alternatives
- **Invalid Destination**: Prevent selection and explain requirements
- **Movement Interruption**: Handle save/load during movement
- **System Errors**: Graceful failure with recovery options

### Testing Requirements
- Test movement between various city combinations
- Verify fuel consumption calculations accuracy
- Test movement interruption and recovery
- Validate save/load during vehicle movement
- Performance test with large numbers of cities