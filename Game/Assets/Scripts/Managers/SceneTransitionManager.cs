using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace LogisticGame.Managers
{
    // AIDEV-NOTE: Scene transition manager for smooth scene loading
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        public static SceneTransitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SceneTransitionManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(SceneTransitionManager));
                        _instance = go.AddComponent<SceneTransitionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Transition Settings")]
        [SerializeField] private float _transitionDuration = 1f;
        [SerializeField] private bool _showLoadingScreen = true;

        // Events for scene transitions
        public static System.Action<string> OnSceneLoadStarted;
        public static System.Action<string> OnSceneLoadCompleted;
        public static System.Action<float> OnSceneLoadProgress;

        private bool _isTransitioning;
        private string _currentSceneName;

        public bool IsTransitioning => _isTransitioning;
        public string CurrentSceneName => _currentSceneName;

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

            _currentSceneName = SceneManager.GetActiveScene().name;
            
            // Subscribe to scene loaded events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _currentSceneName = scene.name;
            _isTransitioning = false;
            OnSceneLoadCompleted?.Invoke(scene.name);
        }

        public void LoadScene(string sceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning($"Scene transition already in progress. Cannot load {sceneName}");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void LoadScene(int buildIndex)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning($"Scene transition already in progress. Cannot load scene at index {buildIndex}");
                return;
            }

            StartCoroutine(LoadSceneAsync(buildIndex));
        }

        public void ReloadCurrentScene()
        {
            LoadScene(_currentSceneName);
        }

        public void LoadMainMenu()
        {
            LoadScene("MainMenu");
        }

        public void LoadGameScene()
        {
            LoadScene("Game");
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            _isTransitioning = true;
            OnSceneLoadStarted?.Invoke(sceneName);

            // Optional: Show loading screen or transition effect
            if (_showLoadingScreen)
            {
                yield return StartCoroutine(ShowTransitionIn());
            }

            // Start loading the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait until the scene is almost loaded
            while (asyncLoad.progress < 0.9f)
            {
                OnSceneLoadProgress?.Invoke(asyncLoad.progress);
                yield return null;
            }

            // Ensure minimum transition time for smooth experience
            float elapsedTime = 0f;
            while (elapsedTime < _transitionDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = Mathf.Lerp(0.9f, 1f, elapsedTime / _transitionDuration);
                OnSceneLoadProgress?.Invoke(progress);
                yield return null;
            }

            // Activate the scene
            asyncLoad.allowSceneActivation = true;

            // Wait for scene to be fully loaded
            yield return asyncLoad;

            // Optional: Hide loading screen or transition effect
            if (_showLoadingScreen)
            {
                yield return StartCoroutine(ShowTransitionOut());
            }
        }

        private IEnumerator LoadSceneAsync(int buildIndex)
        {
            _isTransitioning = true;
            string sceneName = GetSceneNameFromBuildIndex(buildIndex);
            OnSceneLoadStarted?.Invoke(sceneName);

            // Optional: Show loading screen or transition effect
            if (_showLoadingScreen)
            {
                yield return StartCoroutine(ShowTransitionIn());
            }

            // Start loading the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
            asyncLoad.allowSceneActivation = false;

            // Wait until the scene is almost loaded
            while (asyncLoad.progress < 0.9f)
            {
                OnSceneLoadProgress?.Invoke(asyncLoad.progress);
                yield return null;
            }

            // Ensure minimum transition time for smooth experience
            float elapsedTime = 0f;
            while (elapsedTime < _transitionDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = Mathf.Lerp(0.9f, 1f, elapsedTime / _transitionDuration);
                OnSceneLoadProgress?.Invoke(progress);
                yield return null;
            }

            // Activate the scene
            asyncLoad.allowSceneActivation = true;

            // Wait for scene to be fully loaded
            yield return asyncLoad;

            // Optional: Hide loading screen or transition effect
            if (_showLoadingScreen)
            {
                yield return StartCoroutine(ShowTransitionOut());
            }
        }

        private IEnumerator ShowTransitionIn()
        {
            // AIDEV-NOTE: Implement transition in effect (fade in, etc.)
            // This is a placeholder for now
            yield return new WaitForSecondsRealtime(0.2f);
        }

        private IEnumerator ShowTransitionOut()
        {
            // AIDEV-NOTE: Implement transition out effect (fade out, etc.)
            // This is a placeholder for now
            yield return new WaitForSecondsRealtime(0.2f);
        }

        private string GetSceneNameFromBuildIndex(int buildIndex)
        {
            // AIDEV-NOTE: This is a simple implementation. In practice, you might want to store scene names
            string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            if (!string.IsNullOrEmpty(scenePath))
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                return sceneName;
            }
            return $"Scene_{buildIndex}";
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}