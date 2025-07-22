using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogisticGame.Managers;

namespace LogisticGame.SaveSystem
{
    // AIDEV-NOTE: Centralized load progress reporting and UI integration system
    public class LoadProgressReporter : MonoBehaviour
    {
        private static LoadProgressReporter _instance;
        public static LoadProgressReporter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LoadProgressReporter>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(LoadProgressReporter));
                        _instance = go.AddComponent<LoadProgressReporter>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Progress Configuration")]
        [SerializeField] private bool _enableDetailedReporting = true;
        [SerializeField] private bool _enableUIIntegration = true;
        [SerializeField] private float _progressSmoothingSpeed = 2f;
        [SerializeField] private float _minimumProgressDuration = 0.5f;

        [Header("Current Progress State")]
        [SerializeField] private bool _isReporting = false;
        [SerializeField] private string _currentOperation;
        [SerializeField] private float _currentProgress = 0f;
        [SerializeField] private float _smoothedProgress = 0f;
        [SerializeField] private string _currentSlotName;

        // Progress tracking
        private LoadProgressData _currentProgressData;
        private DateTime _progressStartTime;
        private Dictionary<string, float> _operationWeights;
        private Queue<ProgressMessage> _progressMessages;
        private const int MAX_PROGRESS_MESSAGES = 50;

        // Events for UI integration
        public static event Action<LoadProgressData> OnProgressStarted;
        public static event Action<LoadProgressData> OnProgressUpdated;
        public static event Action<LoadProgressData> OnProgressCompleted;
        public static event Action<string, string> OnProgressFailed;
        public static event Action<string> OnProgressMessage;
        public static event Action<LoadOperation, float> OnOperationProgress;

        // Properties
        public bool IsReporting => _isReporting;
        public string CurrentOperation => _currentOperation;
        public float CurrentProgress => _currentProgress;
        public float SmoothedProgress => _smoothedProgress;
        public string CurrentSlotName => _currentSlotName;
        public LoadProgressData CurrentProgressData => _currentProgressData;

        private void Awake()
        {
            // AIDEV-NOTE: Ensure only one instance exists
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeProgressReporter();
        }

        private void Update()
        {
            // Smooth progress updates for UI
            if (_isReporting && _enableUIIntegration)
            {
                _smoothedProgress = Mathf.Lerp(_smoothedProgress, _currentProgress, Time.unscaledDeltaTime * _progressSmoothingSpeed);
                
                // Update progress data
                if (_currentProgressData != null)
                {
                    _currentProgressData.SmoothedProgress = _smoothedProgress;
                    _currentProgressData.ElapsedTime = DateTime.Now - _progressStartTime;
                    
                    // Estimate remaining time
                    if (_smoothedProgress > 0.01f)
                    {
                        double totalEstimatedTime = _currentProgressData.ElapsedTime.TotalSeconds / _smoothedProgress;
                        _currentProgressData.EstimatedRemainingTime = TimeSpan.FromSeconds(totalEstimatedTime * (1 - _smoothedProgress));
                    }
                }
            }
        }

