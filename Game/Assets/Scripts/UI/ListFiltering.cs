using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LogisticGame.UI.Lists
{
    /// <summary>
    /// AIDEV-NOTE: Filtering and search system for list components.
    /// Provides text search, custom filters, and sorting capabilities.
    /// </summary>
    public class ListFiltering<T> where T : ICardData
    {
        // Configuration
        private bool _isCaseSensitive;
        private bool _useRegex;
        private bool _searchInSubfields;
        private float _searchDebounceTime;
        
        // State
        private string _currentSearchText;
        private ListSortCriteria _currentSortCriteria;
        private bool _isAscendingSort;
        private List<Func<T, bool>> _customFilters;
        private Dictionary<string, Func<T, object>> _sortKeySelectors;
        
        // Data
        private List<T> _originalItems;
        private List<T> _filteredItems;
        private List<int> _originalIndices;
        
        // Events
        public event Action<List<T>, int> OnFilteredItemsChanged;
        public event Action<string, int> OnSearchCompleted;
        public event Action<ListSortCriteria, bool> OnSortChanged;
        
        /// <summary>
        /// Current search text
        /// </summary>
        public string SearchText => _currentSearchText ?? "";
        
        /// <summary>
        /// Current sort criteria
        /// </summary>
        public ListSortCriteria SortCriteria => _currentSortCriteria;
        
        /// <summary>
        /// Whether sorting is in ascending order
        /// </summary>
        public bool IsAscendingSort => _isAscendingSort;
        
        /// <summary>
        /// Total number of original items
        /// </summary>
        public int OriginalCount => _originalItems?.Count ?? 0;
        
        /// <summary>
        /// Number of filtered items
        /// </summary>
        public int FilteredCount => _filteredItems?.Count ?? 0;
        
        /// <summary>
        /// Whether any filters are currently active
        /// </summary>
        public bool HasActiveFilters => !string.IsNullOrEmpty(_currentSearchText) || 
                                        (_customFilters?.Count > 0) || 
                                        _currentSortCriteria != ListSortCriteria.None;
        
        /// <summary>
        /// AIDEV-NOTE: Initialize the filtering system
        /// </summary>
        public ListFiltering()
        {
            _isCaseSensitive = false;
            _useRegex = false;
            _searchInSubfields = true;
            _searchDebounceTime = 0.3f;
            
            _currentSortCriteria = ListSortCriteria.None;
            _isAscendingSort = true;
            _customFilters = new List<Func<T, bool>>();
            _sortKeySelectors = new Dictionary<string, Func<T, object>>();
            
            _originalItems = new List<T>();
            _filteredItems = new List<T>();
            _originalIndices = new List<int>();
            
            InitializeDefaultSortKeySelectors();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Configure filtering behavior
        /// </summary>
        public void Configure(bool caseSensitive = false, bool useRegex = false, 
                              bool searchInSubfields = true, float debounceTime = 0.3f)
        {
            _isCaseSensitive = caseSensitive;
            _useRegex = useRegex;
            _searchInSubfields = searchInSubfields;
            _searchDebounceTime = debounceTime;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Set the original dataset to filter
        /// </summary>
        public void SetItems(List<T> items)
        {
            _originalItems = new List<T>(items ?? new List<T>());
            _originalIndices = Enumerable.Range(0, _originalItems.Count).ToList();
            
            ApplyFilters();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Add an item to the original dataset
        /// </summary>
        public void AddItem(T item)
        {
            if (item == null) return;
            
            _originalItems.Add(item);
            _originalIndices.Add(_originalItems.Count - 1);
            
            ApplyFilters();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Remove an item from the dataset
        /// </summary>
        public void RemoveItem(T item)
        {
            if (item == null) return;
            
            int index = _originalItems.IndexOf(item);
            if (index >= 0)
            {
                _originalItems.RemoveAt(index);
                _originalIndices.RemoveAt(index);
                
                // Adjust remaining indices
                for (int i = index; i < _originalIndices.Count; i++)
                {
                    _originalIndices[i]--;
                }
                
                ApplyFilters();
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Clear all items
        /// </summary>
        public void ClearItems()
        {
            _originalItems.Clear();
            _originalIndices.Clear();
            _filteredItems.Clear();
            
            OnFilteredItemsChanged?.Invoke(_filteredItems, 0);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Set search text and apply filtering
        /// </summary>
        public void SetSearchText(string searchText)
        {
            _currentSearchText = searchText;
            ApplyFilters();
            OnSearchCompleted?.Invoke(searchText, FilteredCount);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Clear search text
        /// </summary>
        public void ClearSearch()
        {
            SetSearchText("");
        }
        
        /// <summary>
        /// AIDEV-NOTE: Set sort criteria and apply sorting
        /// </summary>
        public void SetSort(ListSortCriteria criteria, bool ascending = true)
        {
            _currentSortCriteria = criteria;
            _isAscendingSort = ascending;
            
            ApplyFilters();
            OnSortChanged?.Invoke(criteria, ascending);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Toggle sort direction
        /// </summary>
        public void ToggleSortDirection()
        {
            SetSort(_currentSortCriteria, !_isAscendingSort);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Add a custom filter predicate
        /// </summary>
        public void AddCustomFilter(Func<T, bool> filter)
        {
            if (filter != null)
            {
                _customFilters.Add(filter);
                ApplyFilters();
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Remove a custom filter predicate
        /// </summary>
        public void RemoveCustomFilter(Func<T, bool> filter)
        {
            if (_customFilters.Remove(filter))
            {
                ApplyFilters();
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Clear all custom filters
        /// </summary>
        public void ClearCustomFilters()
        {
            _customFilters.Clear();
            ApplyFilters();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Add a custom sort key selector
        /// </summary>
        public void AddSortKeySelector(string name, Func<T, object> keySelector)
        {
            if (!string.IsNullOrEmpty(name) && keySelector != null)
            {
                _sortKeySelectors[name] = keySelector;
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Get filtered items
        /// </summary>
        public List<T> GetFilteredItems()
        {
            return new List<T>(_filteredItems);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Get filtered item at specific index
        /// </summary>
        public T GetFilteredItem(int index)
        {
            if (index >= 0 && index < _filteredItems.Count)
            {
                return _filteredItems[index];
            }
            return default(T);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Get original index of filtered item
        /// </summary>
        public int GetOriginalIndex(int filteredIndex)
        {
            if (filteredIndex >= 0 && filteredIndex < _originalIndices.Count)
            {
                return _originalIndices[filteredIndex];
            }
            return -1;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Refresh all filters
        /// </summary>
        public void RefreshFilters()
        {
            ApplyFilters();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Apply all filters, sorting, and search
        /// </summary>
        private void ApplyFilters()
        {
            _filteredItems.Clear();
            var filteredIndices = new List<int>();
            
            for (int i = 0; i < _originalItems.Count; i++)
            {
                var item = _originalItems[i];
                
                // Apply text search
                if (!string.IsNullOrEmpty(_currentSearchText) && !MatchesSearchText(item))
                {
                    continue;
                }
                
                // Apply custom filters
                bool passesCustomFilters = true;
                foreach (var filter in _customFilters)
                {
                    if (!filter(item))
                    {
                        passesCustomFilters = false;
                        break;
                    }
                }
                
                if (!passesCustomFilters)
                {
                    continue;
                }
                
                _filteredItems.Add(item);
                filteredIndices.Add(i);
            }
            
            // Apply sorting
            if (_currentSortCriteria != ListSortCriteria.None)
            {
                ApplySorting(filteredIndices);
            }
            
            _originalIndices = filteredIndices;
            OnFilteredItemsChanged?.Invoke(_filteredItems, _originalItems.Count);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Check if item matches search text
        /// </summary>
        private bool MatchesSearchText(T item)
        {
            if (string.IsNullOrEmpty(_currentSearchText)) return true;
            
            var searchTerms = new List<string>();
            
            // Basic searchable fields
            if (!string.IsNullOrEmpty(item.Title))
                searchTerms.Add(item.Title);
            if (!string.IsNullOrEmpty(item.Subtitle))
                searchTerms.Add(item.Subtitle);
            if (!string.IsNullOrEmpty(item.BadgeText))
                searchTerms.Add(item.BadgeText);
            
            // Extended fields for detailed items
            if (_searchInSubfields && item is IDetailedCardData detailedItem)
            {
                if (!string.IsNullOrEmpty(detailedItem.Description))
                    searchTerms.Add(detailedItem.Description);
                
                if (detailedItem.Details != null)
                {
                    foreach (var kvp in detailedItem.Details)
                    {
                        searchTerms.Add(kvp.Key);
                        searchTerms.Add(kvp.Value);
                    }
                }
            }
            
            // Perform search
            foreach (var term in searchTerms)
            {
                if (PerformTextMatch(term, _currentSearchText))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Perform text matching with configured options
        /// </summary>
        private bool PerformTextMatch(string text, string searchText)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchText)) return false;
            
            var comparison = _isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            if (_useRegex)
            {
                try
                {
                    var options = _isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    return Regex.IsMatch(text, searchText, options);
                }
                catch (ArgumentException)
                {
                    // Fall back to simple contains if regex is invalid
                    return text.Contains(searchText, comparison);
                }
            }
            else
            {
                return text.Contains(searchText, comparison);
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Apply sorting to filtered items
        /// </summary>
        private void ApplySorting(List<int> filteredIndices)
        {
            var itemsWithIndices = _filteredItems
                .Select((item, index) => new { Item = item, Index = filteredIndices[index] })
                .ToList();
            
            switch (_currentSortCriteria)
            {
                case ListSortCriteria.Name:
                case ListSortCriteria.Title:
                    var titleSorted = _isAscendingSort
                        ? itemsWithIndices.OrderBy(x => x.Item.Title ?? "")
                        : itemsWithIndices.OrderByDescending(x => x.Item.Title ?? "");
                    UpdateSortedResults(titleSorted, filteredIndices);
                    break;
                    
                case ListSortCriteria.Status:
                    var statusSorted = _isAscendingSort
                        ? itemsWithIndices.OrderBy(x => x.Item.BadgeText ?? "")
                        : itemsWithIndices.OrderByDescending(x => x.Item.BadgeText ?? "");
                    UpdateSortedResults(statusSorted, filteredIndices);
                    break;
                    
                case ListSortCriteria.Custom:
                    // Try to find a custom sort key selector
                    if (_sortKeySelectors.ContainsKey("Custom"))
                    {
                        var keySelector = _sortKeySelectors["Custom"];
                        var customSorted = _isAscendingSort
                            ? itemsWithIndices.OrderBy(x => keySelector(x.Item))
                            : itemsWithIndices.OrderByDescending(x => keySelector(x.Item));
                        UpdateSortedResults(customSorted, filteredIndices);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Helper method to update sorted results without using dynamic types
        /// </summary>
        private void UpdateSortedResults<TAnonymous>(IEnumerable<TAnonymous> sortedItems, List<int> filteredIndices) where TAnonymous : class
        {
            var sortedList = sortedItems.ToList();
            _filteredItems.Clear();
            filteredIndices.Clear();
            
            foreach (var item in sortedList)
            {
                // Use reflection to get the Item and Index properties from the anonymous type
                var itemProperty = item.GetType().GetProperty("Item");
                var indexProperty = item.GetType().GetProperty("Index");
                
                if (itemProperty != null && indexProperty != null)
                {
                    var cardItem = (T)itemProperty.GetValue(item);
                    var originalIndex = (int)indexProperty.GetValue(item);
                    
                    _filteredItems.Add(cardItem);
                    filteredIndices.Add(originalIndex);
                }
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Initialize default sort key selectors
        /// </summary>
        private void InitializeDefaultSortKeySelectors()
        {
            // Add common sort key selectors that can be used with different data types
            _sortKeySelectors["Title"] = item => item.Title ?? "";
            _sortKeySelectors["Subtitle"] = item => item.Subtitle ?? "";
            _sortKeySelectors["Badge"] = item => item.BadgeText ?? "";
            _sortKeySelectors["Id"] = item => item.Id ?? "";
        }
    }
}