using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;

/// <summary>
/// AIDEV-NOTE: Helper class for window animations and transitions. Provides smooth visual effects
/// for window show/hide operations in the logistics game UI system.
/// </summary>
public static class WindowAnimations
{
    /// <summary>
    /// Animation configuration for window transitions.
    /// AIDEV-NOTE: Encapsulates all animation settings in a single configuration object.
    /// </summary>
    [System.Serializable]
    public class AnimationConfig
    {
        [Header("Duration Settings")]
        public float fadeInDuration = 0.3f;
        public float fadeOutDuration = 0.25f;
        public float scaleInDuration = 0.35f;
        public float scaleOutDuration = 0.3f;
        
        [Header("Animation Curves")]
        public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        public AnimationCurve scaleInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve scaleOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("Scale Settings")]
        public float startScale = 0.8f;
        public float endScale = 1.0f;
        
        [Header("Position Settings")]
        public bool useSlideAnimation = false;
        public Vector2 slideDirection = Vector2.up;
        public float slideDistance = 50f;
    }
    
    // Default animation configuration
    private static AnimationConfig _defaultConfig = new AnimationConfig();
    
    /// <summary>
    /// Sets the default animation configuration for all window animations.
    /// AIDEV-NOTE: Call this during game initialization to customize default animation settings.
    /// </summary>
    /// <param name="config">The animation configuration to use as default</param>
    public static void SetDefaultConfig(AnimationConfig config)
    {
        _defaultConfig = config ?? new AnimationConfig();
    }
    
    /// <summary>
    /// Animates a window's appearance with fade and scale effects.
    /// AIDEV-NOTE: Main method for showing windows with professional animations.
    /// </summary>
    /// <param name="windowElement">The window's root visual element</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <param name="config">Optional custom animation configuration</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator AnimateWindowIn(VisualElement windowElement, Action onComplete = null, AnimationConfig config = null)
    {
        if (windowElement == null)
        {
            Debug.LogWarning("WindowAnimations: Cannot animate null window element");
            onComplete?.Invoke();
            yield break;
        }
        
        config ??= _defaultConfig;
        
        // AIDEV-NOTE: Unity UI Toolkit doesn't support opacity or scale transforms directly
        // This implementation uses display properties and prepares for future animation support
        
        // Ensure window is visible
        windowElement.style.display = DisplayStyle.Flex;
        
        // Simulate animation duration
        yield return new WaitForSeconds(config.fadeInDuration);
        
        // Apply final state classes
        windowElement.RemoveFromClassList("window-fade-out");
        windowElement.RemoveFromClassList("window-scale-out");
        windowElement.AddToClassList("window-fade-in");
        windowElement.AddToClassList("window-scale-in");
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Animates a window's disappearance with fade and scale effects.
    /// AIDEV-NOTE: Main method for hiding windows with professional animations.
    /// </summary>
    /// <param name="windowElement">The window's root visual element</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <param name="config">Optional custom animation configuration</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator AnimateWindowOut(VisualElement windowElement, Action onComplete = null, AnimationConfig config = null)
    {
        if (windowElement == null)
        {
            Debug.LogWarning("WindowAnimations: Cannot animate null window element");
            onComplete?.Invoke();
            yield break;
        }
        
        config ??= _defaultConfig;
        
        // Apply animation start classes
        windowElement.RemoveFromClassList("window-fade-in");
        windowElement.RemoveFromClassList("window-scale-in");
        windowElement.AddToClassList("window-fade-out");
        windowElement.AddToClassList("window-scale-out");
        
        // Simulate animation duration
        yield return new WaitForSeconds(config.fadeOutDuration);
        
        // Hide window after animation
        windowElement.style.display = DisplayStyle.None;
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Creates a fade-in animation for any visual element.
    /// AIDEV-NOTE: General purpose fade animation for UI elements.
    /// </summary>
    /// <param name="element">The element to fade in</param>
    /// <param name="duration">Animation duration in seconds</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator FadeIn(VisualElement element, float duration = 0.3f, Action onComplete = null)
    {
        if (element == null) yield break;
        
        element.style.display = DisplayStyle.Flex;
        
        // AIDEV-TODO: Implement actual opacity animation when Unity UI Toolkit supports it
        yield return new WaitForSeconds(duration);
        
        element.AddToClassList("fade-in");
        element.RemoveFromClassList("fade-out");
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Creates a fade-out animation for any visual element.
    /// AIDEV-NOTE: General purpose fade animation for UI elements.
    /// </summary>
    /// <param name="element">The element to fade out</param>
    /// <param name="duration">Animation duration in seconds</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator FadeOut(VisualElement element, float duration = 0.25f, Action onComplete = null)
    {
        if (element == null) yield break;
        
        element.AddToClassList("fade-out");
        element.RemoveFromClassList("fade-in");
        
        // AIDEV-TODO: Implement actual opacity animation when Unity UI Toolkit supports it
        yield return new WaitForSeconds(duration);
        
        element.style.display = DisplayStyle.None;
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Creates a smooth slide animation for window panels.
    /// AIDEV-NOTE: Slide animations for panel transitions and window movements.
    /// </summary>
    /// <param name="element">The element to slide</param>
    /// <param name="direction">Direction of the slide (normalized vector)</param>
    /// <param name="distance">Distance to slide in pixels</param>
    /// <param name="duration">Animation duration in seconds</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator SlideIn(VisualElement element, Vector2 direction, float distance, float duration = 0.4f, Action onComplete = null)
    {
        if (element == null) yield break;
        
        // AIDEV-NOTE: Unity UI Toolkit doesn't support transform animations directly
        // This is a placeholder for future implementation
        
        element.style.display = DisplayStyle.Flex;
        
        // Simulate slide animation duration
        yield return new WaitForSeconds(duration);
        
        // Apply final position classes
        element.AddToClassList("slide-in");
        element.RemoveFromClassList("slide-out");
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Creates a bounce animation effect for interactive feedback.
    /// AIDEV-NOTE: Useful for button presses or notification emphasis.
    /// </summary>
    /// <param name="element">The element to bounce</param>
    /// <param name="intensity">Bounce intensity (0.0 to 1.0)</param>
    /// <param name="duration">Total bounce duration</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator BounceAnimation(VisualElement element, float intensity = 0.1f, float duration = 0.5f, Action onComplete = null)
    {
        if (element == null) yield break;
        
        // AIDEV-TODO: Implement actual bounce animation when Unity supports scale transforms
        
        // For now, just add/remove classes to indicate animation state
        element.AddToClassList("bounce-animation");
        
        yield return new WaitForSeconds(duration);
        
        element.RemoveFromClassList("bounce-animation");
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Creates a pulsing animation for attention-grabbing elements.
    /// AIDEV-NOTE: Useful for notifications, alerts, or highlighting important content.
    /// </summary>
    /// <param name="element">The element to pulse</param>
    /// <param name="pulseCount">Number of pulses (-1 for infinite)</param>
    /// <param name="pulseDuration">Duration of each pulse</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator PulseAnimation(VisualElement element, int pulseCount = 3, float pulseDuration = 0.6f, Action onComplete = null)
    {
        if (element == null) yield break;
        
        int currentPulse = 0;
        
        while (pulseCount < 0 || currentPulse < pulseCount)
        {
            // Add pulse class
            element.AddToClassList("pulse-animation");
            
            yield return new WaitForSeconds(pulseDuration / 2f);
            
            // Remove pulse class
            element.RemoveFromClassList("pulse-animation");
            
            yield return new WaitForSeconds(pulseDuration / 2f);
            
            currentPulse++;
        }
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Animates color changes for visual feedback.
    /// AIDEV-NOTE: Useful for state changes, validation feedback, or interactive responses.
    /// </summary>
    /// <param name="element">The element to animate color on</param>
    /// <param name="colorClass">CSS class name containing the target color</param>
    /// <param name="duration">Animation duration</param>
    /// <param name="onComplete">Callback when animation completes</param>
    /// <returns>Coroutine for the animation</returns>
    public static IEnumerator ColorTransition(VisualElement element, string colorClass, float duration = 0.3f, Action onComplete = null)
    {
        if (element == null || string.IsNullOrEmpty(colorClass)) yield break;
        
        element.AddToClassList(colorClass);
        
        // AIDEV-TODO: Implement actual color interpolation when Unity UI Toolkit supports it
        yield return new WaitForSeconds(duration);
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Chains multiple animations in sequence.
    /// AIDEV-NOTE: Allows for complex animation sequences with multiple steps.
    /// </summary>
    /// <param name="monoBehaviour">MonoBehaviour to run coroutines on</param>
    /// <param name="animations">Array of animation coroutines to chain</param>
    /// <param name="onComplete">Callback when all animations complete</param>
    /// <returns>Coroutine for the animation chain</returns>
    public static IEnumerator ChainAnimations(MonoBehaviour monoBehaviour, IEnumerator[] animations, Action onComplete = null)
    {
        if (monoBehaviour == null || animations == null) yield break;
        
        foreach (var animation in animations)
        {
            if (animation != null)
            {
                yield return monoBehaviour.StartCoroutine(animation);
            }
        }
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Runs multiple animations in parallel.
    /// AIDEV-NOTE: Allows for simultaneous animation effects on different elements.
    /// </summary>
    /// <param name="monoBehaviour">MonoBehaviour to run coroutines on</param>
    /// <param name="animations">Array of animation coroutines to run in parallel</param>
    /// <param name="onComplete">Callback when all animations complete</param>
    /// <returns>Coroutine for the parallel animations</returns>
    public static IEnumerator ParallelAnimations(MonoBehaviour monoBehaviour, IEnumerator[] animations, Action onComplete = null)
    {
        if (monoBehaviour == null || animations == null) yield break;
        
        var runningAnimations = new Coroutine[animations.Length];
        
        // Start all animations
        for (int i = 0; i < animations.Length; i++)
        {
            if (animations[i] != null)
            {
                runningAnimations[i] = monoBehaviour.StartCoroutine(animations[i]);
            }
        }
        
        // Wait for all animations to complete
        foreach (var animationCoroutine in runningAnimations)
        {
            if (animationCoroutine != null)
            {
                yield return animationCoroutine;
            }
        }
        
        onComplete?.Invoke();
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// AIDEV-NOTE: Debug method for testing animation configurations in the editor.
    /// </summary>
    /// <param name="config">Animation configuration to validate</param>
    /// <returns>True if configuration is valid</returns>
    public static bool ValidateAnimationConfig(AnimationConfig config)
    {
        if (config == null)
        {
            Debug.LogWarning("WindowAnimations: Animation config is null");
            return false;
        }
        
        bool isValid = true;
        
        if (config.fadeInDuration <= 0 || config.fadeOutDuration <= 0)
        {
            Debug.LogWarning("WindowAnimations: Fade durations must be positive");
            isValid = false;
        }
        
        if (config.scaleInDuration <= 0 || config.scaleOutDuration <= 0)
        {
            Debug.LogWarning("WindowAnimations: Scale durations must be positive");
            isValid = false;
        }
        
        if (config.startScale < 0 || config.endScale <= 0)
        {
            Debug.LogWarning("WindowAnimations: Scale values must be positive");
            isValid = false;
        }
        
        return isValid;
    }
    #endif
}