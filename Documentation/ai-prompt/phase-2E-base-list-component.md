# Phase 2E - Base List Component

## Overview
Create a reusable list UI component using the dark graphite theme for displaying collections of cards and items throughout the game interfaces.

## Tasks

### List Component Architecture
- Design BaseList class for managing collections of UI elements
- Create list UXML template with header, content area, and controls
- Implement list USS styling using dark graphite theme
- Set up list controller for data management and user interactions

### List Structure Elements
- Optional header with title and action buttons
- Scrollable content area for list items
- Search/filter input field (optional)
- Pagination or infinite scroll controls
- Empty state display for no items
- Loading state display during data fetching

### List Features
- Virtualized rendering for large datasets
- Item selection (single and multi-select)
- Sorting by different criteria
- Filtering and search functionality
- Drag and drop reordering (where applicable)
- Context menus for list items

### Integration with Cards
- Seamless integration with BaseCard components
- Support for different card layouts within same list
- Card recycling for performance optimization
- Dynamic card content updates

### List Data Management
- Generic data binding for different item types
- Observable collection support for automatic updates
- Lazy loading for large datasets
- Cache management for performance

### List Interaction Patterns
- Keyboard navigation (arrow keys, page up/down)
- Mouse interactions (click, double-click, scroll)
- Touch support for mobile/tablet interfaces
- Accessibility features for screen readers

## Acceptance Criteria

### Visual Design
- ✅ List uses dark graphite theme consistently
- ✅ Clear visual hierarchy and spacing
- ✅ Smooth scrolling and item transitions
- ✅ Professional appearance matching game theme

### Functionality
- ✅ List displays items correctly and efficiently
- ✅ Selection, sorting, and filtering work properly
- ✅ Search functionality provides relevant results
- ✅ Empty and loading states display appropriately

### Performance
- ✅ Smooth scrolling with large numbers of items
- ✅ Efficient memory usage through virtualization
- ✅ Fast item updates and data refreshes
- ✅ Responsive interactions without lag

### Accessibility
- ✅ Keyboard navigation works throughout list
- ✅ Screen reader support for all list content
- ✅ High contrast mode compatibility
- ✅ Focus management and visual indicators

## Technical Notes

### Component Structure
```
BaseList/
├── BaseList.cs (list controller)
├── BaseListDocument.uxml (list structure)
├── BaseListStyles.uss (list styling)
├── ListVirtualization.cs (performance optimization)
└── ListFiltering.cs (search and filter system)
```

### UXML Structure
- Root list container with styling
- Header section with title and controls
- Search/filter input area
- Scrollable content area with item container
- Pagination or scroll controls
- Empty and loading state containers

### USS Class Naming
- .base-list (root container)
- .list-header (title and controls)
- .list-search (search input area)
- .list-content (scrollable item area)
- .list-item (individual list items)
- .list-empty (empty state display)
- .list-loading (loading state display)

### Virtualization System
- Only render visible items for performance
- Recycle item containers for memory efficiency
- Calculate item positions dynamically
- Support variable item heights

## Dependencies
- Requires Phase 2A (Dark Graphite Theme) for styling
- Integrates with Phase 2D (Base Card Component) for items
- Will be used by contracts, vehicles, and shopping interfaces

## Integration Points
- BaseCard components serve as list item templates
- Data structures from Phase 1C provide list content
- Event system handles list interactions and selections
- Search system integrates with game data filtering

## Notes

### List Types in Game
- **Contracts List**: Available and active contracts
- **Vehicles List**: Owned vehicles and their status
- **Shopping List**: Available items for purchase
- **Cities List**: Available destinations and status
- **Licenses List**: Available and owned licenses

### Performance Optimizations
- Virtual scrolling for lists with 100+ items
- Item pooling to reduce memory allocation
- Incremental search with debounced input
- Lazy loading of item details

### Search and Filter Features
- Real-time search with highlighting
- Multiple filter criteria support
- Saved filter presets for common searches
- Clear search/filter actions

### Sorting Options
- Multiple sort criteria (name, date, price, status)
- Ascending and descending order
- Custom sort functions for complex data
- Visual indicators for current sort state

### Empty State Design
- Informative message explaining why list is empty
- Helpful actions to populate the list
- Consistent with dark graphite theme
- Optional illustration or icon

### Loading State Design
- Skeleton loading animation for list items
- Progress indicators for long operations
- Graceful error handling and retry options
- User feedback during data operations

### Testing Requirements
- Test with various list sizes (empty, small, large)
- Verify performance with 1000+ items
- Test search and filter functionality
- Validate accessibility with keyboard navigation
- Test responsive behavior at different screen sizes