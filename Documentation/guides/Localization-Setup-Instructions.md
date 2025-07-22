# Localization System Setup Instructions

This document provides step-by-step instructions for setting up the Unity Localization system for the logistics game project.

## Prerequisites

- Unity 6.1 (6000.1.10f1) ✅ Already installed
- Unity Localization package 1.5.5 ✅ Already installed
- LocalizationManager script ✅ Already created

## Step 1: Configure Localization Settings

1. **Open Project Settings**
   - Go to `Edit` → `Project Settings`
   - Navigate to `Localization` in the left sidebar

2. **Create Localization Settings**
   - Click "Create" to create the main Localization Settings asset
   - This will create the asset in your project and configure the system

3. **Configure Basic Settings**
   - Set "Initialization Selector" to "System Locale Selector" (to detect system language)
   - Add "Command Line Locale Selector" if you want command-line override capability

## Step 2: Create Locales

1. **Open Locale Generator**
   - In the Localization Settings, click "Locale Generator..."
   - This opens the Locale Generator window

2. **Create English Locale**
   - In the Locale Generator window, select "English" from the language dropdown
   - Select "United States" from the region dropdown (creates en-US)
   - Click "Generate Locale"
   - Save the locale asset to `Assets/Localization/Locales/`

3. **Create Brazilian Portuguese Locale**
   - Select "Portuguese" from the language dropdown
   - Select "Brazil" from the region dropdown (creates pt-BR)
   - Click "Generate Locale"
   - Save the locale asset to `Assets/Localization/Locales/`

4. **Add Locales to Settings**
   - In the Localization Settings, go to "Available Locales"
   - Click the "+" button to add your created locales
   - Set English (en-US) as the default locale by dragging it to the top

## Step 3: Create String Table Collections

1. **Open Localization Tables Window**
   - Go to `Window` → `Asset Management` → `Localization Tables`

2. **Create MainMenu String Table Collection**
   - Click "New Table Collection"
   - Select "String Table Collection"
   - Name it "MainMenu"
   - Choose the locales you created (en-US, pt-BR)
   - Click "Create"
   - Save to `Assets/Localization/String Tables/`

3. **Create Additional String Table Collections**
   - Repeat the process for:
     - "Game" (in-game UI strings)
     - "Settings" (settings and preferences)
     - "Common" (shared UI elements)
     - "Vehicles" (vehicle-related content)
     - "Contracts" (contract-related content)

## Step 4: Add Initial String Entries

### MainMenu Table
Add these entries to the MainMenu string table:

| Key | English Value | Portuguese Value (Placeholder) |
|-----|---------------|--------------------------------|
| NewGame_Button | New Game | Novo Jogo |
| LoadGame_Button | Load Game | Carregar Jogo |
| Settings_Button | Settings | Configurações |
| Exit_Button | Exit | Sair |
| Title_Text | Logistics Manager | Gerenciador de Logística |

### Settings Table
Add these entries to the Settings string table:

| Key | English Value | Portuguese Value (Placeholder) |
|-----|---------------|--------------------------------|
| Language_Label | Language | Idioma |
| Graphics_Label | Graphics | Gráficos |
| Audio_Label | Audio | Áudio |
| Apply_Button | Apply | Aplicar |
| Cancel_Button | Cancel | Cancelar |
| Save_Button | Save | Salvar |

### Common Table
Add these entries to the Common string table:

| Key | English Value | Portuguese Value (Placeholder) |
|-----|---------------|--------------------------------|
| OK_Button | OK | OK |
| Cancel_Button | Cancel | Cancelar |
| Yes_Button | Yes | Sim |
| No_Button | No | Não |
| Close_Button | Close | Fechar |
| Back_Button | Back | Voltar |

## Step 5: Configure Addressables (If Required)

1. **Open Addressables Window**
   - Go to `Window` → `Asset Management` → `Addressables` → `Groups`

2. **Configure Localization Group**
   - If a "Localization-String-Tables" group doesn't exist, create one
   - Ensure your string tables are marked as Addressable
   - This allows for efficient loading and memory management

## Step 6: Test the Setup

1. **Open Test Scene**
   - Open the Main scene in `Assets/Scenes/`

2. **Add Manager Initializer**
   - Create an empty GameObject in the scene
   - Add the `ManagerInitializer` component
   - This will automatically initialize the LocalizationManager

3. **Test in Play Mode**
   - Enter Play Mode
   - Check the Console for localization initialization messages
   - Look for "LocalizationManager initialized" message

4. **Test Language Switching**
   - In Play Mode, access the LocalizationManager in the Inspector
   - Use the context menu "Print Current Locale Info" to verify setup

## Step 7: Using Localization in UI Toolkit

### Method 1: UI Builder (Recommended)
1. Open a UXML file in UI Builder
2. Select a Label or Button element
3. In the Inspector, right-click on the "text" field
4. Select "Add binding" → "LocalizedString"
5. Configure:
   - Table Collection: Select your table (e.g., "MainMenu")
   - Table Entry: Select your key (e.g., "NewGame_Button")

### Method 2: UXML Code
```xml
<ui:Label text="Default Text">
    <Bindings>
        <UnityEngine.Localization.LocalizedString 
            property="text" 
            table="MainMenu" 
            entry="NewGame_Button" />
    </Bindings>
</ui:Label>
```

### Method 3: C# Scripting
```csharp
// Using LocalizationManager utility
string localizedText = LocalizationManager.Instance.GetLocalizedString("MainMenu", "NewGame_Button");

// Using direct binding
var localizedString = new LocalizedString("MainMenu", "NewGame_Button");
labelElement.SetBinding("text", localizedString);
```

## Expected Results

After completing these steps:

- ✅ Localization system is configured and active
- ✅ English and Portuguese locales are available
- ✅ String tables contain initial content
- ✅ LocalizationManager handles language switching
- ✅ UI Toolkit elements can use localized strings
- ✅ Language preferences persist between sessions

## Troubleshooting

### Common Issues:

1. **"LocalizationSettings not initialized" error**
   - Ensure Localization Settings asset exists in Project Settings
   - Check that LocalizationManager.InitializeLocalization() is called

2. **String tables not loading**
   - Verify string tables are marked as Addressable
   - Check that table names match exactly in code and assets

3. **Language switching not working**
   - Confirm UI elements use data binding, not hardcoded text
   - Verify LocalizedString bindings are properly configured

4. **Missing translations showing as keys**
   - Check that entries exist in all locale tables
   - Ensure fallback locale (English) has all required entries

## Next Steps

Once the basic setup is complete:

1. Add more detailed string content for all game systems
2. Implement language selection UI in settings
3. Test with longer Portuguese text for UI layout
4. Add localized assets (images, audio) if needed
5. Set up translator workflow for Brazilian Portuguese content