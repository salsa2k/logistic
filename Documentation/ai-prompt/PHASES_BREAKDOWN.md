# Project Phases Breakdown

Based on the ORIGINAL_PLAN.md, the following is a breakdown of the phases and files for the project.
Create a new file for each phase and include the relevant details as shown below.
Each phase will have its own file in the `Documentation/ai-prompt/` directory.
**DO NOT** include any code examples, only the instructions for each phase, like tasks, acceptance criteria, notes and other relevant information.
I would like to confirm each phase of the project before proceeding to the next one.

## Foundation Phases (5 files)

- phase-1A-basic-unity-setup.md - Unity setup, Asset Manager, folder structure
- phase-1B-localization-system.md - English/Portuguese localization setup
- phase-1C-data-structures.md - ScriptableObjects for Cities, Vehicles, Goods
- phase-1D-save-system.md - Full save implementation for all game data
- phase-1E-load-system.md - Full load implementation for all game data

## Dark Graphite Theme System (1 file)

- phase-2A-dark-graphite-theme.md - Professional dark theme with graphite focus

## UI Component Foundation Phases (6 files)

- phase-2B-base-window-component.md - Reusable window UI using dark graphite theme
- phase-2C-base-modal-component.md - Reusable modal component using dark theme
- phase-2D-base-card-component.md - Reusable card component using dark theme
- phase-2E-base-list-component.md - Reusable list component using dark theme
- phase-2F-base-button-styles.md - Button styles using dark graphite theme
- phase-2G-notifications-ui-component.md - Dark themed snackbar notification system

## Main Menu Phases (4 files)

- phase-3A-main-menu-window.md - Dark themed main menu
- phase-3B-settings-modal.md - Dark themed settings modal (integrates with save/load)
- phase-3C-credits-scene.md - Dark themed credits scene
- phase-3D-new-game-modal.md - Dark themed company creation (saves company data)

## Core Game UI Phases (3 files)

- phase-4A-game-scene-layout.md - Dark themed main game scene
- phase-4B-top-menu-bar.md - Dark graphite navigation bar
- phase-4C-top-menu-credits-display-widget.md - Yellow credits on dark background

## Core Business UI Systems (8 files)

### Shopping System (3 files)

- phase-5A-shopping-window.md - Dark themed shopping interface (saves purchases)
- phase-5B-vehicle-purchase.md - Dark themed vehicle purchase (saves vehicle data)
- phase-5C-license-purchase.md - Dark themed license purchase (saves license data)

### Company System (2 files)

- phase-5D-company-view-window.md - Dark themed company info (displays saved data)
- phase-5E-company-data-management.md - Company state (integrates with save/load)

### Settings System (3 files)

- phase-5F-game-settings-modal.md - In-game settings (saves preferences)
- phase-5G-settings-persistence.md - Settings integration with save system
- phase-5H-settings-application.md - Apply settings throughout game

### Contract System (5 files)

- phase-6A-contract-generation.md - Generate contracts (saves to game state)
- phase-6B-contracts-view-window.md - Dark themed contracts list (loads from save)
- phase-6C-contract-cards.md - Dark graphite contract cards (displays saved data)
- phase-6D-contract-acceptance.md - Accept contracts (saves acceptance state)
- phase-6E-contract-completion.md - Complete contracts (saves completion state)

### Vehicle Management (4 files)

- phase-7A-vehicles-view-window.md - Dark themed vehicles list (loads from save)
- phase-7B-vehicle-cards.md - Dark graphite vehicle cards (displays saved data)
- phase-7C-move-vehicle-modal.md - Move vehicle (saves vehicle position)
- phase-7D-vehicle-interaction.md - Vehicle selection (saves selection state)

### 2D Map & Connected Road Network (4 files)

- phase-8A-2d-map-foundation.md - 2D map (saves city visited status)
- phase-8B-connected-road-network.md - Road network (saves network data)
- phase-8C-vehicle-positioning.md - Vehicle positioning (saves vehicle locations)
- phase-8D-conditional-map-details.md - Map details (saves display preferences)

### Vehicle Movement & Calculations (7 files)

- phase-9A-vehicle-selection.md - Vehicle selection (saves selected vehicle)
- phase-9B-network-pathfinding.md - Pathfinding (saves route data)
- phase-9C-vehicle-movement-logic.md - Movement (saves vehicle position/status)
- phase-9D-fuel-consumption-system.md - Fuel consumption (saves fuel levels)
- phase-9E-speed-calculation-system.md - Speed calculations (saves vehicle speed)
- phase-9F-real-time-vehicle-updates.md - Real-time updates (triggers auto-save)
- phase-9G-vehicle-status-tracking.md - Status tracking (saves vehicle status)

### Gas Station System (3 files)

- phase-10A-gas-stations-placement.md - Gas station placement (saves station data)
- phase-10B-gas-station-interaction.md - Refuel interaction (saves refuel actions)
- phase-10C-gas-station-modal.md - Refuel modal (triggers save on refuel)

### Police Checkpoint System (3 files)

- phase-10D-police-checkpoints-placement.md - Checkpoint placement (saves checkpoint data)
- phase-10E-speed-detection-system.md - Speed detection (saves fine history)
- phase-10F-police-fine-modal.md - Fine modal (triggers save on fine payment)

### Emergency Systems (1 file)

- phase-10G-emergency-refuel-modal.md - Emergency refuel (saves emergency actions)