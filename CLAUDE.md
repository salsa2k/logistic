# Logistics Game Design Document
This document outlines the design and architecture of the logistics game, including the user interface, game mechanics, and visual layout.

## Project Context
The logistics game is a 2D top-down vehicle simulation game where players can manage contracts, vehicles, and shopping. Players will complete contracts by transporting goods between cities while managing their vehicle's speed and gas levels.

## AI Guidelines

### The Golden Rule  
When unsure about implementation details, ALWAYS ask the developer.

### What AI Must NEVER Do  

1. **Never assume business logic** - Always ask
2. **Never remove AIDEV- comments** - They're there for a reason
3. **Never create test sample code** - Focus on production code only
4. **Never create documentation files** unless explicitly requested
5. **Never use hardcoded values** - Use constants or configuration files
6. **Never use magic numbers** - Define constants for reusable values

### Documentation Guidelines

- **Special documentation**: If any special documentation needs to be written, place it in the `Documentation/` folder with a proper child folder structure
- **Folder organization**: Use descriptive subfolder names based on the documentation type (e.g., `Documentation/ai-prompt/`, `Documentation/guides/`, `Documentation/architecture/`)

### Decision-Making Boundaries

- **Ask First**: Business logic, game mechanics, UI/UX decisions, data relationships
- **Infer Safely**: Code style consistency, standard patterns, obvious bug fixes
- **Examples of Good AI Behavior**: Following existing patterns, maintaining consistency, asking for clarification
- **Examples of Bad AI Behavior**: Assuming game rules, creating new features without approval, removing existing functionality

## Anchor comments  

Add specially formatted comments throughout the codebase, where appropriate, for yourself as inline knowledge that can be easily `grep`ped for.  

### Guidelines:  

- Use `AIDEV-NOTE:`, `AIDEV-TODO:`, or `AIDEV-QUESTION:` (all-caps prefix) for comments aimed at AI and developers.  
- Keep them concise (≤ 120 chars).  
- **Important:** Before scanning files, always first try to **locate existing anchors** `AIDEV-*` in relevant subdirectories.  
- **Update relevant anchors** when modifying associated code.  
- **Do not remove `AIDEV-NOTE`s** without explicit human instruction.  

Example: 
```csharp
// AIDEV-NOTE: This method handles vehicle movement logic.
public void MoveVehicle()
{
    // Movement logic here
}
```

