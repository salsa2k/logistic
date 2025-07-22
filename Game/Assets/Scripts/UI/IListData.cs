using System;
using System.Collections.Generic;
using System.Collections.Specialized;

/// <summary>
/// AIDEV-NOTE: Interface for list data management with generic support for different item types.
/// Provides observable collection behavior and filtering capabilities.
/// </summary>
public interface IListData<T> : INotifyCollectionChanged where T : ICardData
{
    /// <summary>
    /// Total number of items in the collection
    /// </summary>
    int Count { get; }
    
    /// <summary>
    /// Whether the collection is currently loading data
    /// </summary>
    bool IsLoading { get; }
    
    /// <summary>
    /// Current filter text applied to the collection
    /// </summary>
    string FilterText { get; set; }
    
    /// <summary>
    /// Current sort criteria applied to the collection
    /// </summary>
    ListSortCriteria SortCriteria { get; set; }
    
    /// <summary>
    /// Whether the collection is sorted in ascending order
    /// </summary>
    bool IsAscending { get; set; }
    
    /// <summary>
    /// Gets item at the specified index
    /// </summary>
    T GetItem(int index);
    
    /// <summary>
    /// Gets all items in the collection (filtered and sorted)
    /// </summary>
    IEnumerable<T> GetAllItems();
    
    /// <summary>
    /// Gets a range of items for virtualization
    /// </summary>
    IEnumerable<T> GetItemRange(int startIndex, int count);
    
    /// <summary>
    /// Adds an item to the collection
    /// </summary>
    void AddItem(T item);
    
    /// <summary>
    /// Removes an item from the collection
    /// </summary>
    void RemoveItem(T item);
    
    /// <summary>
    /// Clears all items from the collection
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Refreshes the collection (re-applies filters and sorting)
    /// </summary>
    void Refresh();
    
    /// <summary>
    /// Applies a custom filter predicate
    /// </summary>
    void SetCustomFilter(Func<T, bool> filter);
    
    /// <summary>
    /// Event fired when items are selected
    /// </summary>
    event Action<IEnumerable<T>> OnItemsSelected;
    
    /// <summary>
    /// Event fired when filtering starts or completes
    /// </summary>
    event Action<bool> OnFilteringChanged;
}

/// <summary>
/// AIDEV-NOTE: Sort criteria options for list data
/// </summary>
public enum ListSortCriteria
{
    None,
    Name,
    Title,
    Date,
    Price,
    Status,
    Priority,
    Custom
}

/// <summary>
/// AIDEV-NOTE: Selection mode for list interactions
/// </summary>
public enum ListSelectionMode
{
    None,        // No selection allowed
    Single,      // Single item selection
    Multiple     // Multiple item selection
}

/// <summary>
/// AIDEV-NOTE: Virtualization mode for list performance
/// </summary>
public enum ListVirtualizationMode
{
    Disabled,    // Show all items (use for small lists)
    Fixed,       // Fixed item height virtualization
    Dynamic      // Dynamic item height virtualization
}