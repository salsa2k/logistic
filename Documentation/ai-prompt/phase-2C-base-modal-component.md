# Phase 2C - Base Modal Component

## Overview
Create a reusable modal dialog component using the dark graphite theme for all popup dialogs and confirmations throughout the game.

## Tasks

### Modal Component Architecture
- Design BaseModal class extending BaseWindow for modal-specific behavior
- Create modal UXML template with backdrop, dialog, and content areas
- Implement modal USS styling with dark theme and overlay effects
- Set up modal controller for dialog interactions and lifecycle

### Modal Structure Elements
- Full-screen backdrop with semi-transparent overlay
- Centered dialog container with dark graphite styling
- Modal header with title and close button
- Content area for dynamic modal content
- Footer area with action buttons (OK, Cancel, etc.)

### Modal Behaviors
- Show/hide with backdrop fade and dialog scale animations
- Click-outside-to-close functionality (configurable)
- Escape key to close modal
- Focus trapping within modal dialog
- Modal stacking for nested dialogs

### Modal Types Support
- Confirmation dialogs (Yes/No, OK/Cancel)
- Information displays (alerts, notifications)
- Input forms (text input, selections)
- Settings and configuration dialogs
- Complex interactive modals

### Integration with Events
- Modal result system for user choices
- Event-driven modal opening and closing
- Callback system for modal actions
- Integration with game systems for modal triggers

### Accessibility Features
- Screen reader support for modal content
- Keyboard navigation within modals
- Focus management when opening/closing
- High contrast mode compatibility

## Acceptance Criteria

### Visual Design
- ✅ Modal uses dark graphite theme consistently
- ✅ Backdrop provides clear visual separation
- ✅ Dialog stands out clearly from backdrop
- ✅ Professional appearance matching game theme

### Functionality
- ✅ Modal opens and closes smoothly with animations
- ✅ Click-outside-to-close works when enabled
- ✅ Escape key closes modal appropriately
- ✅ Action buttons trigger correct events

### User Experience
- ✅ Focus is trapped within modal when open
- ✅ Focus returns to trigger element when closed
- ✅ Modal content is clearly readable
- ✅ Interactions feel responsive and immediate

### Technical Requirements
- ✅ Modal system supports multiple simultaneous modals
- ✅ Memory usage is optimized for frequent use
- ✅ Event system integration works correctly
- ✅ Modal templates are easily customizable

## Technical Notes

### Component Structure
```
BaseModal/
├── BaseModal.cs (modal controller extending BaseWindow)
├── BaseModalDocument.uxml (modal structure)
├── BaseModalStyles.uss (modal-specific styles)
└── ModalManager.cs (modal system coordinator)
```

### UXML Structure
- Root backdrop container (full screen)
- Modal dialog container (centered)
- Header with title and close button
- Scrollable content area
- Footer with action buttons

### USS Class Naming
- .modal-backdrop (full-screen overlay)
- .modal-dialog (centered dialog container)
- .modal-header (title area)
- .modal-content (main content area)
- .modal-footer (action button area)
- .modal-button-primary (primary action button)
- .modal-button-secondary (secondary action button)

### Animation System
- Backdrop fade in/out (0.2s ease)
- Dialog scale and fade animation (0.3s ease-out)
- Button hover and press animations
- Smooth transitions between modal states

## Dependencies
- Requires Phase 2B (Base Window Component) for foundation
- Requires Phase 2A (Dark Graphite Theme) for styling
- Will be used by all dialog-based UI phases

## Integration Points
- Extends BaseWindow for core window functionality
- ModalManager coordinates multiple modals
- Event system handles modal triggers and results
- All game systems can show modals through this component

## Notes

### Modal Design Patterns
- **Confirmation**: Simple yes/no or OK/cancel dialogs
- **Alert**: Information display with single dismiss button
- **Form**: Input collection with submit/cancel actions
- **Custom**: Complex interactions with custom content

### Performance Considerations
- Use object pooling for frequently shown modals
- Optimize backdrop rendering for performance
- Efficient event handling and cleanup
- Minimal memory allocation during modal operations

### Backdrop Styling
- Semi-transparent dark overlay (rgba(0,0,0,0.6))
- Prevents interaction with background UI
- Smooth fade animation for professional feel
- Optional blur effect for enhanced focus

### Button Layout Standards
- Primary action on right (OK, Save, Confirm)
- Secondary action on left (Cancel, Back)
- Destructive actions clearly marked (red color)
- Consistent spacing and sizing across all modals

### Testing Requirements
- Test modal stacking and z-order management
- Verify focus management and keyboard navigation
- Test animations on different performance levels
- Validate accessibility with screen readers
- Test modal behavior with game controller input