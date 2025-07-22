using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace LogisticGame.UI.Notifications
{
    // AIDEV-NOTE: UI controller for notification display and animations
    public class NotificationController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset _notificationTemplate;

        [Header("Animation Settings")]
        [SerializeField] private float _showAnimationDuration = 0.3f;
        [SerializeField] private float _dismissAnimationDuration = 0.5f;
        [SerializeField] private float _stackSpacing = 60f;

        [Header("Interaction Settings")]
        [SerializeField] private bool _pauseOnHover = true;
        [SerializeField] private bool _enableKeyboardDismiss = true;

        private VisualElement _rootElement;
        private readonly Dictionary<string, NotificationView> _activeViews = new Dictionary<string, NotificationView>();
        private readonly Queue<NotificationView> _viewPool = new Queue<NotificationView>();
        
        private NotificationManager _notificationManager;

        private void Awake()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            InitializeUI();
        }

        private void Start()
        {
            _notificationManager = NotificationManager.Instance;
            if (_notificationManager != null)
            {
                SubscribeToNotificationEvents();
            }
            else
            {
                Debug.LogError("[NotificationController] NotificationManager instance not found!");
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromNotificationEvents();
        }

        private void InitializeUI()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("[NotificationController] UIDocument is null!");
                return;
            }

            _rootElement = _uiDocument.rootVisualElement;
            
            if (_enableKeyboardDismiss)
            {
                _rootElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
            }

            Debug.Log("[NotificationController] UI initialized");
        }

        private void SubscribeToNotificationEvents()
        {
            if (_notificationManager != null)
            {
                _notificationManager.OnNotificationShown += HandleNotificationShown;
                _notificationManager.OnNotificationDismissed += HandleNotificationDismissed;
            }
        }

        private void UnsubscribeFromNotificationEvents()
        {
            if (_notificationManager != null)
            {
                _notificationManager.OnNotificationShown -= HandleNotificationShown;
                _notificationManager.OnNotificationDismissed -= HandleNotificationDismissed;
            }
        }

        private void HandleNotificationShown(Notification notification)
        {
            CreateNotificationView(notification);
        }

        private void HandleNotificationDismissed(Notification notification)
        {
            DismissNotificationView(notification.Id);
        }

        private void CreateNotificationView(Notification notification)
        {
            if (_notificationTemplate == null)
            {
                Debug.LogError("[NotificationController] Notification template is null!");
                return;
            }

            var view = GetPooledView();
            view.Initialize(notification, _notificationTemplate.CloneTree());
            view.OnCloseClicked += () => HandleCloseClicked(notification.Id);
            view.OnActionClicked += (index) => HandleActionClicked(notification.Id, index);
            view.OnHoverChanged += (isHovered) => HandleHoverChanged(notification.Id, isHovered);

            _activeViews[notification.Id] = view;
            _rootElement.Add(view.RootElement);

            UpdateNotificationPositions();
            StartCoroutine(ShowNotificationAnimation(view));

            Debug.Log($"[NotificationController] Created view for notification: {notification.Id}");
        }

        private void DismissNotificationView(string notificationId)
        {
            if (_activeViews.TryGetValue(notificationId, out var view))
            {
                StartCoroutine(DismissNotificationAnimation(view, notificationId));
            }
        }

        private NotificationView GetPooledView()
        {
            if (_viewPool.Count > 0)
            {
                return _viewPool.Dequeue();
            }

            return new NotificationView();
        }

        private void ReturnViewToPool(NotificationView view)
        {
            view.Reset();
            _viewPool.Enqueue(view);
        }

        private void UpdateNotificationPositions()
        {
            var sortedViews = _activeViews.Values
                .OrderBy(v => v.Notification.TimeShown)
                .ToList();

            for (int i = 0; i < sortedViews.Count; i++)
            {
                var view = sortedViews[i];
                view.SetStackPosition(i, _stackSpacing);
            }
        }

        private IEnumerator ShowNotificationAnimation(NotificationView view)
        {
            view.RootElement.AddToClassList("visible");
            yield return new WaitForSecondsRealtime(_showAnimationDuration);
        }

        private IEnumerator DismissNotificationAnimation(NotificationView view, string notificationId)
        {
            view.RootElement.AddToClassList("dismissing");
            yield return new WaitForSecondsRealtime(_dismissAnimationDuration);

            if (_activeViews.ContainsKey(notificationId))
            {
                _rootElement.Remove(view.RootElement);
                _activeViews.Remove(notificationId);
                ReturnViewToPool(view);
                UpdateNotificationPositions();
            }
        }

        private void HandleCloseClicked(string notificationId)
        {
            _notificationManager?.DismissNotification(notificationId, true);
        }

        private void HandleActionClicked(string notificationId, int actionIndex)
        {
            if (_activeViews.TryGetValue(notificationId, out var view))
            {
                view.Notification.TriggerAction(actionIndex);
                _notificationManager?.DismissNotification(notificationId, true);
            }
        }

        private void HandleHoverChanged(string notificationId, bool isHovered)
        {
            if (_pauseOnHover && _activeViews.TryGetValue(notificationId, out var view))
            {
                view.SetHoverPaused(isHovered);
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape && _activeViews.Count > 0)
            {
                var mostRecentView = _activeViews.Values
                    .OrderByDescending(v => v.Notification.TimeShown)
                    .FirstOrDefault();
                
                if (mostRecentView != null)
                {
                    _notificationManager?.DismissNotification(mostRecentView.Notification.Id, true);
                }
                
                evt.StopPropagation();
            }
        }

        public void ShowTestNotifications()
        {
            if (_notificationManager == null) return;

            _notificationManager.ShowSuccess("Contract completed successfully! You earned $1,200.", 5f);
            _notificationManager.ShowWarning("Vehicle is running low on fuel.", 4f);
            _notificationManager.ShowError("Insufficient credits to purchase this vehicle.", 6f);
            _notificationManager.ShowInfo("New city discovered: Springfield.", 3f);
            _notificationManager.ShowProgress("Vehicle en route to destination...", 8f);
        }
    }

    // AIDEV-NOTE: Individual notification view wrapper for UI management
    internal class NotificationView
    {
        public VisualElement RootElement { get; private set; }
        public Notification Notification { get; private set; }
        
        private Label _messageLabel;
        private Button _closeButton;
        private VisualElement _iconElement;
        private VisualElement _actionsContainer;
        private VisualElement _progressContainer;
        private VisualElement _progressBar;
        
        private bool _isHoverPaused;
        private Coroutine _progressUpdateCoroutine;

        public event Action OnCloseClicked;
        public event Action<int> OnActionClicked;
        public event Action<bool> OnHoverChanged;

        public void Initialize(Notification notification, VisualElement rootElement)
        {
            Notification = notification;
            RootElement = rootElement;
            
            SetupElements();
            BindData();
            SetupEventHandlers();
        }

        public void Reset()
        {
            Notification = null;
            RootElement = null;
            _messageLabel = null;
            _closeButton = null;
            _iconElement = null;
            _actionsContainer = null;
            _progressContainer = null;
            _progressBar = null;
            _isHoverPaused = false;
            
            OnCloseClicked = null;
            OnActionClicked = null;
            OnHoverChanged = null;
            
            if (_progressUpdateCoroutine != null)
            {
                // Note: Coroutine cleanup would be handled by the controller
                _progressUpdateCoroutine = null;
            }
        }

        public void SetStackPosition(int stackIndex, float spacing)
        {
            RootElement.RemoveFromClassList("stacked-1");
            RootElement.RemoveFromClassList("stacked-2");
            RootElement.RemoveFromClassList("stacked-3");

            if (stackIndex > 0 && stackIndex <= 3)
            {
                RootElement.AddToClassList($"stacked-{stackIndex}");
            }
        }

        public void SetHoverPaused(bool isPaused)
        {
            _isHoverPaused = isPaused;
        }

        private void SetupElements()
        {
            _messageLabel = RootElement.Q<Label>("notification-message");
            _closeButton = RootElement.Q<Button>("notification-close-button");
            _iconElement = RootElement.Q<VisualElement>("notification-icon");
            _actionsContainer = RootElement.Q<VisualElement>("notification-actions");
            _progressContainer = RootElement.Q<VisualElement>("notification-progress");
            _progressBar = RootElement.Q<VisualElement>("progress-bar");
        }

        private void BindData()
        {
            if (_messageLabel != null)
            {
                _messageLabel.text = Notification.Message;
            }

            SetupTypeSpecificStyling();
            SetupActionButtons();
            SetupProgressBar();
        }

        private void SetupTypeSpecificStyling()
        {
            string typeClass = $"notification-{Notification.Type.ToString().ToLower()}";
            RootElement.AddToClassList(typeClass);

            if (_iconElement != null)
            {
                _iconElement.style.backgroundColor = Notification.GetTypeColor();
            }
        }

        private void SetupActionButtons()
        {
            if (_actionsContainer == null || !Notification.HasActions())
                return;

            _actionsContainer.AddToClassList("has-actions");
            _actionsContainer.Clear();

            for (int i = 0; i < Notification.ActionLabels.Length; i++)
            {
                var button = new Button();
                button.AddToClassList("notification-action-button");
                button.text = Notification.ActionLabels[i];
                
                int actionIndex = i;
                button.clicked += () => OnActionClicked?.Invoke(actionIndex);
                
                _actionsContainer.Add(button);
            }
        }

        private void SetupProgressBar()
        {
            if (_progressContainer == null || _progressBar == null || Notification.Duration <= 0f)
                return;

            _progressContainer.AddToClassList("visible");
            // Note: Progress bar animation would be handled by a coroutine in the controller
        }

        private void SetupEventHandlers()
        {
            if (_closeButton != null)
            {
                _closeButton.clicked += () => OnCloseClicked?.Invoke();
            }

            RootElement.RegisterCallback<MouseEnterEvent>(evt => OnHoverChanged?.Invoke(true));
            RootElement.RegisterCallback<MouseLeaveEvent>(evt => OnHoverChanged?.Invoke(false));
        }
    }
}