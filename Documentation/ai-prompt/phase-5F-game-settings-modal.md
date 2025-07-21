# Phase 5F - Game Settings Modal

## Overview
Create an in-game settings modal accessible during gameplay for adjusting language, unit system, and other preferences with immediate application and save integration.

## Tasks

### In-Game Settings Modal Layout
- Design settings modal using BaseModal component for consistent styling
- Create organized settings sections with clear categories
- Implement dark graphite theme throughout the interface
- Set up responsive layout for different screen sizes

### Settings Categories Organization
- Language and localization settings
- Unit system preferences (metric/imperial)
- Audio and sound settings
- Display and graphics options
- Gameplay preferences and options

### Language Settings Implementation
- Language dropdown with current game languages (English/Portuguese)
- Immediate language switching throughout all game interfaces
- Visual feedback for language change application
- Integration with localization system for real-time updates

### Unit System Settings
- Toggle or dropdown for metric/imperial unit selection
- Immediate conversion of all displayed measurements
- Clear labeling of current unit system
- Real-time updates of vehicle specifications, distances, and weights

### Audio Settings Integration
- Master volume control for overall game audio
- Separate controls for music, sound effects, and UI sounds
- Audio test buttons for immediate feedback
- Mute toggles for different audio categories

### Display Settings
- Resolution options (if applicable)
- Fullscreen/windowed mode toggle
- UI scale options for different screen sizes
- Color accessibility options

### Settings Application System
- Immediate application of settings changes
- Real-time preview of setting effects
- Validation of setting combinations
- Revert options for problematic settings

### Save Integration
- Automatic saving of settings changes
- Settings persistence through game sessions
- Integration with main settings persistence system
- Conflict resolution with main menu settings

## Acceptance Criteria

### Visual Design
- ✅ Settings modal uses dark graphite theme consistently
- ✅ Clear organization and intuitive layout
- ✅ Professional appearance matching game interface
- ✅ Responsive design works at all supported resolutions

### Functionality
- ✅ Language changes apply immediately throughout game
- ✅ Unit conversions update all relevant displays
- ✅ Audio settings provide immediate feedback
- ✅ All settings persist correctly through sessions

### User Experience
- ✅ Settings changes feel immediate and responsive
- ✅ Clear feedback for all setting modifications
- ✅ Easy access during gameplay without disruption
- ✅ Intuitive controls and clear labeling

### Integration
- ✅ Seamless integration with main settings system
- ✅ Save system stores all preference changes
- ✅ Localization system responds to language changes
- ✅ All game systems respect updated settings

## Technical Notes

### Component Structure
```
GameSettingsModal/
├── GameSettingsController.cs (modal behavior)
├── GameSettingsDocument.uxml (settings layout)
├── GameSettingsStyles.uss (settings styling)
├── SettingsValidator.cs (settings validation)
└── SettingsApplicator.cs (immediate application)
```

### Settings Data Structure
```
GameSettings:
- Language: Current language selection
- UnitSystem: Metric or Imperial
- AudioSettings: Volume levels and preferences
- DisplaySettings: Resolution and visual options
- GameplaySettings: Game-specific preferences
```

### Integration with Main Settings
- Shared settings data structure with main menu settings
- Consistent behavior between main menu and in-game settings
- Synchronized save/load with main settings system
- Conflict resolution for simultaneous changes

## Dependencies
- Requires Phase 2C (Base Modal Component) for foundation
- Requires Phase 3B (Settings Modal) for settings system integration
- Requires Phase 1B (Localization System) for language switching

## Integration Points
- Main settings system provides data persistence
- Localization system handles language changes
- Audio system responds to volume settings
- All game interfaces update with setting changes

## Notes

### Setting Categories
- **Language**: English/Portuguese selection with immediate switching
- **Units**: Metric/Imperial with real-time conversion
- **Audio**: Master, Music, SFX, UI volume controls
- **Display**: Resolution, fullscreen, UI scale options
- **Gameplay**: Tutorial settings, auto-save preferences

### Immediate Application
- Language changes update all visible text immediately
- Unit changes convert all displayed measurements
- Audio changes provide immediate volume feedback
- Display changes apply without requiring restart

### Performance Considerations
- Efficient setting application without game interruption
- Minimal performance impact during setting changes
- Optimized localization switching
- Fast save operations for setting persistence

### Testing Requirements
- Test all setting changes during active gameplay
- Verify immediate application without game disruption
- Test save/load integration with settings persistence
- Validate localization switching with all UI elements