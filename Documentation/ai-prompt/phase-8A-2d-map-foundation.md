# Phase 8A - 2D Map Foundation

## Overview
Create the foundational 2D map system for the logistics game with top-down view, city display, and basic navigation capabilities while saving city visited status.

## Tasks

### 2D Map Architecture
- Design top-down 2D map view for game world visualization
- Create map coordinate system and world space mapping
- Implement camera controls for map navigation (pan, zoom)
- Set up map rendering system with layers and visual hierarchy

### City Display System
- Position 10 predefined cities on the map with realistic spacing
- Create city visual representation with icons and labels
- Implement city state visualization (visited/unvisited)
- Set up city interaction system for selection and information

### Map Navigation Controls
- Mouse/touch-based map panning and dragging
- Zoom controls with mouse wheel and touch gestures
- Keyboard navigation support for accessibility
- Smooth camera movement and bounds management

### Visual Map Elements
- Map background with appropriate terrain/geographic styling
- City markers with distinctive visual design
- Visual hierarchy for different map elements
- Professional cartographic styling consistent with game theme

### City State Management
- Track visited/unvisited status for each city
- Visual differentiation between visited and unvisited cities
- Save system integration for persistent city visit status
- City unlock progression as player explores

### Map Coordinate System
- Establish consistent coordinate system for all map elements
- Convert between screen coordinates and world coordinates
- Handle different zoom levels and camera positions
- Ensure accurate positioning for all map elements

### Performance Optimization
- Efficient rendering for map elements at different zoom levels
- Level-of-detail system for complex map features
- Culling of off-screen elements for performance
- Optimized update cycles for map animations

## Acceptance Criteria

### Visual Design
- ✅ Map uses dark graphite theme with professional cartographic styling
- ✅ Cities are clearly visible and appropriately positioned
- ✅ Visual hierarchy guides user attention effectively
- ✅ Map appearance is consistent with overall game design

### Navigation
- ✅ Map navigation is smooth and responsive
- ✅ Zoom controls work intuitively across different input methods
- ✅ Camera bounds prevent navigation outside valid map area
- ✅ Performance remains stable during navigation

### City System
- ✅ Cities display visited/unvisited status accurately
- ✅ City interaction provides appropriate feedback
- ✅ City positions reflect realistic geographic relationships
- ✅ Save system maintains city visit status correctly

### Technical Performance
- ✅ Map rendering is efficient at all zoom levels
- ✅ Coordinate system calculations are accurate
- ✅ Memory usage is optimized for map display
- ✅ Update frequency maintains smooth user experience

## Technical Notes

### Component Structure
```
MapFoundation/
├── MapController.cs (main map management)
├── MapCamera.cs (camera and navigation)
├── CityManager.cs (city display and interaction)
├── MapRenderer.cs (visual rendering system)
└── CoordinateSystem.cs (coordinate conversions)
```

### Map Coordinate System
```
MapCoordinates:
- WorldSpace: Game world coordinates in kilometers
- ScreenSpace: UI pixel coordinates
- MapSpace: Normalized map coordinates (0-1)
- CityPositions: Fixed positions for 10 cities
- ConversionMethods: Transform between coordinate systems
```

### City Data Structure
```
CityMapData:
- Name: City identifier and display name
- Position: World coordinates on map
- VisitedStatus: Player has been to this city
- UnlockStatus: City is available for contracts
- VisualState: Current display appearance
- InteractionBounds: Click/touch detection area
```

### Predefined Cities Layout
- Port Vireo: Coastal position, starting city
- Brunholt: Northern region, industrial center
- Calderique: Central location, major hub
- Nordhagen: Northwestern area, mountain region
- Arelmoor: Eastern location, agricultural area
- Sundale Ridge: Southern region, suburban area
- Veltrona: Western coast, tourist destination
- Duskwell: Central-east, mining town
- New Halvern: Northeast, technology center
- Eastmere Bay: Southeastern coast, fishing port

## Dependencies
- Requires Phase 1C (Data Structures) for city data
- Requires Phase 1D (Save System) for visit status persistence
- Provides foundation for all subsequent map features

## Integration Points
- Save system stores and loads city visit status
- City data structures provide map positioning information
- Game scene layout provides map display area
- Vehicle systems will use map for positioning display

## Notes

### Map Visual Design
- Professional cartographic styling with dark theme
- Subtle grid or landmark references for navigation
- Clear visual distinction between land and water areas
- Appropriate color scheme for extended viewing

### City Visual States
- **Unvisited**: Dimmed or outline-only appearance
- **Visited**: Full color and detailed icon display
- **Current Location**: Highlighted or animated indicator
- **Contract Available**: Special indicator for opportunities

### Navigation Behavior
- Smooth camera movement with easing
- Zoom levels from overview to detailed city view
- Map bounds prevent scrolling beyond game world
- Center-on-city functionality for quick navigation

### Performance Considerations
- Efficient rendering of map background
- LOD system for city details based on zoom
- Culling system for off-screen elements
- Optimized coordinate transformation calculations

### Save Integration
- City visit status stored in player progress
- Persistent across game sessions
- Version-compatible save format
- Efficient save/load for map state

### Accessibility Features
- Keyboard navigation for map movement
- Screen reader support for city information
- High contrast mode for visual elements
- Adjustable zoom limits for vision accessibility

### Testing Requirements
- Test map navigation across all zoom levels
- Verify city positioning accuracy and interaction
- Test save/load integration for city visit status
- Validate performance with complex map rendering
- Test accessibility features and keyboard navigation