        private void InitializeProgressReporter()
        {
            // AIDEV-NOTE: Initialize progress tracking and default operation weights
            _progressMessages = new Queue<ProgressMessage>();
            
            // Define default weights for different load operations
            _operationWeights = new Dictionary<string, float>
            {
                { LoadOperation.Initializing.ToString(), 0.05f },
                { LoadOperation.Validating.ToString(), 0.1f },
                { LoadOperation.CheckingCompatibility.ToString(), 0.05f },
                { LoadOperation.MigratingVersion.ToString(), 0.1f },
                { LoadOperation.LoadingData.ToString(), 0.4f },
                { LoadOperation.LoadingCoreData.ToString(), 0.2f },
                { LoadOperation.ValidatingData.ToString(), 0.1f },
                { LoadOperation.SanitizingData.ToString(), 0.05f },
                { LoadOperation.FinalValidation.ToString(), 0.05f },
                { LoadOperation.ApplyingData.ToString(), 0.1f },
                { LoadOperation.Completed.ToString(), 0.0f }
            };

            // Subscribe to load manager events
            if (LogisticGame.Managers.LoadManager.Instance != null)
            {
                LogisticGame.Managers.LoadManager.OnLoadStarted += HandleLoadStarted;
                LogisticGame.Managers.LoadManager.OnLoadCompleted += HandleLoadCompleted;
                LogisticGame.Managers.LoadManager.OnLoadFailed += HandleLoadFailed;
                LogisticGame.Managers.LoadManager.OnLoadProgress += HandleLoadProgress;
                LogisticGame.Managers.LoadManager.OnLoadOperationChanged += HandleOperationChanged;
            }

            Debug.Log("LoadProgressReporter initialized");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (LogisticGame.Managers.LoadManager.Instance != null)
            {
                LogisticGame.Managers.LoadManager.OnLoadStarted -= HandleLoadStarted;
                LogisticGame.Managers.LoadManager.OnLoadCompleted -= HandleLoadCompleted;
                LogisticGame.Managers.LoadManager.OnLoadFailed -= HandleLoadFailed;
                LogisticGame.Managers.LoadManager.OnLoadProgress -= HandleLoadProgress;
                LogisticGame.Managers.LoadManager.OnLoadOperationChanged -= HandleOperationChanged;
            }
        }

        // Main progress reporting methods
        public void StartProgressReporting(string slotName, LoadStrategy strategy = LoadStrategy.Full)
        {
            _isReporting = true;
            _currentSlotName = slotName;
            _currentProgress = 0f;
            _smoothedProgress = 0f;
            _progressStartTime = DateTime.Now;

            _currentProgressData = new LoadProgressData
            {
                SlotName = slotName,
                Strategy = strategy,
                StartTime = _progressStartTime,
                Progress = 0f,
                SmoothedProgress = 0f,
                CurrentOperation = LoadOperation.Initializing,
                ElapsedTime = TimeSpan.Zero,
                EstimatedRemainingTime = TimeSpan.Zero,
                Messages = new List<ProgressMessage>()
            };

            ClearProgressMessages();
            AddProgressMessage("Load operation started", ProgressMessageType.Info);

            OnProgressStarted?.Invoke(_currentProgressData);
            
            if (_enableDetailedReporting)
            {
                Debug.Log($"Progress reporting started for slot: {slotName} using {strategy} strategy");
            }
        }

        public void UpdateProgress(float progress, LoadOperation operation, string message = null)
        {
            if (!_isReporting)
                return;

            _currentProgress = Mathf.Clamp01(progress);
            _currentOperation = operation.ToString();

            if (_currentProgressData != null)
            {
                _currentProgressData.Progress = _currentProgress;
                _currentProgressData.CurrentOperation = operation;
                
                if (!string.IsNullOrEmpty(message))
                {
                    AddProgressMessage(message, ProgressMessageType.Info);
                }
            }

            OnOperationProgress?.Invoke(operation, _currentProgress);
            
            if (_enableUIIntegration)
            {
                OnProgressUpdated?.Invoke(_currentProgressData);
            }

            if (_enableDetailedReporting && !string.IsNullOrEmpty(message))
            {
                Debug.Log($"Load Progress ({_currentProgress:P0}): {operation} - {message}");
            }
        }

        public void CompleteProgressReporting(bool success, string message = null)
        {
            if (!_isReporting)
                return;

            _currentProgress = 1f;
            _smoothedProgress = 1f;
            
            if (_currentProgressData != null)
            {
                _currentProgressData.Progress = 1f;
                _currentProgressData.SmoothedProgress = 1f;
                _currentProgressData.IsCompleted = true;
                _currentProgressData.WasSuccessful = success;
                _currentProgressData.EndTime = DateTime.Now;
                _currentProgressData.TotalDuration = _currentProgressData.EndTime - _currentProgressData.StartTime;
                
                var completionMessage = success ? "Load completed successfully" : $"Load failed: {message}";
                AddProgressMessage(completionMessage, success ? ProgressMessageType.Success : ProgressMessageType.Error);
            }

            if (success)
            {
                OnProgressCompleted?.Invoke(_currentProgressData);
            }
            else
            {
                OnProgressFailed?.Invoke(_currentSlotName, message ?? "Load operation failed");
            }

            // Clean up after minimum duration
            StartCoroutine(CleanupAfterDelay());

            if (_enableDetailedReporting)
            {
                var duration = DateTime.Now - _progressStartTime;
                Debug.Log($"Progress reporting completed for {_currentSlotName}. Duration: {duration.TotalSeconds:F2}s, Success: {success}");
            }
        }

