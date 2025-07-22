# Phase 6C - Contract Cards

## Overview
Create specialized contract cards using BaseCard component with dark graphite theme to display contract information, status, and actions in the contracts list interface.

## Tasks

### Contract Card Design
- Design contract card layout using BaseCard component as foundation
- Create contract-specific information hierarchy and styling
- Implement dark graphite theme with professional appearance
- Set up responsive card sizing for different screen widths

### Contract Information Display
- Contract title and description prominently displayed
- Origin and destination cities with route visualization
- Cargo type, weight, and required license information
- Reward amount with professional currency formatting
- Contract status and progress indicators

### Visual Status Indicators
- Color-coded status indicators for different contract states
- Progress bars for contracts in progress
- Deadline countdown timers for urgent contracts
- License requirement badges and warnings
- Vehicle assignment indicators for accepted contracts

### Action Button Integration
- Context-sensitive action buttons based on contract state
- Accept contract button for available contracts
- Cancel contract option for accepted contracts
- Complete contract button for ready-to-complete contracts
- View details button for comprehensive contract information

### Card State Management
- Visual differentiation between contract states
- Hover effects and interaction feedback
- Selection state for multi-contract operations
- Disabled state for contracts that cannot be accepted

### Contract Details Integration
- Integration with contract data from generation system
- Real-time updates when contract status changes
- Vehicle assignment display for accepted contracts
- Progress tracking for contracts in transit

### Responsive Design
- Card layout adapts to different container widths
- Information prioritization for narrow displays
- Proper spacing and alignment across screen sizes
- Touch-friendly sizing for potential mobile support

## Acceptance Criteria

### Visual Design
- ✅ Contract cards use dark graphite theme consistently
- ✅ Professional appearance with clear information hierarchy
- ✅ Status indicators are clear and intuitive
- ✅ Responsive design works at all supported screen sizes

### Information Display
- ✅ All contract information is clearly visible and accurate
- ✅ Status indicators correctly reflect contract state
- ✅ Currency and measurement formatting follows user preferences
- ✅ License requirements are prominently displayed

### User Interaction
- ✅ Action buttons are context-appropriate and functional
- ✅ Hover effects provide clear interaction feedback
- ✅ Card selection and focus states work properly
- ✅ Touch interactions work on supported devices

### Technical Integration
- ✅ Cards integrate properly with BaseCard component
- ✅ Real-time updates reflect contract state changes
- ✅ Performance is optimized for large contract lists
- ✅ Event handling works correctly for all interactions

## Technical Notes

### Component Structure
```
ContractCards/
├── ContractCard.cs (contract-specific card behavior)
├── ContractCardDocument.uxml (contract card layout)
├── ContractCardStyles.uss (contract card styling)
├── ContractStatusIndicator.cs (status display component)
└── ContractActions.cs (action button management)
```

### UXML Structure
- Root card container extending BaseCard
- Header section with title and status indicator
- Route section with origin/destination information
- Cargo section with goods type and weight
- Reward section with payment information
- Actions section with context-sensitive buttons

### USS Class Naming
- .contract-card (root container extending base-card)
- .contract-header (title and status area)
- .contract-status (status indicator)
- .contract-route (origin/destination display)
- .contract-cargo (goods information)
- .contract-reward (payment display)
- .contract-actions (button area)
- .contract-available (available state styling)
- .contract-accepted (accepted state styling)
- .contract-ready (ready-to-complete state styling)

### Contract Card States
```
ContractStates:
- Available: Green accent, "Accept" button
- Accepted: Yellow accent, vehicle info, "Cancel" option
- InProgress: Blue accent, progress indicator
- ReadyToComplete: Green flash, "Complete" button
- Completed: Gray accent, completion date
- Expired: Red accent, no actions available
```

### Status Indicator System
- Color-coded background or border for quick recognition
- Icon indicators for contract type and requirements
- Progress bars for contracts with vehicle assignments
- Countdown timers for urgent or deadline-sensitive contracts
- License requirement badges with clear icons

## Dependencies
- Requires Phase 2D (Base Card Component) for foundation
- Requires Phase 6A (Contract Generation) for contract data
- Integrates with contract state management systems

## Integration Points
- BaseCard provides core card functionality and styling
- Contract generation system provides contract data
- Vehicle management system provides assignment information
- License system determines contract eligibility

## Notes

### Card Layout Hierarchy
1. **Contract Title**: Large, prominent text at top
2. **Status Indicator**: Color-coded status with icon
3. **Route Information**: Origin → Destination with distance
4. **Cargo Details**: Goods type, weight, license requirement
5. **Reward**: Payment amount in prominent display
6. **Actions**: Context-appropriate buttons at bottom

### Action Button Logic
- **Available Contracts**:
  - "Accept" button (primary action)
  - "View Details" button (secondary action)
  - Disabled if no suitable vehicle or license
- **Accepted Contracts**:
  - "View Progress" button (primary action)
  - "Cancel Contract" button (destructive action)
- **Ready to Complete**:
  - "Complete Contract" button (primary action, highlighted)

### Visual Feedback System
- Hover: Subtle elevation and highlight
- Selected: Border highlight and slightly raised appearance
- Disabled: Reduced opacity and disabled button states
- Loading: Spinner overlay during contract operations

### Performance Considerations
- Efficient card rendering for large lists
- Lazy loading of detailed contract information
- Optimized re-rendering when contract states change
- Memory management for card creation and destruction

### Currency and Unit Display
- Use player's preferred unit system for weight and distance
- Format currency with appropriate thousands separators
- Show license requirements with clear icons and tooltips
- Display deadlines in user-friendly relative time format

### Accessibility Features
- Screen reader support for all contract information
- Keyboard navigation between card elements
- High contrast mode support for status indicators
- Focus indicators that work with card selection

### Testing Requirements
- Test card display with various contract types and states
- Verify action buttons work correctly for all contract states
- Test responsive behavior at different screen sizes
- Validate performance with large numbers of cards
- Test accessibility features and keyboard navigation