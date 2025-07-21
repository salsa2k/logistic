# Phase 1A - Basic Unity Setup

## Overview
Establish the foundational Unity project structure, core managers, and Asset Manager system for the logistics game.

## Tasks

### Unity Project Setup
- Configure Unity 6.1 with Universal 2D template SRP
- Set minimum resolution to 1280x720 with responsive scaling
- Configure build settings for target platforms

### Folder Structure Implementation
- Create the complete folder structure as specified in CLAUDE.md:
  - Assets/Art/ (Sprites/, Icons/, Backgrounds/)
  - Assets/Scripts/ (Managers/, UI/, Vehicles/, Contracts/)
  - Assets/Prefabs/ (Vehicles/, UI/, Environment/)
  - Assets/Scenes/ (MainMenu/, Game/, Test/)
  - Assets/Audio/ (Music/, SFX/, UI/)
  - Assets/Resources/ (Configs/, Templates/)
  - Assets/Animations/ (Characters/, Vehicles/)
  - Assets/Fonts/ (UI/, Titles/)
  - Assets/Materials/ (UI/, Vehicles/, Environment/)
  - Assets/Data/ (ScriptableObjects/, SaveData/)
  - Assets/UI/ (Documents/, StyleSheets/, Resources/, Themes/)

### Core Manager Systems
- Implement GameManager as singleton pattern for global game state
- Create AssetManager as singleton for centralized asset management
- Set up SceneManager for scene transitions
- Configure Event System for decoupled communication

### AssetManager Implementation
- Create centralized system for managing all game assets
- Implement easy access patterns for sprites, sounds, and prefabs
- Configure editor assignments for asset references
- Set up asset loading and unloading mechanisms

## Acceptance Criteria

### Project Structure
- ✅ Unity 6.1 project with Universal 2D template configured
- ✅ All folder structures created and organized
- ✅ Project settings configured for 1280x720 minimum resolution
- ✅ Build settings properly configured

### Core Systems
- ✅ GameManager singleton implemented with proper initialization
- ✅ AssetManager singleton with asset reference system
- ✅ Event system configured for game communication
- ✅ Scene management system ready for menu/game transitions

### Asset Management
- ✅ AssetManager provides easy access to all game assets
- ✅ Editor inspector shows asset assignment slots
- ✅ Asset loading/unloading system implemented
- ✅ Performance considerations for asset management addressed

## Technical Notes

### Architecture Decisions
- Use Unity's built-in event system (C# events or UnityEvent)
- Implement Singleton pattern for global managers
- Follow composition over inheritance principle
- Use ScriptableObjects for data containers

### Performance Considerations
- Avoid Find() and GetComponent() calls in Update()
- Implement object pooling where appropriate
- Consider asset streaming for larger projects
- Use Unity's built-in serialization system

### Code Style Compliance
- Follow naming conventions: PascalCase for classes, _camelCase for private fields
- Use [SerializeField] for private fields shown in Inspector
- Group Unity event methods at top of classes
- Use nameof() instead of string literals

## Dependencies
- None (Foundation phase)

## Integration Points
- Provides foundation for all subsequent phases
- AssetManager will be used by all UI and game systems
- GameManager will coordinate with save/load systems
- Event system will be used throughout the project

## Notes
- This phase establishes the architectural foundation
- All subsequent phases depend on these core systems
- Proper setup here prevents major refactoring later
- Focus on scalability and maintainability