# Phase 6D - Contract Acceptance

## Overview
Implement the contract acceptance system with vehicle assignment, validation, and state management integration with the save system.

## Tasks

### Contract Acceptance Workflow
- Design contract acceptance process from selection to vehicle assignment
- Create vehicle selection modal for contract assignment
- Implement validation rules for contract acceptance
- Set up contract state management and persistence

### Vehicle Selection System
- Display available vehicles at contract origin city
- Show vehicle specifications and suitability for contract
- Filter vehicles by license requirements and cargo capacity
- Provide vehicle recommendation system for optimal selection

### Acceptance Validation Rules
- Verify player has required license for goods type
- Check vehicle presence at contract origin city
- Validate vehicle cargo capacity against contract requirements
- Ensure player has positive credit balance (if required)
- Prevent acceptance of expired or invalid contracts

### Contract Assignment Process
- Assign selected vehicle to accepted contract
- Update contract status from Available to Accepted
- Set vehicle status to assigned/busy
- Create contract-vehicle relationship for tracking
- Update vehicle location to contract origin if needed

### Save System Integration
- Save contract acceptance state to game data
- Persist vehicle assignment and contract relationships
- Update player progress with accepted contracts
- Handle save failures with transaction rollback

### Acceptance Confirmation
- Display contract and vehicle details before final acceptance
- Show estimated delivery time and route information
- Confirm contract terms and vehicle assignment
- Provide clear cancellation option before commitment

### Error Handling
- Handle insufficient vehicle capacity gracefully
- Manage missing license requirements clearly
- Deal with vehicle unavailability appropriately
- Provide helpful error messages for all failure scenarios

## Acceptance Criteria

### Validation System
- ✅ All acceptance rules are enforced correctly
- ✅ License requirements prevent invalid acceptances
- ✅ Vehicle capacity validation works accurately
- ✅ City presence requirements are checked properly

### Vehicle Assignment
- ✅ Vehicle selection shows appropriate options
- ✅ Vehicle-contract assignment persists correctly
- ✅ Vehicle status updates reflect assignment
- ✅ Multiple contracts can be managed simultaneously

### User Experience
- ✅ Acceptance process feels guided and secure
- ✅ Vehicle recommendations help optimal selection
- ✅ Error messages provide actionable feedback
- ✅ Confirmation prevents accidental acceptance

### System Integration
- ✅ Save system persists all acceptance data
- ✅ Contract and vehicle systems stay synchronized
- ✅ Event notifications trigger appropriate updates
- ✅ UI updates reflect acceptance state changes

## Technical Notes

### Component Structure
```
ContractAcceptance/
├── ContractAcceptanceController.cs (acceptance logic)
├── VehicleSelectionModal.cs (vehicle choice interface)
├── AcceptanceValidator.cs (validation rules)
├── ContractAssignment.cs (vehicle-contract linking)
└── AcceptanceConfirmation.cs (final confirmation)
```

### Acceptance Workflow Steps
1. Player selects "Accept" on available contract
2. System validates basic acceptance requirements
3. Vehicle selection modal shows eligible vehicles
4. Player selects vehicle from available options
5. System validates selected vehicle suitability
6. Acceptance confirmation shows complete details
7. Player confirms final acceptance
8. Contract assigned to vehicle and state updated
9. Save system persists all changes
10. UI updates reflect new contract status

### Vehicle Eligibility Rules
```
VehicleEligibility:
- Vehicle must be at contract origin city
- Vehicle cargo capacity >= contract cargo weight
- Player must own required license for goods type
- Vehicle must not be assigned to another contract
- Vehicle must be in operational condition
```

### Contract Assignment Data
```
ContractAssignment:
- ContractId: Reference to accepted contract
- VehicleId: Assigned vehicle identifier
- AssignmentDate: When contract was accepted
- EstimatedDeliveryTime: Calculated delivery duration
- CurrentStatus: Assignment status tracking
- RouteData: Planned route information
```

## Dependencies
- Requires Phase 6A (Contract Generation) for contract data
- Requires Phase 6C (Contract Cards) for UI integration
- Requires Phase 1D (Save System) for persistence
- Will integrate with vehicle management systems

## Integration Points
- Contract cards trigger acceptance workflow
- Vehicle management provides available vehicles
- License system validates acceptance requirements
- Save system persists acceptance state and assignments

## Notes

### Vehicle Selection Interface
- Display vehicle name, type, and specifications
- Show current location and distance to contract origin
- Indicate cargo capacity vs. contract requirements
- Highlight recommended vehicles for the contract
- Show any compatibility issues or warnings

### Acceptance Validation Messages
- **License Required**: "This contract requires [License Name]. Purchase from Shopping."
- **No Suitable Vehicle**: "No vehicles available at [City] with sufficient capacity."
- **Vehicle Busy**: "Selected vehicle is already assigned to another contract."
- **Insufficient Capacity**: "Vehicle can carry [X]kg, contract requires [Y]kg."

### Contract Assignment Effects
- Contract status changes from "Available" to "Accepted"
- Vehicle status changes to "Assigned" with contract reference
- Vehicle becomes unavailable for other contracts
- Contract appears in "Accepted" filter of contracts view
- Estimated delivery time calculated and displayed

### Acceptance Confirmation Details
- **Contract**: Title, origin, destination, cargo, reward
- **Vehicle**: Name, type, capacity, current location
- **Estimate**: Delivery time, route distance, fuel cost
- **Requirements**: License confirmation, capacity confirmation

### Error Recovery
- Failed acceptance due to save errors: Allow retry
- Vehicle becomes unavailable during selection: Refresh list
- Contract expires during acceptance: Cancel gracefully
- Data inconsistency detected: Validate and correct

### Performance Considerations
- Efficient vehicle filtering for large fleets
- Fast validation without blocking UI
- Optimized save operations for acceptance data
- Cached vehicle eligibility calculations

### Testing Requirements
- Test acceptance with various vehicle and contract combinations
- Verify all validation rules with edge cases
- Test save system integration and error recovery
- Validate UI updates and state synchronization
- Performance test with large numbers of vehicles and contracts