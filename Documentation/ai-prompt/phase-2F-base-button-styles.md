# Phase 2F - Base Button Styles

## Overview
Create comprehensive button styling system using the dark graphite theme to ensure consistent button appearance and behavior across all game interfaces.

## Tasks

### Button Style Categories
- Primary buttons for main actions (Save, Confirm, Start)
- Secondary buttons for alternative actions (Cancel, Back)
- Icon buttons for compact interfaces and toolbars
- Text buttons for subtle actions and links
- Toggle buttons for on/off states
- Destructive buttons for delete/remove actions

### Button States and Variations
- Default state with dark graphite styling
- Hover state with subtle highlighting
- Pressed/active state with visual feedback
- Disabled state with reduced opacity
- Loading state with spinner animation
- Selected state for toggle buttons

### Button Sizes
- Large buttons for primary actions (48px height)
- Medium buttons for standard actions (36px height)
- Small buttons for compact interfaces (28px height)
- Icon-only buttons with proper touch targets
- Full-width buttons for forms and dialogs

### Icon Integration
- Icon + text button layouts
- Icon-only buttons with tooltips
- Icon positioning (left, right, center)
- Icon scaling for different button sizes
- Asset Manager integration for icon loading

### Button Content Styling
- Typography hierarchy for button text
- Icon and text spacing standards
- Color schemes for different button types
- Loading spinner styling and animation
- Badge/notification indicator styling

### Interactive Behaviors
- Smooth hover and press animations
- Click feedback with visual and audio cues
- Keyboard focus indicators
- Touch interaction support
- Accessibility features for screen readers

## Acceptance Criteria

### Visual Consistency
- ✅ All button types use dark graphite theme
- ✅ Consistent spacing, typography, and colors
- ✅ Clear visual hierarchy between button types
- ✅ Professional appearance matching game design

### Interactive States
- ✅ Hover effects provide clear feedback
- ✅ Pressed states give immediate response
- ✅ Disabled states are clearly distinguishable
- ✅ Loading states communicate ongoing operations

### Accessibility
- ✅ Keyboard navigation works for all buttons
- ✅ Screen reader support with proper labels
- ✅ Focus indicators are clearly visible
- ✅ Touch targets meet accessibility guidelines

### Performance
- ✅ Smooth animations without performance impact
- ✅ Efficient CSS rendering for many buttons
- ✅ Fast hover response on all platforms
- ✅ Memory efficient icon and style management

## Technical Notes

### Style Sheet Organization
```
ButtonStyles/
├── base-buttons.uss (core button styles)
├── button-variants.uss (primary, secondary, etc.)
├── button-states.uss (hover, active, disabled)
├── button-sizes.uss (large, medium, small)
└── button-animations.uss (transitions and effects)
```

### USS Class Naming Convention
- .btn (base button class)
- .btn-primary (primary action button)
- .btn-secondary (secondary action button)
- .btn-icon (icon-only button)
- .btn-text (text-only button)
- .btn-destructive (delete/remove button)
- .btn-large, .btn-medium, .btn-small (sizes)

### Color Scheme
- **Primary**: Bright accent color for main actions
- **Secondary**: Subtle graphite with borders
- **Destructive**: Red background for dangerous actions
- **Icon**: Transparent background with icon styling
- **Text**: No background, styled text only

### Animation Specifications
- Hover transitions: 0.2s ease
- Press feedback: 0.1s ease-out
- Loading spinner: 1s linear infinite
- Focus outline: instant with smooth fade-out

## Dependencies
- Requires Phase 2A (Dark Graphite Theme) for color variables
- Will be used by all UI components requiring buttons

## Integration Points
- Asset Manager provides button icons
- Theme system supplies all color variables
- All UI components will use these button styles
- Event system handles button click interactions

## Notes

### Button Usage Guidelines
- **Primary**: One per dialog/window for main action
- **Secondary**: Supporting actions like Cancel/Back
- **Icon**: Toolbars and compact interfaces
- **Text**: Subtle actions and navigation links
- **Destructive**: Clearly mark dangerous operations

### Responsive Design
- Buttons scale appropriately for screen size
- Touch targets remain accessible on mobile
- Text remains readable at minimum resolution
- Icons scale proportionally with button size

### Loading State Behavior
- Disable button during loading
- Show spinner with loading text
- Prevent multiple clicks during operation
- Restore normal state when complete

### Focus Management
- Clear focus indicators for keyboard navigation
- Logical tab order through button groups
- Focus trapping in modal dialogs
- Consistent focus styles across all buttons

### Icon Standards
- 16px icons for small buttons
- 20px icons for medium buttons
- 24px icons for large buttons
- SVG format for scalability
- Consistent visual weight across icon set

### Testing Requirements
- Test all button states and transitions
- Verify accessibility with keyboard navigation
- Test touch interactions on mobile devices
- Validate color contrast for accessibility
- Performance test with many buttons on screen