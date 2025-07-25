using UnityEngine;
using System.Collections.Generic;

namespace LogisticGame.Managers
{
    // AIDEV-NOTE: ScriptableObject container for asset references
    [CreateAssetMenu(fileName = "AssetDatabase", menuName = "Logistic Game/Asset Database")]
    public class AssetDatabase : ScriptableObject
    {
        [Header("UI Assets")]
        [SerializeField] private Sprite[] _uiSprites;
        [SerializeField] private AudioClip[] _uiSounds;
        
        [Header("Company Assets")]
        [SerializeField] private Sprite[] _companyLogos;

        [Header("Vehicle Assets")]
        [SerializeField] private Sprite[] _vehicleSprites;
        [SerializeField] private GameObject[] _vehiclePrefabs;

        [Header("Environment Assets")]
        [SerializeField] private Sprite[] _environmentSprites;
        [SerializeField] private GameObject[] _environmentPrefabs;

        [Header("Audio Assets")]
        [SerializeField] private AudioClip[] _musicClips;
        [SerializeField] private AudioClip[] _sfxClips;

        // Public getters for asset arrays
        public Sprite[] UISprites => _uiSprites;
        public AudioClip[] UISounds => _uiSounds;
        public Sprite[] CompanyLogos => _companyLogos;
        public Sprite[] VehicleSprites => _vehicleSprites;
        public GameObject[] VehiclePrefabs => _vehiclePrefabs;
        public Sprite[] EnvironmentSprites => _environmentSprites;
        public GameObject[] EnvironmentPrefabs => _environmentPrefabs;
        public AudioClip[] MusicClips => _musicClips;
        public AudioClip[] SfxClips => _sfxClips;
    }

    // AIDEV-NOTE: Singleton manager for centralized asset access
    public class AssetManager : MonoBehaviour
    {
        private static AssetManager _instance;
        public static AssetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AssetManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(AssetManager));
                        _instance = go.AddComponent<AssetManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Asset Database")]
        [SerializeField] private AssetDatabase _assetDatabase;

        // Cached asset dictionaries for fast lookup
        private Dictionary<string, Sprite> _spriteCache;
        private Dictionary<string, AudioClip> _audioCache;
        private Dictionary<string, GameObject> _prefabCache;

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

