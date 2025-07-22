using UnityEngine;
using System;
using UnityEngine.Localization.Settings;

// AIDEV-NOTE: ScriptableObject for game settings and preferences
[CreateAssetMenu(fileName = "Settings Data", menuName = "Logistics Game/Settings Data")]
public class SettingsData : ScriptableObject
{
    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float _uiVolume = 0.6f;
    [SerializeField] private bool _audioEnabled = true;
    
    [Header("Graphics Settings")]
    [SerializeField] private int _targetFrameRate = 60;
    [SerializeField] private bool _vSyncEnabled = true;
    [SerializeField] private FullScreenMode _screenMode = FullScreenMode.Windowed;
    [SerializeField] private Resolution _resolution;
    [SerializeField, Range(0.5f, 2f)] private float _uiScale = 1f;
    
    [Header("Gameplay Settings")]
    [SerializeField, Range(0.1f, 5f)] private float _gameSpeed = 1f;
    [SerializeField] private bool _autopauseOnLowFuel = true;
    [SerializeField] private bool _autopauseOnContractExpiry = true;
    [SerializeField] private bool _showDistanceInKm = true; // vs miles
    [SerializeField] private bool _show24HourTime = true; // vs 12-hour
    [SerializeField] private bool _confirmDangerousActions = true;
    
    [Header("UI Settings")]
    [SerializeField] private bool _showTooltips = true;
    [SerializeField] private bool _showNotifications = true;
    [SerializeField, Range(1f, 10f)] private float _tooltipDelay = 1f; // seconds
    [SerializeField, Range(1f, 30f)] private float _notificationDuration = 5f; // seconds
    [SerializeField] private bool _minimizeToSystemTray = false;
    
    [Header("Localization Settings")]
    [SerializeField] private SystemLanguage _language = SystemLanguage.English;
    [SerializeField] private string _localeCode = "en";
    [SerializeField] private string _currencySymbol = "$";
    [SerializeField] private string _distanceUnit = "km";
    [SerializeField] private string _weightUnit = "kg";
    [SerializeField] private string _volumeUnit = "m³";
    [SerializeField] private string _fuelUnit = "L";
    
    [Header("Advanced Settings")]
    [SerializeField] private bool _enableDebugMode = false;
    [SerializeField] private bool _enableTelemetry = true;
    [SerializeField] private int _autosaveInterval = 5; // minutes
    [SerializeField] private int _maxAutosaves = 5;
    [SerializeField] private bool _loadLastSaveOnStart = true;
    
    // Events for settings changes
    public static event Action<float> OnMasterVolumeChanged;
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSfxVolumeChanged;
    public static event Action<float> OnUiVolumeChanged;
    public static event Action<bool> OnAudioEnabledChanged;
    public static event Action<SystemLanguage> OnLanguageChanged;
    public static event Action<float> OnGameSpeedChanged;
    public static event Action<float> OnUiScaleChanged;
    
    // Properties
    public float MasterVolume => _masterVolume;
    public float MusicVolume => _musicVolume;
    public float SfxVolume => _sfxVolume;
    public float UiVolume => _uiVolume;
    public bool AudioEnabled => _audioEnabled;
    public int TargetFrameRate => _targetFrameRate;
    public bool VSyncEnabled => _vSyncEnabled;
    public FullScreenMode ScreenMode => _screenMode;
    public Resolution Resolution => _resolution;
    public float UiScale => _uiScale;
    public float GameSpeed => _gameSpeed;
    public bool AutopauseOnLowFuel => _autopauseOnLowFuel;
    public bool AutopauseOnContractExpiry => _autopauseOnContractExpiry;
    public bool ShowDistanceInKm => _showDistanceInKm;
    public bool Show24HourTime => _show24HourTime;
    public bool ConfirmDangerousActions => _confirmDangerousActions;
    public bool ShowTooltips => _showTooltips;
    public bool ShowNotifications => _showNotifications;
    public float TooltipDelay => _tooltipDelay;
    public float NotificationDuration => _notificationDuration;
    public bool MinimizeToSystemTray => _minimizeToSystemTray;
    public SystemLanguage Language => _language;
    public string LocaleCode => _localeCode;
    public string CurrencySymbol => _currencySymbol;
    public string DistanceUnit => _distanceUnit;
    public string WeightUnit => _weightUnit;
    public string VolumeUnit => _volumeUnit;
    public string FuelUnit => _fuelUnit;
    public bool EnableDebugMode => _enableDebugMode;
    public bool EnableTelemetry => _enableTelemetry;
    public int AutosaveInterval => _autosaveInterval;
    public int MaxAutosaves => _maxAutosaves;
    public bool LoadLastSaveOnStart => _loadLastSaveOnStart;
    
