# Phase 3A - Main Menu Window

## Overview
Create the main menu interface using the dark graphite theme with navigation buttons for New Game, Load Game, Settings, Credits, and Exit.

## Tasks

### Main Menu Layout
- Design main menu using BaseWindow component as foundation
- Create menu UXML template with vertical button layout
- Implement background image integration for game theme
- Set up responsive layout for different screen resolutions

### Menu Button Implementation
- New Game button to start company creation process
- Load Game button to display saved game selection
- Settings button to open main menu settings modal
- Credits button to navigate to credits scene
- Exit button to quit the application

### Visual Design
- Dark graphite theme background with game-themed imagery
- Professional button styling using base button components
- Consistent spacing and typography throughout menu
- Subtle animations for button interactions

### Menu Functionality
- Button click handlers for all menu options
- Smooth transitions between menu and other scenes
- Proper cleanup when leaving main menu
- Integration with save/load system for game detection

### Background and Theming
- Game-themed background image representing logistics theme
- Dark overlay to ensure button readability
- Consistent with overall dark graphite aesthetic
- Professional business application appearance

### Navigation System
- Scene management for transitions to game or credits
- Modal management for settings and load game dialogs
- Proper cleanup of menu resources
- Return to menu functionality from other scenes

## Acceptance Criteria

### Visual Design
- ✅ Main menu uses dark graphite theme consistently
- ✅ Background image enhances theme without compromising readability
- ✅ Button layout is visually appealing and professional
- ✅ Responsive design works at minimum 1280x720 resolution

### Functionality
- ✅ All menu buttons navigate to correct destinations
- ✅ Scene transitions are smooth and responsive
- ✅ Save game detection works for Load Game button
- ✅ Exit functionality works properly on target platforms

### User Experience
- ✅ Menu feels responsive and immediate
- ✅ Navigation is intuitive and predictable
- ✅ Visual hierarchy guides user attention appropriately
- ✅ Accessibility features work for keyboard navigation

### Technical Requirements
- ✅ Proper integration with BaseWindow component
- ✅ Efficient memory usage and resource management
- ✅ Clean code following project conventions
- ✅ Event system integration for menu actions

## Technical Notes

### Component Structure
```
MainMenu/
├── MainMenuController.cs (menu behavior)
├── MainMenuDocument.uxml (menu layout)
├── MainMenuStyles.uss (menu-specific styles)
└── MainMenuBackground/ (background assets)
```

### UXML Structure
- Root window container using BaseWindow
- Background image container with overlay
- Central button container with vertical layout
- Individual menu buttons with consistent styling

### USS Class Naming
- .main-menu (root container)
- .main-menu-background (background image area)
- .main-menu-buttons (button container)
- .main-menu-button (individual button styling)
- .main-menu-title (game title if included)

### Menu Navigation
- New Game → Company creation modal/scene
- Load Game → Load game selection modal
- Settings → Main menu settings modal
- Credits → Credits scene
- Exit → Application quit with confirmation

## Dependencies
- Requires Phase 2B (Base Window Component) for foundation
- Requires Phase 2F (Base Button Styles) for button styling
- Will integrate with Phase 3B (Settings Modal) and Phase 3C (Credits Scene)

## Integration Points
- BaseWindow provides core window functionality
- Asset Manager supplies background images and assets
- Scene Manager handles transitions to other scenes
- Save/Load system provides game state detection

## Notes

### Background Image Requirements
- Logistics/transportation themed artwork
- High enough resolution for largest supported screen size
- Dark enough to maintain button readability
- Professional business aesthetic matching game theme

### Button Layout Specifications
- Vertical stack with consistent spacing
- Center-aligned within menu window
- Large enough for easy clicking/touching
- Clear visual hierarchy with proper contrast

### Platform Considerations
- Exit button behavior varies by platform
- Touch-friendly button sizing for potential mobile support
- Keyboard navigation support for accessibility
- Controller support for potential console versions

### Performance Considerations
- Optimize background image for fast loading
- Efficient UI element management
- Smooth animations without frame drops
- Quick scene transitions

### Accessibility Features
- Keyboard navigation between menu buttons
- Screen reader support for all menu options
- High contrast mode compatibility
- Focus indicators clearly visible

### Testing Requirements
- Test at minimum resolution (1280x720)
- Verify all button functionality
- Test scene transitions and memory cleanup
- Validate accessibility with keyboard navigation
- Performance test menu loading and animations