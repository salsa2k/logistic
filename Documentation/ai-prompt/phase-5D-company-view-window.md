# Phase 5D - Company View Window

## Overview
Create the dark themed company information window displaying company name, logo, credits, and owned licenses with professional business presentation.

## Tasks

### Company View Window Layout
- Design company window using BaseWindow component as foundation
- Create professional business information display layout
- Implement dark graphite theme throughout the interface
- Set up responsive design for different screen sizes

### Company Identity Display
- Company name display with professional typography
- Company logo presentation using selected logo from creation
- Company establishment date and other metadata
- Professional business card style presentation

### Financial Information Section
- Current credits display with formatting
- Financial history summary (optional)
- Credit balance trends and statistics
- Integration with credits display widget styling

### Licenses Management Section
- Scrollable list of owned transportation licenses
- License details including acquisition dates
- License status and capabilities information
- Professional licensing document style presentation

### Company Statistics Display
- Total vehicles owned and their locations
- Completed contracts count and success rate
- Cities visited and operational territory
- Business performance metrics

### License Information Cards
- Individual cards for each owned license using BaseCard component
- License type, description, and acquisition date
- Benefits and cargo types enabled by each license
- Professional certification document styling

### Data Integration
- Load company data from save system
- Real-time updates when company data changes
- Integration with vehicle and contract statistics
- Dynamic content updates for license acquisitions

## Acceptance Criteria

### Visual Design
- ✅ Company view uses dark graphite theme consistently
- ✅ Professional business presentation style
- ✅ Clear information hierarchy and organization
- ✅ Company logo and branding displayed prominently

### Information Display
- ✅ All company information is accurate and up-to-date
- ✅ Licenses are displayed clearly with relevant details
- ✅ Financial information matches actual game state
- ✅ Statistics reflect actual player progress

### User Experience
- ✅ Information is easily readable and well-organized
- ✅ License information is comprehensive and helpful
- ✅ Company identity feels personal and significant
- ✅ Professional appearance maintains business theme

### Technical Integration
- ✅ Save system provides accurate company data
- ✅ Real-time updates reflect license acquisitions
- ✅ Statistics calculations are accurate
- ✅ Window performance is smooth and responsive

## Technical Notes

### Component Structure
```
CompanyView/
├── CompanyViewController.cs (window behavior)
├── CompanyViewDocument.uxml (company layout)
├── CompanyViewStyles.uss (company styling)
├── LicenseCard.cs (license display component)
└── CompanyStatistics.cs (metrics calculation)
```

### UXML Structure
- Root window container using BaseWindow
- Company header section with logo and name
- Financial information section with credits display
- License management section with scrollable list
- Statistics section with business metrics

### USS Class Naming
- .company-view (root container)
- .company-header (name and logo area)
- .company-logo (logo display)
- .company-name (company name styling)
- .financial-section (credits and financial info)
- .licenses-section (license management area)
- .license-card (individual license display)
- .statistics-section (business metrics)

### Company Data Display
```
CompanyData:
- Name: Company name from creation
- Logo: Selected logo reference
- EstablishedDate: Game creation date
- Credits: Current financial balance
- OwnedLicenses: List of purchased licenses
- Statistics: Calculated business metrics
```

## Dependencies
- Requires Phase 2B (Base Window Component) for foundation
- Requires Phase 2D (Base Card Component) for license cards
- Requires Phase 1D (Save System) for company data
- Integrates with license purchase system from Phase 5C

## Integration Points
- Save system provides all company data
- License purchase system updates owned licenses
- Vehicle and contract systems provide statistics
- Credits system provides current financial information

## Notes

### Company Information Layout
- **Header**: Company logo (left) and name/establishment date (right)
- **Financial**: Current credits with professional formatting
- **Licenses**: Scrollable list of owned licenses with details
- **Statistics**: Key business metrics and achievements

### License Display Format
- License name and type prominently displayed
- Acquisition date and current status
- Description of benefits and enabled cargo types
- Professional certification document appearance

### Statistics Calculations
- **Total Vehicles**: Count of vehicles in player fleet
- **Completed Contracts**: Successfully finished contract count
- **Cities Visited**: Number of cities marked as visited
- **Success Rate**: Percentage of contracts completed successfully
- **Average Profit**: Mean profit per completed contract

### Professional Styling
- Business card inspired layout for company identity
- Certificate styling for license information
- Financial report styling for credits and statistics
- Consistent dark graphite theme throughout

### Real-time Updates
- License list updates when new licenses purchased
- Statistics recalculate when contracts completed
- Credits display reflects current game state
- All information remains synchronized with game progress

### Accessibility Features
- Screen reader support for all company information
- Keyboard navigation through license list
- High contrast mode compatibility
- Clear focus indicators for interactive elements

### Testing Requirements
- Test company data display accuracy
- Verify license list updates with purchases
- Test statistics calculations with various game states
- Validate responsive layout at different screen sizes
- Performance test with large numbers of licenses