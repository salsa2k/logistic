# Button Styles System - Implementation Guide

## Overview
This comprehensive button styling system provides consistent, accessible, and visually appealing buttons throughout the logistics game UI. Built on the Dark Graphite Theme foundation, it offers multiple variants, sizes, states, and animations.

## File Structure
```
ButtonStyles/
├── base-buttons.uss        # Core button foundation and utilities
├── button-variants.uss     # Button types (primary, secondary, destructive, etc.)
├── button-states.uss       # Interactive states (hover, active, disabled)
├── button-sizes.uss        # Size variations (small, medium, large, etc.)
├── button-animations.uss   # Transitions and animation effects
└── README.md              # This documentation file
```

## How to Use

### 1. Add Stylesheets to UI Document
In Unity Editor, add these stylesheets to your UI Document component in order:
1. `DarkGraphiteTheme.uss` (required for variables)
2. `ButtonStyles/base-buttons.uss`
3. `ButtonStyles/button-variants.uss` 
4. `ButtonStyles/button-states.uss`
5. `ButtonStyles/button-sizes.uss`
6. `ButtonStyles/button-animations.uss`

### 2. Basic Button Usage
```xml
<!-- Primary button (default medium size) -->
<engine:Button class="btn btn-primary" text="Save" />

<!-- Secondary button with custom size -->
<engine:Button class="btn btn-secondary btn-large" text="Cancel" />

<!-- Icon-only button -->
<engine:Button class="btn btn-icon-only" text="⚙" />
```

## Button Classes Reference

### Base Classes
- `.btn` - Required base class for all buttons
- `.btn-full-width` - Makes button stretch to container width
- `.btn-square` - Square aspect ratio button
- `.btn-rounded` - Fully rounded button corners

### Variants
- `.btn-primary` - Main action button (accent color)
- `.btn-secondary` - Supporting action button  
- `.btn-destructive` - Dangerous actions (red)
- `.btn-outlined` - Transparent with border
- `.btn-text` - Text-only button
- `.btn-ghost` - Minimal styling
- `.btn-icon-only` - Icon without text
- `.btn-toggle` - On/off state button
- `.btn-success` - Success/confirmation (green)
- `.btn-warning` - Warning/caution (orange)
- `.btn-toolbar` - Compact toolbar button
- `.btn-fab` - Floating action button

### Sizes
- `.btn-xs` - 20px height (extra small)
- `.btn-small` - 28px height
- `.btn-medium` - 36px height (default)
- `.btn-large` - 48px height  
- `.btn-xl` - 56px height (extra large)
- `.btn-responsive` - Adapts to screen size
- `.btn-compact` - Reduced padding
- `.btn-wide` - Increased minimum width

### Special States
- `.btn-loading` - Shows loading state with spinner
- `.btn-toggle-selected` - Selected toggle button
- `.btn-disabled` - Use `enabled="false"` in UXML

### Animations (Optional)
- `.btn-pulse` - Pulsing effect for attention
- `.btn-bounce` - Bounce effect for success
- `.btn-shake` - Shake effect for errors
- `.btn-glow` - Glowing effect for highlights
- `.btn-lift` - Lift effect on hover
- `.btn-ripple` - Click ripple effect

## Usage Guidelines

### Button Hierarchy
1. **Primary Button**: One per dialog/window for main action
2. **Secondary Buttons**: Supporting actions (Cancel, Back)
3. **Text Buttons**: Subtle actions and links
4. **Icon Buttons**: Space-constrained interfaces

### Size Guidelines
- **Large**: Important CTAs and primary interface actions
- **Medium**: Standard buttons in forms and dialogs
- **Small**: Secondary actions and compact interfaces
- **Extra Small**: Minimal space situations

### Accessibility Features
- All buttons include focus indicators
- Proper contrast ratios maintained
- Touch targets meet minimum size requirements
- Screen reader compatible markup
- Reduced motion support included

## Example Implementations

### Dialog Footer Buttons
```xml
<engine:VisualElement class="btn-group">
    <engine:Button class="btn btn-secondary" text="Cancel" />
    <engine:Button class="btn btn-primary" text="Save Changes" />
</engine:VisualElement>
```

### Toolbar Actions
```xml
<engine:VisualElement class="btn-group">
    <engine:Button class="btn btn-toolbar" text="⚙" />
    <engine:Button class="btn btn-toolbar" text="📁" />
    <engine:Button class="btn btn-toolbar" text="💾" />
</engine:VisualElement>
```

### Form Controls
```xml
<engine:Button class="btn btn-primary btn-large btn-full-width" text="Start Game" />
<engine:Button class="btn btn-destructive btn-small" text="Delete Save" />
```

## Testing
Use `ButtonShowcaseDocument.uxml` in the Examples folder to test all button styles and ensure proper rendering.

## Unity UI Toolkit Limitations

### CSS Features Not Supported
- `::before` and `::after` pseudo-elements
- `@media` queries (use conditional C# classes instead)
- `transform` properties (limited support)
- `box-shadow` properties (limited support)
- Complex animations (require C# implementation)

### Workarounds Implemented
- **Responsive Design**: Use `.btn-responsive-mobile` class conditionally via C#
- **Animations**: Basic color transitions only, complex effects need C# scripting
- **Media Queries**: Use conditional class application in C# scripts
- **Accessibility**: Use `.btn-no-animation` class for reduced motion preferences

### C# Integration Required For
- Loading spinner animations
- Complex button state transitions
- Responsive behavior based on screen size
- Advanced accessibility features
- Ripple effects and complex interactions

## Notes
- All colors reference CSS variables from DarkGraphiteTheme.uss
- Icon integration assumes Asset Manager provides button icons
- Basic hover/active states work with Unity's built-in pseudo-class support
- Complex animations should be implemented via Unity's UIElements animation system