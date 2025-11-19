using System.Collections;
using UnityEngine;

namespace ArrowPath.Utils
{
    /// <summary>
    /// Reusable component for fading out and destroying GameObjects
    /// Can be used on any GameObject with SpriteRenderer components
    /// </summary>
    public class FadeAndDestroy : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private float fadeOutDuration = 2f;
        [SerializeField] private bool destroyOnComplete = true;
        [SerializeField] private bool fadeOnStart = false;
        
        [Header("Advanced Settings")]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private bool includeChildren = true;
        
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;
        private bool isFading = false;
        private bool isDestroyed = false;
        
        // Events
        public System.Action OnFadeStarted;
        public System.Action OnFadeCompleted;
        public System.Action OnDestroyed;
        
        private void Awake()
        {
            InitializeSpriteRenderers();
        }
        
        private void Start()
        {
            if (fadeOnStart)
            {
                StartFade();
            }
        }
        
        private void InitializeSpriteRenderers()
        {
            if (includeChildren)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            }
            else
            {
                spriteRenderers = GetComponents<SpriteRenderer>();
            }
            
            // Store original colors
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
            }
        }
        
        /// <summary>
        /// Start the fade out process
        /// </summary>
        public void StartFade()
        {
            if (isFading || isDestroyed) return;
            
            StartCoroutine(FadeCoroutine());
        }
        
        /// <summary>
        /// Start fade with custom duration
        /// </summary>
        public void StartFade(float customDuration)
        {
            if (isFading || isDestroyed) return;
            
            float originalDuration = fadeOutDuration;
            fadeOutDuration = customDuration;
            StartCoroutine(FadeCoroutine());
            fadeOutDuration = originalDuration; // Restore original
        }
        
        /// <summary>
        /// Instantly destroy without fade
        /// </summary>
        public void DestroyImmediate()
        {
            if (isDestroyed) return;
            
            isDestroyed = true;
            StopAllCoroutines();
            
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Reset to original state (stop fade, restore colors)
        /// </summary>
        public void ResetFade()
        {
            if (isDestroyed) return;
            
            StopAllCoroutines();
            isFading = false;
            
            // Restore original colors
            for (int i = 0; i < spriteRenderers.Length && i < originalColors.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = originalColors[i];
                }
            }
        }
        
        /// <summary>
        /// Set alpha directly (0-1)
        /// </summary>
        public void SetAlpha(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            
            for (int i = 0; i < spriteRenderers.Length && i < originalColors.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color color = originalColors[i];
                    color.a = originalColors[i].a * alpha;
                    spriteRenderers[i].color = color;
                }
            }
        }
        
        private IEnumerator FadeCoroutine()
        {
            isFading = true;
            OnFadeStarted?.Invoke();
            
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeOutDuration;
                
                // Apply fade curve
                float curveValue = fadeCurve.Evaluate(progress);
                
                // Update alpha for all sprite renderers
                for (int i = 0; i < spriteRenderers.Length && i < originalColors.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                    {
                        Color color = originalColors[i];
                        color.a = originalColors[i].a * curveValue;
                        spriteRenderers[i].color = color;
                    }
                }
                
                yield return null;
            }
            
            // Ensure final alpha is 0
            SetAlpha(0f);
            
            isFading = false;
            OnFadeCompleted?.Invoke();
            
            // Destroy if enabled
            if (destroyOnComplete)
            {
                DestroyImmediate();
            }
        }
        
        // Public properties
        public bool IsFading => isFading;
        public bool IsDestroyed => isDestroyed;
        public float FadeDuration => fadeOutDuration;
        
        // Editor helper
        private void OnValidate()
        {
            fadeOutDuration = Mathf.Max(0.1f, fadeOutDuration);
        }
    }
}