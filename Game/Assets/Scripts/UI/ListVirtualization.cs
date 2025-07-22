using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LogisticGame.UI.Lists
{
    /// <summary>
    /// AIDEV-NOTE: Virtualization system for list performance optimization.
    /// Only renders visible items to handle large datasets efficiently.
    /// </summary>
    public class ListVirtualization
    {
        // Configuration
        private ListVirtualizationMode _virtualizationMode;
        private float _fixedItemHeight;
        private int _bufferItems;
        private int _maxVisibleItems;
        
        // UI References
        private ScrollView _scrollView;
        private VisualElement _viewport;
        private VisualElement _itemsContainer;
        private VisualElement _topSpacer;
        private VisualElement _bottomSpacer;
        
        // State
        private int _totalItemCount;
        private int _firstVisibleIndex;
        private int _lastVisibleIndex;
        private float _scrollPosition;
        private Dictionary<int, float> _itemHeights;
        private Queue<VisualElement> _recycledElements;
        private List<VirtualizedItem> _visibleItems;
        
        // Events
        public event Action<int, int, float> OnScrollPositionChanged;
        public event Action<int, VisualElement> OnItemElementRequested;
        public event Action<int, VisualElement> OnItemElementReleased;
        
        /// <summary>
        /// Current virtualization mode
        /// </summary>
        public ListVirtualizationMode VirtualizationMode => _virtualizationMode;
        
        /// <summary>
        /// Total number of items in the dataset
        /// </summary>
        public int TotalItemCount => _totalItemCount;
        
        /// <summary>
        /// Index of the first visible item
        /// </summary>
        public int FirstVisibleIndex => _firstVisibleIndex;
        
        /// <summary>
        /// Index of the last visible item
        /// </summary>
        public int LastVisibleIndex => _lastVisibleIndex;
        
        /// <summary>
        /// Whether virtualization is enabled
        /// </summary>
        public bool IsVirtualizationEnabled => _virtualizationMode != ListVirtualizationMode.Disabled;
        
        /// <summary>
        /// AIDEV-NOTE: Represents a virtualized list item with its index and element
        /// </summary>
        private struct VirtualizedItem
        {
            public int Index;
            public VisualElement Element;
            public float Height;
            
            public VirtualizedItem(int index, VisualElement element, float height)
            {
                Index = index;
                Element = element;
                Height = height;
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Initialize virtualization system with UI references
        /// </summary>
        public void Initialize(ScrollView scrollView, VisualElement viewport, VisualElement itemsContainer,
                               VisualElement topSpacer, VisualElement bottomSpacer)
        {
            _scrollView = scrollView;
            _viewport = viewport;
            _itemsContainer = itemsContainer;
            _topSpacer = topSpacer;
            _bottomSpacer = bottomSpacer;
            
            _itemHeights = new Dictionary<int, float>();
            _recycledElements = new Queue<VisualElement>();
            _visibleItems = new List<VirtualizedItem>();
            
            // Set default configuration
            _virtualizationMode = ListVirtualizationMode.Fixed;
            _fixedItemHeight = 60f;
            _bufferItems = 3;
            _maxVisibleItems = 50;
            
            SetupEventListeners();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Configure virtualization parameters
        /// </summary>
        public void Configure(ListVirtualizationMode mode, float fixedItemHeight = 60f, 
                              int bufferItems = 3, int maxVisibleItems = 50)
        {
            _virtualizationMode = mode;
            _fixedItemHeight = fixedItemHeight;
            _bufferItems = bufferItems;
            _maxVisibleItems = maxVisibleItems;
            
            // Refresh virtualization if already initialized
            if (_totalItemCount > 0)
            {
                RefreshVirtualization();
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Update the total item count and refresh virtualization
        /// </summary>
        public void SetTotalItemCount(int count)
        {
            _totalItemCount = count;
            
            if (_virtualizationMode == ListVirtualizationMode.Disabled)
            {
                // No virtualization - let the list handle all items normally
                _topSpacer.style.height = 0;
                _bottomSpacer.style.height = 0;
                return;
            }
            
            RefreshVirtualization();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Set the height of a specific item (for dynamic height mode)
        /// </summary>
        public void SetItemHeight(int index, float height)
        {
            if (_virtualizationMode != ListVirtualizationMode.Dynamic) return;
            
            _itemHeights[index] = height;
            RefreshVirtualization();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Get the estimated height of an item
        /// </summary>
        public float GetItemHeight(int index)
        {
            if (_virtualizationMode == ListVirtualizationMode.Fixed)
            {
                return _fixedItemHeight;
            }
            
            return _itemHeights.TryGetValue(index, out float height) ? height : _fixedItemHeight;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Handle scroll position changes
        /// </summary>
        public void OnScrollChanged(float scrollPosition)
        {
            _scrollPosition = scrollPosition;
            
            if (!IsVirtualizationEnabled) return;
            
            UpdateVisibleRange();
            UpdateVirtualizedItems();
            
            OnScrollPositionChanged?.Invoke(_firstVisibleIndex, _lastVisibleIndex, scrollPosition);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Refresh the entire virtualization system
        /// </summary>
        public void RefreshVirtualization()
        {
            if (!IsVirtualizationEnabled) return;
            
            ClearVisibleItems();
            UpdateSpacers();
            UpdateVisibleRange();
            UpdateVirtualizedItems();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Clear all virtualized items and return elements to pool
        /// </summary>
        public void ClearVirtualization()
        {
            ClearVisibleItems();
            _itemHeights.Clear();
            _recycledElements.Clear();
            
            _topSpacer.style.height = 0;
            _bottomSpacer.style.height = 0;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Get or create a recycled element for virtualization
        /// </summary>
        public VisualElement GetRecycledElement()
        {
            if (_recycledElements.Count > 0)
            {
                return _recycledElements.Dequeue();
            }
            
            return null;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Return an element to the recycling pool
        /// </summary>
        public void RecycleElement(VisualElement element)
        {
            if (element != null)
            {
                element.RemoveFromHierarchy();
                _recycledElements.Enqueue(element);
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Setup scroll event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            if (_scrollView != null)
            {
                _scrollView.RegisterCallback<GeometryChangedEvent>(OnViewportGeometryChanged);
            }
        }
        
        /// <summary>
        /// AIDEV-NOTE: Handle viewport geometry changes
        /// </summary>
        private void OnViewportGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshVirtualization();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Update the visible item range based on scroll position
        /// </summary>
        private void UpdateVisibleRange()
        {
            if (_totalItemCount == 0) return;
            
            float viewportHeight = _viewport.resolvedStyle.height;
            if (viewportHeight <= 0) return;
            
            float currentY = 0;
            int newFirstVisible = 0;
            int newLastVisible = 0;
            
            // Calculate first visible item
            for (int i = 0; i < _totalItemCount; i++)
            {
                float itemHeight = GetItemHeight(i);
                
                if (currentY + itemHeight > _scrollPosition)
                {
                    newFirstVisible = Mathf.Max(0, i - _bufferItems);
                    break;
                }
                
                currentY += itemHeight;
            }
            
            // Calculate last visible item
            currentY = GetTotalHeightToIndex(newFirstVisible);
            for (int i = newFirstVisible; i < _totalItemCount; i++)
            {
                if (currentY - _scrollPosition > viewportHeight + (_bufferItems * _fixedItemHeight))
                {
                    newLastVisible = i;
                    break;
                }
                
                currentY += GetItemHeight(i);
                newLastVisible = i;
            }
            
            // Ensure we don't exceed max visible items
            if (newLastVisible - newFirstVisible > _maxVisibleItems)
            {
                newLastVisible = newFirstVisible + _maxVisibleItems;
            }
            
            _firstVisibleIndex = newFirstVisible;
            _lastVisibleIndex = Mathf.Min(newLastVisible, _totalItemCount - 1);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Update virtualized items based on visible range
        /// </summary>
        private void UpdateVirtualizedItems()
        {
            // Remove items that are no longer visible
            for (int i = _visibleItems.Count - 1; i >= 0; i--)
            {
                var item = _visibleItems[i];
                if (item.Index < _firstVisibleIndex || item.Index > _lastVisibleIndex)
                {
                    OnItemElementReleased?.Invoke(item.Index, item.Element);
                    RecycleElement(item.Element);
                    _visibleItems.RemoveAt(i);
                }
            }
            
            // Add new visible items
            for (int i = _firstVisibleIndex; i <= _lastVisibleIndex; i++)
            {
                bool itemExists = false;
                foreach (var existingItem in _visibleItems)
                {
                    if (existingItem.Index == i)
                    {
                        itemExists = true;
                        break;
                    }
                }
                
                if (!itemExists)
                {
                    var element = GetRecycledElement();
                    if (element != null)
                    {
                        _itemsContainer.Add(element);
                        OnItemElementRequested?.Invoke(i, element);
                        
                        var virtualItem = new VirtualizedItem(i, element, GetItemHeight(i));
                        _visibleItems.Add(virtualItem);
                    }
                }
            }
            
            UpdateSpacers();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Update spacer heights to maintain scroll position
        /// </summary>
        private void UpdateSpacers()
        {
            float topSpacerHeight = GetTotalHeightToIndex(_firstVisibleIndex);
            float bottomSpacerHeight = GetTotalHeightFromIndex(_lastVisibleIndex + 1);
            
            _topSpacer.style.height = topSpacerHeight;
            _bottomSpacer.style.height = bottomSpacerHeight;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Calculate total height from start to specified index
        /// </summary>
        private float GetTotalHeightToIndex(int index)
        {
            float totalHeight = 0;
            for (int i = 0; i < index && i < _totalItemCount; i++)
            {
                totalHeight += GetItemHeight(i);
            }
            return totalHeight;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Calculate total height from specified index to end
        /// </summary>
        private float GetTotalHeightFromIndex(int index)
        {
            float totalHeight = 0;
            for (int i = index; i < _totalItemCount; i++)
            {
                totalHeight += GetItemHeight(i);
            }
            return totalHeight;
        }
        
        /// <summary>
        /// AIDEV-NOTE: Clear all visible items and return them to pool
        /// </summary>
        private void ClearVisibleItems()
        {
            foreach (var item in _visibleItems)
            {
                OnItemElementReleased?.Invoke(item.Index, item.Element);
                RecycleElement(item.Element);
            }
            
            _visibleItems.Clear();
        }
        
        /// <summary>
        /// AIDEV-NOTE: Dispose of virtualization system
        /// </summary>
        public void Dispose()
        {
            ClearVirtualization();
            
            if (_scrollView != null)
            {
                _scrollView.UnregisterCallback<GeometryChangedEvent>(OnViewportGeometryChanged);
            }
        }
    }
}