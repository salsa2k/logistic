using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace LogisticGame.UI.Notifications
{
    // AIDEV-NOTE: Helper class for smooth notification animations and transitions
    public static class NotificationAnimations
    {
        private const float DEFAULT_SHOW_DURATION = 0.3f;
        private const float DEFAULT_DISMISS_DURATION = 0.5f;
        private const float DEFAULT_PROGRESS_UPDATE_INTERVAL = 0.1f;

        public static class Easings
        {
            public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
            public static float EaseInCubic(float t) => t * t * t;
            public static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
            public static float EaseOutBack(float t) => 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
        }

        public static IEnumerator AnimateShow(VisualElement element, float duration = DEFAULT_SHOW_DURATION, 
            Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);
            Vector3 targetScale = Vector3.one;
            float startOpacity = 0f;
            float targetOpacity = 1f;

            element.style.opacity = startOpacity;
            element.style.scale = new Scale(startScale);
            element.AddToClassList("visible");

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseOutBack(t);
                
                element.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, Easings.EaseOutCubic(t));
                element.style.scale = new Scale(Vector3.Lerp(startScale, targetScale, easedT));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.style.opacity = targetOpacity;
            element.style.scale = new Scale(targetScale);
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateDismiss(VisualElement element, float duration = DEFAULT_DISMISS_DURATION, 
            Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            Vector3 startScale = Vector3.one;
            Vector3 targetScale = new Vector3(0.8f, 0.8f, 1f);
            float startOpacity = 1f;
            float targetOpacity = 0f;

            element.AddToClassList("dismissing");

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseInCubic(t);
                
                element.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, easedT);
                element.style.scale = new Scale(Vector3.Lerp(startScale, targetScale, easedT));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.style.opacity = targetOpacity;
            element.style.scale = new Scale(targetScale);
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateSlideInFromBottom(VisualElement element, float distance = 100f, 
            float duration = DEFAULT_SHOW_DURATION, Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            Vector3 startTranslate = new Vector3(0f, distance, 0f);
            Vector3 targetTranslate = Vector3.zero;

            element.style.translate = new Translate(startTranslate.x, startTranslate.y);
            element.style.opacity = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseOutCubic(t);
                
                var lerpedTranslate = Vector3.Lerp(startTranslate, targetTranslate, easedT);
                element.style.translate = new Translate(lerpedTranslate.x, lerpedTranslate.y);
                element.style.opacity = Mathf.Lerp(0f, 1f, easedT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.style.translate = new Translate(targetTranslate.x, targetTranslate.y);
            element.style.opacity = 1f;
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateSlideOutToBottom(VisualElement element, float distance = 100f, 
            float duration = DEFAULT_DISMISS_DURATION, Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            Vector3 startTranslate = Vector3.zero;
            Vector3 targetTranslate = new Vector3(0f, distance, 0f);

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseInCubic(t);
                
                var lerpedTranslate = Vector3.Lerp(startTranslate, targetTranslate, easedT);
                element.style.translate = new Translate(lerpedTranslate.x, lerpedTranslate.y);
                element.style.opacity = Mathf.Lerp(1f, 0f, easedT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.style.translate = new Translate(targetTranslate.x, targetTranslate.y);
            element.style.opacity = 0f;
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateProgressBar(VisualElement progressBar, float duration, 
            Func<bool> isPausedFunc = null, Action onComplete = null)
        {
            if (progressBar == null || duration <= 0f) yield break;

            float elapsed = 0f;
            float pausedTime = 0f;

            while (elapsed < duration)
            {
                bool isPaused = isPausedFunc?.Invoke() ?? false;
                
                if (!isPaused)
                {
                    elapsed += Time.unscaledDeltaTime;
                }
                else
                {
                    pausedTime += Time.unscaledDeltaTime;
                }

                float progress = Mathf.Clamp01(1f - (elapsed / duration));
                progressBar.style.width = Length.Percent(progress * 100f);

                yield return null;
            }

            progressBar.style.width = Length.Percent(0f);
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateStackReposition(VisualElement element, int stackIndex, float spacing, 
            float duration = 0.2f, Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            float targetYOffset = -(stackIndex * spacing);
            
            var startTransform = element.transform.position;
            var targetTransform = new Vector3(startTransform.x, targetYOffset, startTransform.z);

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseOutCubic(t);
                
                var currentTransform = Vector3.Lerp(startTransform, targetTransform, easedT);
                element.transform.position = currentTransform;

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.transform.position = targetTransform;
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateHoverEffect(VisualElement element, bool isHovered, 
            float duration = 0.15f, Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            Vector3 startScale = element.style.scale.value.value;
            Vector3 targetScale = isHovered ? new Vector3(1.02f, 1.02f, 1f) : Vector3.one;
            float startBrightness = isHovered ? 1f : 1.1f;
            float targetBrightness = isHovered ? 1.1f : 1f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseOutCubic(t);
                
                element.style.scale = new Scale(Vector3.Lerp(startScale, targetScale, easedT));
                
                var filter = $"brightness({Mathf.Lerp(startBrightness, targetBrightness, easedT)})";
                // Note: CSS filters would need to be applied through USS or direct style manipulation

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.style.scale = new Scale(targetScale);
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateButtonPress(VisualElement button, float duration = 0.1f, 
            Action onComplete = null)
        {
            if (button == null) yield break;

            Vector3 originalScale = button.style.scale.value.value;
            Vector3 pressedScale = originalScale * 0.95f;

            yield return AnimateScale(button, pressedScale, duration * 0.5f);
            yield return AnimateScale(button, originalScale, duration * 0.5f);
            
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateScale(VisualElement element, Vector3 targetScale, float duration, 
            Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            Vector3 startScale = element.style.scale.value.value;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseOutCubic(t);
                
                element.style.scale = new Scale(Vector3.Lerp(startScale, targetScale, easedT));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.style.scale = new Scale(targetScale);
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateFade(VisualElement element, float targetOpacity, float duration, 
            Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            float startOpacity = element.style.opacity.value;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseInOutCubic(t);
                
                element.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, easedT);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            element.style.opacity = targetOpacity;
            onComplete?.Invoke();
        }

        public static IEnumerator AnimateColorTransition(VisualElement element, Color targetColor, 
            string styleProperty, float duration, Action onComplete = null)
        {
            if (element == null) yield break;

            float elapsed = 0f;
            Color startColor = Color.white;
            
            // Note: Getting current color from USS would require accessing computed styles
            // This is a simplified version that assumes starting from white

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = Easings.EaseInOutCubic(t);
                
                Color currentColor = Color.Lerp(startColor, targetColor, easedT);
                
                switch (styleProperty.ToLower())
                {
                    case "background-color":
                        element.style.backgroundColor = currentColor;
                        break;
                    case "color":
                        element.style.color = currentColor;
                        break;
                    case "border-color":
                        element.style.borderBottomColor = currentColor;
                        element.style.borderTopColor = currentColor;
                        element.style.borderLeftColor = currentColor;
                        element.style.borderRightColor = currentColor;
                        break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}