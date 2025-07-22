using System;
using UnityEngine;
using LogisticGame.Events;

namespace LogisticGame.UI.Notifications
{
    // AIDEV-NOTE: Individual notification data container and behavior class
    [System.Serializable]
    public class Notification
    {
        public string Id { get; private set; }
        public NotificationType Type { get; private set; }
        public string Message { get; private set; }
        public float Duration { get; private set; }
        public string[] ActionLabels { get; private set; }
        public Action[] ActionCallbacks { get; private set; }
        public bool IsPriority { get; private set; }
        public bool IsVisible { get; set; }
        public bool IsDismissed { get; private set; }
        public float TimeCreated { get; private set; }
        public float TimeShown { get; private set; }

        private const float DEFAULT_DURATION = 4f;

        public Notification(NotificationType type, string message, float duration = DEFAULT_DURATION, 
            string[] actionLabels = null, Action[] actionCallbacks = null, bool isPriority = false)
        {
            Id = System.Guid.NewGuid().ToString();
            Type = type;
            Message = message ?? string.Empty;
            Duration = Mathf.Max(0.5f, duration);
            ActionLabels = actionLabels ?? new string[0];
            ActionCallbacks = actionCallbacks ?? new Action[0];
            IsPriority = isPriority;
            IsVisible = false;
            IsDismissed = false;
            TimeCreated = Time.unscaledTime;
            TimeShown = 0f;

            ValidateActions();
        }

        public static Notification FromEvent(NotificationRequestedEvent eventData)
        {
            return new Notification(
                eventData.Type,
                eventData.Message,
                eventData.Duration,
                eventData.ActionLabels,
                eventData.ActionCallbacks,
                eventData.IsPriority
            );
        }

        public void Show()
        {
            if (!IsVisible && !IsDismissed)
            {
                IsVisible = true;
                TimeShown = Time.unscaledTime;
                EventBus.Publish(new NotificationShownEvent(Id, Type));
            }
        }

        public void Dismiss(bool wasManual = false)
        {
            if (!IsDismissed)
            {
                IsDismissed = true;
                IsVisible = false;
                EventBus.Publish(new NotificationDismissedEvent(Id, wasManual));
            }
        }

        public void TriggerAction(int actionIndex)
        {
            if (actionIndex >= 0 && actionIndex < ActionCallbacks.Length && ActionCallbacks[actionIndex] != null)
            {
                try
                {
                    ActionCallbacks[actionIndex].Invoke();
                    EventBus.Publish(new NotificationActionTriggeredEvent(Id, actionIndex));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error executing notification action {actionIndex} for notification {Id}: {ex.Message}");
                }
            }
        }

        public bool ShouldAutoDismiss()
        {
            return IsVisible && !IsDismissed && Duration > 0f && 
                   (Time.unscaledTime - TimeShown) >= Duration;
        }

        public float GetRemainingTime()
        {
            if (!IsVisible || IsDismissed || Duration <= 0f)
                return 0f;
                
            float elapsed = Time.unscaledTime - TimeShown;
            return Mathf.Max(0f, Duration - elapsed);
        }

        public float GetElapsedTime()
        {
            if (!IsVisible)
                return 0f;
                
            return Time.unscaledTime - TimeShown;
        }

        public bool HasActions()
        {
            return ActionLabels != null && ActionLabels.Length > 0;
        }

        public string GetTypeDisplayName()
        {
            return Type switch
            {
                NotificationType.Success => "Success",
                NotificationType.Warning => "Warning", 
                NotificationType.Error => "Error",
                NotificationType.Info => "Info",
                NotificationType.Progress => "Progress",
                _ => "Unknown"
            };
        }

        public Color GetTypeColor()
        {
            return Type switch
            {
                NotificationType.Success => new Color(0.298f, 0.686f, 0.314f, 1f), // #4CAF50
                NotificationType.Warning => new Color(1f, 0.596f, 0f, 1f),        // #FF9800
                NotificationType.Error => new Color(0.957f, 0.263f, 0.212f, 1f),  // #F44336
                NotificationType.Info => new Color(0.129f, 0.588f, 0.953f, 1f),   // #2196F3
                NotificationType.Progress => new Color(1f, 0.843f, 0f, 1f),       // #FFD700
                _ => Color.white
            };
        }

        public string GetIconName()
        {
            return Type switch
            {
                NotificationType.Success => "icon-check-circle",
                NotificationType.Warning => "icon-warning-triangle",
                NotificationType.Error => "icon-error-circle",
                NotificationType.Info => "icon-info-circle",
                NotificationType.Progress => "icon-clock",
                _ => "icon-circle"
            };
        }

        private void ValidateActions()
        {
            if (ActionLabels.Length != ActionCallbacks.Length)
            {
                Debug.LogWarning($"Notification {Id}: Action labels and callbacks count mismatch. " +
                               $"Labels: {ActionLabels.Length}, Callbacks: {ActionCallbacks.Length}");
                
                int minLength = Mathf.Min(ActionLabels.Length, ActionCallbacks.Length);
                var newLabels = new string[minLength];
                var newCallbacks = new Action[minLength];
                Array.Copy(ActionLabels, newLabels, minLength);
                Array.Copy(ActionCallbacks, newCallbacks, minLength);
                ActionLabels = newLabels;
                ActionCallbacks = newCallbacks;
            }

            for (int i = 0; i < ActionLabels.Length; i++)
            {
                if (string.IsNullOrEmpty(ActionLabels[i]))
                {
                    ActionLabels[i] = $"Action {i + 1}";
                }
            }
        }

        public override string ToString()
        {
            return $"Notification[{Id}] {Type}: {Message} (Priority: {IsPriority}, Duration: {Duration}s)";
        }

        public override bool Equals(object obj)
        {
            return obj is Notification other && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}