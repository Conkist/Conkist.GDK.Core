using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Conkist.GDK
{
    /// <summary>
    /// A robust and simple LoadingScreenController under the KISS pattern.
    /// Listens to loading and progress events and manages the CanvasGroup fade transition.
    /// </summary>
    [AddComponentMenu("Conkist/UI/LoadingScreenController")]
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreenController : MonoBehaviour,
        EventListener<LoadingEvents.LoadingStartEvent>,
        EventListener<LoadingEvents.LoadProgressUpdateEvent>,
        EventListener<LoadingEvents.LoadingStateChangeEvent>
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressBar;

        [Header("Fade Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.4f;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            // Start hidden by default
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (progressBar != null)
            {
                progressBar.value = 0f;
            }
        }

        private void OnEnable()
        {
            this.Subscribe<LoadingEvents.LoadingStartEvent>();
            this.Subscribe<LoadingEvents.LoadProgressUpdateEvent>();
            this.Subscribe<LoadingEvents.LoadingStateChangeEvent>();
        }

        private void OnDisable()
        {
            this.Unsubscribe<LoadingEvents.LoadingStartEvent>();
            this.Unsubscribe<LoadingEvents.LoadProgressUpdateEvent>();
            this.Unsubscribe<LoadingEvents.LoadingStateChangeEvent>();
        }

        #region Event Callbacks

        public void OnEventCallback(LoadingEvents.LoadingStartEvent eventData)
        {
            if (eventData.loadType == LoadType.Hidden)
            {
                // Ignore hidden loads
                return;
            }

            if (progressBar != null)
            {
                progressBar.value = 0f;
            }

            Fade(1f, fadeInDuration);
        }

        public void OnEventCallback(LoadingEvents.LoadProgressUpdateEvent eventData)
        {
            if (progressBar != null)
            {
                progressBar.value = eventData.progress;
            }
        }

        public void OnEventCallback(LoadingEvents.LoadingStateChangeEvent eventData)
        {
            // Exit fade starts or when loading completes
            if (eventData.loadingState == LoadingStates.ExitFade ||
                eventData.loadingState == LoadingStates.LoadTransitionComplete)
            {
                Fade(0f, fadeOutDuration);
            }
        }

        #endregion

        #region Smooth Fading

        private void Fade(float targetAlpha, float duration)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            if (targetAlpha > 0f)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;

            if (targetAlpha <= 0f)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            _fadeCoroutine = null;
        }

        #endregion
    }
}
