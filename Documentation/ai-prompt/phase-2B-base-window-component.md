# Phase 2B - Base Window Component

## Overview
Create a reusable window UI component using the dark graphite theme that serves as the foundation for all major game interfaces.

## Tasks

### Window Component Architecture
- Design BaseWindow class inheriting from MonoBehaviour
- Create window UXML template with title bar, content area, and optional footer
- Implement window USS styling using dark graphite theme
- Set up window controller for common window behaviors

### Window Structure Elements
- Title bar with window title and optional close button
- Main content container with scrollable area support
- Optional toolbar/button area below title
- Optional footer area for action buttons
- Resize and drag handles if needed

### Window Styling
- Apply dark graphite theme colors and spacing
- Create professional window borders and shadows
- Implement responsive sizing for different screen sizes
- Style title bar with consistent typography

### Window Behaviors
- Window show/hide animations with smooth transitions
- Focus management for window interactions
- Z-order management for multiple windows
- Window positioning and centering utilities

### Base Window Features
- Generic content loading system
- Title and icon management
- Close button functionality with events
- Modal and non-modal window support

### Integration with Theme System
- Use theme variables for all colors and spacing
- Follow typography hierarchy for window text
- Implement theme-consistent interactive states
- Support theme switching (for future use)

## Acceptance Criteria

### Visual Design
- ✅ Window uses dark graphite theme consistently
- ✅ Professional appearance with proper contrast
- ✅ Title bar and content areas clearly defined
- ✅ Responsive design works at minimum resolution

### Functionality
- ✅ Window can be shown and hidden smoothly
- ✅ Content area supports dynamic content loading
- ✅ Close button triggers proper events
- ✅ Window positioning works correctly

### Reusability
- ✅ BaseWindow can be easily extended for specific use cases
- ✅ UXML template supports content customization
- ✅ USS classes allow style overrides when needed
- ✅ Window behaviors are configurable

### Performance
- ✅ Smooth animations without frame drops
- ✅ Efficient UI element management
- ✅ Memory usage optimized for multiple windows
- ✅ Fast show/hide operations

## Technical Notes

### Component Structure
```
BaseWindow/
├── BaseWindow.cs (MonoBehaviour controller)
├── BaseWindowDocument.uxml (UI structure)
├── BaseWindowStyles.uss (window-specific styles)
└── WindowAnimations.cs (animation helpers)
```

### UXML Structure
- Root container with window styling
- Title bar with title label and close button
- Content container with overflow handling
- Footer container for action buttons

### USS Class Naming
- .base-window (root container)
- .window-title-bar (title area)
- .window-content (main content area)
- .window-footer (button area)
- .window-close-button (close button)

### Animation System
- Fade in/out transitions
- Scale animations for window appearance
- Smooth easing curves for professional feel
- Configurable animation duration and curves

## Dependencies
- Requires Phase 1A (Basic Unity Setup) for project structure
- Requires Phase 2A (Dark Graphite Theme) for styling
- Will be extended by all window-based UI phases

## Integration Points
- Asset Manager provides access to window assets
- Theme system supplies all styling variables
- Event system handles window open/close events
- All major game interfaces will extend this component

## Notes

### Design Principles
- Consistent appearance across all game windows
- Intuitive user interactions
- Professional business application feel
- Accessibility considerations for all users

### Extensibility
- Easy to create specialized window types
- Content area supports any UI Toolkit elements
- Style overrides available for special cases
- Event system allows custom window behaviors

### Window Types to Support
- Main game windows (Contracts, Vehicles, Shopping)
- Modal dialogs (Settings, Confirmations)
- Information displays (Company, Credits)
- Interactive forms (New Game setup)

### Performance Considerations
- Use object pooling for frequently shown windows
- Optimize UI element creation and destruction
- Efficient event handling and cleanup
- Memory management for window content

### Testing Requirements
- Test window at different screen resolutions
- Verify animation performance on target platforms
- Test multiple windows open simultaneously
- Validate accessibility and keyboard navigation