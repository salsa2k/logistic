using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;
using LogisticGame.Events;

/// <summary>
/// AIDEV-NOTE: Manages modal dialog stack, layering, and global modal behaviors.
/// Handles multiple overlapping modals, focus management, and keyboard interactions.
/// </summary>
public class ModalManager : MonoBehaviour
{
    [Header("Modal Management Settings")]
    [SerializeField] private int _maxModalStack = 5;
    [SerializeField] private bool _enableGlobalEscapeKey = true;
    [SerializeField] private bool _enableGlobalBackdropClose = true;
    [SerializeField] private float _stackZIndexIncrement = 100f;
    
    [Header("Focus Management")]
    [SerializeField] private bool _manageFocusAutomatically = true;
    [SerializeField] private bool _restoreFocusOnClose = true;
    
    // Singleton instance
    public static ModalManager Instance { get; private set; }
    
    // Modal stack and management
    private List<BaseModal> _modalStack = new List<BaseModal>();
    private List<BaseModal> _registeredModals = new List<BaseModal>();
    private Stack<Focusable> _focusStack = new Stack<Focusable>();
    
    // Events
    public event Action<BaseModal> OnModalPushed;
    public event Action<BaseModal> OnModalPopped;
    public event Action OnAllModalsClosed;
    public event Action<int> OnStackDepthChanged;
    
    // Properties
    public int StackDepth => _modalStack.Count;
    public BaseModal TopModal => _modalStack.Count > 0 ? _modalStack[_modalStack.Count - 1] : null;
    public bool HasActiveModals => _modalStack.Count > 0;
    public IReadOnlyList<BaseModal> ActiveModals => _modalStack.AsReadOnly();
    
    private void Awake()
    {
        // AIDEV-NOTE: Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        HandleGlobalInput();
    }
    
    private void OnDestroy()
    {
        // AIDEV-NOTE: Clean up event listeners and references
        CleanupManager();
    }
    
    /// <summary>
    /// Initializes the modal manager and sets up event listeners.
    /// AIDEV-NOTE: Sets up global input handling and event system integration.
    /// </summary>
    private void InitializeManager()
    {
        Debug.Log("ModalManager: Initialized modal management system");
        
        // Subscribe to relevant events
        EventBus.Subscribe<WindowOpenedEvent>(OnWindowOpened);
        EventBus.Subscribe<WindowClosedEvent>(OnWindowClosed);
    }
    
    /// <summary>
    /// Cleans up the modal manager resources.
    /// AIDEV-NOTE: Unsubscribes from events and clears references.
    /// </summary>
    private void CleanupManager()
    {
        EventBus.Unsubscribe<WindowOpenedEvent>(OnWindowOpened);
        EventBus.Unsubscribe<WindowClosedEvent>(OnWindowClosed);
        
        _modalStack.Clear();
        _registeredModals.Clear();
        _focusStack.Clear();
    }
    
    /// <summary>
    /// Registers a modal with the manager for tracking and management.
    /// AIDEV-NOTE: Call this method when creating modal instances.
    /// </summary>
    /// <param name="modal">The modal to register</param>
    public void RegisterModal(BaseModal modal)
    {
        if (modal == null)
        {
            Debug.LogWarning("ModalManager: Attempted to register null modal");
            return;
        }
        
        if (!_registeredModals.Contains(modal))
        {
            _registeredModals.Add(modal);
            Debug.Log($"ModalManager: Registered modal '{modal.name}'");
        }
    }
    
    /// <summary>
    /// Unregisters a modal from the manager.
    /// AIDEV-NOTE: Call this method when destroying modal instances.
    /// </summary>
    /// <param name="modal">The modal to unregister</param>
    public void UnregisterModal(BaseModal modal)
    {
        if (modal == null) return;
        
        // Remove from registered modals
        _registeredModals.Remove(modal);
        
        // Remove from active stack if present
        if (_modalStack.Contains(modal))
        {
            PopModal(modal);
        }
        
        Debug.Log($"ModalManager: Unregistered modal '{modal.name}'");
    }
    
