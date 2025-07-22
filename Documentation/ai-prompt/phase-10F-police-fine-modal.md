# Phase 10F - Police Fine Modal

## Overview
Create the police fine modal interface using dark graphite theme that displays speed violations, fine amounts, and processes fine payments with credit system integration.

## Tasks

### Police Fine Modal Design
- Design fine modal using BaseModal component for consistency
- Display violation information and enforcement details
- Show fine amount calculation and payment options
- Implement dark graphite theme with police/enforcement styling

### Violation Information Display
- Speed limit and actual detected speed clearly shown
- Violation amount (speed excess) prominently displayed
- Location and checkpoint information for context
- Timestamp and vehicle identification details

### Fine Calculation Breakdown
- Base fine amount and calculation method
- Severity multiplier based on speed excess
- Repeat offender penalties if applicable
- Total fine amount with clear cost breakdown

### Payment Processing Interface
- Current player credit balance display
- Fine payment confirmation with balance impact
- Negative credit handling for insufficient funds
- Payment completion confirmation and receipt

### Enforcement Context
- Police checkpoint identification and location
- Speed detection method and measurement details
- Legal information about speed limit enforcement
- Appeal or contest options (if implemented)

### Vehicle Impact Information
- Effect on vehicle status during fine processing
- Temporary vehicle stop simulation for realism
- Resume travel notification after fine payment
- Integration with vehicle movement and status systems

### Save System Integration
- Fine payment records and violation history
- Player behavior tracking for enforcement statistics
- Credit transaction logging for financial records
- Persistent violation data across game sessions

## Acceptance Criteria

### Visual Design
- ✅ Modal uses dark graphite theme with appropriate enforcement styling
- ✅ Professional law enforcement interface appearance
- ✅ Clear information hierarchy and violation details
- ✅ Serious but fair enforcement presentation

### Information Display
- ✅ Violation details are comprehensive and accurate
- ✅ Fine calculation is transparent and understandable
- ✅ Payment options are clear and accessible
- ✅ All information uses consistent metric units

### Payment Processing
- ✅ Credit validation and payment processing work correctly
- ✅ Negative credit scenarios are handled appropriately
- ✅ Payment confirmation provides clear feedback
- ✅ Transaction records are maintained accurately

### System Integration
- ✅ Modal integrates properly with BaseModal component
- ✅ Speed detection data displays correctly
- ✅ Credit system processes fine payments accurately
- ✅ Vehicle systems coordinate with fine processing

## Technical Notes

### Component Structure
```
PoliceFineModal/
├── PoliceFineModalController.cs (modal behavior)
├── PoliceFineModalDocument.uxml (modal layout)
├── PoliceFineModalStyles.uss (modal styling)
├── FineCalculationDisplay.cs (fine breakdown presentation)
└── PaymentProcessor.cs (fine payment handling)
```

### UXML Structure
- Root modal container using BaseModal
- Header section with enforcement branding and title
- Violation details section with speed and location info
- Fine calculation section with breakdown and total
- Payment section with credit balance and confirmation
- Action buttons section with pay/contest options

### USS Class Naming
- .police-fine-modal (root container)
- .enforcement-header (police branding area)
- .violation-details (speed and location information)
- .fine-breakdown (calculation details)
- .payment-section (credit and payment information)
- .fine-actions (confirmation buttons)

### Fine Modal Data Structure
```
PoliceFineModalData:
- VehicleId: Vehicle that received violation
- CheckpointId: Enforcement checkpoint location
- SpeedLimit: Posted speed limit at checkpoint
- DetectedSpeed: Vehicle's measured speed
- ViolationAmount: Speed excess over limit
- BaseFine: Base fine amount before multipliers
- TotalFine: Final fine amount after calculations
- PlayerCredits: Current player credit balance
- ViolationTime: When violation occurred
```

### Fine Display Information
```
FineDisplayData:
- ViolationSummary: "Speeding: XX km/h in YY km/h zone"
- LocationInfo: "Checkpoint: [Location] on [Road]"
- TimeStamp: "Violation detected at [Time]"
- SpeedExcess: "Speed exceeded by XX km/h"
- FineBreakdown: "Base fine + Severity penalty + Repeat offender"
- PaymentDue: "Total fine: C XXX.XX"
```

## Dependencies
- Requires Phase 2C (Base Modal Component) for foundation
- Requires Phase 10E (Speed Detection System) for violation data
- Requires Phase 4C (Credits Display) for payment processing

## Integration Points
- BaseModal provides modal dialog framework
- Speed detection system provides violation data
- Credit system processes fine payments and updates balance
- Vehicle status system coordinates with fine processing

## Notes

### Modal Layout Design
- **Header**: Police/enforcement branding with violation notice
- **Violation**: Speed limit, detected speed, location, time
- **Calculation**: Base fine, multipliers, total amount
- **Payment**: Current credits, payment impact, new balance
- **Actions**: Pay fine button and optional contest option

### Violation Information Format
- Clear speed comparison: "65 km/h in 50 km/h zone"
- Location context: "Highway A-12, Police Checkpoint #5"
- Time reference: "Today at 14:35" or specific timestamp
- Violation severity: "Moderate speeding violation"

### Fine Calculation Display
```
FineBreakdown:
- Base Fine: C 100.00
- Severity Penalty (+25%): C 25.00
- Repeat Offender (+50%): C 62.50
- Total Fine: C 187.50
```

### Payment Confirmation Process
1. Display current credit balance
2. Show fine amount and payment impact
3. Calculate remaining credits after payment
4. Provide payment confirmation button
5. Process payment through credit system
6. Display payment confirmation and receipt
7. Update vehicle status and resume movement

### Credit Balance Handling
- **Sufficient Credits**: Normal payment processing
- **Insufficient Credits**: Allow negative balance with warning
- **Zero Credits**: Emergency payment with debt notification
- **Payment Impact**: Clear display of remaining balance

### Enforcement Realism
- Vehicle stops briefly during fine processing
- Realistic enforcement interaction timing
- Professional law enforcement presentation
- Fair but consequential fine amounts

### Error Handling
- **Payment Failure**: Retry mechanism with clear error messages
- **Invalid Data**: Graceful handling with default values
- **System Errors**: Fallback to basic fine processing
- **Network Issues**: Offline fine processing if applicable

### Accessibility Features
- Screen reader support for all violation and fine information
- Keyboard navigation through modal elements
- High contrast mode for enforcement and payment details
- Clear focus indicators for payment confirmation

### Testing Requirements
- Test modal display with various violation scenarios
- Verify fine calculation accuracy and display
- Test payment processing and credit integration
- Validate modal accessibility and usability
- Test integration with speed detection and vehicle systems