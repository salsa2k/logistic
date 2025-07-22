using System.Collections.Generic;
using LogisticGame.Events;

namespace LogisticGame.UI.Lists
{
    /// <summary>
    /// AIDEV-NOTE: Event fired when a list item is selected
    /// </summary>
    public struct ListItemSelectedEvent : IGameEvent
    {
        public string ListId { get; }
        public string ItemId { get; }
        public int ItemIndex { get; }
        
        public ListItemSelectedEvent(string listId, string itemId, int itemIndex)
        {
            ListId = listId;
            ItemId = itemId;
            ItemIndex = itemIndex;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when multiple list items are selected
    /// </summary>
    public struct ListItemsSelectedEvent : IGameEvent
    {
        public string ListId { get; }
        public IEnumerable<string> ItemIds { get; }
        public IEnumerable<int> ItemIndices { get; }
        
        public ListItemsSelectedEvent(string listId, IEnumerable<string> itemIds, IEnumerable<int> itemIndices)
        {
            ListId = listId;
            ItemIds = itemIds;
            ItemIndices = itemIndices;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list filter is changed
    /// </summary>
    public struct ListFilterChangedEvent : IGameEvent
    {
        public string ListId { get; }
        public string FilterText { get; }
        public int ResultCount { get; }
        
        public ListFilterChangedEvent(string listId, string filterText, int resultCount)
        {
            ListId = listId;
            FilterText = filterText;
            ResultCount = resultCount;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list sort criteria changes
    /// </summary>
    public struct ListSortChangedEvent : IGameEvent
    {
        public string ListId { get; }
        public ListSortCriteria SortCriteria { get; }
        public bool IsAscending { get; }
        
        public ListSortChangedEvent(string listId, ListSortCriteria sortCriteria, bool isAscending)
        {
            ListId = listId;
            SortCriteria = sortCriteria;
            IsAscending = isAscending;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list data is refreshed
    /// </summary>
    public struct ListDataRefreshedEvent : IGameEvent
    {
        public string ListId { get; }
        public int TotalItems { get; }
        public int VisibleItems { get; }
        
        public ListDataRefreshedEvent(string listId, int totalItems, int visibleItems)
        {
            ListId = listId;
            TotalItems = totalItems;
            VisibleItems = visibleItems;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list scroll position changes (for virtualization)
    /// </summary>
    public struct ListScrollChangedEvent : IGameEvent
    {
        public string ListId { get; }
        public int FirstVisibleIndex { get; }
        public int LastVisibleIndex { get; }
        public float ScrollPosition { get; }
        
        public ListScrollChangedEvent(string listId, int firstVisibleIndex, int lastVisibleIndex, float scrollPosition)
        {
            ListId = listId;
            FirstVisibleIndex = firstVisibleIndex;
            LastVisibleIndex = lastVisibleIndex;
            ScrollPosition = scrollPosition;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list item action is triggered
    /// </summary>
    public struct ListItemActionEvent : IGameEvent
    {
        public string ListId { get; }
        public string ItemId { get; }
        public string ActionType { get; }
        public int ItemIndex { get; }
        
        public ListItemActionEvent(string listId, string itemId, string actionType, int itemIndex)
        {
            ListId = listId;
            ItemId = itemId;
            ActionType = actionType;
            ItemIndex = itemIndex;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list context menu is requested
    /// </summary>
    public struct ListContextMenuEvent : IGameEvent
    {
        public string ListId { get; }
        public string ItemId { get; }
        public UnityEngine.Vector2 Position { get; }
        
        public ListContextMenuEvent(string listId, string itemId, UnityEngine.Vector2 position)
        {
            ListId = listId;
            ItemId = itemId;
            Position = position;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list enters loading state
    /// </summary>
    public struct ListLoadingStateChangedEvent : IGameEvent
    {
        public string ListId { get; }
        public bool IsLoading { get; }
        public string LoadingMessage { get; }
        
        public ListLoadingStateChangedEvent(string listId, bool isLoading, string loadingMessage = "")
        {
            ListId = listId;
            IsLoading = isLoading;
            LoadingMessage = loadingMessage;
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Event fired when list drag and drop operation occurs
    /// </summary>
    public struct ListItemDragDropEvent : IGameEvent
    {
        public string ListId { get; }
        public string ItemId { get; }
        public int FromIndex { get; }
        public int ToIndex { get; }
        
        public ListItemDragDropEvent(string listId, string itemId, int fromIndex, int toIndex)
        {
            ListId = listId;
            ItemId = itemId;
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }
    }
}