# Phase 10G - Emergency Refuel Modal

## Overview
Create the emergency refuel modal interface using dark graphite theme for critical fuel situations, providing immediate fuel purchase options with premium pricing and limited fuel amounts.

## Tasks

### Emergency Refuel Modal Design
- Design emergency modal using BaseModal component with urgent styling
- Display critical fuel situation information and vehicle status
- Show emergency refuel options with premium pricing
- Implement dark graphite theme with emergency/warning elements

### Emergency Situation Display
- Vehicle out-of-fuel status and current location
- Distance to nearest gas stations for context
- Emergency service availability and response time
- Critical situation messaging and urgency indicators

### Emergency Refuel Options
- Limited fuel amount (20% of tank capacity as specified)
- Premium pricing (20% markup over nearest station)
- Immediate fuel delivery service description
- Alternative options if emergency service unavailable

### Cost Calculation and Display
- Emergency fuel amount calculation (20% of vehicle capacity)
- Premium pricing calculation (20% over nearest station rate)
- Total emergency service cost breakdown
- Credit impact and negative balance handling

### Service Description
- Emergency fuel delivery service explanation
- Service limitations and fuel amount restrictions
- Delivery time simulation and vehicle resumption
- Professional emergency service presentation

### Payment Processing
- Immediate credit deduction for emergency service
- Negative credit allowance for emergency situations
- Payment confirmation and service activation
- Emergency transaction logging and records

### Save System Integration
- Emergency refuel transaction history
- Vehicle fuel level updates after emergency service
- Emergency service usage statistics tracking
- Credit transaction records for emergency purchases

## Acceptance Criteria

### Emergency Presentation
- ✅ Modal conveys urgency and critical nature of situation
- ✅ Emergency service options are clearly explained
- ✅ Cost information is transparent and prominent
- ✅ Alternative solutions are provided when possible

### Service Functionality
- ✅ Emergency refuel provides correct fuel amount (20% capacity)
- ✅ Premium pricing calculation is accurate (20% markup)
- ✅ Credit processing handles emergency transactions correctly
- ✅ Vehicle fuel level updates appropriately after service

### User Experience
- ✅ Emergency situation is clearly communicated
- ✅ Service options are practical and accessible
- ✅ Payment process is streamlined for emergency context
- ✅ Vehicle resumption feels realistic and immediate

### System Integration
- ✅ Modal integrates properly with BaseModal component
- ✅ Fuel system coordinates with emergency refuel operations
- ✅ Credit system processes emergency payments correctly
- ✅ Vehicle movement resumes after emergency service

## Technical Notes

### Component Structure
```
EmergencyRefuelModal/
├── EmergencyRefuelController.cs (modal behavior)
├── EmergencyRefuelDocument.uxml (modal layout)
├── EmergencyRefuelStyles.uss (emergency styling)
├── EmergencyCalculator.cs (pricing and fuel calculations)
└── EmergencyProcessor.cs (service delivery simulation)
```

### UXML Structure
- Root modal container using BaseModal with emergency styling
- Header section with emergency alert and situation summary
- Vehicle status section showing fuel level and location
- Service options section with emergency refuel details
- Cost breakdown section with premium pricing information
- Action buttons section with emergency service confirmation

### USS Class Naming
- .emergency-refuel-modal (root container)
- .emergency-alert-header (urgent situation display)
- .vehicle-status-emergency (vehicle information)
- .emergency-service-options (service description)
- .emergency-cost-breakdown (pricing details)
- .emergency-actions (confirmation buttons)

### Emergency Refuel Calculations
```
EmergencyRefuelCalculation:
- VehicleTankCapacity: Vehicle's maximum fuel capacity
- EmergencyFuelAmount: TankCapacity * 0.20 (20%)
- NearestStationPrice: Closest gas station fuel price
- PremiumMarkup: NearestStationPrice * 0.20 (20% increase)
- EmergencyPrice: NearestStationPrice + PremiumMarkup
- TotalCost: EmergencyFuelAmount * EmergencyPrice
```

### Emergency Modal Data
```
EmergencyRefuelData:
- VehicleId: Vehicle requiring emergency service
- VehicleLocation: Current vehicle position
- CurrentFuelLevel: Fuel level (should be 0 or near 0)
- TankCapacity: Vehicle's maximum fuel capacity
- NearestStationDistance: Distance to closest gas station
- NearestStationPrice: Fuel price at closest station
- EmergencyServiceCost: Total cost for emergency refuel
- PlayerCredits: Current player credit balance
```

### Emergency Service Workflow
1. Vehicle runs out of fuel and stops
2. Emergency refuel modal automatically appears
3. System calculates emergency fuel amount and pricing
4. Player reviews emergency service options and costs
5. Player confirms emergency refuel service
6. Credit deduction processed with premium pricing
7. Emergency fuel delivered to vehicle (simulation)
8. Vehicle fuel level updated to 20% capacity
9. Vehicle movement resumes from current location

## Dependencies
- Requires Phase 2C (Base Modal Component) for foundation
- Requires Phase 9D (Fuel Consumption System) for fuel calculations
- Requires Phase 4C (Credits Display) for payment processing

## Integration Points
- BaseModal provides modal dialog framework with emergency styling
- Fuel consumption system triggers emergency modal when fuel depleted
- Credit system processes emergency payments with premium pricing
- Vehicle movement system resumes operation after emergency service

## Notes

### Emergency Trigger Conditions
- Vehicle fuel level reaches 0% (completely empty)
- Vehicle stops movement due to fuel depletion
- No gas stations within immediate vicinity
- Player has not marked vehicle for refuel at nearby stations

### Emergency Service Limitations
- Only provides 20% of tank capacity (not full refuel)
- Premium pricing 20% above nearest station rates
- Service designed for emergency continuation, not convenience
- Limited to one emergency refuel per journey segment

### Cost Structure Justification
- Premium pricing reflects emergency service delivery
- Limited fuel amount encourages proper fuel planning
- Service cost balanced to be expensive but not game-breaking
- Emergency nature justifies higher pricing structure

### Alternative Solutions Display
- Show distance and direction to nearest gas stations
- Provide route planning to gas stations if fuel sufficient
- Suggest waiting for fuel delivery if applicable
- Display estimated time to reach fuel if vehicle movement possible

### Emergency Service Simulation
- Brief delay to simulate emergency fuel delivery
- Professional emergency service messaging
- Vehicle status updates during service delivery
- Realistic timing for fuel delivery and vehicle resumption

### Credit Handling for Emergencies
- Allow negative credit balance for emergency situations
- Clear warning about credit impact of emergency service
- Payment confirmation required despite emergency urgency
- Transaction logging with emergency service categorization

### Error Handling
- **Service Unavailable**: Provide alternative solutions
- **Insufficient Credits**: Allow negative balance with warnings
- **System Failures**: Graceful fallback with basic fuel addition
- **Invalid Vehicle State**: Validate emergency conditions

### Accessibility Features
- High contrast emergency indicators for critical situations
- Screen reader priority announcements for emergency status
- Keyboard navigation optimized for urgent decision making
- Clear focus indicators for emergency action buttons

### Testing Requirements
- Test emergency modal triggering when vehicles run out of fuel
- Verify emergency fuel calculations and premium pricing accuracy
- Test credit processing for emergency transactions
- Validate vehicle movement resumption after emergency service
- Test emergency modal accessibility and urgent interaction patterns