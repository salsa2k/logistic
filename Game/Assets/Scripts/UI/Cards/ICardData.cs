using UnityEngine;

/// <summary>
/// AIDEV-NOTE: Generic interface for card data binding system. Allows cards to display different types of content.
/// </summary>
public interface ICardData
{
    /// <summary>
    /// Primary title displayed in the card header
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Optional subtitle displayed below the title
    /// </summary>
    string Subtitle { get; }
    
    /// <summary>
    /// Optional icon displayed in the card header
    /// </summary>
    Sprite Icon { get; }
    
    /// <summary>
    /// Unique identifier for the card data
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Whether this card should be displayed as selected
    /// </summary>
    bool IsSelected { get; }
    
    /// <summary>
    /// Whether this card should be displayed as disabled
    /// </summary>
    bool IsDisabled { get; }
    
    /// <summary>
    /// Optional badge text for categorization
    /// </summary>
    string BadgeText { get; }
}

/// <summary>
/// AIDEV-NOTE: Extended interface for cards that display detailed information
/// </summary>
public interface IDetailedCardData : ICardData
{
    /// <summary>
    /// Main description text displayed in the card content area
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Optional image displayed in the card image area
    /// </summary>
    Sprite PreviewImage { get; }
    
    /// <summary>
    /// Collection of key-value pairs for displaying detailed information
    /// </summary>
    System.Collections.Generic.Dictionary<string, string> Details { get; }
}

/// <summary>
/// AIDEV-NOTE: Interface for cards that support actions (buttons in footer)
/// </summary>
public interface IActionCardData : ICardData
{
    /// <summary>
    /// Primary action text (e.g., "Accept Contract", "Purchase Vehicle")
    /// </summary>
    string PrimaryActionText { get; }
    
    /// <summary>
    /// Whether the primary action is available
    /// </summary>
    bool IsPrimaryActionEnabled { get; }
    
    /// <summary>
    /// Optional secondary action text (e.g., "View Details")
    /// </summary>
    string SecondaryActionText { get; }
    
    /// <summary>
    /// Whether the secondary action is available
    /// </summary>
    bool IsSecondaryActionEnabled { get; }
}