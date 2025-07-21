# Phase 3B - Settings Modal

## Overview
Create a dark themed settings modal accessible from the main menu, allowing players to configure language, unit system, and other preferences with integration to the save/load system.

## Tasks

### Settings Modal Layout
- Design settings modal using BaseModal component
- Create settings UXML template with organized sections
- Implement settings controls using dark graphite theme
- Set up responsive layout for different screen sizes

### Language Settings
- Language dropdown with English and Brazilian Portuguese options
- Immediate language switching throughout the interface
- Language preference persistence through save system
- Localization integration for all settings text

### Unit System Settings
- Toggle switch for metric/imperial unit systems
- Immediate unit conversion throughout game interface
- Unit preference persistence through save system
- Clear labeling of current unit system

### Settings Organization
- Grouped settings sections (Language, Units, Audio, etc.)
- Clear section headers and descriptions
- Logical flow and organization of options
- Expandable sections for advanced settings if needed

### Settings Actions
- Apply button to save changes
- Cancel button to discard changes
- Reset to Defaults button for all settings
- Close button for modal dismissal

### Settings Persistence
- Integration with save system for preference storage
- Immediate application of settings changes
- Validation of settings before saving
- Error handling for invalid settings values

## Acceptance Criteria

### Visual Design
- ✅ Settings modal uses dark graphite theme consistently
- ✅ Professional appearance with clear organization
- ✅ Controls are clearly labeled and intuitive
- ✅ Responsive layout works at minimum resolution

### Functionality
- ✅ Language changes apply immediately throughout interface
- ✅ Unit system changes apply to all relevant displays
- ✅ Settings persist correctly through save system
- ✅ Reset to defaults restores original settings

### User Experience
- ✅ Settings organization is logical and intuitive
- ✅ Changes provide immediate visual feedback
- ✅ Modal navigation is smooth and responsive
- ✅ Help text explains setting effects clearly

### Integration
- ✅ Localization system responds to language changes
- ✅ Save system persists all setting preferences
- ✅ Settings apply across all game interfaces
- ✅ Error handling provides helpful feedback

## Technical Notes

### Component Structure
```
SettingsModal/
├── SettingsModalController.cs (modal behavior)
├── SettingsModalDocument.uxml (settings layout)
├── SettingsModalStyles.uss (settings styling)
└── SettingsData.cs (settings data structure)
```

### UXML Structure
- Root modal container using BaseModal
- Settings sections with headers and controls
- Language dropdown with flag icons
- Unit system toggle with clear labels
- Action buttons in modal footer

### USS Class Naming
- .settings-modal (root container)
- .settings-section (grouped settings area)
- .settings-header (section title)
- .settings-control (individual setting)
- .settings-dropdown (language selector)
- .settings-toggle (unit system switch)

### Settings Data Structure
```
SettingsData:
- Language (enum: English, Portuguese)
- UnitSystem (enum: Metric, Imperial)
- AudioVolume (float: 0.0-1.0)
- ShowTutorials (bool)
- AutoSave (bool)
```

## Dependencies
- Requires Phase 2C (Base Modal Component) for foundation
- Requires Phase 1B (Localization System) for language switching
- Requires Phase 1D (Save System) for settings persistence

## Integration Points
- BaseModal provides modal dialog functionality
- Localization system handles language changes
- Save system persists settings preferences
- All game interfaces respond to settings changes

## Notes

### Language Switching Implementation
- Update all UI text immediately when language changes
- Reload localized assets if necessary
- Maintain current game state during language switch
- Test all interfaces for proper text updates

### Unit System Implementation
- Convert all displayed values (distance, weight, volume)
- Update unit labels throughout interface
- Maintain internal metric calculations
- Provide conversion utilities for display

### Settings Validation
- Validate language selection availability
- Check unit system compatibility
- Ensure audio volume within valid range
- Prevent invalid setting combinations

### Default Settings
- English language as default
- Metric units as default
- Medium audio volume as default
- Tutorials enabled for new users
- Auto-save enabled for convenience

### Error Handling
- Handle missing localization files gracefully
- Provide fallback for corrupted settings
- Display helpful error messages for invalid settings
- Allow settings reset when problems occur

### Accessibility Features
- Keyboard navigation through all settings
- Screen reader support for all controls
- Clear focus indicators for all interactive elements
- High contrast mode compatibility

### Testing Requirements
- Test language switching with all interface elements
- Verify unit conversions display correctly
- Test settings persistence through game restart
- Validate error handling with corrupted settings
- Performance test settings application speed