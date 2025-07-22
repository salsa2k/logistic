# Phase 10C - Gas Station Modal

## Overview
Create the gas station refuel modal interface using dark graphite theme that displays pricing, refuel options, and processes fuel purchases with credit integration.

## Tasks

### Gas Station Modal Design
- Design refuel modal using BaseModal component for consistency
- Display gas station brand information and visual identity
- Show current fuel prices and refuel options clearly
- Implement dark graphite theme throughout the interface

### Fuel Information Display
- Current vehicle fuel level with visual fuel gauge
- Gas station brand name and pricing per liter
- Maximum refuel amount based on tank capacity
- Total cost calculation for selected refuel amount

### Refuel Options Interface
- Slider or input for custom refuel amount selection
- Quick buttons for common refuel amounts (25%, 50%, 75%, 100%)
- Real-time cost calculation as amount changes
- Visual fuel gauge showing projected fuel level after refuel

### Credit Integration
- Current player credit balance display
- Real-time affordability checking for selected amount
- Maximum affordable refuel calculation
- Credit balance after refuel projection

### Brand Information Display
- Gas station brand logo and name
- Brand-specific services and benefits
- Service level indicators (speed, amenities)
- Loyalty program information if applicable

### Refuel Confirmation System
- Clear cost breakdown before final confirmation
- Fuel amount and total cost summary
- Credit balance validation and warnings
- Confirm/cancel buttons with appropriate styling

### Emergency Refuel Options
- Emergency refuel button for critical fuel situations
- Premium pricing display for emergency services
- Limited refuel amount for emergency situations
- Immediate processing without extensive confirmation

## Acceptance Criteria

### Visual Design
- ✅ Modal uses dark graphite theme consistently
- ✅ Professional gas station interface appearance
- ✅ Clear information hierarchy and visual organization
- ✅ Brand identity elements are prominently displayed

### Functionality
- ✅ Refuel amount selection works intuitively
- ✅ Cost calculations are accurate and real-time
- ✅ Credit validation prevents invalid purchases
- ✅ Confirmation process is clear and secure

### User Experience
- ✅ Refuel process feels realistic and professional
- ✅ Information is comprehensive and easy to understand
- ✅ Quick refuel options provide convenient shortcuts
- ✅ Error handling provides helpful guidance

### System Integration
- ✅ Modal integrates properly with BaseModal component
- ✅ Credit system processes transactions correctly
- ✅ Vehicle fuel system receives accurate refuel data
- ✅ Gas station data displays correctly

## Technical Notes

### Component Structure
```
GasStationModal/
├── GasStationModalController.cs (modal behavior)
├── GasStationModalDocument.uxml (modal layout)
├── GasStationModalStyles.uss (modal styling)
├── RefuelCalculator.cs (cost and amount calculations)
└── BrandDisplay.cs (brand information presentation)
```

### UXML Structure
- Root modal container using BaseModal
- Header section with gas station brand information
- Fuel status section showing current and projected levels
- Refuel selection section with amount controls
- Cost summary section with pricing breakdown
- Action buttons section with confirm/cancel options

### USS Class Naming
- .gas-station-modal (root container)
- .station-brand-header (brand information area)
- .fuel-status-display (current fuel information)
- .refuel-selection (amount selection controls)
- .fuel-gauge (visual fuel level indicator)
- .cost-summary (pricing breakdown)
- .refuel-actions (confirmation buttons)

### Refuel Calculation Logic
```
RefuelCalculation:
- CurrentFuel: Vehicle's current fuel level in liters
- TankCapacity: Vehicle's maximum fuel capacity
- AvailableCapacity: TankCapacity - CurrentFuel
- SelectedAmount: Player-selected refuel amount
- PricePerLiter: Gas station's current fuel price
- TotalCost: SelectedAmount * PricePerLiter
- AffordableAmount: Min(AvailableCapacity, Credits / PricePerLiter)
```

### Modal Data Structure
```
GasStationModalData:
- StationBrand: Gas station brand identifier
- BrandName: Display name for gas station
- FuelPrice: Current price per liter
- VehicleFuelLevel: Current fuel amount
- VehicleTankCapacity: Maximum fuel capacity
- PlayerCredits: Current credit balance
- SelectedRefuelAmount: Player's selected amount
```

## Dependencies
- Requires Phase 2C (Base Modal Component) for foundation
- Requires Phase 10B (Gas Station Interaction) for integration
- Requires Phase 4C (Credits Display) for credit information

## Integration Points
- BaseModal provides modal dialog framework
- Gas station interaction triggers modal display
- Credit system provides balance and processes transactions
- Vehicle fuel system receives refuel completion data

## Notes

### Modal Layout Design
- **Header**: Gas station brand logo and name
- **Fuel Status**: Current fuel gauge and level
- **Selection**: Refuel amount slider and quick buttons
- **Projection**: Fuel gauge showing level after refuel
- **Cost**: Price breakdown and total cost
- **Credits**: Current balance and remaining after refuel
- **Actions**: Confirm refuel and cancel buttons

### Refuel Amount Selection
- Slider control for precise amount selection
- Quick buttons: "Fill 25%", "Fill 50%", "Fill 75%", "Fill Tank"
- Manual input field for exact amount entry
- Real-time validation against tank capacity and credit balance

### Visual Fuel Gauge
- Current fuel level shown in blue
- Additional fuel shown in green overlay
- Empty capacity shown in dark background
- Percentage and liter amounts displayed clearly

### Cost Breakdown Display
- Fuel amount: "X liters"
- Price per liter: "C X.XX per liter"
- Total cost: "C XX.XX total"
- Credits remaining: "C XX.XX remaining"

### Brand Information
- Brand logo displayed prominently
- Brand name and service level
- Special services or benefits
- Pricing tier indicator (budget/standard/premium)

### Error Prevention
- Disable amounts exceeding tank capacity
- Highlight unaffordable amounts in red
- Show maximum affordable refuel amount
- Prevent confirmation of invalid selections

### Accessibility Features
- Screen reader support for all modal content
- Keyboard navigation through refuel options
- High contrast mode for fuel gauge and indicators
- Clear focus indicators for all interactive elements

### Testing Requirements
- Test modal display with various gas station brands
- Verify refuel calculations and credit validation
- Test user interface responsiveness and usability
- Validate integration with gas station interaction system
- Test accessibility features and keyboard navigation