            InitializeAssetCaches();
        }

        private void InitializeAssetCaches()
        {
            if (_assetDatabase == null)
            {
                Debug.LogError("AssetDatabase is not assigned to AssetManager!");
                return;
            }

            // Initialize caches
            _spriteCache = new Dictionary<string, Sprite>();
            _audioCache = new Dictionary<string, AudioClip>();
            _prefabCache = new Dictionary<string, GameObject>();

            // Cache UI sprites
            CacheAssets(_assetDatabase.UISprites, _spriteCache, "UI");
            CacheAssets(_assetDatabase.CompanyLogos, _spriteCache, "Company");
            CacheAssets(_assetDatabase.VehicleSprites, _spriteCache, "Vehicle");
            CacheAssets(_assetDatabase.EnvironmentSprites, _spriteCache, "Environment");

            // Cache audio clips
            CacheAssets(_assetDatabase.UISounds, _audioCache, "UI");
            CacheAssets(_assetDatabase.MusicClips, _audioCache, "Music");
            CacheAssets(_assetDatabase.SfxClips, _audioCache, "SFX");

            // Cache prefabs
            CacheAssets(_assetDatabase.VehiclePrefabs, _prefabCache, "Vehicle");
            CacheAssets(_assetDatabase.EnvironmentPrefabs, _prefabCache, "Environment");

            Debug.Log($"AssetManager initialized. Cached {_spriteCache.Count} sprites, {_audioCache.Count} audio clips, {_prefabCache.Count} prefabs.");
        }

        private void CacheAssets<T>(T[] assets, Dictionary<string, T> cache, string category) where T : Object
        {
            if (assets == null) return;

            foreach (var asset in assets)
            {
                if (asset != null)
                {
                    string key = $"{category}_{asset.name}";
                    cache[key] = asset;
                }
            }
        }

        // Asset retrieval methods
        public Sprite GetSprite(string spriteName, string category = "")
        {
            string key = string.IsNullOrEmpty(category) ? spriteName : $"{category}_{spriteName}";
            
            if (_spriteCache.TryGetValue(key, out Sprite sprite))
            {
                return sprite;
            }

            Debug.LogWarning($"Sprite not found: {key}");
            return null;
        }

        public AudioClip GetAudioClip(string clipName, string category = "")
        {
            string key = string.IsNullOrEmpty(category) ? clipName : $"{category}_{clipName}";
            
            if (_audioCache.TryGetValue(key, out AudioClip clip))
            {
                return clip;
            }

            Debug.LogWarning($"Audio clip not found: {key}");
            return null;
        }

        public GameObject GetPrefab(string prefabName, string category = "")
        {
            string key = string.IsNullOrEmpty(category) ? prefabName : $"{category}_{prefabName}";
            
            if (_prefabCache.TryGetValue(key, out GameObject prefab))
            {
                return prefab;
            }

            Debug.LogWarning($"Prefab not found: {key}");
            return null;
        }

        // Convenience methods for specific asset types
        public Sprite GetUISprite(string spriteName) => GetSprite(spriteName, "UI");
        public Sprite GetCompanyLogo(string logoName) => GetSprite(logoName, "Company");
        public Sprite GetVehicleSprite(string spriteName) => GetSprite(spriteName, "Vehicle");
        public Sprite GetEnvironmentSprite(string spriteName) => GetSprite(spriteName, "Environment");
        
        public AudioClip GetUISound(string soundName) => GetAudioClip(soundName, "UI");
        public AudioClip GetMusicClip(string musicName) => GetAudioClip(musicName, "Music");
        public AudioClip GetSFXClip(string sfxName) => GetAudioClip(sfxName, "SFX");
        
        public GameObject GetVehiclePrefab(string prefabName) => GetPrefab(prefabName, "Vehicle");
        public GameObject GetEnvironmentPrefab(string prefabName) => GetPrefab(prefabName, "Environment");

        // Instantiate methods
        public GameObject InstantiatePrefab(string prefabName, string category = "", Transform parent = null)
        {
            GameObject prefab = GetPrefab(prefabName, category);
            if (prefab != null)
            {
                return Instantiate(prefab, parent);
            }
            return null;
        }

        public GameObject InstantiateVehiclePrefab(string prefabName, Transform parent = null)
        {
            return InstantiatePrefab(prefabName, "Vehicle", parent);
        }

        public GameObject InstantiateEnvironmentPrefab(string prefabName, Transform parent = null)
        {
            return InstantiatePrefab(prefabName, "Environment", parent);
        }
        
        /// <summary>
        /// AIDEV-NOTE: Gets company logo by index for NewGameModal integration.
        /// Returns null if index is out of bounds or no logos are available.
        /// </summary>
        /// <param name="logoIndex">Index of the logo (0-based)</param>
        /// <returns>Company logo sprite or null if not found</returns>
        public Sprite GetCompanyLogoByIndex(int logoIndex)
        {
            if (_assetDatabase?.CompanyLogos == null || logoIndex < 0 || logoIndex >= _assetDatabase.CompanyLogos.Length)
            {
                Debug.LogWarning($"Company logo index {logoIndex} is out of bounds. Available logos: {_assetDatabase?.CompanyLogos?.Length ?? 0}");
                return null;
            }
            
            return _assetDatabase.CompanyLogos[logoIndex];
        }
        
        /// <summary>
        /// AIDEV-NOTE: Gets the total number of available company logos.
        /// </summary>
        /// <returns>Number of company logos available</returns>
        public int GetCompanyLogoCount()
        {
            return _assetDatabase?.CompanyLogos?.Length ?? 0;
        }
    }
}