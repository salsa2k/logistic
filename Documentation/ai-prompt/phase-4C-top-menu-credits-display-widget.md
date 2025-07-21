# Phase 4C - Top Menu Credits Display Widget

## Overview
Create the yellow credits display widget positioned in the top-right corner of the screen, showing the player's current financial balance with real-time updates and professional styling.

## Tasks

### Credits Display Widget Design
- Design credits display widget using yellow color scheme as specified
- Create widget UXML template with currency symbol and amount
- Implement dark graphite background styling with yellow text
- Set up responsive sizing for different screen resolutions

### Credits Amount Formatting
- Display credits with currency symbol (C100.000 format as specified)
- Implement number formatting with proper thousands separators
- Handle large credit amounts with appropriate abbreviations (K, M)
- Support both positive and negative credit displays

### Real-Time Updates
- Integration with game financial system for automatic updates
- Smooth animation transitions when credits change
- Visual feedback for credit gains and losses
- Update triggers from contract completion, purchases, fines, etc.

### Visual Feedback System
- Positive credit changes show green flash or growth animation
- Negative credit changes show red flash or reduction animation
- Neutral state maintains steady yellow color
- Optional particle effects for significant credit changes

### Widget Positioning and Layout
- Right-aligned positioning in top menu bar
- Consistent spacing from screen edge and other elements
- Responsive positioning that works at all screen sizes
- Integration with top menu bar layout system

### Integration with Financial Events
- Contract completion credit rewards
- Vehicle and license purchase deductions
- Fuel purchase and emergency refuel costs
- Police fine deductions
- Any other financial transactions

## Acceptance Criteria

### Visual Design
- ✅ Credits display uses yellow color as specified
- ✅ Dark graphite background provides good contrast
- ✅ Professional appearance matching business theme
- ✅ Currency symbol and formatting are clear and readable

### Functionality
- ✅ Credits amount updates in real-time with financial changes
- ✅ Number formatting handles all credit ranges properly
- ✅ Animation transitions are smooth and professional
- ✅ Widget positioning is consistent and responsive

### User Experience
- ✅ Credit changes provide immediate visual feedback
- ✅ Display is always clearly visible and readable
- ✅ Animations enhance understanding without distraction
- ✅ Widget doesn't interfere with other UI elements

### Technical Requirements
- ✅ Efficient updates without performance impact
- ✅ Proper event handling for financial system integration
- ✅ Memory optimization for permanent UI element
- ✅ Clean code following project conventions

## Technical Notes

### Component Structure
```
CreditsDisplay/
├── CreditsDisplayController.cs (widget behavior)
├── CreditsDisplayDocument.uxml (widget layout)
├── CreditsDisplayStyles.uss (widget styling)
└── CreditsAnimations.cs (update animations)
```

### UXML Structure
- Root credits container with yellow styling
- Currency symbol element (C)
- Credits amount element with number formatting
- Optional animation overlay for visual effects

### USS Class Naming
- .credits-display (root container)
- .credits-symbol (currency symbol)
- .credits-amount (numerical value)
- .credits-gain (positive change animation)
- .credits-loss (negative change animation)
- .credits-background (widget background)

### Color Specifications
- **Primary Text**: Yellow (#FFD700) as specified
- **Background**: Dark graphite with slight transparency
- **Gain Animation**: Green flash (#4CAF50)
- **Loss Animation**: Red flash (#F44336)
- **Border**: Subtle dark border for definition

## Dependencies
- Requires Phase 4B (Top Menu Bar) for positioning integration
- Requires Phase 2A (Dark Graphite Theme) for styling foundation
- Will integrate with all financial systems throughout the game

## Integration Points
- Top Menu Bar provides positioning and layout context
- Financial systems trigger credit updates through events
- Save/Load system persists current credit amounts
- All purchase and reward systems update credits through this widget

## Notes

### Number Formatting Examples
- **1,000**: C1.000
- **10,000**: C10.000
- **100,000**: C100.000 (starting amount)
- **1,000,000**: C1.000M
- **Negative**: C-5.000 (red color override)

### Animation Specifications
- **Credit Gain**: Subtle scale pulse (1.0 → 1.1 → 1.0) over 0.5s
- **Credit Loss**: Brief red flash overlay over 0.3s
- **Large Changes**: Optional particle effect for major transactions
- **Easing**: Smooth ease-out for all animations

### Responsive Behavior
- Maintain fixed position relative to screen edge
- Scale font size appropriately for screen resolution
- Ensure readability at minimum resolution (1280x720)
- Handle very long credit amounts gracefully

### Event Integration
- Listen for CreditsChanged events from financial system
- Update display immediately when credits change
- Trigger appropriate animations based on change type
- Maintain accuracy with actual game financial state

### Accessibility Features
- Screen reader announces credit changes
- High contrast mode uses alternative color scheme
- Keyboard navigation can focus on credits display
- Visual indicators supplement color-based feedback

### Testing Requirements
- Test credit updates from all financial systems
- Verify number formatting with various credit amounts
- Test animations and visual feedback
- Validate responsive positioning at different resolutions
- Performance test with frequent credit updates