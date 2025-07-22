# Phase 7A - Vehicles View Window

## Overview
Create the dark themed vehicles management window displaying owned vehicles with their status, specifications, and management options using BaseWindow and BaseList components.

## Tasks

### Vehicles Window Architecture
- Design vehicles window using BaseWindow component as foundation
- Implement BaseList component for vehicle display and management
- Create vehicle status filtering and organization system
- Set up vehicle information display and interaction

### Vehicle List Display
- Scrollable list of owned vehicles using BaseCard components
- Vehicle status filtering (Available, Assigned, Moving, Maintenance)
- Sort options (Name, Type, Location, Status, Fuel Level)
- Search functionality for finding specific vehicles

### Vehicle Information Display
- Vehicle name, type, and specifications
- Current location and operational status
- Fuel level with visual fuel gauge
- Current cargo load and capacity utilization
- Contract assignment status and details

### Vehicle Status Management
- Available vehicles: Ready for contract assignment
- Assigned vehicles: Currently assigned to contracts
- Moving vehicles: En route with active contracts
- Maintenance vehicles: Requiring repair or service
- Out of fuel vehicles: Requiring refueling

### Filter and Sort System
- Filter by vehicle status and availability
- Filter by vehicle type and capabilities
- Filter by current location city
- Sort by various vehicle attributes
- Combined filter criteria for complex searches

### Vehicle Actions Integration
- Move vehicle to different city option
- View vehicle details and specifications
- Show vehicle path and route when moving
- Refuel vehicle option for low fuel
- Cancel contract assignment when applicable

### Real-time Status Updates
- Live updates when vehicle status changes
- Fuel level updates during movement
- Location updates as vehicles travel
- Contract assignment updates
- Automatic refresh of vehicle information

## Acceptance Criteria

### Visual Design
- ✅ Vehicles window uses dark graphite theme consistently
- ✅ Professional fleet management interface appearance
- ✅ Clear vehicle information hierarchy and status indicators
- ✅ Responsive design works at minimum resolution

### Functionality
- ✅ Vehicle filtering works correctly for all status types
- ✅ Sort options organize vehicles appropriately
- ✅ Real-time updates reflect vehicle state changes
- ✅ Vehicle actions integrate with game systems

### Information Display
- ✅ All vehicle information is accurate and comprehensive
- ✅ Status indicators clearly show vehicle state
- ✅ Fuel and cargo levels are visually intuitive
- ✅ Location information is current and accurate

### Performance
- ✅ Large vehicle fleets display efficiently
- ✅ Real-time updates don't impact performance
- ✅ Filtering and sorting operations are fast
- ✅ Memory usage optimized for vehicle management

## Technical Notes

### Component Structure
```
VehiclesView/
├── VehiclesViewController.cs (window behavior)
├── VehiclesViewDocument.uxml (vehicles layout)
├── VehiclesViewStyles.uss (vehicles styling)
├── VehicleFilter.cs (filtering system)
├── VehicleActions.cs (vehicle action handling)
└── VehicleStatusTracker.cs (real-time updates)
```

### UXML Structure
- Root window container using BaseWindow
- Header with filter controls and search input
- Main content area with BaseList for vehicle display
- Vehicle cards using BaseCard component for each vehicle
- Action buttons and status indicators per vehicle

### USS Class Naming
- .vehicles-view (root container)
- .vehicles-header (filter and search area)
- .vehicles-filters (filter controls)
- .vehicles-search (search input)
- .vehicles-list (main vehicle list)
- .vehicle-card (individual vehicle display)
- .vehicle-status (status indicator)
- .vehicle-actions (action buttons area)

### Vehicle Display Data
```
VehicleDisplayData:
- Name: Vehicle identification name
- Type: Vehicle category (Van, Truck, Lorry, etc.)
- Location: Current city or "En Route"
- Status: Current operational state
- FuelLevel: Current fuel percentage
- CargoLoad: Current cargo weight/capacity
- Assignment: Current contract if assigned
- Specifications: Speed, capacity, fuel tank size
```

### Vehicle Status Types
- **Available**: Green indicator, ready for contracts
- **Assigned**: Yellow indicator, contract assigned but not started
- **Moving**: Blue indicator, en route with cargo
- **Arrived**: Green flash, ready for contract completion
- **Low Fuel**: Orange warning, needs refueling
- **Out of Fuel**: Red alert, cannot move

## Dependencies
- Requires Phase 2B (Base Window Component) for foundation
- Requires Phase 2E (Base List Component) for vehicle listing
- Requires Phase 2D (Base Card Component) for vehicle display
- Integrates with vehicle purchase and management systems

## Integration Points
- BaseWindow provides window framework and styling
- BaseList handles efficient vehicle list display
- BaseCard components display individual vehicle information
- Vehicle data from purchase and assignment systems

## Notes

### Vehicle Card Layout
- **Header**: Vehicle name and type with status indicator
- **Location**: Current city or route information
- **Status**: Operational state with visual indicators
- **Fuel**: Fuel gauge with percentage and range
- **Cargo**: Load information with capacity utilization
- **Actions**: Available actions based on vehicle state

### Filter Options
- **All Vehicles**: Show entire fleet
- **Available**: Ready for contract assignment
- **Working**: Currently assigned or moving
- **Maintenance**: Needing attention or service
- **By Type**: Filter by vehicle category
- **By Location**: Filter by current city

### Sort Options
- **By Name**: Alphabetical vehicle names
- **By Type**: Group by vehicle categories
- **By Location**: Organize by current city
- **By Status**: Group by operational state
- **By Fuel**: Lowest fuel first for maintenance
- **By Capacity**: Largest to smallest vehicles

### Vehicle Actions
- **Move Vehicle**: Relocate to different city
- **View Details**: Comprehensive vehicle information
- **Show Route**: Display current path on map
- **Refuel**: Add fuel to vehicle tank
- **Cancel Contract**: Remove contract assignment

### Real-time Update System
- Vehicle position updates during movement
- Fuel consumption updates during travel
- Status changes when contracts assigned/completed
- Location updates when vehicles arrive at destinations
- Automatic refresh without disrupting user interaction

### Performance Optimization
- Virtual scrolling for large vehicle fleets
- Efficient status update propagation
- Cached vehicle data for frequent access
- Optimized rendering for vehicle status changes

### Testing Requirements
- Test vehicle display with various fleet sizes
- Verify filtering and sorting with different criteria
- Test real-time updates during vehicle operations
- Validate action integration with vehicle systems
- Performance test with 50+ vehicles in fleet