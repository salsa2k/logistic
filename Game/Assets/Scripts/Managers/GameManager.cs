using UnityEngine;

namespace LogisticGame.Managers
{
    public class GameManager : MonoBehaviour
    {
        // AIDEV-NOTE: Singleton instance for global game state management
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(GameManager));
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Game State")]
        [SerializeField] private bool _isGamePaused;
        [SerializeField] private bool _isGameStarted;

        [Header("Player Data")]
        [SerializeField] private float _playerCredits = 10000f;
        [SerializeField] private string _companyName = "Player Company";

        // Events for game state changes
        public static System.Action<bool> OnGamePausedChanged;
        public static System.Action<bool> OnGameStartedChanged;
        public static System.Action<float> OnPlayerCreditsChanged;

        // Properties
        public bool IsGamePaused => _isGamePaused;
        public bool IsGameStarted => _isGameStarted;
        public float PlayerCredits => _playerCredits;
        public string CompanyName => _companyName;

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
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            // AIDEV-NOTE: Initialize game systems and default values
            Debug.Log($"GameManager initialized. Company: {_companyName}, Credits: {_playerCredits:C}");
        }

        public void SetGamePaused(bool paused)
        {
            if (_isGamePaused != paused)
            {
                _isGamePaused = paused;
                Time.timeScale = paused ? 0f : 1f;
                OnGamePausedChanged?.Invoke(_isGamePaused);
            }
        }

        public void StartGame()
        {
            if (!_isGameStarted)
            {
                _isGameStarted = true;
                OnGameStartedChanged?.Invoke(_isGameStarted);
                Debug.Log("Game started!");
            }
        }

        public void EndGame()
        {
            if (_isGameStarted)
            {
                _isGameStarted = false;
                OnGameStartedChanged?.Invoke(_isGameStarted);
                Debug.Log("Game ended!");
            }
        }

        public bool TrySpendCredits(float amount)
        {
            if (_playerCredits >= amount)
            {
                _playerCredits -= amount;
                OnPlayerCreditsChanged?.Invoke(_playerCredits);
                return true;
            }
            return false;
        }

        public void AddCredits(float amount)
        {
            _playerCredits += amount;
            OnPlayerCreditsChanged?.Invoke(_playerCredits);
        }

        public void SetCompanyName(string companyName)
        {
            _companyName = companyName;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (_isGameStarted)
            {
                SetGamePaused(pauseStatus);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_isGameStarted && !hasFocus)
            {
                SetGamePaused(true);
            }
        }
    }
}