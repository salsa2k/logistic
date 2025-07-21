# Phase 5C - License Purchase

## Overview
Implement the license purchase system for goods transportation permits, enabling players to transport specialized cargo types with credit validation and save system integration.

## Tasks

### License Purchase Workflow
- Implement complete license purchasing process from selection to ownership
- Create license confirmation modal with detailed information
- Set up credit validation and transaction processing
- Integrate with save system for persistent license ownership

### License Types and Requirements
- Shopping Goods license (basic transportation)
- Perishable Goods license (refrigerated transport)
- Fragile Goods license (careful handling required)
- Heavy Goods license (specialized equipment)
- Hazardous Materials license (safety regulations)

### License Information Display
- License name, description, and requirements
- Price in credits and any prerequisite licenses
- Benefits and cargo types enabled by license
- Legal and safety information for immersion

### Credit Transaction System
- Validate player has sufficient credits for license purchase
- Process credit deduction upon license acquisition
- Handle negative credit scenarios if allowed
- Integrate with credits display widget for real-time updates

### License Confirmation Modal
- Display comprehensive license information before purchase
- Show total cost and credit balance after purchase
- Explain which cargo types will become available
- Require explicit confirmation to prevent accidental purchases

### Company Integration
- Add purchased licenses to company profile
- Update company capabilities and available contract types
- Display owned licenses in company view
- Enable contract generation for licensed cargo types

### Save System Integration
- Save new license to player's company data
- Update contract system to reflect new capabilities
- Persist license ownership and acquisition dates
- Handle save failures with appropriate error recovery

## Acceptance Criteria

### Purchase Process
- ✅ License selection displays comprehensive information
- ✅ Credit validation prevents invalid purchases
- ✅ Purchase confirmation modal shows all relevant details
- ✅ Explicit confirmation required for purchase completion

### License Management
- ✅ Purchased licenses appear in company profile immediately
- ✅ Contract system recognizes new license capabilities
- ✅ License benefits are immediately available
- ✅ Save system persists license data correctly

### User Experience
- ✅ License benefits and requirements are clearly explained
- ✅ Purchase process feels informed and intentional
- ✅ Success feedback confirms license acquisition
- ✅ Error handling provides helpful guidance

### System Integration
- ✅ Contract generation uses license data correctly
- ✅ Credits system processes transactions accurately
- ✅ Company view displays owned licenses properly
- ✅ Save/load maintains license ownership state

## Technical Notes

### Component Structure
```
LicensePurchase/
├── LicensePurchaseController.cs (purchase logic)
├── LicenseConfirmationModal.cs (confirmation dialog)
├── LicenseValidation.cs (requirements checking)
├── CompanyLicenseManager.cs (company integration)
└── ContractUnlocker.cs (contract type enabling)
```

### License Data Structure
```
LicenseData:
- Name: Human-readable license name
- Description: Detailed license information
- Price: Cost in credits
- RequiredLicenses: Prerequisites (if any)
- EnabledGoods: List of goods types this license allows
- AcquisitionDate: When player purchased license
```

### License Purchase Workflow
1. Player selects license from shopping interface
2. System displays license details and requirements
3. System validates player credits and prerequisites
4. License confirmation modal shows all information
5. Player confirms purchase with explicit action
6. Credits deducted, license added to company profile
7. Contract system updated with new capabilities
8. Success notification and UI updates

### License Requirements and Benefits
- **Shopping Goods**: Basic license, enables standard cargo contracts
- **Perishable Goods**: Requires refrigerated vehicles, enables food/medical transport
- **Fragile Goods**: Requires careful handling, enables electronics/art transport
- **Heavy Goods**: Requires heavy vehicles, enables industrial equipment transport
- **Hazardous Materials**: Requires specialized vehicles, enables chemical/fuel transport

## Dependencies
- Requires Phase 5A (Shopping Window) for integration
- Requires Phase 1C (Data Structures) for license data
- Requires Phase 1D (Save System) for persistence
- Will integrate with contract generation system

## Integration Points
- Shopping Window triggers license purchase process
- Company system displays owned licenses
- Contract generation system checks license requirements
- Save system stores license data in company profile

## Notes

### License Validation Logic
- Check player's current credit balance against license price
- Verify any prerequisite licenses are already owned
- Prevent duplicate license purchases
- Validate license data integrity before purchase

### Contract System Integration
- Licensed goods types become available for contract generation
- Contracts require appropriate licenses to accept
- Contract rewards may increase for specialized licenses
- License ownership affects contract availability and variety

### Company Profile Updates
- Licenses appear immediately in company view
- License acquisition dates tracked for record keeping
- Company capabilities expand with each license
- Professional licensing theme maintains immersion

### Purchase Benefits
- Immediate access to new contract types
- Expanded earning potential through specialized cargo
- Progression system encourages strategic purchasing
- Business simulation depth through licensing requirements

### Error Handling
- **Insufficient Credits**: Show shortfall and suggest earning methods
- **Missing Prerequisites**: List required licenses and their costs
- **Duplicate Purchase**: Prevent and explain existing ownership
- **Save Failure**: Retry mechanism with transaction rollback

### License Pricing Strategy
- Basic licenses affordable for early game progression
- Specialized licenses require significant investment
- Pricing encourages strategic decision making
- Cost reflects potential earning increases

### Testing Requirements
- Test license purchase workflow for all license types
- Verify prerequisite validation and credit checking
- Test contract system integration with new licenses
- Validate save system persistence and recovery
- Performance test license validation and purchase completion