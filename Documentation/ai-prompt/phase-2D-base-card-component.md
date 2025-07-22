# Phase 2D - Base Card Component

## Overview
Create a reusable card UI component using the dark graphite theme for displaying information items like contracts, vehicles, and other game entities.

## Tasks

### Card Component Architecture
- Design BaseCard class as a flexible UI component
- Create card UXML template with header, content, and action areas
- Implement card USS styling using dark graphite theme
- Set up card controller for interactions and data binding

### Card Structure Elements
- Header area with title, subtitle, and optional icon
- Main content area for item-specific information
- Optional image/preview area for visual content
- Footer area for action buttons or status indicators
- Optional badge/tag area for categorization

### Card Visual Design
- Dark graphite background with subtle borders
- Hover and selection states with visual feedback
- Card elevation and shadow effects
- Responsive sizing for different content types
- Professional spacing and typography

### Card Interaction Features
- Click selection with visual state changes
- Hover effects for better user feedback
- Keyboard navigation support
- Context menu support (right-click)
- Drag and drop support for future features

### Card Content Types
- Contract cards with origin, destination, and reward info
- Vehicle cards with specifications and status
- License cards with description and price
- City cards with information and status
- General information cards for various content

### Data Binding System
- Generic data binding interface for card content
- Template system for different card layouts
- Dynamic content loading and updating
- Event-driven data refresh capabilities

## Acceptance Criteria

### Visual Design
- ✅ Cards use dark graphite theme consistently
- ✅ Professional appearance with clear information hierarchy
- ✅ Hover and selection states provide clear feedback
- ✅ Cards maintain readability at minimum resolution

### Functionality
- ✅ Cards display data correctly and completely
- ✅ Interactive states (hover, selected, disabled) work properly
- ✅ Action buttons trigger appropriate events
- ✅ Data binding updates content dynamically

### Reusability
- ✅ BaseCard can be configured for different content types
- ✅ Card templates support various layouts
- ✅ Style overrides allow customization when needed
- ✅ Data binding works with different data types

### Performance
- ✅ Smooth hover and selection animations
- ✅ Efficient rendering for large lists of cards
- ✅ Memory usage optimized for card recycling
- ✅ Fast data updates without visual glitches

## Technical Notes

### Component Structure
```
BaseCard/
├── BaseCard.cs (card controller)
├── BaseCardDocument.uxml (card structure)
├── BaseCardStyles.uss (card styling)
├── CardDataBinding.cs (data binding system)
└── CardTemplates/ (specific card layouts)
    ├── ContractCard.uxml
    ├── VehicleCard.uxml
    └── LicenseCard.uxml
```

### UXML Structure
- Root card container with interaction handling
- Header section with title, subtitle, and icon
- Content section with flexible layout
- Image section for visual content (optional)
- Footer section with actions and status

### USS Class Naming
- .base-card (root container)
- .card-header (title and icon area)
- .card-content (main information area)
- .card-image (visual content area)
- .card-footer (actions and status)
- .card-selected (selection state)
- .card-hover (hover state)

### Card States
- **Default**: Normal appearance
- **Hover**: Highlighted appearance
- **Selected**: Clearly marked as selected
- **Disabled**: Grayed out, non-interactive
- **Loading**: Shows loading indicator

## Dependencies
- Requires Phase 2A (Dark Graphite Theme) for styling
- Will be used by contract, vehicle, and shopping systems
- May integrate with Phase 2E (Base List Component)

## Integration Points
- Data structures from Phase 1C provide card content
- Event system handles card selection and actions
- List components will use cards for item display
- Asset Manager provides access to card assets

## Notes

### Card Layout Variations
- **Compact**: Single line with essential info
- **Standard**: Multi-line with detailed information
- **Expanded**: Full details with actions
- **Image**: Prominent visual with supporting text

### Interaction Patterns
- Single click for selection
- Double click for primary action
- Right click for context menu
- Keyboard navigation with arrow keys
- Space/Enter for selection/action

### Content Guidelines
- Essential information always visible
- Secondary details in expandable sections
- Actions clearly labeled and positioned
- Status indicators use color and icons
- Text truncation for overflowing content

### Animation System
- Subtle hover animations (scale, border, shadow)
- Smooth selection state transitions
- Loading state animations
- Content update animations

### Accessibility Features
- Screen reader support for all content
- Keyboard navigation between cards
- High contrast mode compatibility
- Focus indicators clearly visible

### Testing Requirements
- Test cards with various content lengths
- Verify interactions on different input methods
- Test performance with large numbers of cards
- Validate accessibility compliance
- Test responsive behavior at different sizes