using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LogisticGame.Events;

namespace LogisticGame.UI.Notifications
{
    // AIDEV-NOTE: Singleton manager for centralized notification handling
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("Notification Settings")]
        [SerializeField] private int _maxVisibleNotifications = 3;
        [SerializeField] private float _defaultDuration = 4f;
        [SerializeField] private bool _allowDuplicates = false;
        [SerializeField] private float _duplicateCheckWindow = 2f;

        [Header("Audio Settings")]
        [SerializeField] private bool _playAudioCues = true;
        [SerializeField] private float _audioVolume = 0.7f;

        private readonly Queue<Notification> _notificationQueue = new Queue<Notification>();
        private readonly List<Notification> _visibleNotifications = new List<Notification>();
        private readonly List<Notification> _recentNotifications = new List<Notification>();
        
        public event Action<Notification> OnNotificationQueued;
        public event Action<Notification> OnNotificationShown;
        public event Action<Notification> OnNotificationDismissed;

        public int QueuedCount => _notificationQueue.Count;
        public int VisibleCount => _visibleNotifications.Count;
        public int MaxVisibleNotifications => _maxVisibleNotifications;

        private void Awake()
        {
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

        private void InitializeManager()
        {
            EventBus.Subscribe<NotificationRequestedEvent>(HandleNotificationRequest);
            
            EventBus.Subscribe<ContractCompletedEvent>(HandleContractCompleted);
            EventBus.Subscribe<VehiclePurchasedEvent>(HandleVehiclePurchased);
            EventBus.Subscribe<CreditsChangedEvent>(HandleCreditsChanged);
            EventBus.Subscribe<GameSavedEvent>(HandleGameSaved);
            EventBus.Subscribe<GameLoadedEvent>(HandleGameLoaded);

            Debug.Log("[NotificationManager] Initialized and subscribed to events");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                EventBus.Unsubscribe<NotificationRequestedEvent>(HandleNotificationRequest);
                EventBus.Unsubscribe<ContractCompletedEvent>(HandleContractCompleted);
                EventBus.Unsubscribe<VehiclePurchasedEvent>(HandleVehiclePurchased);
                EventBus.Unsubscribe<CreditsChangedEvent>(HandleCreditsChanged);
                EventBus.Unsubscribe<GameSavedEvent>(HandleGameSaved);
                EventBus.Unsubscribe<GameLoadedEvent>(HandleGameLoaded);
                
                Instance = null;
            }
        }

        private void Update()
        {
            ProcessNotificationQueue();
            CheckAutoDismissNotifications();
            CleanupRecentNotifications();
        }

        public void ShowNotification(NotificationType type, string message, float duration = -1f, 
            string[] actionLabels = null, Action[] actionCallbacks = null, bool isPriority = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[NotificationManager] Attempted to show notification with empty message");
                return;
            }

            float actualDuration = duration < 0 ? _defaultDuration : duration;
            var notification = new Notification(type, message, actualDuration, actionLabels, actionCallbacks, isPriority);
            
            QueueNotification(notification);
        }

        public void ShowSuccess(string message, float duration = -1f, bool isPriority = false)
        {
            ShowNotification(NotificationType.Success, message, duration, isPriority: isPriority);
        }

        public void ShowWarning(string message, float duration = -1f, bool isPriority = false)
        {
            ShowNotification(NotificationType.Warning, message, duration, isPriority: isPriority);
        }

        public void ShowError(string message, float duration = -1f, bool isPriority = false)
        {
            ShowNotification(NotificationType.Error, message, duration, isPriority: isPriority);
        }

        public void ShowInfo(string message, float duration = -1f, bool isPriority = false)
        {
            ShowNotification(NotificationType.Info, message, duration, isPriority: isPriority);
        }

        public void ShowProgress(string message, float duration = -1f, bool isPriority = false)
        {
            ShowNotification(NotificationType.Progress, message, duration, isPriority: isPriority);
        }

        public void DismissNotification(string notificationId, bool wasManual = false)
        {
            var notification = _visibleNotifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                DismissNotificationInternal(notification, wasManual);
            }
        }

        public void DismissAllNotifications(bool wasManual = false)
        {
            var notificationsToRemove = _visibleNotifications.ToList();
            foreach (var notification in notificationsToRemove)
            {
                DismissNotificationInternal(notification, wasManual);
            }
        }

        public void ClearQueue()
        {
            int clearedCount = _notificationQueue.Count;
            _notificationQueue.Clear();
            
            if (clearedCount > 0)
            {
                Debug.Log($"[NotificationManager] Cleared {clearedCount} queued notifications");
            }
        }

        public List<Notification> GetVisibleNotifications()
        {
            return new List<Notification>(_visibleNotifications);
        }

        public bool HasVisibleNotifications()
        {
            return _visibleNotifications.Count > 0;
        }

        private void QueueNotification(Notification notification)
        {
            if (!_allowDuplicates && IsDuplicate(notification))
            {
                Debug.Log($"[NotificationManager] Skipping duplicate notification: {notification.Message}");
                return;
            }

            if (notification.IsPriority)
            {
                var queueArray = _notificationQueue.ToArray();
                _notificationQueue.Clear();
                
                _notificationQueue.Enqueue(notification);
                foreach (var queued in queueArray)
                {
                    _notificationQueue.Enqueue(queued);
                }
            }
            else
            {
                _notificationQueue.Enqueue(notification);
            }

            _recentNotifications.Add(notification);
            OnNotificationQueued?.Invoke(notification);
            
            Debug.Log($"[NotificationManager] Queued {notification.GetTypeDisplayName()} notification: {notification.Message}");
        }

        private void ProcessNotificationQueue()
        {
            if (_notificationQueue.Count == 0 || _visibleNotifications.Count >= _maxVisibleNotifications)
                return;

            var notification = _notificationQueue.Dequeue();
            ShowNotificationInternal(notification);
        }

        private void ShowNotificationInternal(Notification notification)
        {
            notification.Show();
            _visibleNotifications.Add(notification);
            OnNotificationShown?.Invoke(notification);
            
            PlayAudioCue(notification.Type);
            Debug.Log($"[NotificationManager] Showing notification: {notification}");
        }

        private void DismissNotificationInternal(Notification notification, bool wasManual)
        {
            if (_visibleNotifications.Remove(notification))
            {
                notification.Dismiss(wasManual);
                OnNotificationDismissed?.Invoke(notification);
                
                Debug.Log($"[NotificationManager] Dismissed notification: {notification.Id} (Manual: {wasManual})");
            }
        }

        private void CheckAutoDismissNotifications()
        {
            var notificationsToRemove = _visibleNotifications.Where(n => n.ShouldAutoDismiss()).ToList();
            foreach (var notification in notificationsToRemove)
            {
                DismissNotificationInternal(notification, false);
            }
        }

        private void CleanupRecentNotifications()
        {
            float currentTime = Time.unscaledTime;
            _recentNotifications.RemoveAll(n => (currentTime - n.TimeCreated) > _duplicateCheckWindow);
        }

        private bool IsDuplicate(Notification newNotification)
        {
            return _recentNotifications.Any(n => 
                n.Type == newNotification.Type && 
                n.Message == newNotification.Message &&
                (Time.unscaledTime - n.TimeCreated) < _duplicateCheckWindow);
        }

        private void PlayAudioCue(NotificationType type)
        {
            if (!_playAudioCues)
                return;

            // AIDEV-NOTE: Audio cues will be implemented when AssetManager audio system is ready
            // string audioClipName = GetAudioClipName(type);
            // AssetManager.Instance.PlayUISound(audioClipName, _audioVolume);
        }

        private string GetAudioClipName(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => "notification-success",
                NotificationType.Warning => "notification-warning",
                NotificationType.Error => "notification-error",
                NotificationType.Info => "notification-info",
                NotificationType.Progress => "notification-progress",
                _ => "notification-default"
            };
        }

        private void HandleNotificationRequest(NotificationRequestedEvent eventData)
        {
            var notification = Notification.FromEvent(eventData);
            QueueNotification(notification);
        }

        private void HandleContractCompleted(ContractCompletedEvent eventData)
        {
            ShowSuccess($"Contract completed! Earned ${eventData.Reward:F0}", isPriority: true);
        }

        private void HandleVehiclePurchased(VehiclePurchasedEvent eventData)
        {
            ShowSuccess($"Vehicle purchased for ${eventData.Cost:F0}");
        }

        private void HandleCreditsChanged(CreditsChangedEvent eventData)
        {
            if (eventData.NewAmount <= 100f && eventData.Change < 0)
            {
                ShowWarning("Low credits! Consider completing contracts to earn more.", isPriority: true);
            }
        }

        private void HandleGameSaved(GameSavedEvent eventData)
        {
            if (eventData.Success)
            {
                ShowInfo($"Game saved to {eventData.SlotName}");
            }
            else
            {
                ShowError($"Failed to save game to {eventData.SlotName}");
            }
        }

        private void HandleGameLoaded(GameLoadedEvent eventData)
        {
            if (eventData.Success)
            {
                ShowInfo($"Game loaded from {eventData.SlotName}");
            }
            else
            {
                ShowError($"Failed to load game from {eventData.SlotName}");
            }
        }
    }
}