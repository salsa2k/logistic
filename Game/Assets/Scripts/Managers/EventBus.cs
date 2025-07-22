using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogisticGame.Events
{
    // AIDEV-NOTE: Base interface for all events in the game
    public interface IGameEvent { }

    // AIDEV-NOTE: Common game events
    public struct GameStartedEvent : IGameEvent { }
    public struct GameEndedEvent : IGameEvent { }
    public struct GamePausedEvent : IGameEvent
    {
        public bool IsPaused { get; }
        public GamePausedEvent(bool isPaused) => IsPaused = isPaused;
    }

    public struct CreditsChangedEvent : IGameEvent
    {
        public float NewAmount { get; }
        public float Change { get; }
        public CreditsChangedEvent(float newAmount, float change)
        {
            NewAmount = newAmount;
            Change = change;
        }
    }

    // AIDEV-NOTE: Save system related events
    public struct SaveGameRequestedEvent : IGameEvent 
    { 
        public string SlotName { get; }
        public string Reason { get; }
        public SaveGameRequestedEvent(string slotName, string reason)
        {
            SlotName = slotName;
            Reason = reason;
        }
    }

    public struct GameSavedEvent : IGameEvent 
    { 
        public string SlotName { get; }
        public bool Success { get; }
        public GameSavedEvent(string slotName, bool success)
        {
            SlotName = slotName;
            Success = success;
        }
    }

    public struct GameLoadedEvent : IGameEvent 
    { 
        public string SlotName { get; }
        public bool Success { get; }
        public GameLoadedEvent(string slotName, bool success)
        {
            SlotName = slotName;
            Success = success;
        }
    }

    // AIDEV-NOTE: Contract related events (for future auto-save triggers)
    public struct ContractCompletedEvent : IGameEvent
    {
        public string ContractId { get; }
        public float Reward { get; }
        public ContractCompletedEvent(string contractId, float reward)
        {
            ContractId = contractId;
            Reward = reward;
        }
    }

    public struct VehiclePurchasedEvent : IGameEvent
    {
        public string VehicleId { get; }
        public float Cost { get; }
        public VehiclePurchasedEvent(string vehicleId, float cost)
        {
            VehicleId = vehicleId;
            Cost = cost;
        }
    }

    public struct CityDiscoveredEvent : IGameEvent
    {
        public string CityName { get; }
        public CityDiscoveredEvent(string cityName)
        {
            CityName = cityName;
        }
    }

    // AIDEV-NOTE: UI Window related events for BaseWindow integration
    public struct WindowOpenedEvent : IGameEvent
    {
        public string WindowName { get; }
        public bool IsModal { get; }
        public WindowOpenedEvent(string windowName, bool isModal)
        {
            WindowName = windowName;
            IsModal = isModal;
        }
    }

    public struct WindowClosedEvent : IGameEvent
    {
        public string WindowName { get; }
        public bool IsModal { get; }
        public WindowClosedEvent(string windowName, bool isModal)
        {
            WindowName = windowName;
            IsModal = isModal;
        }
    }

    // AIDEV-NOTE: Modal-specific events for BaseModal and ModalManager
    public struct ModalOpenedEvent : IGameEvent
    {
        public string ModalName { get; }
        public ModalSize ModalSize { get; }
        public ModalOpenedEvent(string modalName, ModalSize modalSize)
        {
            ModalName = modalName;
            ModalSize = modalSize;
        }
    }

    public struct ModalClosedEvent : IGameEvent
    {
        public string ModalName { get; }
        public ModalSize ModalSize { get; }
        public ModalClosedEvent(string modalName, ModalSize modalSize)
        {
            ModalName = modalName;
            ModalSize = modalSize;
        }
    }

    public struct ModalStackChangedEvent : IGameEvent
    {
        public int StackDepth { get; }
        public string ModalName { get; }
        public bool WasPushed { get; }
        public ModalStackChangedEvent(int stackDepth, string modalName, bool wasPushed)
        {
            StackDepth = stackDepth;
            ModalName = modalName;
            WasPushed = wasPushed;
        }
    }

    public struct AllModalsClosedEvent : IGameEvent { }

    // AIDEV-NOTE: Notification system events
    public enum NotificationType
    {
        Success,
        Warning,
        Error,
        Info,
        Progress
    }

    public struct NotificationRequestedEvent : IGameEvent
    {
        public NotificationType Type { get; }
        public string Message { get; }
        public float Duration { get; }
        public string[] ActionLabels { get; }
        public System.Action[] ActionCallbacks { get; }
        public bool IsPriority { get; }
        
        public NotificationRequestedEvent(NotificationType type, string message, float duration = 4f, 
            string[] actionLabels = null, System.Action[] actionCallbacks = null, bool isPriority = false)
        {
            Type = type;
            Message = message;
            Duration = duration;
            ActionLabels = actionLabels;
            ActionCallbacks = actionCallbacks;
            IsPriority = isPriority;
        }
    }

    public struct NotificationShownEvent : IGameEvent
    {
        public string NotificationId { get; }
        public NotificationType Type { get; }
        public NotificationShownEvent(string notificationId, NotificationType type)
        {
            NotificationId = notificationId;
            Type = type;
        }
    }

    public struct NotificationDismissedEvent : IGameEvent
    {
        public string NotificationId { get; }
        public bool WasManual { get; }
        public NotificationDismissedEvent(string notificationId, bool wasManual)
        {
            NotificationId = notificationId;
            WasManual = wasManual;
        }
    }

    public struct NotificationActionTriggeredEvent : IGameEvent
    {
        public string NotificationId { get; }
        public int ActionIndex { get; }
        public NotificationActionTriggeredEvent(string notificationId, int actionIndex)
        {
            NotificationId = notificationId;
            ActionIndex = actionIndex;
        }
    }

    // AIDEV-NOTE: Card-related events for BaseCard system
    public struct CardDataBoundEvent : IGameEvent
    {
        public string CardId { get; }
        public string DataType { get; }
        public CardDataBoundEvent(string cardId, string dataType)
        {
            CardId = cardId;
            DataType = dataType;
        }
    }

    public struct CardSelectedEvent : IGameEvent
    {
        public string CardId { get; }
        public CardSelectedEvent(string cardId) => CardId = cardId;
    }

    public struct CardDeselectedEvent : IGameEvent
    {
        public string CardId { get; }
        public CardDeselectedEvent(string cardId) => CardId = cardId;
    }

    public struct CardActionEvent : IGameEvent
    {
        public string CardId { get; }
        public string ActionType { get; }
        public CardActionEvent(string cardId, string actionType)
        {
            CardId = cardId;
            ActionType = actionType;
        }
    }

    // AIDEV-NOTE: Global event bus for decoupled communication between systems
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<object>> _eventHandlers = new Dictionary<Type, List<object>>();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<object>();
            }
            
            _eventHandlers[eventType].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Remove(handler);
                
                if (_eventHandlers[eventType].Count == 0)
                {
                    _eventHandlers.Remove(eventType);
                }
            }
        }

        public static void Publish<T>(T eventData) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (_eventHandlers.ContainsKey(eventType))
            {
                // Create a copy to avoid modification during iteration
                var handlers = new List<object>(_eventHandlers[eventType]);
                
                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Action<T> typedHandler)
                        {
                            typedHandler.Invoke(eventData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error executing event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        public static void Clear()
        {
            _eventHandlers.Clear();
        }

        public static void Clear<T>() where T : IGameEvent
        {
            Type eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers.Remove(eventType);
            }
        }

        public static int GetSubscriberCount<T>() where T : IGameEvent
        {
            Type eventType = typeof(T);
            return _eventHandlers.ContainsKey(eventType) ? _eventHandlers[eventType].Count : 0;
        }
    }
}