    /// <summary>
    /// Pushes a modal onto the modal stack, making it the active modal.
    /// AIDEV-NOTE: Handles z-index layering and focus management automatically.
    /// </summary>
    /// <param name="modal">The modal to push onto the stack</param>
    public void PushModal(BaseModal modal)
    {
        if (modal == null)
        {
            Debug.LogWarning("ModalManager: Attempted to push null modal onto stack");
            return;
        }
        
        if (_modalStack.Contains(modal))
        {
            Debug.LogWarning($"ModalManager: Modal '{modal.name}' is already on the stack");
            return;
        }
        
        // Check stack limit
        if (_modalStack.Count >= _maxModalStack)
        {
            Debug.LogWarning($"ModalManager: Modal stack limit ({_maxModalStack}) reached. Cannot push modal '{modal.name}'");
            return;
        }
        
        // Store focus before showing modal
        if (_manageFocusAutomatically && _restoreFocusOnClose)
        {
            StoreFocus();
        }
        
        // Add to stack
        _modalStack.Add(modal);
        
        // Apply z-index layering
        ApplyModalLayering();
        
        OnModalPushed?.Invoke(modal);
        OnStackDepthChanged?.Invoke(_modalStack.Count);
        
        Debug.Log($"ModalManager: Pushed modal '{modal.name}' onto stack. Stack depth: {_modalStack.Count}");
        
        // Publish stack changed event
        EventBus.Publish(new ModalStackChangedEvent(_modalStack.Count, modal.name, true));
    }
    
    /// <summary>
    /// Pops a modal from the stack, typically when it's closed.
    /// AIDEV-NOTE: Handles focus restoration and stack management automatically.
    /// </summary>
    /// <param name="modal">The modal to remove from the stack</param>
    public void PopModal(BaseModal modal)
    {
        if (modal == null || !_modalStack.Contains(modal))
        {
            return;
        }
        
        // Remove from stack
        _modalStack.Remove(modal);
        
        // Restore focus if this was the top modal
        if (_manageFocusAutomatically && _restoreFocusOnClose && _modalStack.Count == 0)
        {
            RestoreFocus();
        }
        
        // Apply updated layering
        ApplyModalLayering();
        
        OnModalPopped?.Invoke(modal);
        OnStackDepthChanged?.Invoke(_modalStack.Count);
        
        Debug.Log($"ModalManager: Popped modal '{modal.name}' from stack. Stack depth: {_modalStack.Count}");
        
        // Check if all modals are closed
        if (_modalStack.Count == 0)
        {
            OnAllModalsClosed?.Invoke();
            EventBus.Publish(new AllModalsClosedEvent());
        }
        
        // Publish stack changed event
        EventBus.Publish(new ModalStackChangedEvent(_modalStack.Count, modal.name, false));
    }
    
    /// <summary>
    /// Closes the top modal in the stack.
    /// AIDEV-NOTE: Convenient method for closing the most recently opened modal.
    /// </summary>
    public void CloseTopModal()
    {
        var topModal = TopModal;
        if (topModal != null)
        {
            topModal.HideWindow();
        }
    }
    
    /// <summary>
    /// Closes all active modals in the stack.
    /// AIDEV-NOTE: Useful for emergency cleanup or scene transitions.
    /// </summary>
    public void CloseAllModals()
    {
        Debug.Log($"ModalManager: Closing all {_modalStack.Count} active modals");
        
        // Create a copy to iterate over since the original list will be modified
        var modalsToClose = new List<BaseModal>(_modalStack);
        
        foreach (var modal in modalsToClose)
        {
            modal.HideWindow();
        }
    }
    
    /// <summary>
    /// Applies z-index layering to maintain proper modal stacking order.
    /// AIDEV-NOTE: Ensures newest modals appear above older ones.
    /// </summary>
    private void ApplyModalLayering()
    {
        for (int i = 0; i < _modalStack.Count; i++)
        {
            var modal = _modalStack[i];
            if (modal != null)
            {
                // Calculate z-index based on stack position
                var stackLevel = i + 1;
                var className = $"modal-stack-{stackLevel}";
                
                // Remove existing stack classes
                var rootElement = modal.GetComponent<UIDocument>()?.rootVisualElement;
                if (rootElement != null)
                {
                    for (int j = 1; j <= _maxModalStack; j++)
                    {
                        rootElement.RemoveFromClassList($"modal-stack-{j}");
                    }
                    
                    // Add current stack class
                    rootElement.AddToClassList(className);
                }
            }
        }
    }
    
    /// <summary>
    /// Handles global input for all active modals.
    /// AIDEV-NOTE: Processes ESC key and other global modal shortcuts.
    /// </summary>
    private void HandleGlobalInput()
    {
        if (!HasActiveModals) return;
        
        // Handle global escape key
        if (_enableGlobalEscapeKey && Input.GetKeyDown(KeyCode.Escape))
        {
            var topModal = TopModal;
            if (topModal != null && topModal.CloseOnEscapeKey)
            {
                topModal.HideWindow();
            }
        }
    }
    
    /// <summary>
    /// Stores the current focus for later restoration.
    /// AIDEV-NOTE: Part of accessibility and UX focus management.
    /// </summary>
    private void StoreFocus()
    {
        // AIDEV-TODO: Implement focus storage when UI Toolkit focus management is available
        // For now, this is a placeholder for future focus management
        Debug.Log("ModalManager: Focus stored (placeholder implementation)");
    }
    
