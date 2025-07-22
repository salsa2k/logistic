# Phase 8D - Conditional Map Details

## Overview
Implement conditional display of map details based on vehicle selection and context, showing gas stations and police checkpoints only when relevant to selected vehicles.

## Tasks

### Conditional Detail System
- Display gas stations and police checkpoints only when vehicle is selected and moving
- Hide irrelevant map details when no vehicle is selected
- Context-sensitive information display based on vehicle status
- Save display preferences for user-customized map views

### Gas Station Display Logic
- Show gas stations only when vehicle is selected and moving
- Display gas stations along selected vehicle's route
- Highlight refuel options based on vehicle fuel level
- Provide distance and pricing information for route planning

### Police Checkpoint Display
- Display checkpoints only when vehicle is selected and moving
- Show checkpoints along selected vehicle's route
- Highlight speed enforcement zones for route awareness
- Provide speed limit and fine information

### Route-Specific Details
- Filter map details based on selected vehicle's planned route
- Show only relevant points of interest along the path
- Highlight upcoming facilities and checkpoints
- Provide contextual information for route decisions

### Vehicle Context Integration
- Different detail levels based on vehicle type and license
- Show specialized facilities relevant to vehicle capabilities
- Filter information based on cargo type and requirements
- Display emergency services for vehicles in distress

### Map Detail Preferences
- User preferences for map detail visibility
- Customizable display options for different information types
- Save personal preferences through settings system
- Quick toggle options for detail categories

### Performance Optimization
- Efficient rendering of conditional details
- Dynamic loading of detail information based on context
- Culling of irrelevant details for performance
- Optimized update cycles for changing contexts

## Acceptance Criteria

### Conditional Display
- ✅ Gas stations and checkpoints appear only when appropriate
- ✅ Map details change correctly based on vehicle selection
- ✅ Irrelevant information is hidden to reduce visual clutter
- ✅ Context switching is smooth and responsive

### Route Integration
- ✅ Details display accurately along selected vehicle routes
- ✅ Route-specific filtering shows only relevant information
- ✅ Distance and timing information is accurate
- ✅ Contextual data helps with route planning decisions

### User Experience
- ✅ Map detail visibility feels logical and helpful
- ✅ Information appears at appropriate zoom levels
- ✅ Detail preferences persist across game sessions
- ✅ Toggle controls are accessible and intuitive

### Performance
- ✅ Conditional rendering maintains smooth performance
- ✅ Detail loading doesn't cause frame drops
- ✅ Memory usage is optimized for dynamic content
- ✅ Update frequency maintains real-time responsiveness

## Technical Notes

### Component Structure
```
ConditionalMapDetails/
├── MapDetailManager.cs (detail visibility control)
├── GasStationDisplay.cs (gas station rendering)
├── CheckpointDisplay.cs (police checkpoint rendering)
├── RouteDetailFilter.cs (route-based filtering)
└── DetailPreferences.cs (user preference management)
```

### Detail Visibility Rules
```
VisibilityConditions:
- NoVehicleSelected: Hide all conditional details
- VehicleSelectedStationary: Show city-specific details only
- VehicleSelectedMoving: Show route details (gas stations, checkpoints)
- EmergencyState: Show emergency services and refuel options
- CustomPreferences: Apply user-defined visibility settings
```

### Route-Based Filtering
```
RouteFiltering:
- RouteAnalysis: Identify facilities along planned route
- DistanceCalculation: Calculate distances from vehicle position
- RelevanceScoring: Prioritize most relevant facilities
- UpdateTriggers: Refresh when route or vehicle changes
```

### Gas Station Display System
- Station markers appear when vehicle is moving
- Fuel price information displayed on hover/selection
- Distance and detour calculation from main route
- Fuel level warnings trigger enhanced station visibility

### Police Checkpoint Display
- Checkpoint markers shown for vehicles in transit
- Speed limit information and fine amounts
- Warning indicators for vehicles exceeding limits
- Route timing affected by checkpoint delays

## Dependencies
- Requires Phase 8C (Vehicle Positioning) for vehicle context
- Requires gas station and checkpoint placement systems
- Requires vehicle selection and route planning systems

## Integration Points
- Vehicle positioning provides context for detail display
- Route planning determines relevant facilities along paths
- User preferences system stores display settings
- Save system maintains map detail preferences

## Notes

### Display Logic Flow
1. Check vehicle selection status
2. Determine vehicle operational state
3. Calculate current or planned route
4. Filter relevant map details for route
5. Apply user preference overrides
6. Render conditional details with appropriate styling
7. Update details when context changes

### Gas Station Visibility Rules
- **No Vehicle Selected**: All gas stations hidden
- **Vehicle Selected (Stationary)**: Local gas stations visible
- **Vehicle Moving**: Route gas stations highlighted
- **Low Fuel Warning**: All nearby stations emphasized
- **Emergency Fuel**: Closest stations with emergency service

### Checkpoint Visibility Rules
- **No Vehicle Selected**: Checkpoints hidden
- **Vehicle Moving**: Route checkpoints visible
- **Speeding Vehicle**: Relevant checkpoints highlighted
- **Fine History**: Previous fine locations marked
- **Route Planning**: Checkpoint delays calculated

### Detail Information Hierarchy
- **Critical**: Emergency services, fuel warnings
- **Important**: Route checkpoints, recommended gas stations
- **Helpful**: Alternative routes, optional services
- **Background**: General points of interest

### Zoom Level Considerations
- Detail visibility scales with zoom level
- Important details appear at lower zoom levels
- Comprehensive information at higher zoom levels
- Text and icon scaling for readability

### User Preference Options
- **Always Show**: Override conditional logic for specific details
- **Never Show**: Hide details even when contextually relevant
- **Smart Display**: Use conditional logic (default)
- **Custom Filters**: User-defined visibility rules

### Performance Considerations
- Lazy loading of detail information
- Efficient culling of off-screen details
- Cached calculation results for route filtering
- Optimized rendering for dynamic visibility changes

### Testing Requirements
- Test detail visibility with various vehicle states
- Verify route-based filtering accuracy
- Test user preference persistence and application
- Validate performance with complex route scenarios
- Test accessibility of detail information