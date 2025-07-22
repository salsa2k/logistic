using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// AIDEV-NOTE: Manages UI themes for the logistics game. Handles theme switching and application.
/// </summary>
public class ThemeManager : MonoBehaviour
{
    [Header("Theme Settings")]
    [SerializeField] private StyleSheet _darkGraphiteTheme;
    [SerializeField] private bool _applyThemeOnStart = true;
    
    [Header("UI Documents")]
    [SerializeField] private UIDocument[] _uiDocuments;
    
    // Singleton instance
    public static ThemeManager Instance { get; private set; }
    
    // Events
    public System.Action<StyleSheet> OnThemeChanged;
    
    // Properties
    public StyleSheet CurrentTheme => _darkGraphiteTheme;
    
    private void Awake()
    {
        // AIDEV-NOTE: Singleton pattern implementation for global theme management
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (_applyThemeOnStart)
        {
            ApplyTheme(_darkGraphiteTheme);
        }
    }
    
    /// <summary>
    /// Applies the specified theme to all registered UI documents.
    /// AIDEV-NOTE: This method handles the core theme switching functionality.
    /// </summary>
    /// <param name="theme">The StyleSheet to apply</param>
    public void ApplyTheme(StyleSheet theme)
    {
        if (theme == null)
        {
            Debug.LogWarning("ThemeManager: Attempted to apply null theme");
            return;
        }
        
        // Apply theme to all registered UI documents
        foreach (var uiDocument in _uiDocuments)
        {
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                ApplyThemeToDocument(uiDocument, theme);
            }
        }
        
        // Notify listeners of theme change
        OnThemeChanged?.Invoke(theme);
        
        Debug.Log($"ThemeManager: Applied theme '{theme.name}' to {_uiDocuments.Length} UI documents");
    }
    
    /// <summary>
    /// Applies theme to a specific UI document.
    /// AIDEV-NOTE: Helper method for individual document theme application.
    /// </summary>
    /// <param name="uiDocument">The UI document to apply theme to</param>
    /// <param name="theme">The StyleSheet to apply</param>
    public void ApplyThemeToDocument(UIDocument uiDocument, StyleSheet theme)
    {
        if (uiDocument == null || theme == null)
        {
            Debug.LogWarning("ThemeManager: UIDocument or theme is null");
            return;
        }
        
        var root = uiDocument.rootVisualElement;
        
        // Clear existing stylesheets (optional - remove if you want to stack themes)
        root.styleSheets.Clear();
        
        // Add the new theme
        root.styleSheets.Add(theme);
        
        // Add dark-theme class to root for styling
        root.AddToClassList("dark-theme");
    }
    
    /// <summary>
    /// Registers a UI document with the theme manager.
    /// AIDEV-NOTE: Call this method for UI documents created at runtime.
    /// </summary>
    /// <param name="uiDocument">The UI document to register</param>
    public void RegisterUIDocument(UIDocument uiDocument)
    {
        if (uiDocument == null)
        {
            Debug.LogWarning("ThemeManager: Attempted to register null UI document");
            return;
        }
        
        // Resize array and add new document
        var newArray = new UIDocument[_uiDocuments.Length + 1];
        System.Array.Copy(_uiDocuments, newArray, _uiDocuments.Length);
        newArray[_uiDocuments.Length] = uiDocument;
        _uiDocuments = newArray;
        
        // Apply current theme to new document
        ApplyThemeToDocument(uiDocument, _darkGraphiteTheme);
        
        Debug.Log($"ThemeManager: Registered UI document '{uiDocument.name}'");
    }
    
    /// <summary>
    /// Unregisters a UI document from the theme manager.
    /// AIDEV-NOTE: Call this method when UI documents are destroyed to prevent null references.
    /// </summary>
    /// <param name="uiDocument">The UI document to unregister</param>
    public void UnregisterUIDocument(UIDocument uiDocument)
    {
        if (uiDocument == null) return;
        
        var newList = new System.Collections.Generic.List<UIDocument>(_uiDocuments);
        newList.Remove(uiDocument);
        _uiDocuments = newList.ToArray();
        
        Debug.Log($"ThemeManager: Unregistered UI document '{uiDocument.name}'");
    }
    
    /// <summary>
    /// Applies the dark graphite theme specifically.
    /// AIDEV-NOTE: Convenience method for the main game theme.
    /// </summary>
    public void ApplyDarkGraphiteTheme()
    {
        ApplyTheme(_darkGraphiteTheme);
    }
    
    /// <summary>
    /// Refreshes the theme on all registered UI documents.
    /// AIDEV-NOTE: Useful for runtime theme updates or debugging.
    /// </summary>
    public void RefreshTheme()
    {
        ApplyTheme(_darkGraphiteTheme);
    }
    
    /// <summary>
    /// Gets the current theme name for display or debugging purposes.
    /// </summary>
    /// <returns>The name of the current theme</returns>
    public string GetCurrentThemeName()
    {
        return _darkGraphiteTheme != null ? _darkGraphiteTheme.name : "No Theme";
    }
    
    #if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;
    
    /// <summary>
    /// AIDEV-NOTE: Debug method for editor testing. Remove in production build.
    /// </summary>
    [ContextMenu("Apply Theme (Debug)")]
    private void DebugApplyTheme()
    {
        if (_debugMode && _darkGraphiteTheme != null)
        {
            ApplyTheme(_darkGraphiteTheme);
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method to find and register all UI documents in the scene.
    /// </summary>
    [ContextMenu("Find All UI Documents (Debug)")]
    private void DebugFindAllUIDocuments()
    {
        if (_debugMode)
        {
            var allUIDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            _uiDocuments = allUIDocuments;
            Debug.Log($"ThemeManager: Found and registered {allUIDocuments.Length} UI documents");
        }
    }
    #endif
}