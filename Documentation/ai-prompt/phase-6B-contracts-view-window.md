# Phase 6B - Contracts View Window

## Overview
Create the dark themed contracts management window using BaseWindow and BaseList components to display available, accepted, and completed contracts with filtering and search capabilities.

## Tasks

### Contracts Window Architecture
- Design contracts window using BaseWindow component as foundation
- Implement BaseList component for contract display and management
- Create filtering system for different contract states
- Set up search functionality for contract discovery

### Contract List Display
- Scrollable list of contracts using BaseCard components for each contract
- Contract state filtering (Available, Accepted, Ready to Complete)
- Sort options (Date, Reward, Distance, Status)
- Search functionality for finding specific contracts

### Contract State Management
- Available contracts: Not yet accepted, can be taken
- Accepted contracts: Assigned to vehicles, in progress
- Ready to complete: Vehicle arrived at destination
- Completed contracts: Historical record of finished work
- Expired contracts: No longer available for acceptance

### Filter System Implementation
- Filter tabs or dropdown for contract state selection
- Real-time list updates when filters change
- Multiple filter criteria combination
- Clear all filters functionality

### Contract Information Display
- Contract title and description
- Origin and destination cities with distance
- Cargo type, weight, and required license
- Reward amount and payment terms
- Contract status and progress indicators
- Estimated delivery time and deadlines

### Contract Actions Integration
- Accept contract button for available contracts
- Cancel contract option for accepted contracts
- Complete contract button for ready contracts
- View details action for all contracts

### Search and Sort Features
- Text search across contract titles and descriptions
- City-based search for origin/destination filtering
- Goods type filtering for specialized cargo
- Sort by reward, distance, deadline, or status

## Acceptance Criteria

### Visual Design
- ✅ Contracts window uses dark graphite theme consistently
- ✅ Professional business interface appearance
- ✅ Clear contract information hierarchy and layout
- ✅ Responsive design works at minimum resolution

### Functionality
- ✅ Contract filtering works correctly for all states
- ✅ Search functionality finds relevant contracts
- ✅ Sort options organize contracts appropriately
- ✅ Contract actions integrate with game systems

### User Experience
- ✅ Contract browsing feels intuitive and efficient
- ✅ Filter and search provide immediate results
- ✅ Contract information is comprehensive and clear
- ✅ Navigation between contract states is smooth

### Performance
- ✅ Large contract lists display efficiently
- ✅ Filtering and search operations are fast
- ✅ Real-time updates don't impact performance
- ✅ Memory usage optimized for long gameplay sessions

## Technical Notes

### Component Structure
```
ContractsView/
├── ContractsViewController.cs (window behavior)
├── ContractsViewDocument.uxml (contracts layout)
├── ContractsViewStyles.uss (contracts styling)
├── ContractFilter.cs (filtering system)
├── ContractSearch.cs (search functionality)
└── ContractActions.cs (contract action handling)
```

### UXML Structure
- Root window container using BaseWindow
- Header with filter tabs/dropdown and search input
- Main content area with BaseList for contract display
- Contract cards using BaseCard component for each item
- Action buttons and status indicators per contract

### USS Class Naming
- .contracts-view (root container)
- .contracts-header (filter and search area)
- .contracts-filters (filter controls)
- .contracts-search (search input)
- .contracts-list (main contract list)
- .contract-card (individual contract display)
- .contract-actions (action buttons area)

### Contract Display Data
```
ContractDisplayData:
- Title: Contract name for display
- Origin: Starting city name
- Destination: Target city name
- Distance: Route distance in current units
- CargoType: Goods type with icon
- CargoWeight: Weight in current units
- Reward: Payment amount formatted
- Status: Current contract state
- Deadline: Time remaining or completion date
- RequiredLicense: License needed (if any)
```

### Filter States
- **All Contracts**: Show all contracts regardless of state
- **Available**: Contracts that can be accepted
- **Accepted**: Contracts assigned to vehicles
- **Ready to Complete**: Vehicles arrived at destination
- **Completed**: Historical finished contracts

## Dependencies
- Requires Phase 2B (Base Window Component) for foundation
- Requires Phase 2E (Base List Component) for contract listing
- Requires Phase 2D (Base Card Component) for contract display
- Requires Phase 6A (Contract Generation) for contract data

## Integration Points
- BaseWindow provides window framework and styling
- BaseList handles efficient contract list display
- BaseCard components display individual contract information
- Contract generation system provides available contracts

## Notes

### Contract Card Layout
- **Header**: Contract title and reward prominently displayed
- **Route**: Origin → Destination with distance
- **Cargo**: Goods type, weight, and license requirement
- **Status**: Current state with progress indicator
- **Actions**: Accept, Cancel, or Complete buttons

### Search Functionality
- Text search across contract titles and descriptions
- City name search for origin and destination
- Goods type filtering with category selection
- Combined search criteria for precise filtering

### Sort Options
- **By Reward**: Highest to lowest payment
- **By Distance**: Shortest to longest routes
- **By Deadline**: Most urgent first
- **By Status**: Group by contract state
- **By Date**: Newest or oldest first

### Performance Optimization
- Virtual scrolling for large contract lists
- Lazy loading of contract details
- Efficient filtering without full list recreation
- Cached search results for repeated queries

### Contract State Indicators
- **Available**: Green indicator, "Accept" button
- **Accepted**: Yellow indicator, vehicle assignment shown
- **In Progress**: Blue indicator, progress percentage
- **Ready**: Green flash, "Complete" button prominent
- **Completed**: Gray indicator, completion date shown

### Accessibility Features
- Keyboard navigation through contract list
- Screen reader support for all contract information
- High contrast mode for contract status indicators
- Focus management for filter and search controls

### Testing Requirements
- Test filtering with various contract combinations
- Verify search functionality with different criteria
- Test sort operations with large contract lists
- Validate real-time updates when contract states change
- Performance test with 100+ contracts in list