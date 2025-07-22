# Phase 2A - Dark Graphite Theme System

## Overview
Create a professional dark graphite theme system using UI Toolkit (USS) that provides consistent styling across all game interfaces.

## Tasks

### Core Theme Definitions
- Define dark graphite color palette with primary, secondary, and accent colors
- Create CSS custom properties (USS variables) for consistent color usage
- Establish typography hierarchy with font sizes, weights, and spacing
- Define spacing and layout constants for consistent UI spacing

### Base Style Sheets
- Create master theme USS file with all color and spacing variables
- Implement base styles for common UI elements (buttons, labels, containers)
- Define hover, focus, and active states for interactive elements
- Create responsive layout utilities for different screen sizes

### Color Palette Specification
- **Primary Dark**: Deep graphite (#2C2C2C)
- **Secondary Dark**: Medium graphite (#3C3C3C)
- **Accent Light**: Light graphite (#5C5C5C)
- **Text Primary**: High contrast white (#FFFFFF)
- **Text Secondary**: Medium contrast gray (#CCCCCC)
- **Accent Yellow**: Credits/money color (#FFD700)
- **Success Green**: Positive actions (#4CAF50)
- **Warning Orange**: Warnings and alerts (#FF9800)
- **Error Red**: Errors and negative actions (#F44336)

### Typography System
- Define primary font family for UI text
- Set up heading hierarchy (H1, H2, H3, etc.)
- Create body text styles for different contexts
- Implement button text and label styles

### Interactive Element Styles
- Button styles with hover, focus, and pressed states
- Input field and dropdown styling
- Toggle and checkbox custom styling
- Progress bar and slider styling

### Layout and Spacing
- Define consistent spacing units (4px, 8px, 16px, 24px, 32px)
- Create margin and padding utility classes
- Implement responsive breakpoints for different screen sizes
- Set up grid and flexbox layout helpers

## Acceptance Criteria

### Theme Consistency
- ✅ All UI elements use defined color palette
- ✅ Typography hierarchy applied consistently across interfaces
- ✅ Spacing follows defined scale throughout the game
- ✅ Interactive states (hover, focus, active) work properly

### Visual Quality
- ✅ Dark theme provides good contrast and readability
- ✅ Color choices create professional, modern appearance
- ✅ UI elements have appropriate visual hierarchy
- ✅ Theme works well at minimum resolution (1280x720)

### Technical Implementation
- ✅ USS variables allow easy theme modifications
- ✅ Styles are organized and well-documented
- ✅ Theme system supports UI component reusability
- ✅ Performance is optimized for UI rendering

### Accessibility
- ✅ Text contrast meets accessibility standards
- ✅ Interactive elements are clearly distinguishable
- ✅ Focus indicators are visible and consistent
- ✅ Color coding includes non-color indicators where needed

## Technical Notes

### USS File Organization
```
Assets/UI/StyleSheets/
├── Themes/
│   ├── dark-graphite-theme.uss (main theme file)
│   ├── colors.uss (color variables)
│   ├── typography.uss (font and text styles)
│   ├── spacing.uss (layout and spacing)
│   └── components.uss (component-specific styles)
```

### CSS Custom Properties Usage
- Use --color-primary-dark for main background colors
- Use --color-text-primary for main text
- Use --spacing-unit-x for consistent spacing (x = 1,2,3,4,6,8)
- Use --font-size-x for typography scale

### Performance Considerations
- Minimize CSS specificity conflicts
- Use efficient selectors for UI Toolkit
- Optimize for Unity's UI rendering pipeline
- Consider memory usage for large UI hierarchies

### Responsive Design
- Define breakpoints for different screen sizes
- Create responsive spacing and font size scales
- Ensure UI scales properly from 1280x720 upward
- Test on different aspect ratios

## Dependencies
- Requires Phase 1A (Basic Unity Setup) for UI folder structure
- Will be used by all UI component phases

## Integration Points
- All UI components will reference this theme system
- Asset Manager will provide access to theme stylesheets
- Settings system may allow theme switching in future
- All UI Toolkit documents will import these stylesheets

## Notes

### Design Philosophy
- Professional and modern appearance
- High contrast for readability
- Consistent with logistics/business theme
- Suitable for extended gameplay sessions

### Future Extensibility
- Theme system should support multiple themes
- Variables allow easy color scheme changes
- Component styles can be extended or overridden
- Consider day/night theme variants for future

### Testing Requirements
- Test theme at minimum resolution (1280x720)
- Verify readability in different lighting conditions
- Test interactive states on all platforms
- Validate accessibility compliance

### Documentation Requirements
- Document color palette with usage guidelines
- Create style guide for UI component creation
- Provide examples of proper theme usage
- Document any platform-specific considerations