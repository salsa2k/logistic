# Phase 2G - Notifications UI Component

## Overview
Create a dark themed snackbar notification system for displaying temporary messages, alerts, and feedback to players throughout the game.

## Tasks

### Notification Component Architecture
- Design NotificationManager as singleton for centralized notification handling
- Create notification UXML template with message area and optional actions
- Implement notification USS styling using dark graphite theme
- Set up notification controller for queuing and display management

### Notification Types
- Success notifications (contract completed, purchase successful)
- Warning notifications (low fuel, approaching deadline)
- Error notifications (insufficient credits, invalid action)
- Information notifications (vehicle arrived, general updates)
- Progress notifications (vehicle en route, loading operations)

### Notification Features
- Auto-dismiss timer with configurable duration
- Manual dismiss with close button
- Action buttons for interactive notifications
- Notification queuing for multiple messages
- Priority system for important notifications
- Sound effects integration for different notification types

### Visual Design
- Dark graphite background with appropriate accent colors
- Icon support for different notification types
- Smooth slide-in/out animations from bottom of screen
- Responsive text layout for different message lengths
- Professional appearance consistent with game theme

### Notification Positioning
- Bottom-center positioning as specified in requirements
- Proper z-index to appear above other UI elements
- Non-blocking positioning that doesn't interfere with gameplay
- Stack management for multiple simultaneous notifications

### Integration with Game Events
- Contract completion notifications
- Vehicle status change alerts
- Financial transaction confirmations
- Police fine notifications
- Emergency refuel alerts
- System message display

## Acceptance Criteria

### Visual Design
- ✅ Notifications use dark graphite theme consistently
- ✅ Different types clearly distinguishable by color/icon
- ✅ Professional appearance matching game design
- ✅ Readable text with appropriate contrast

### Functionality
- ✅ Notifications appear and dismiss smoothly
- ✅ Auto-dismiss timing works correctly
- ✅ Multiple notifications queue properly
- ✅ Action buttons trigger appropriate events

### User Experience
- ✅ Notifications don't block important UI elements
- ✅ Messages are clear and actionable
- ✅ Timing feels natural (not too fast/slow)
- ✅ Easy to dismiss when needed

### Technical Performance
- ✅ Smooth animations without frame drops
- ✅ Efficient memory usage for notification queue
- ✅ Fast message display without delays
- ✅ Proper cleanup when notifications expire

## Technical Notes

### Component Structure
```
Notifications/
├── NotificationManager.cs (singleton manager)
├── Notification.cs (individual notification)
├── NotificationDocument.uxml (notification structure)
├── NotificationStyles.uss (notification styling)
└── NotificationAnimations.cs (animation system)
```

### UXML Structure
- Root notification container with positioning
- Icon area for notification type indicators
- Message text area with overflow handling
- Action button area (optional)
- Close button for manual dismissal

### USS Class Naming
- .notification (root container)
- .notification-success (success styling)
- .notification-warning (warning styling)
- .notification-error (error styling)
- .notification-info (information styling)
- .notification-icon (icon area)
- .notification-message (text content)
- .notification-actions (button area)

### Color Coding
- **Success**: Green accent (#4CAF50)
- **Warning**: Orange accent (#FF9800)
- **Error**: Red accent (#F44336)
- **Info**: Blue accent (#2196F3)
- **Progress**: Yellow accent (#FFD700)

## Dependencies
- Requires Phase 2A (Dark Graphite Theme) for styling
- May integrate with Phase 2F (Base Button Styles) for action buttons
- Will be triggered by all major game systems

## Integration Points
- Game systems trigger notifications through NotificationManager
- Sound system plays audio for different notification types
- Event system handles notification actions and dismissals
- Asset Manager provides notification icons

## Notes

### Notification Content Guidelines
- Keep messages concise and actionable
- Use consistent terminology across game
- Include relevant context (amounts, vehicle names, etc.)
- Provide clear next steps when appropriate

### Animation Specifications
- Slide in from bottom: 0.3s ease-out
- Auto-dismiss fade: 0.5s ease-in
- Stacking offset: 60px vertical spacing
- Hover pause: pause auto-dismiss on mouse over

### Queue Management
- Maximum 3 visible notifications at once
- FIFO queue for overflow notifications
- Priority system for urgent messages
- Duplicate message prevention

### Sound Integration
- Success: Pleasant chime sound
- Warning: Attention beep sound
- Error: Alert warning sound
- Info: Subtle notification sound
- Configurable volume through settings

### Accessibility Features
- Screen reader announcements for notifications
- High contrast mode support
- Keyboard dismissal with Escape key
- Configurable auto-dismiss timing

### Testing Requirements
- Test notification stacking and queuing
- Verify auto-dismiss timing accuracy
- Test with very long message content
- Validate accessibility compliance
- Performance test with rapid notification generation