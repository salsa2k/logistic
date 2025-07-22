# Phase 3D - New Game Modal

## Overview
Create a dark themed company creation modal for new game setup, allowing players to choose company name, logo, and vehicle color while integrating with the save system.

## Tasks

### New Game Modal Layout
- Design company creation modal using BaseModal component
- Create multi-step form layout with clear progress indication
- Implement dark graphite theme throughout the interface
- Set up validation and error handling for user inputs

### Company Name Input
- Text input field for company name entry
- Real-time validation for name length and characters
- Name uniqueness checking against existing saves
- Clear error messaging for invalid names

### Logo Selection
- Display 3 predefined logo options from Art/UI/Company
- Visual selection interface with preview capability
- Clear indication of currently selected logo
- Logo preview in context of company branding

### Vehicle Color Selection
- Color palette interface with predefined vehicle colors
- Visual preview of selected color on vehicle representation
- Color swatches with clear selection indication
- Professional color options suitable for business vehicles

### Starting Credits Setup
- Display starting credits amount (100,000 credits)
- Clear explanation of starting resources
- Professional presentation of financial information
- Integration with credit display systems

### Form Validation and Submission
- Real-time validation of all required fields
- Clear error messaging for incomplete/invalid data
- Confirmation of choices before game creation
- Progress indication through setup steps

### Game Creation Process
- Save new company data through save system
- Initialize game state with chosen parameters
- Transition to main game scene with new game state
- Error handling for save failures

## Acceptance Criteria

### Visual Design
- ✅ Modal uses dark graphite theme consistently
- ✅ Professional appearance suitable for business setup
- ✅ Clear visual hierarchy and organization
- ✅ Responsive layout at minimum resolution

### Functionality
- ✅ Company name validation works correctly
- ✅ Logo selection provides clear preview
- ✅ Color selection shows accurate vehicle representation
- ✅ New game creation saves data properly

### User Experience
- ✅ Setup process feels guided and intuitive
- ✅ Validation provides helpful feedback
- ✅ Preview features help users make informed choices
- ✅ Error handling is graceful and informative

### Integration
- ✅ Save system stores company data correctly
- ✅ Game state initializes with chosen parameters
- ✅ Transition to game scene works smoothly
- ✅ Asset Manager provides logos and color previews

## Technical Notes

### Component Structure
```
NewGameModal/
├── NewGameModalController.cs (modal behavior)
├── NewGameModalDocument.uxml (setup layout)
├── NewGameModalStyles.uss (setup styling)
├── CompanySetup.cs (setup data and validation)
└── LogoPreview.cs (logo display component)
```

### UXML Structure
- Root modal container using BaseModal
- Multi-step form with progress indicator
- Company name input with validation feedback
- Logo selection grid with preview
- Color palette with vehicle preview
- Action buttons for navigation and submission

### USS Class Naming
- .new-game-modal (root container)
- .setup-step (individual setup section)
- .company-name-input (name entry field)
- .logo-selection (logo choice interface)
- .color-palette (color selection)
- .vehicle-preview (color preview area)
- .setup-actions (navigation buttons)

### Company Data Structure
```
CompanyData:
- Name (string, 3-50 characters)
- LogoId (int, 0-2 for three logo options)
- VehicleColor (Color, from predefined palette)
- StartingCredits (decimal, 100000)
- CreationDate (DateTime)
```

## Dependencies
- Requires Phase 2C (Base Modal Component) for foundation
- Requires Phase 1D (Save System) for company data persistence
- Will create initial game state for main game scene

## Integration Points
- BaseModal provides modal dialog functionality
- Asset Manager supplies company logos and color swatches
- Save system persists new company data
- Game state initializes with company information

## Notes

### Company Name Validation Rules
- Minimum 3 characters, maximum 50 characters
- Allow letters, numbers, spaces, and common punctuation
- Prevent inappropriate or offensive names
- Check uniqueness against existing save files

### Logo Options (Art/UI/Company)
- **Logo 1**: Classic transport/shipping theme
- **Logo 2**: Modern logistics design
- **Logo 3**: Industrial/heavy freight theme
- All logos professional and suitable for business context

### Vehicle Color Palette
- Professional business colors (blues, grays, whites)
- Avoid overly bright or unprofessional colors
- Ensure good visibility and contrast
- Colors work well with vehicle designs

### Setup Flow
1. Welcome screen with explanation
2. Company name entry and validation
3. Logo selection with preview
4. Vehicle color selection with preview
5. Confirmation screen with all choices
6. Game creation and transition

### Error Handling
- Clear validation messages for each field
- Graceful handling of save system failures
- Option to retry failed operations
- Fallback options for corrupted data

### Accessibility Features
- Keyboard navigation through all setup steps
- Screen reader support for all form elements
- Clear focus indicators and labels
- High contrast mode compatibility

### Testing Requirements
- Test all validation rules thoroughly
- Verify logo and color preview accuracy
- Test save system integration
- Validate error handling scenarios
- Performance test setup completion