        private System.Collections.IEnumerator CleanupAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_minimumProgressDuration);
            
            _isReporting = false;
            _currentSlotName = null;
            _currentOperation = null;
            _currentProgressData = null;
        }

        // Message management
        public void AddProgressMessage(string message, ProgressMessageType type = ProgressMessageType.Info)
        {
            var progressMessage = new ProgressMessage
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now
            };

            _progressMessages.Enqueue(progressMessage);
            
            // Limit message queue size
            while (_progressMessages.Count > MAX_PROGRESS_MESSAGES)
            {
                _progressMessages.Dequeue();
            }

            if (_currentProgressData != null)
            {
                _currentProgressData.Messages.Add(progressMessage);
            }

            OnProgressMessage?.Invoke(message);

            if (_enableDetailedReporting)
            {
                Debug.Log($"[{type}] {message}");
            }
        }

        public void ClearProgressMessages()
        {
            _progressMessages.Clear();
            if (_currentProgressData != null)
            {
                _currentProgressData.Messages.Clear();
            }
        }

        public List<ProgressMessage> GetRecentMessages(int count = 10)
        {
            var messages = _progressMessages.ToArray();
            int startIndex = Mathf.Max(0, messages.Length - count);
            var recentMessages = new List<ProgressMessage>();
            
            for (int i = startIndex; i < messages.Length; i++)
            {
                recentMessages.Add(messages[i]);
            }
            
            return recentMessages;
        }

        // Event handlers
        private void HandleLoadStarted(string slotName)
        {
            if (_enableUIIntegration)
            {
                StartProgressReporting(slotName);
            }
        }

        private void HandleLoadCompleted(string slotName, SaveData saveData)
        {
            if (_enableUIIntegration)
            {
                CompleteProgressReporting(true, "Load completed successfully");
            }
        }

        private void HandleLoadFailed(string slotName, string error)
        {
            if (_enableUIIntegration)
            {
                CompleteProgressReporting(false, error);
            }
        }

        private void HandleLoadProgress(string slotName, float progress)
        {
            if (_enableUIIntegration && _currentSlotName == slotName)
            {
                UpdateProgress(progress, _currentProgressData?.CurrentOperation ?? LoadOperation.LoadingData);
            }
        }

        private void HandleOperationChanged(string slotName, LoadOperation operation)
        {
            if (_enableUIIntegration && _currentSlotName == slotName)
            {
                string operationMessage = GetOperationDisplayMessage(operation);
                UpdateProgress(_currentProgress, operation, operationMessage);
            }
        }

        // Utility methods
        private string GetOperationDisplayMessage(LoadOperation operation)
        {
            switch (operation)
            {
                case LoadOperation.Initializing:
                    return "Initializing load operation...";
                case LoadOperation.Validating:
                    return "Validating save file integrity...";
                case LoadOperation.CheckingCompatibility:
                    return "Checking version compatibility...";
                case LoadOperation.MigratingVersion:
                    return "Migrating save data to current version...";
                case LoadOperation.LoadingData:
                    return "Loading save data...";
                case LoadOperation.LoadingCoreData:
                    return "Loading core game data...";
                case LoadOperation.ValidatingData:
                    return "Validating loaded data...";
                case LoadOperation.SanitizingData:
                    return "Sanitizing data inconsistencies...";
                case LoadOperation.FinalValidation:
                    return "Performing final validation...";
                case LoadOperation.ApplyingData:
                    return "Applying loaded data to game...";
                case LoadOperation.Completed:
                    return "Load operation completed";
                case LoadOperation.Cancelled:
                    return "Load operation cancelled";
                default:
                    return "Processing...";
            }
        }

        public float GetOperationWeight(LoadOperation operation)
        {
            string operationName = operation.ToString();
            return _operationWeights.ContainsKey(operationName) ? _operationWeights[operationName] : 0.1f;
        }

        public void SetOperationWeight(LoadOperation operation, float weight)
        {
            _operationWeights[operation.ToString()] = Mathf.Clamp01(weight);
        }

        // Configuration methods
        public void SetProgressSmoothingSpeed(float speed)
        {
            _progressSmoothingSpeed = Mathf.Max(0.1f, speed);
        }

        public void SetDetailedReporting(bool enabled)
        {
            _enableDetailedReporting = enabled;
        }

        public void SetUIIntegration(bool enabled)
        {
            _enableUIIntegration = enabled;
        }

        // Debug and diagnostic methods
        public string GetProgressReport()
        {
            if (!_isReporting || _currentProgressData == null)
                return "No load operation in progress";

            var report = new System.Text.StringBuilder();
            report.AppendLine($"Load Progress Report");
            report.AppendLine($"Slot: {_currentProgressData.SlotName}");
            report.AppendLine($"Strategy: {_currentProgressData.Strategy}");
            report.AppendLine($"Operation: {_currentProgressData.CurrentOperation}");
            report.AppendLine($"Progress: {_currentProgressData.Progress:P1} (Smoothed: {_currentProgressData.SmoothedProgress:P1})");
            report.AppendLine($"Elapsed: {_currentProgressData.ElapsedTime.TotalSeconds:F1}s");
            report.AppendLine($"Estimated Remaining: {_currentProgressData.EstimatedRemainingTime.TotalSeconds:F1}s");
            report.AppendLine($"Messages: {_currentProgressData.Messages.Count}");
            
            return report.ToString();
        }

        public void LogCurrentState()
        {
            Debug.Log(GetProgressReport());
        }
    }

    // AIDEV-NOTE: Data structure for comprehensive load progress information
    [System.Serializable]
    public class LoadProgressData
    {
        public string SlotName;
        public LoadStrategy Strategy;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan TotalDuration;
        public TimeSpan ElapsedTime;
        public TimeSpan EstimatedRemainingTime;
        
        public float Progress;
        public float SmoothedProgress;
        public LoadOperation CurrentOperation;
        
        public bool IsCompleted = false;
        public bool WasSuccessful = false;
        
        public List<ProgressMessage> Messages = new List<ProgressMessage>();
        
        // Calculated properties
        public bool IsInProgress => !IsCompleted && Progress > 0f;
        public float ProgressPercentage => Progress * 100f;
        public string EstimatedRemainingTimeDisplay => EstimatedRemainingTime.TotalSeconds > 0 ? 
            $"{EstimatedRemainingTime.TotalSeconds:F0}s" : "Unknown";
        public string ElapsedTimeDisplay => $"{ElapsedTime.TotalSeconds:F1}s";
        
        public string GetStatusSummary()
        {
            if (IsCompleted)
                return WasSuccessful ? "Completed Successfully" : "Failed";
            
            if (IsInProgress)
                return $"{CurrentOperation} ({ProgressPercentage:F0}%)";
            
            return "Not Started";
        }
    }

    // AIDEV-NOTE: Data structure for progress messages
    [System.Serializable]
    public class ProgressMessage
    {
        public string Message;
        public ProgressMessageType Type;
        public DateTime Timestamp;
        
        public string TimeDisplay => Timestamp.ToString("HH:mm:ss.fff");
        public string TypeDisplay => Type.ToString().ToUpper();
        
        public string GetFormattedMessage()
        {
            return $"[{TimeDisplay}] {TypeDisplay}: {Message}";
        }
    }

    // AIDEV-NOTE: Enumeration for progress message types
    public enum ProgressMessageType
    {
        Info,
        Warning,
        Error,
        Success,
        Debug
    }
}