    // Calculated properties
    public float EffectiveMusicVolume => _audioEnabled ? _masterVolume * _musicVolume : 0f;
    public float EffectiveSfxVolume => _audioEnabled ? _masterVolume * _sfxVolume : 0f;
    public float EffectiveUiVolume => _audioEnabled ? _masterVolume * _uiVolume : 0f;
    
    // Initialization
    private void Awake()
    {
        // Set default resolution if not set
        if (_resolution.width == 0 || _resolution.height == 0)
        {
            _resolution = Screen.currentResolution;
        }
    }
    
    // Audio settings
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        OnMasterVolumeChanged?.Invoke(_masterVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        OnMusicVolumeChanged?.Invoke(_musicVolume);
    }
    
    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }
    
    public void SetUiVolume(float volume)
    {
        _uiVolume = Mathf.Clamp01(volume);
        OnUiVolumeChanged?.Invoke(_uiVolume);
    }
    
    public void SetAudioEnabled(bool enabled)
    {
        _audioEnabled = enabled;
        OnAudioEnabledChanged?.Invoke(_audioEnabled);
    }
    
    // Graphics settings
    public void SetTargetFrameRate(int frameRate)
    {
        _targetFrameRate = Mathf.Clamp(frameRate, 30, 120);
        Application.targetFrameRate = _targetFrameRate;
    }
    
    public void SetVSyncEnabled(bool enabled)
    {
        _vSyncEnabled = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
    
    public void SetScreenMode(FullScreenMode mode)
    {
        _screenMode = mode;
        Screen.fullScreenMode = mode;
    }
    
    public void SetResolution(Resolution resolution)
    {
        _resolution = resolution;
        Screen.SetResolution(resolution.width, resolution.height, _screenMode, resolution.refreshRateRatio);
    }
    
    public void SetUiScale(float scale)
    {
        _uiScale = Mathf.Clamp(scale, 0.5f, 2f);
        OnUiScaleChanged?.Invoke(_uiScale);
    }
    
    // Gameplay settings
    public void SetGameSpeed(float speed)
    {
        _gameSpeed = Mathf.Clamp(speed, 0.1f, 5f);
        OnGameSpeedChanged?.Invoke(_gameSpeed);
    }
    
    public void SetAutopauseOnLowFuel(bool enabled)
    {
        _autopauseOnLowFuel = enabled;
    }
    
    public void SetAutopauseOnContractExpiry(bool enabled)
    {
        _autopauseOnContractExpiry = enabled;
    }
    
    public void SetShowDistanceInKm(bool useKm)
    {
        _showDistanceInKm = useKm;
        _distanceUnit = useKm ? "km" : "mi";
    }
    
    public void SetShow24HourTime(bool use24Hour)
    {
        _show24HourTime = use24Hour;
    }
    
    public void SetConfirmDangerousActions(bool confirm)
    {
        _confirmDangerousActions = confirm;
    }
    
    // UI settings
    public void SetShowTooltips(bool show)
    {
        _showTooltips = show;
    }
    
    public void SetShowNotifications(bool show)
    {
        _showNotifications = show;
    }
    
    public void SetTooltipDelay(float delay)
    {
        _tooltipDelay = Mathf.Clamp(delay, 0f, 10f);
    }
    
    public void SetNotificationDuration(float duration)
    {
        _notificationDuration = Mathf.Clamp(duration, 1f, 30f);
    }
    
    // Localization settings
    public void SetLanguage(SystemLanguage language)
    {
        _language = language;
        
        // Update locale code based on language
        _localeCode = language switch
        {
            SystemLanguage.English => "en",
            SystemLanguage.Portuguese => "pt-BR",
            SystemLanguage.Spanish => "es",
            SystemLanguage.French => "fr",
            SystemLanguage.German => "de",
            SystemLanguage.Italian => "it",
            SystemLanguage.Russian => "ru",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.Korean => "ko",
            SystemLanguage.Chinese => "zh-CN",
            _ => "en"
        };
        
        OnLanguageChanged?.Invoke(_language);
    }
    
    public void SetCurrencySymbol(string symbol)
    {
        _currencySymbol = string.IsNullOrEmpty(symbol) ? "$" : symbol;
    }
    
    // Advanced settings
    public void SetEnableDebugMode(bool enabled)
    {
        _enableDebugMode = enabled;
        Debug.unityLogger.logEnabled = enabled;
    }
    
    public void SetEnableTelemetry(bool enabled)
    {
        _enableTelemetry = enabled;
    }
    
    public void SetAutosaveInterval(int minutes)
    {
        _autosaveInterval = Mathf.Clamp(minutes, 1, 60);
    }
    
    public void SetMaxAutosaves(int count)
    {
        _maxAutosaves = Mathf.Clamp(count, 1, 20);
    }
    
    public void SetLoadLastSaveOnStart(bool load)
    {
        _loadLastSaveOnStart = load;
    }
    
    // Unit conversion helpers
    public float ConvertDistance(float kilometers)
    {
        return _showDistanceInKm ? kilometers : kilometers * 0.621371f; // km to miles
    }
    
    public string FormatDistance(float kilometers)
    {
        float value = ConvertDistance(kilometers);
        return $"{value:F1} {_distanceUnit}";
    }
    
    public string FormatWeight(float kilograms)
    {
        return $"{kilograms:F1} {_weightUnit}";
    }
    
    public string FormatVolume(float cubicMeters)
    {
        return $"{cubicMeters:F2} {_volumeUnit}";
    }
    
    public string FormatFuel(float liters)
    {
        return $"{liters:F1} {_fuelUnit}";
    }
    
    public string FormatCurrency(float amount)
    {
        return $"{_currencySymbol}{amount:F0}";
    }
    
    public string FormatTime(DateTime time)
    {
        return _show24HourTime ? time.ToString("HH:mm") : time.ToString("h:mm tt");
    }
    
    // Apply all settings
    public void ApplySettings()
    {
        // Apply graphics settings
        Application.targetFrameRate = _targetFrameRate;
        QualitySettings.vSyncCount = _vSyncEnabled ? 1 : 0;
        Screen.fullScreenMode = _screenMode;
        
        // Apply resolution if valid
        if (_resolution.width > 0 && _resolution.height > 0)
        {
            Screen.SetResolution(_resolution.width, _resolution.height, _screenMode, _resolution.refreshRateRatio);
        }
        
        // Debug mode
        Debug.unityLogger.logEnabled = _enableDebugMode;
        
        // Trigger events for immediate updates
        OnMasterVolumeChanged?.Invoke(_masterVolume);
        OnMusicVolumeChanged?.Invoke(_musicVolume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
        OnUiVolumeChanged?.Invoke(_uiVolume);
        OnAudioEnabledChanged?.Invoke(_audioEnabled);
        OnGameSpeedChanged?.Invoke(_gameSpeed);
        OnUiScaleChanged?.Invoke(_uiScale);
        OnLanguageChanged?.Invoke(_language);
    }
    
    // Load default settings
    public void LoadDefaults()
    {
        _masterVolume = 1f;
        _musicVolume = 0.7f;
        _sfxVolume = 0.8f;
        _uiVolume = 0.6f;
        _audioEnabled = true;
        
        _targetFrameRate = 60;
        _vSyncEnabled = true;
        _screenMode = FullScreenMode.Windowed;
        _resolution = Screen.currentResolution;
        _uiScale = 1f;
        
        _gameSpeed = 1f;
        _autopauseOnLowFuel = true;
        _autopauseOnContractExpiry = true;
        _showDistanceInKm = true;
        _show24HourTime = true;
        _confirmDangerousActions = true;
        
        _showTooltips = true;
        _showNotifications = true;
        _tooltipDelay = 1f;
        _notificationDuration = 5f;
        _minimizeToSystemTray = false;
        
        _language = SystemLanguage.English;
        _localeCode = "en";
        _currencySymbol = "$";
        _distanceUnit = "km";
        _weightUnit = "kg";
        _volumeUnit = "m³";
        _fuelUnit = "L";
        
        _enableDebugMode = false;
        _enableTelemetry = true;
        _autosaveInterval = 5;
        _maxAutosaves = 5;
        _loadLastSaveOnStart = true;
    }
    
    // Validation
    private void OnValidate()
    {
        _masterVolume = Mathf.Clamp01(_masterVolume);
        _musicVolume = Mathf.Clamp01(_musicVolume);
        _sfxVolume = Mathf.Clamp01(_sfxVolume);
        _uiVolume = Mathf.Clamp01(_uiVolume);
        _targetFrameRate = Mathf.Clamp(_targetFrameRate, 30, 120);
        _uiScale = Mathf.Clamp(_uiScale, 0.5f, 2f);
        _gameSpeed = Mathf.Clamp(_gameSpeed, 0.1f, 5f);
        _tooltipDelay = Mathf.Clamp(_tooltipDelay, 0f, 10f);
        _notificationDuration = Mathf.Clamp(_notificationDuration, 1f, 30f);
        _autosaveInterval = Mathf.Clamp(_autosaveInterval, 1, 60);
        _maxAutosaves = Mathf.Clamp(_maxAutosaves, 1, 20);
    }
}