## Architecture Decisions
- This project will use Unity's 6.1
- The setup is using the universal 2d template SRP.
- The game will be developed using C# and Unity's built-in features.
- Use Unity’s event system (C# events or UnityEvent) for decoupled communication between game systems (e.g., contract completion, vehicle status updates).
- Icons, images and sounds **MUST** be stored on a AssetManager to be assigned on the editor for easy access and management.
- It should use the Localization package for managing multiple languages (English and Brazilian Portuguese).
- UI Toolkit will be used for the user interface, with a focus on responsive design for different screen sizes.
- The minimum resolution will be 1280x720, and the game should scale appropriately for larger resolutions.
- When creating Prefabs, guide the developer on how to confure the Prefab to ensure it is reusable and maintainable. This includes:
  - Using appropriate naming conventions.
  - Organizing components logically.
  - Ensuring that all necessary references are set up in the Inspector.
  - Documenting any specific setup requirements in the Prefab's description.

## Folder Structure
```
Assets/Art/                # e.g., Sprites/, Icons/, Backgrounds/
Assets/Scripts/            # e.g., Managers/, UI/, Vehicles/, Contracts/
Assets/Prefabs/            # e.g., Vehicles/, UI/, Environment/
Assets/Scenes/             # e.g., MainMenu/, Game/, Test/
Assets/Audio/              # e.g., Music/, SFX/, UI/
Assets/Resources/          # e.g., Configs/, Templates/
Assets/Animations/         # e.g., Characters/, Vehicles/
Assets/Fonts/              # e.g., UI/, Titles/
Assets/Materials/          # e.g., UI/, Vehicles/, Environment/
Assets/Data/               # e.g., ScriptableObjects/, SaveData/
Assets/UI/                 # UI Toolkit files
├── Documents/             # e.g., .uxml files
├── StyleSheets/           # e.g., .uss files
├── Resources/             # e.g., Runtime UI assets
└── Themes/               # e.g., Theme style sheets
```

## Code Style

### Unity 6 C# Code Style Guidelines

- **File Organization**
  - One class per file (unless tightly coupled).
  - File name matches the class name.

- **Naming Conventions**
  - Classes, structs, enums: `PascalCase` (e.g., `PlayerController`)
  - Methods, properties, public fields: `PascalCase`
  - Private fields: `_camelCase` (prefix with underscore)
  - Local variables, parameters: `camelCase`
  - Constants: `UPPER_CASE_WITH_UNDERSCORES`
  - ScriptableObject assets: end with `Data` or `Config` (e.g., `VehicleData`)

- **Braces & Indentation**
  - Use Allman style (braces on new lines).

- **Unity-Specific**
  - Use `[SerializeField]` for private fields that need to be shown in the Inspector.
  - Use `private` instead of `public` for fields, expose via properties or methods.
  - Group Unity event methods (`Awake`, `Start`, `Update`, etc.) at the top of the class.
  - Use `nameof()` instead of string literals for event names and property names.

- **Comments**
  - Use XML documentation comments for public APIs.
  - Use `//` for inline comments, avoid block comments unless necessary.

#### Example

```csharp
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _fuel = 100f;

    private void Awake()
    {
        // Initialization logic
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        // Movement logic
    }
}
```

## Patterns to Follow
- Use the Singleton pattern for global managers (e.g., GameManager, AssetManager).
- Use the Observer pattern for event-driven communication (e.g., vehicle status updates).
- Avoid using `Find()` and `GetComponent()` in `Update()`.
- Use `const` or `readonly` where possible.
- Prefer composition over inheritance.
- Use `ScriptableObject` for data containers.
- Use Unity's built-in serialization for saving/loading game state.
- Instead of singletons, consider using ScriptableObjects for better flexibility.

## Syntax checking

### Main game code (Assets/Game/Scripts)
```
dotnet build Assembly-CSharp.csproj --no-restore
```

### Editor scripts (Assets/*/Editor)
```
dotnet build Assembly-CSharp-Editor.csproj --no-restore
```

### Quick check for any errors
```
dotnet build --no-restore | grep -E "error CS|: error"
```

### Development Workflow
1. **Before coding**: Query Context7 for Unity documentation and best practices
2. **During development**: Use unityMCP for testing and validation
3. **For USS/UI Toolkit**: Always verify properties with Context7 Unity UI documentation
4. **For debugging**: Use unityMCP console tools instead of manual Unity editor checks

## UI Toolkit Guidelines

### UI Toolkit Structure
- Use `.uxml` files for UI layouts created with UI Builder
- Use `.uss` files for styling (USS is similar to CSS)
- Organize UI documents in `Assets/UI/` folder structure:
  ```
  Assets/UI/
  ├── Documents/          # .uxml files
  ├── StyleSheets/        # .uss files
  ├── Resources/          # Runtime UI assets
  └── Themes/            # Theme style sheets
  ```

### UI Toolkit Naming Conventions
- UXML files: `PascalCase` ending with `Document` (e.g., `MainMenuDocument.uxml`)
- USS files: `PascalCase` ending with `Styles` (e.g., `MainMenuStyles.uss`)
- USS class names: `kebab-case` (e.g., `.main-button`, `.vehicle-item`)
- Element names in UXML: `kebab-case` (e.g., `name="credits-display"`)

### UI Toolkit Best Practices
- Use UI Builder for visual design, code for dynamic behavior
- Bind UI elements using `UQueryExtensions` (e.g., `root.Q<Button>("my-button")`)
- Use data binding where possible for automatic UI updates
- Separate presentation (UXML/USS) from logic (C# scripts)
- Use USS variables for consistent theming
- Prefer USS classes over inline styles

### Example UI Toolkit Structure
```csharp
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    private Button _newGameButton;
    
    private void OnEnable()
    {
        var root = _uiDocument.rootVisualElement;
        _newGameButton = root.Q<Button>("new-game-button");        
        _newGameButton.clicked += OnNewGameClicked;
    }
}
```