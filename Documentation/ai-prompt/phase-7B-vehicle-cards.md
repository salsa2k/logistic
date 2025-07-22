# Phase 7B - Vehicle Cards

## Overview
Create specialized vehicle cards using BaseCard component with dark graphite theme to display vehicle information, status, and actions in the vehicles list interface.

## Tasks

### Vehicle Card Design
- Design vehicle card layout using BaseCard component as foundation
- Create vehicle-specific information hierarchy and styling
- Implement dark graphite theme with professional fleet management appearance
- Set up responsive card sizing for different vehicle data

### Vehicle Information Display
- Vehicle name and type prominently displayed
- Current location with city name or "En Route" status
- Fuel level with visual fuel gauge and range indicator
- Cargo load with weight and capacity utilization
- Vehicle specifications (speed, capacity, fuel tank)

### Visual Status System
- Color-coded status indicators for vehicle operational state
- Fuel level visualization with warning indicators
- Cargo load visualization with capacity percentage
- Movement status with progress indicators
- Contract assignment indicators with contract details

### Action Button Integration
- Context-sensitive action buttons based on vehicle state
- Move vehicle button for available vehicles
- Show route button for vehicles in transit
- Complete contract button for arrived vehicles
- Refuel and maintenance options when needed

### Vehicle State Visualization
- Available state: Green indicators, ready for assignment
- Assigned state: Yellow indicators, contract information
- Moving state: Blue indicators, progress tracking
- Arrived state: Green flash, completion option
- Maintenance state: Orange warnings, service required

### Real-time Data Updates
- Live fuel consumption during movement
- Position updates for vehicles in transit
- Status changes when contracts assigned or completed
- Cargo load updates during loading/unloading
- Location updates when vehicles arrive at destinations

### Vehicle Specifications Display
- Maximum speed and current speed limits
- Fuel capacity and consumption rates
- Cargo capacity and current load
- Vehicle type and specialized capabilities
- Purchase date and operational statistics

## Acceptance Criteria

### Visual Design
- ✅ Vehicle cards use dark graphite theme consistently
- ✅ Professional fleet management appearance
- ✅ Clear status indicators and information hierarchy
- ✅ Responsive design adapts to different screen sizes

### Information Display
- ✅ All vehicle information is accurate and current
- ✅ Status indicators clearly show vehicle state
- ✅ Fuel and cargo visualizations are intuitive
- ✅ Specifications are comprehensive and well-organized

### User Interaction
- ✅ Action buttons are context-appropriate and functional
- ✅ Card interactions provide clear feedback
- ✅ Vehicle selection and focus states work properly
- ✅ Real-time updates don't disrupt user interaction

### Technical Integration
- ✅ Cards integrate properly with BaseCard component
- ✅ Real-time updates reflect vehicle state changes accurately
- ✅ Performance optimized for large vehicle fleets
- ✅ Event handling works correctly for all interactions

## Technical Notes

### Component Structure
```
VehicleCards/
├── VehicleCard.cs (vehicle-specific card behavior)
├── VehicleCardDocument.uxml (vehicle card layout)
├── VehicleCardStyles.uss (vehicle card styling)
├── VehicleStatusIndicator.cs (status display)
├── FuelGauge.cs (fuel visualization)
└── VehicleActions.cs (action button management)
```

### UXML Structure
- Root card container extending BaseCard
- Header section with vehicle name and type
- Status section with operational state indicators
- Location section with current position
- Specifications section with vehicle details
- Actions section with context-sensitive buttons

### USS Class Naming
- .vehicle-card (root container extending base-card)
- .vehicle-header (name and type area)
- .vehicle-status (status indicators)
- .vehicle-location (position display)
- .vehicle-fuel-gauge (fuel visualization)
- .vehicle-cargo-meter (cargo load display)
- .vehicle-specs (specifications area)
- .vehicle-actions (button area)

### Vehicle Card States
```
VehicleStates:
- Available: Green accent, "Move" and "Assign" options
- Assigned: Yellow accent, contract info, "Cancel" option
- Moving: Blue accent, progress bar, "Show Route" option
- Arrived: Green flash, "Complete Contract" button
- LowFuel: Orange warning, "Refuel" button prominent
- OutOfFuel: Red alert, "Emergency Refuel" option
- Maintenance: Gray accent, "Service Required" indicator
```

### Fuel Gauge Component
- Visual fuel tank with current level
- Color coding: Green (>50%), Yellow (20-50%), Red (<20%)
- Percentage display with estimated range
- Low fuel warning indicators
- Empty tank emergency indicators

## Dependencies
- Requires Phase 2D (Base Card Component) for foundation
- Requires vehicle data from purchase and management systems
- Integrates with movement and contract systems for real-time data

## Integration Points
- BaseCard provides core card functionality and styling
- Vehicle management system provides vehicle data
- Movement system provides position and status updates
- Contract system provides assignment information

## Notes

### Card Layout Hierarchy
1. **Vehicle Name**: Large, prominent text at top
2. **Vehicle Type**: Secondary text with vehicle category
3. **Status Indicator**: Color-coded operational state
4. **Location**: Current city or route information
5. **Fuel Gauge**: Visual fuel level with percentage
6. **Cargo Meter**: Load visualization with capacity
7. **Specifications**: Key vehicle stats and capabilities
8. **Actions**: Context-appropriate buttons at bottom

### Action Button Logic
- **Available Vehicles**:
  - "Move Vehicle" (primary action)
  - "View Details" (secondary action)
- **Assigned Vehicles**:
  - "Show Contract" (primary action)
  - "Cancel Assignment" (destructive action)
- **Moving Vehicles**:
  - "Show Route" (primary action)
  - "Track Progress" (secondary action)
- **Arrived Vehicles**:
  - "Complete Contract" (primary action, highlighted)

### Performance Considerations
- Efficient card rendering for vehicle fleets
- Optimized real-time updates for vehicle status
- Lazy loading of detailed vehicle information
- Memory management for card lifecycle

### Real-time Update Frequency
- Position updates: Every 5-10 seconds for moving vehicles
- Fuel updates: Every 30 seconds during movement
- Status updates: Immediate when state changes
- Cargo updates: When loading/unloading occurs

### Testing Requirements
- Test cards with various vehicle types and states
- Verify real-time updates during vehicle operations
- Test action buttons for all vehicle states
- Validate performance with large vehicle fleets
- Test responsive behavior at different screen sizes