using UnityEngine;
using LogisticGame.UI.Notifications;
using LogisticGame.Events;

namespace LogisticGame.Test
{
    // AIDEV-NOTE: Test script for notification system functionality
    public class NotificationSystemTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _runTestsOnStart = false;
        [SerializeField] private float _testInterval = 2f;

        private NotificationManager _notificationManager;
        private int _testCounter = 0;

        private void Start()
        {
            if (_runTestsOnStart)
            {
                _notificationManager = NotificationManager.Instance;
                if (_notificationManager != null)
                {
                    InvokeRepeating(nameof(RunRandomTest), 1f, _testInterval);
                }
                else
                {
                    Debug.LogError("[NotificationSystemTest] NotificationManager not found!");
                }
            }
        }

        [ContextMenu("Test All Notification Types")]
        public void TestAllNotificationTypes()
        {
            if (_notificationManager == null)
                _notificationManager = NotificationManager.Instance;

            if (_notificationManager == null)
            {
                Debug.LogError("[NotificationSystemTest] NotificationManager not found!");
                return;
            }

            _notificationManager.ShowSuccess("✅ Contract completed successfully! Earned $1,200", 4f);
            _notificationManager.ShowWarning("⚠️ Vehicle fuel is running low (15% remaining)", 5f);
            _notificationManager.ShowError("❌ Insufficient credits to purchase this vehicle", 6f);
            _notificationManager.ShowInfo("ℹ️ New city discovered: Springfield", 3f);
            _notificationManager.ShowProgress("🚛 Vehicle en route to destination (ETA: 5 min)", 8f);
        }

        [ContextMenu("Test Priority Notifications")]
        public void TestPriorityNotifications()
        {
            if (_notificationManager == null)
                _notificationManager = NotificationManager.Instance;

            if (_notificationManager == null) return;

            _notificationManager.ShowInfo("Regular notification 1", 5f);
            _notificationManager.ShowInfo("Regular notification 2", 5f);
            _notificationManager.ShowError("🚨 URGENT: Police fine issued!", 10f, isPriority: true);
            _notificationManager.ShowInfo("Regular notification 3", 5f);
        }

        [ContextMenu("Test Action Notifications")]
        public void TestActionNotifications()
        {
            if (_notificationManager == null)
                _notificationManager = NotificationManager.Instance;

            if (_notificationManager == null) return;

            string[] actionLabels = { "View Details", "Dismiss" };
            System.Action[] actionCallbacks = {
                () => Debug.Log("View Details clicked!"),
                () => Debug.Log("Dismiss clicked!")
            };

            _notificationManager.ShowNotification(NotificationType.Warning, "Low fuel detected. Refuel recommended.", 10f, 
                actionLabels, actionCallbacks);
        }

        [ContextMenu("Test Event-Based Notifications")]
        public void TestEventBasedNotifications()
        {
            EventBus.Publish(new ContractCompletedEvent("CONTRACT_001", 1500f));
            EventBus.Publish(new VehiclePurchasedEvent("TRUCK_BASIC", 25000f));
            EventBus.Publish(new CreditsChangedEvent(50f, -500f)); // Should trigger low credits warning
            EventBus.Publish(new GameSavedEvent("SaveSlot1", true));
            EventBus.Publish(new CityDiscoveredEvent("New Haven"));
        }

        [ContextMenu("Test Notification Queue")]
        public void TestNotificationQueue()
        {
            if (_notificationManager == null)
                _notificationManager = NotificationManager.Instance;

            if (_notificationManager == null) return;

            for (int i = 1; i <= 8; i++)
            {
                _notificationManager.ShowInfo($"Queued notification #{i}", 3f);
            }
        }

        [ContextMenu("Test Long Message")]
        public void TestLongMessage()
        {
            if (_notificationManager == null)
                _notificationManager = NotificationManager.Instance;

            if (_notificationManager == null) return;

            string longMessage = "This is a very long notification message that should test the text wrapping " +
                               "and layout capabilities of the notification system. It contains multiple lines " +
                               "of text to ensure proper display and readability.";

            _notificationManager.ShowInfo(longMessage, 8f);
        }

        [ContextMenu("Clear All Notifications")]
        public void ClearAllNotifications()
        {
            if (_notificationManager == null)
                _notificationManager = NotificationManager.Instance;

            if (_notificationManager == null) return;

            _notificationManager.DismissAllNotifications(true);
        }

        [ContextMenu("Clear Notification Queue")]
        public void ClearNotificationQueue()
        {
            if (_notificationManager == null)
                _notificationManager = NotificationManager.Instance;

            if (_notificationManager == null) return;

            _notificationManager.ClearQueue();
        }

        private void RunRandomTest()
        {
            if (_notificationManager == null) return;

            _testCounter++;
            var testType = (NotificationType)(Random.Range(0, 5));
            string message = GetRandomMessage(testType, _testCounter);

            switch (testType)
            {
                case NotificationType.Success:
                    _notificationManager.ShowSuccess(message);
                    break;
                case NotificationType.Warning:
                    _notificationManager.ShowWarning(message);
                    break;
                case NotificationType.Error:
                    _notificationManager.ShowError(message);
                    break;
                case NotificationType.Info:
                    _notificationManager.ShowInfo(message);
                    break;
                case NotificationType.Progress:
                    _notificationManager.ShowProgress(message);
                    break;
            }
        }

        private string GetRandomMessage(NotificationType type, int counter)
        {
            return type switch
            {
                NotificationType.Success => $"✅ Success #{counter}: Task completed successfully!",
                NotificationType.Warning => $"⚠️ Warning #{counter}: Check vehicle status",
                NotificationType.Error => $"❌ Error #{counter}: Operation failed",
                NotificationType.Info => $"ℹ️ Info #{counter}: System update available",
                NotificationType.Progress => $"🔄 Progress #{counter}: Loading data...",
                _ => $"Test notification #{counter}"
            };
        }

        private void OnGUI()
        {
            if (_notificationManager == null) return;

            GUI.Box(new Rect(10, 10, 200, 120), "Notification System Test");
            
            if (GUI.Button(new Rect(20, 35, 180, 20), "Test All Types"))
                TestAllNotificationTypes();
                
            if (GUI.Button(new Rect(20, 60, 180, 20), "Test Priority"))
                TestPriorityNotifications();
                
            if (GUI.Button(new Rect(20, 85, 180, 20), "Test Actions"))
                TestActionNotifications();
                
            if (GUI.Button(new Rect(20, 110, 180, 20), "Clear All"))
                ClearAllNotifications();

            string status = $"Queue: {_notificationManager.QueuedCount} | " +
                          $"Visible: {_notificationManager.VisibleCount}";
            GUI.Label(new Rect(20, 135, 180, 20), status);
        }
    }
}