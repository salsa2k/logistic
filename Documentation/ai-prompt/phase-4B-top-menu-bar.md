# Phase 4B - Top Menu Bar

## Overview
Create the dark graphite themed navigation bar that spans the top of the main game scene, providing access to Contracts, Vehicles, Shopping, Company, and Settings interfaces.

## Tasks

### Top Menu Bar Layout
- Design horizontal navigation bar using dark graphite theme
- Create menu UXML template with button layout and spacing
- Implement responsive design for different screen widths
- Set up left-aligned navigation buttons and right-aligned utility buttons

### Navigation Button Implementation
- Contracts button to open contracts management window
- Vehicles button to open vehicles management window
- Shopping button to open vehicle and license purchasing interface
- Company button to open company information and licenses view
- Consistent button styling using base button components

### Right-Aligned Utility Area
- Settings button for accessing in-game settings modal
- Credits display widget integration (handled in Phase 4C)
- Proper spacing and alignment for utility elements
- Responsive behavior for different screen sizes

### Active State Management
- Visual indication of currently active section
- State persistence during navigation between views
- Proper highlighting and selection feedback
- Integration with window management system

### Menu Bar Styling
- Dark graphite background consistent with game theme
- Professional business application appearance
- Subtle borders and shadows for visual definition
- Hover and active states for all interactive elements

### Integration with Game Systems
- Window management coordination for opening/closing views
- Event system integration for navigation actions
- State management for active view tracking
- Save system integration for preserving user preferences

## Acceptance Criteria

### Visual Design
- ✅ Menu bar uses dark graphite theme consistently
- ✅ Professional appearance matching business application standards
- ✅ Clear visual hierarchy and button organization
- ✅ Responsive design works at minimum 1280x720 resolution

### Functionality
- ✅ All navigation buttons open correct windows/views
- ✅ Active state indication works properly
- ✅ Settings button opens in-game settings modal
- ✅ Menu bar integrates smoothly with game scene layout

### User Experience
- ✅ Navigation feels immediate and responsive
- ✅ Button hover states provide clear feedback
- ✅ Active section is always clearly indicated
- ✅ Menu bar doesn't interfere with main game content

### Technical Requirements
- ✅ Efficient UI rendering and event handling
- ✅ Proper integration with window management system
- ✅ Clean code following project conventions
- ✅ Memory optimization for permanent UI element

## Technical Notes

### Component Structure
```
TopMenuBar/
├── TopMenuBarController.cs (menu bar behavior)
├── TopMenuBarDocument.uxml (menu bar layout)
├── TopMenuBarStyles.uss (menu bar styling)
└── NavigationManager.cs (view state management)
```

### UXML Structure
- Root menu bar container spanning full width
- Left section with navigation buttons (Contracts, Vehicles, Shopping, Company)
- Right section with utility buttons (Settings) and credits display
- Responsive flexbox layout for proper spacing

### USS Class Naming
- .top-menu-bar (root container)
- .menu-nav-section (left navigation area)
- .menu-utility-section (right utility area)
- .menu-nav-button (navigation buttons)
- .menu-nav-button-active (active state)
- .menu-utility-button (utility buttons)

### Navigation Button Layout
- **Left-aligned**: Contracts | Vehicles | Shopping | Company
- **Right-aligned**: Credits Display | Settings
- Consistent spacing and sizing throughout
- Professional business application styling

## Dependencies
- Requires Phase 4A (Game Scene Layout) for integration
- Requires Phase 2F (Base Button Styles) for button styling
- Will integrate with Phase 4C (Credits Display Widget)

## Integration Points
- Game Scene Layout provides the top menu bar area
- Window Manager handles opening/closing of different views
- Base Button Styles provide consistent navigation button appearance
- Save system may store active view preferences

## Notes

### Button Behavior Specifications
- Single click to switch between main views
- Active state remains until different view is selected
- Hover effects provide immediate visual feedback
- Keyboard navigation support with tab/arrow keys

### Responsive Design Considerations
- Menu bar maintains fixed height (60px)
- Button text may be hidden on very narrow screens
- Icon-only mode for mobile/compact interfaces
- Proper touch targets for tablet interfaces

### State Management
- Track currently active main view (Contracts, Vehicles, etc.)
- Coordinate with window management system
- Persist user's preferred starting view
- Handle multiple window scenarios appropriately

### Performance Considerations
- Menu bar is permanent UI element, optimize for efficiency
- Fast button response without animation delays
- Minimal memory allocation for frequent interactions
- Efficient event handling for navigation actions

### Accessibility Features
- Keyboard navigation between all menu buttons
- Screen reader support with proper button labels
- High contrast mode compatibility
- Clear focus indicators for all interactive elements

### Testing Requirements
- Test all navigation button functionality
- Verify active state management across different views
- Test responsive behavior at various screen widths
- Validate accessibility with keyboard navigation
- Performance test menu bar rendering and interactions