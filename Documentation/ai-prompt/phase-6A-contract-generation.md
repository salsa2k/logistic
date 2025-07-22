# Phase 6A - Contract Generation

## Overview
Implement dynamic contract generation system that creates realistic transportation contracts based on player progress, city relationships, available goods, and business logic rules.

## Tasks

### Contract Generation Engine
- Design ContractGenerator for creating randomized but logical contracts
- Implement contract generation rules based on game requirements
- Create contract validation system for generated contracts
- Set up contract refresh and availability management

### Contract Generation Rules
- Origin city must be different from destination city
- Origin city must be a city the player has visited before
- Contracts generated based on goods available in origin city
- Contract rewards scale with distance, cargo weight, and difficulty
- Player must have appropriate licenses for specialized goods

### Contract Data Creation
- Generate contract titles and descriptions
- Calculate appropriate cargo sizes based on available vehicles
- Determine realistic delivery rewards based on distance and cargo
- Set contract difficulty and special requirements
- Create contract expiration and urgency systems

### Goods and City Integration
- Use predefined goods types (Shopping, Perishable, Fragile, Heavy, Hazardous)
- Integrate with city data for available goods and trade relationships
- Consider city specializations and economic relationships
- Generate contracts that make logical sense for city pairs

### Contract Availability Management
- Generate initial set of 8 contracts at game start
- Refresh contracts periodically with new opportunities
- Remove expired or invalid contracts
- Maintain appropriate contract difficulty progression

### License Requirement System
- Check player's owned licenses for contract eligibility
- Generate contracts that match player's current capabilities
- Create aspirational contracts for unlicensed goods (visible but locked)
- Scale contract generation with player's license portfolio

### Save System Integration
- Save generated contracts with game state
- Restore contract availability on game load
- Persist contract acceptance and completion states
- Handle contract migration between game versions

## Acceptance Criteria

### Contract Logic
- ✅ Generated contracts follow all business rules correctly
- ✅ Contract rewards are fair and scaled appropriately
- ✅ License requirements are enforced properly
- ✅ City and goods relationships make logical sense

### Variety and Balance
- ✅ Contract generation provides good variety in types and routes
- ✅ Contract difficulty scales appropriately with player progress
- ✅ Rewards encourage player progression and investment
- ✅ Contract availability maintains player engagement

### Technical Implementation
- ✅ Contract generation is efficient and doesn't cause delays
- ✅ Generated contracts integrate properly with save system
- ✅ Contract data validation prevents invalid states
- ✅ Contract refresh maintains game balance

### Game Integration
- ✅ Contracts integrate properly with vehicle and license systems
- ✅ Contract completion affects player progress appropriately
- ✅ Contract availability reflects player's current capabilities
- ✅ Save/load maintains contract state correctly

## Technical Notes

### Component Structure
```
ContractGeneration/
├── ContractGenerator.cs (main generation logic)
├── ContractRules.cs (business rules validation)
├── ContractRewards.cs (reward calculation)
├── ContractValidator.cs (generated contract validation)
└── ContractRefresh.cs (availability management)
```

### Contract Generation Algorithm
1. Select random origin city from visited cities
2. Select random destination city (different from origin)
3. Choose goods type based on origin city's available goods
4. Check player license requirements for selected goods
5. Calculate cargo size based on available vehicles
6. Determine delivery distance and route difficulty
7. Calculate appropriate reward based on multiple factors
8. Generate contract title and description
9. Validate complete contract against all rules
10. Add to available contracts if valid

### Contract Data Structure
```
ContractData:
- ID: Unique contract identifier
- Title: Player-facing contract name
- Description: Detailed contract information
- OriginCity: Starting city for pickup
- DestinationCity: Delivery destination
- GoodsType: Type of cargo to transport
- CargoWeight: Amount of goods in kg
- Reward: Payment in credits for completion
- RequiredLicense: License needed to accept contract
- GeneratedDate: When contract was created
- ExpirationDate: When contract becomes unavailable
- Difficulty: Contract complexity rating
```

### Reward Calculation Formula
```
BaseReward = CargoWeight * GoodsTypeMultiplier
DistanceBonus = Distance * DistanceRate
DifficultyMultiplier = 1.0 + (Difficulty * 0.2)
FinalReward = (BaseReward + DistanceBonus) * DifficultyMultiplier
```

## Dependencies
- Requires Phase 1C (Data Structures) for city and goods data
- Requires Phase 5C (License Purchase) for license system integration
- Requires Phase 1D (Save System) for contract persistence

## Integration Points
- City data provides available goods and visited status
- License system determines contract eligibility
- Vehicle system influences cargo size calculations
- Save system persists generated and active contracts

## Notes

### Contract Generation Frequency
- Initial generation: 8 contracts at game start
- Refresh trigger: When contracts drop below 5 available
- New contract rate: 1-3 new contracts per refresh
- Expiration: Contracts expire after 7-14 game days

### Goods Type Distribution
- **Shopping Goods**: 40% of contracts (most common)
- **Perishable Goods**: 25% (requires refrigerated vehicles)
- **Heavy Goods**: 20% (requires heavy vehicles)
- **Fragile Goods**: 10% (requires careful handling)
- **Hazardous Materials**: 5% (requires special license and vehicles)

### Distance and Reward Scaling
- Short routes (< 50km): Lower rewards, good for beginners
- Medium routes (50-150km): Standard rewards, main gameplay
- Long routes (> 150km): Higher rewards, requires planning
- Cross-region routes: Premium rewards, advanced gameplay

### Contract Difficulty Factors
- **Route complexity**: Number of turns, road conditions
- **Cargo sensitivity**: Fragile, perishable, hazardous requirements
- **Distance**: Longer routes increase difficulty
- **Time pressure**: Urgent deliveries with tight deadlines
- **License requirements**: Specialized cargo increases difficulty

### Special Contract Types
- **Rush Orders**: Higher rewards, tight deadlines
- **Bulk Contracts**: Large cargo loads, multiple deliveries
- **Chain Contracts**: Sequential deliveries, bonus rewards
- **Emergency Contracts**: High urgency, premium rewards

### Testing Requirements
- Test contract generation with various player progress states
- Verify all business rules are enforced correctly
- Test reward calculations for fairness and balance
- Validate save/load integration with generated contracts
- Performance test contract generation and refresh systems