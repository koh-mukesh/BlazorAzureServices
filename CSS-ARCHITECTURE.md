# CSS Architecture Documentation

## Overview
This Blazor Azure Services application uses a centralized CSS architecture where all component styles are consolidated into a single file for better maintainability, performance, and consistency.

## File Structure

### Main CSS Files
- **`wwwroot/app.css`** - Base application styles and Bootstrap overrides
- **`wwwroot/components.css`** - All component-specific styles (newly created)
- **`wwwroot/bootstrap/bootstrap.min.css`** - Bootstrap framework styles

### CSS Loading Order
The CSS files are loaded in the following order in `Components/App.razor`:
1. Bootstrap CSS
2. Base application styles (`app.css`)
3. Component styles (`components.css`)
4. Blazor generated styles (`BlazorAzureServices.styles.css`)

## Component Styles Included

### 1. My Frequent Services Table Component
- **Classes**: `.table-container`, `.services-table`, `.table-*`
- **Features**: Responsive table layout, hover effects, status indicators
- **Modals**: Add resource modal with form validation styles

### 2. Bitbucket Branching Diagram Component  
- **Classes**: `.diagram-container`, `.branch-*`, `.commit`, `.legend-*`
- **Features**: Interactive branching diagram, workflow visualization
- **Responsive**: Mobile-friendly layout adjustments

### 3. Common Component Styles
- **Navigation**: `.back-nav`, `.back-btn`
- **Layout**: `.container`, `header`, `.subtitle`
- **Forms**: `.form-group`, `.form-control`, `.btn-*`

## Benefits of Centralized CSS

### Performance
- **Reduced HTTP Requests**: Single CSS file instead of multiple inline styles
- **Better Caching**: CSS can be cached by browsers across page visits
- **Minification Ready**: Single file easier to optimize for production

### Maintainability  
- **Single Source of Truth**: All styles in one location
- **Easier Updates**: Change styles in one place affects all components
- **Better Organization**: Styles grouped by component with clear sections

### Consistency
- **Shared Variables**: Common colors, spacing, and typography
- **Reusable Classes**: Common patterns like buttons, forms, and layouts
- **Design System**: Consistent visual language across components

## Development Guidelines

### Adding New Styles
1. Add new component styles to `wwwroot/components.css`
2. Use clear section comments to organize styles by component
3. Follow the existing naming conventions (kebab-case, BEM-like structure)

### Naming Conventions
- **Component prefixes**: Use descriptive prefixes like `.table-`, `.branch-`, `.modal-`
- **State classes**: Use classes like `.running`, `.collapsed`, `.show`
- **Responsive classes**: Follow mobile-first approach with media queries

### Responsive Design
- All components include responsive breakpoints at 768px and 480px
- Mobile-first approach with progressive enhancement
- Touch-friendly sizing for mobile devices

## File Organization
```
wwwroot/
├── app.css              (Base styles)
├── components.css       (Component styles) 
├── azure-services.css   (Legacy - can be consolidated)
└── bootstrap/
    └── bootstrap.min.css
```

## Migration Notes
- Removed all inline `<style>` blocks from Razor components
- Preserved all existing styling and functionality
- Added responsive improvements and consistency enhancements
- No breaking changes to component behavior or appearance

## Future Improvements
- Consider CSS custom properties (CSS variables) for theme support
- Implement CSS modules or scoped styles for larger applications
- Add CSS minification and autoprefixer for production builds
- Consider moving to Sass/SCSS for advanced features like mixins and nesting