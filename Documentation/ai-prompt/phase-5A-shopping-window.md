# Phase 5A - Shopping Window

## Overview
Create the dark themed shopping interface window with tabs for Vehicles and Licenses, allowing players to purchase vehicles and transportation licenses with integration to the save system.

## Tasks

### Shopping Window Architecture
- Design shopping window using BaseWindow component as foundation
- Create tabbed interface with Vehicles and Licenses sections
- Implement dark graphite theme throughout the interface
- Set up window management and navigation between tabs

### Tab System Implementation
- Vehicles tab displaying available vehicles for purchase
- Licenses tab showing available transportation licenses
- Tab switching with visual state management
- Content area that updates based on selected tab

### Vehicle Shopping Interface
- Display grid/list of available vehicles using BaseCard components
- Show vehicle specifications (speed, fuel capacity, weight capacity, price)
- Vehicle icons and visual previews from Asset Manager
- Purchase button integration with credit system

### License Shopping Interface
- Display available transportation licenses for different goods types
- Show license descriptions, requirements, and prices
- License icons and category information
- Purchase button integration with credit system

### City Selection for Vehicle Purchases
- Modal dialog for selecting purchase city
- Display only cities that player has visited
- City information and current vehicle count
- Confirmation of purchase location

### Purchase Transaction System
- Credit validation before allowing purchases
- Transaction confirmation dialogs
- Real-time credit updates during purchases
- Save system integration for storing purchases

### Shopping Data Management
- Load available vehicles and licenses from data structures
- Filter purchased items appropriately
- Update shopping availability based on game progress
- Integration with player progress and unlocked content

## Acceptance Criteria

### Visual Design
- ✅ Shopping window uses dark graphite theme consistently
- ✅ Tabbed interface is intuitive and professional
- ✅ Vehicle and license cards display information clearly
- ✅ Purchase interface is user-friendly and responsive

### Functionality
- ✅ Tab switching works smoothly between Vehicles and Licenses
- ✅ Vehicle purchases require city selection and save correctly
- ✅ License purchases process correctly and update player data
- ✅ Credit validation prevents invalid purchases

### User Experience
- ✅ Shopping process feels guided and secure
- ✅ Purchase confirmations prevent accidental transactions
- ✅ City selection is logical and restricted appropriately
- ✅ Visual feedback confirms successful purchases

### Integration
- ✅ Save system stores all purchase data correctly
- ✅ Credit system updates in real-time during transactions
- ✅ Asset Manager provides all necessary icons and previews
- ✅ Data structures supply accurate vehicle and license information

## Technical Notes

### Component Structure
```
ShoppingWindow/
├── ShoppingWindowController.cs (window behavior)
├── ShoppingWindowDocument.uxml (shopping layout)
├── ShoppingWindowStyles.uss (shopping styling)
├── ShoppingTabManager.cs (tab switching)
├── VehiclePurchase.cs (vehicle buying logic)
└── LicensePurchase.cs (license buying logic)
```

### UXML Structure
- Root window container using BaseWindow
- Tab header section with Vehicles and Licenses buttons
- Content area that switches between vehicle and license displays
- Purchase modal overlays for confirmations and city selection

### USS Class Naming
- .shopping-window (root container)
- .shopping-tabs (tab header area)
- .shopping-tab-button (individual tab buttons)
- .shopping-tab-active (active tab state)
- .shopping-content (main content area)
- .vehicle-grid (vehicle display area)
- .license-list (license display area)

### Vehicle Data Display
- Name, description, and specifications
- Speed limit, fuel capacity, weight capacity
- Purchase price in credits
- Icon/preview image from Asset Manager
- Purchase button with credit validation

## Dependencies
- Requires Phase 2B (Base Window Component) for foundation
- Requires Phase 2D (Base Card Component) for item display
- Requires Phase 1C (Data Structures) for vehicle and license data
- Requires Phase 1D (Save System) for purchase persistence

## Integration Points
- BaseWindow provides window framework
- BaseCard components display vehicles and licenses
- Save system stores purchase data and player progress
- Credit system validates and processes transactions

## Notes

### Vehicle Purchase Process
1. Player selects vehicle from shopping interface
2. System checks player credits against vehicle price
3. City selection modal appears for purchase location
4. Player confirms purchase with credit deduction
5. Vehicle is added to player's fleet at selected city
6. Save system persists the new vehicle data

### License Purchase Process
1. Player selects license from shopping interface
2. System checks player credits against license price
3. Purchase confirmation modal displays license details
4. Player confirms purchase with credit deduction
5. License is added to player's company profile
6. Save system persists the new license data

### Vehicle Availability Rules
- All vehicles available for purchase initially
- Prices reflect vehicle capabilities and specifications
- Vehicle variety encourages strategic purchasing decisions
- Special vehicles may have prerequisite requirements

### License System Integration
- Licenses enable transport of specific goods types
- Required for contracts involving specialized cargo
- License ownership affects available contract types
- Provides progression system for player advancement

### Error Handling
- Insufficient credits prevent purchase attempts
- Invalid city selections are blocked
- Save failures trigger retry mechanisms
- Network errors (if any) handled gracefully

### Testing Requirements
- Test all vehicle and license purchasing workflows
- Verify credit validation and deduction accuracy
- Test city selection restrictions and validation
- Validate save system integration for purchases
- Performance test with large vehicle/license catalogs