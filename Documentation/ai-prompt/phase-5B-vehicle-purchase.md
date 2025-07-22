# Phase 5B - Vehicle Purchase

## Overview
Implement the vehicle purchase system with city selection, credit validation, and integration with the save system to add purchased vehicles to the player's fleet.

## Tasks

### Vehicle Purchase Workflow
- Implement complete vehicle purchasing process from selection to delivery
- Create city selection modal for vehicle delivery location
- Set up credit validation and transaction processing
- Integrate with save system for persistent vehicle ownership

### City Selection Modal
- Design modal for selecting vehicle purchase/delivery city
- Display only cities that player has previously visited
- Show city information and current vehicle count
- Implement city validation and selection confirmation

### Credit Transaction System
- Validate player has sufficient credits before purchase
- Process credit deduction upon purchase confirmation
- Handle negative credit scenarios appropriately
- Integrate with credits display widget for real-time updates

### Vehicle Data Integration
- Load vehicle specifications from VehicleData ScriptableObjects
- Create VehicleInstance for purchased vehicles
- Set initial vehicle state (fuel, position, availability)
- Assign vehicle to selected city location

### Purchase Confirmation System
- Display vehicle details and total cost before purchase
- Show selected delivery city and any additional costs
- Require explicit confirmation to prevent accidental purchases
- Provide clear cancellation options

### Save System Integration
- Save new vehicle to player's vehicle fleet
- Update city data with stationed vehicle
- Persist vehicle ownership and specifications
- Handle save failures with appropriate error messaging

### Error Handling and Validation
- Prevent purchase if insufficient credits
- Block invalid city selections
- Handle save system failures gracefully
- Provide clear error messages for all failure scenarios

## Acceptance Criteria

### Purchase Process
- ✅ Vehicle selection leads to proper purchase workflow
- ✅ City selection modal displays only valid cities
- ✅ Credit validation prevents invalid purchases
- ✅ Purchase confirmation requires explicit user action

### Transaction Handling
- ✅ Credits are deducted accurately upon purchase
- ✅ Vehicle is added to player fleet at selected city
- ✅ Save system persists purchase data correctly
- ✅ Real-time credit updates reflect transaction

### User Experience
- ✅ Purchase process feels secure and guided
- ✅ All information is clearly presented before confirmation
- ✅ Error messages are helpful and actionable
- ✅ Successful purchases provide satisfying feedback

### Technical Integration
- ✅ Vehicle data integration works correctly
- ✅ Save system handles vehicle persistence reliably
- ✅ Event system triggers appropriate notifications
- ✅ Error handling covers all failure scenarios

## Technical Notes

### Component Structure
```
VehiclePurchase/
├── VehiclePurchaseController.cs (purchase logic)
├── CitySelectionModal.cs (city selection)
├── PurchaseConfirmation.cs (confirmation dialog)
├── VehicleCreation.cs (vehicle instance creation)
└── PurchaseValidation.cs (credit and rule validation)
```

### Purchase Workflow Steps
1. Player selects vehicle from shopping interface
2. System validates player can afford vehicle
3. City selection modal displays available cities
4. Player selects delivery city
5. Purchase confirmation shows all details
6. Player confirms purchase
7. Credits deducted, vehicle created and saved
8. Success notification and UI updates

### City Selection Rules
- Only cities marked as visited in player progress
- Cities must have infrastructure for vehicle delivery
- Display city name, description, and current vehicle count
- Prevent selection of invalid or inaccessible cities

### Vehicle Instance Creation
```
VehicleInstance:
- BaseData: Reference to VehicleData ScriptableObject
- CurrentFuel: Set to maximum capacity initially
- CurrentWeight: Set to 0 (empty)
- Position: Set to selected city
- Status: Available for contracts
- PurchaseDate: Current game timestamp
```

## Dependencies
- Requires Phase 5A (Shopping Window) for integration
- Requires Phase 1C (Data Structures) for vehicle data
- Requires Phase 1D (Save System) for persistence
- Requires Phase 4C (Credits Display) for real-time updates

## Integration Points
- Shopping Window triggers vehicle purchase process
- Save system stores new vehicle data and player progress
- Credits system processes transaction and updates display
- Notification system announces successful purchases

## Notes

### Credit Validation Logic
- Check player's current credit balance
- Account for vehicle price and any delivery fees
- Allow purchases that result in small negative balances if configured
- Prevent purchases that would cause significant debt

### City Delivery Rules
- Vehicle delivery is instantaneous (no delivery time)
- Vehicles start with full fuel at delivery city
- Multiple vehicles can be stationed at the same city
- City infrastructure doesn't limit vehicle count

### Vehicle Availability After Purchase
- Purchased vehicles immediately available for contracts
- Vehicles start in "Available" status at selected city
- Vehicle specifications match exactly with VehicleData
- Vehicle color matches company color selection

### Purchase Feedback
- Success notification shows vehicle name and delivery city
- Credits display updates immediately with new balance
- Vehicle appears in player's vehicle list immediately
- Shopping interface updates to reflect purchase

### Error Scenarios
- **Insufficient Credits**: Clear message with current balance and shortfall
- **Invalid City**: Prevent selection and explain requirements
- **Save Failure**: Offer retry option and prevent duplicate charges
- **Data Corruption**: Fallback to safe state with user notification

### Testing Requirements
- Test purchase workflow with various vehicle types
- Verify credit validation with edge cases (exact amount, insufficient funds)
- Test city selection with different progress states
- Validate save system integration and error recovery
- Performance test purchase process completion time