    /// <summary>
    /// Restores previously stored focus.
    /// AIDEV-NOTE: Restores focus to element that was focused before modal opened.
    /// </summary>
    private void RestoreFocus()
    {
        // AIDEV-TODO: Implement focus restoration when UI Toolkit focus management is available
        Debug.Log("ModalManager: Focus restored (placeholder implementation)");
    }
    
    /// <summary>
    /// Gets modal by name from registered modals.
    /// AIDEV-NOTE: Utility method for finding specific modals.
    /// </summary>
    /// <param name="modalName">Name of the modal to find</param>
    /// <returns>The modal if found, null otherwise</returns>
    public BaseModal GetModalByName(string modalName)
    {
        return _registeredModals.FirstOrDefault(m => m != null && m.name == modalName);
    }
    
    /// <summary>
    /// Gets all modals of a specific type.
    /// AIDEV-NOTE: Useful for managing modals by category or functionality.
    /// </summary>
    /// <typeparam name="T">The modal type to search for</typeparam>
    /// <returns>List of modals of the specified type</returns>
    public List<T> GetModalsByType<T>() where T : BaseModal
    {
        return _registeredModals.OfType<T>().ToList();
    }
    
    /// <summary>
    /// Checks if a specific modal is currently active (on the stack).
    /// AIDEV-NOTE: Useful for preventing duplicate modal opens.
    /// </summary>
    /// <param name="modal">The modal to check</param>
    /// <returns>True if the modal is active, false otherwise</returns>
    public bool IsModalActive(BaseModal modal)
    {
        return modal != null && _modalStack.Contains(modal);
    }
    
    /// <summary>
    /// Checks if any modal of a specific type is currently active.
    /// AIDEV-NOTE: Prevents opening multiple instances of the same modal type.
    /// </summary>
    /// <typeparam name="T">The modal type to check</typeparam>
    /// <returns>True if any modal of the type is active, false otherwise</returns>
    public bool IsModalTypeActive<T>() where T : BaseModal
    {
        return _modalStack.OfType<T>().Any();
    }
    
    /// <summary>
    /// Event handler for window opened events.
    /// AIDEV-NOTE: Integrates with the general window event system.
    /// </summary>
    /// <param name="evt">The window opened event</param>
    private void OnWindowOpened(WindowOpenedEvent evt)
    {
        // AIDEV-NOTE: This handles integration with BaseWindow events
        // Modals automatically register themselves, so no additional action needed here
    }
    
    /// <summary>
    /// Event handler for window closed events.
    /// AIDEV-NOTE: Ensures modal stack consistency when windows are closed.
    /// </summary>
    /// <param name="evt">The window closed event</param>
    private void OnWindowClosed(WindowClosedEvent evt)
    {
        // AIDEV-NOTE: Modals automatically unregister themselves from the stack
        // This is mainly for consistency checking
    }
    
    #if UNITY_EDITOR
    [Header("Debug Info")]
    [SerializeField] private bool _showDebugInfo = false;
    
    /// <summary>
    /// AIDEV-NOTE: Debug method to display current modal stack state.
    /// </summary>
    [ContextMenu("Print Modal Stack (Debug)")]
    private void DebugPrintModalStack()
    {
        if (_showDebugInfo)
        {
            Debug.Log($"ModalManager Debug - Stack Depth: {_modalStack.Count}/{_maxModalStack}");
            for (int i = 0; i < _modalStack.Count; i++)
            {
                var modal = _modalStack[i];
                Debug.Log($"  [{i}] {modal?.name ?? "NULL"} - Visible: {modal?.IsVisible}");
            }
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method to display all registered modals.
    /// </summary>
    [ContextMenu("Print Registered Modals (Debug)")]
    private void DebugPrintRegisteredModals()
    {
        if (_showDebugInfo)
        {
            Debug.Log($"ModalManager Debug - Registered Modals: {_registeredModals.Count}");
            for (int i = 0; i < _registeredModals.Count; i++)
            {
                var modal = _registeredModals[i];
                var isActive = IsModalActive(modal);
                Debug.Log($"  [{i}] {modal?.name ?? "NULL"} - Active: {isActive}");
            }
        }
    }
    
    /// <summary>
    /// AIDEV-NOTE: Debug method to force close all modals.
    /// </summary>
    [ContextMenu("Force Close All Modals (Debug)")]
    private void DebugForceCloseAllModals()
    {
        if (_showDebugInfo)
        {
            Debug.Log("ModalManager Debug: Force closing all modals");
            CloseAllModals();
        }
    }
    #endif
}