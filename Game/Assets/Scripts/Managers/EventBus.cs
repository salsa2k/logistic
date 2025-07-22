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