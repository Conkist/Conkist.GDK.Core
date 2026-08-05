using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Conkist.GDK
{
    /// <summary>
    /// Determines how OverlayManager resolves an overlay's additive scene.
    /// </summary>
    public enum OverlaySourceType
    {
        Addressable,
        BuildScene
    }

    /// <summary>
    /// Persistent Overlay and Popup Manager.
    /// Manages additive scene overlays, moving their root GameObjects into DontDestroyOnLoad upon first load.
    /// Subsequent calls activate/deactivate cached overlay scenes without reloading.
    /// Adapts directly to the ShowPopupEvent parameter system established in OverlayManager.
    /// </summary>
    [AddComponentMenu("Conkist/UI/OverlayManager")]
    public class OverlayManager : SingletonBehaviour<OverlayManager>,
        EventListener<ShowPopupEvent>,
        EventListener<HidePopupEvent>
    {
        private class OverlayEntry
        {
            public string Name;
            public List<GameObject> RootObjects = new List<GameObject>();
            public List<IOverlayController> Controllers = new List<IOverlayController>();
            public bool IsLoaded;
            public bool IsVisible;
        }

        [Header("Legacy Popup Settings")]
        [Tooltip("The default visual tree template for the legacy popup layout.")]
        [SerializeField] private VisualTreeAsset defaultPopupTemplate;

        [Header("Overlay Loading")]
        [Tooltip("How overlay scenes are resolved when Show/ShowAsync is called without an explicit sourceType override.")]
        [SerializeField] private OverlaySourceType defaultSourceType = OverlaySourceType.Addressable;
        public OverlaySourceType DefaultSourceType => defaultSourceType;

        private readonly Dictionary<string, OverlayEntry> _overlayCache = new Dictionary<string, OverlayEntry>();

        private UIDocument _uiDocument;
        private VisualElement _overlayBg;
        private VisualElement _popupCard;
        private Label _popupTitle;
        private Label _popupMessage;
        private Button _confirmBtn;
        private Button _cancelBtn;

        private Action _onConfirmAction;
        private Action _onCancelAction;
        private bool _isInitialized = false;

        protected override void Awake()
        {
            base.Awake();

            if (_instance != this) return;

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIDocument>();
            }

            if (_uiDocument.panelSettings == null)
            {
                _uiDocument.panelSettings = Resources.Load<PanelSettings>("PanelSettings");
            }

            if (_uiDocument.visualTreeAsset == null && defaultPopupTemplate != null)
            {
                _uiDocument.visualTreeAsset = defaultPopupTemplate;
            }

            InitializeUI();
        }

        private void OnEnable()
        {
            this.Subscribe<ShowPopupEvent>();
            this.Subscribe<HidePopupEvent>();
        }

        private void OnDisable()
        {
            this.Unsubscribe<ShowPopupEvent>();
            this.Unsubscribe<HidePopupEvent>();
        }

        #region Additive Scene Overlay API

        /// <summary>
        /// Asynchronously shows an overlay from an additive scene, passing ShowPopupEvent parameters.
        /// Loads scene on first request, puts root GameObjects into DontDestroyOnLoad, and activates them.
        /// Reuses cached scene objects on subsequent calls.
        /// </summary>
        /// <param name="sourceType">Overrides <see cref="DefaultSourceType"/> for this call only.</param>
        public async UniTask ShowOverlayAsync(string overlayName, ShowPopupEvent popupData = default, OverlaySourceType? sourceType = null)
        {
            if (string.IsNullOrEmpty(overlayName))
            {
                Debug.LogWarning("[OverlayManager] Cannot show overlay with empty or null name.");
                return;
            }

            // Trigger legacy UI Document popup card if title or message is provided
            if (!string.IsNullOrEmpty(popupData.Title) || !string.IsNullOrEmpty(popupData.Message))
            {
                ShowPopup(popupData);
            }

            if (_overlayCache.TryGetValue(overlayName, out var cachedEntry) && cachedEntry.IsLoaded)
            {
                ActivateOverlayEntry(cachedEntry, popupData);
                return;
            }

            var newEntry = new OverlayEntry { Name = overlayName };
            _overlayCache[overlayName] = newEntry;

            bool loadedSuccessfully = await LoadAdditiveSceneAsync(overlayName, newEntry, sourceType ?? defaultSourceType);
            if (loadedSuccessfully)
            {
                newEntry.IsLoaded = true;
                ActivateOverlayEntry(newEntry, popupData);
            }
            else
            {
                // Fallback to legacy popup card if scene load was requested for a popup without an additive scene file
                if (!string.IsNullOrEmpty(popupData.Title) || !string.IsNullOrEmpty(popupData.Message))
                {
                    ShowPopup(popupData);
                }
                else
                {
                    Debug.LogWarning($"[OverlayManager] Overlay scene '{overlayName}' not found; using fallback visual element state.");
                }
                _overlayCache.Remove(overlayName);
            }
        }

        /// <summary>
        /// Shows an overlay (triggers background load if not already cached), passing ShowPopupEvent parameters.
        /// </summary>
        /// <param name="sourceType">Overrides <see cref="DefaultSourceType"/> for this call only.</param>
        public void ShowOverlay(string overlayName, ShowPopupEvent popupData = default, OverlaySourceType? sourceType = null)
        {
            ShowOverlayAsync(overlayName, popupData, sourceType).Forget();
        }

        /// <summary>
        /// Hides the specified overlay scene without unloading its cached GameObjects from DontDestroyOnLoad.
        /// </summary>
        public void HideOverlay(string overlayName)
        {
            if (string.IsNullOrEmpty(overlayName)) return;

            HideLegacyPopup();

            if (_overlayCache.TryGetValue(overlayName, out var entry) && entry.IsLoaded && entry.IsVisible)
            {
                DeactivateOverlayEntry(entry);
            }
        }

        /// <summary>
        /// Hides all active overlays.
        /// </summary>
        public void HideAllOverlays()
        {
            HideLegacyPopup();

            foreach (var kvp in _overlayCache)
            {
                if (kvp.Value.IsLoaded && kvp.Value.IsVisible)
                {
                    DeactivateOverlayEntry(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Checks if an overlay has been loaded into DontDestroyOnLoad cache.
        /// </summary>
        public bool IsOverlayLoaded(string overlayName)
        {
            return !string.IsNullOrEmpty(overlayName) &&
                   _overlayCache.TryGetValue(overlayName, out var entry) &&
                   entry.IsLoaded;
        }

        /// <summary>
        /// Checks if an overlay is currently visible.
        /// </summary>
        public bool IsOverlayVisible(string overlayName)
        {
            return !string.IsNullOrEmpty(overlayName) &&
                   _overlayCache.TryGetValue(overlayName, out var entry) &&
                   entry.IsVisible;
        }

        private async UniTask<bool> LoadAdditiveSceneAsync(string sceneName, OverlayEntry entry, OverlaySourceType sourceType)
        {
            switch (sourceType)
            {
                case OverlaySourceType.Addressable:
                {
                    if (!IsAddressableKeyValid(sceneName))
                    {
                        Debug.LogWarning($"[OverlayManager] No Addressable entry found for overlay '{sceneName}' (source type: Addressable). Skipping overlay — no transition will be shown.");
                        return false;
                    }

                    try
                    {
                        var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                        var sceneInstance = await handle;
                        if (sceneInstance.Scene.IsValid() && sceneInstance.Scene.isLoaded)
                        {
                            ProcessLoadedScene(sceneInstance.Scene, entry);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[OverlayManager] Addressables scene load for '{sceneName}' failed: {ex.Message}");
                    }

                    return false;
                }

                case OverlaySourceType.BuildScene:
                {
                    if (!Application.CanStreamedLevelBeLoaded(sceneName))
                    {
                        Debug.LogWarning($"[OverlayManager] No build/shared scene list entry found for overlay '{sceneName}' (source type: BuildScene). Skipping overlay — no transition will be shown.");
                        return false;
                    }

                    try
                    {
                        var asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                        if (asyncOp == null) return false;

                        await asyncOp;
                        var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
                        if (loadedScene.IsValid() && loadedScene.isLoaded)
                        {
                            ProcessLoadedScene(loadedScene, entry);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[OverlayManager] Standard SceneManager load for '{sceneName}' failed: {ex.Message}");
                    }

                    return false;
                }

                default:
                    return false;
            }
        }

        /// <summary>
        /// Synchronously checks whether an Addressable key resolves to a location, without triggering
        /// Addressables' own InvalidKeyException/error logging for keys that were never registered.
        /// If catalogs aren't loaded yet, this is permissive (returns true) so a valid key isn't skipped on a cold start.
        /// </summary>
        private static bool IsAddressableKeyValid(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            bool anyLocatorLoaded = false;
            foreach (var locator in Addressables.ResourceLocators)
            {
                anyLocatorLoaded = true;
                if (locator.Locate(key, typeof(SceneInstance), out _))
                {
                    return true;
                }
            }

            return !anyLocatorLoaded;
        }

        private void ProcessLoadedScene(Scene scene, OverlayEntry entry)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            entry.RootObjects.Clear();
            entry.Controllers.Clear();

            foreach (var root in rootObjects)
            {
                // Move root GameObjects to DontDestroyOnLoad so they persist across scene changes
                DontDestroyOnLoad(root);
                entry.RootObjects.Add(root);

                // Collect any IOverlayController components attached to root or children
                var controllers = root.GetComponentsInChildren<IOverlayController>(true);
                entry.Controllers.AddRange(controllers);
            }

            Debug.Log($"[OverlayManager] Overlay '{entry.Name}' loaded additively with {entry.RootObjects.Count} root objects cached in DontDestroyOnLoad.");
        }

        private void ActivateOverlayEntry(OverlayEntry entry, ShowPopupEvent popupData)
        {
            foreach (var go in entry.RootObjects)
            {
                if (go != null) go.SetActive(true);
            }

            foreach (var controller in entry.Controllers)
            {
                try
                {
                    controller?.OnOverlayShow(popupData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[OverlayManager] Exception in OnOverlayShow for overlay '{entry.Name}': {ex.Message}");
                }
            }

            entry.IsVisible = true;
        }

        private void DeactivateOverlayEntry(OverlayEntry entry)
        {
            foreach (var controller in entry.Controllers)
            {
                try
                {
                    controller?.OnOverlayHide();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[OverlayManager] Exception in OnOverlayHide for overlay '{entry.Name}': {ex.Message}");
                }
            }

            foreach (var go in entry.RootObjects)
            {
                if (go != null) go.SetActive(false);
            }

            entry.IsVisible = false;
        }

        #endregion

        #region Event Callbacks & Legacy Popup UI

        public void OnEventCallback(ShowPopupEvent eventData)
        {
            ShowPopup(eventData);
        }

        public void OnEventCallback(HidePopupEvent eventData)
        {
            HideLegacyPopup();
        }

        private void InitializeUI()
        {
            if (_isInitialized) return;
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;

            var root = _uiDocument.rootVisualElement;

            _overlayBg = root.Q<VisualElement>("OverlayBackground");
            _popupCard = root.Q<VisualElement>("PopupCard");
            _popupTitle = root.Q<Label>("PopupTitle");
            _popupMessage = root.Q<Label>("PopupMessage");
            _confirmBtn = root.Q<Button>("ConfirmButton");
            _cancelBtn = root.Q<Button>("CancelButton");

            if (_confirmBtn != null) _confirmBtn.clicked += OnConfirmClicked;
            if (_cancelBtn != null) _cancelBtn.clicked += OnCancelClicked;

            if (_overlayBg != null)
            {
                _overlayBg.style.display = DisplayStyle.None;
                _overlayBg.RemoveFromClassList("overlay-bg--active");
            }

            _isInitialized = true;
        }

        private void ShowPopup(ShowPopupEvent data)
        {
            if (!_isInitialized) InitializeUI();
            if (_overlayBg == null)
            {
                Debug.LogWarning("[OverlayManager] Cannot show popup: OverlayBackground is not initialized yet.");
                return;
            }

            if (_popupTitle != null) _popupTitle.text = data.Title;
            if (_popupMessage != null) _popupMessage.text = data.Message;

            if (_confirmBtn != null)
            {
                _confirmBtn.text = string.IsNullOrEmpty(data.ConfirmText) ? "OK" : data.ConfirmText;
                _confirmBtn.style.display = DisplayStyle.Flex;
            }

            if (_cancelBtn != null)
            {
                if (string.IsNullOrEmpty(data.CancelText))
                {
                    _cancelBtn.style.display = DisplayStyle.None;
                }
                else
                {
                    _cancelBtn.text = data.CancelText;
                    _cancelBtn.style.display = DisplayStyle.Flex;
                }
            }

            _onConfirmAction = data.OnConfirm;
            _onCancelAction = data.OnCancel;

            if (_popupCard != null)
            {
                if (data.IsError)
                {
                    _popupCard.AddToClassList("error");
                }
                else
                {
                    _popupCard.RemoveFromClassList("error");
                }
                _popupCard.style.display = DisplayStyle.Flex;
            }

            PlayFadeIn();
        }

        private void PlayFadeIn()
        {
            if (_overlayBg == null) return;

            _overlayBg.style.display = DisplayStyle.Flex;
            _overlayBg.schedule.Execute(() =>
            {
                _overlayBg.AddToClassList("overlay-bg--active");
            }).StartingIn(1);
        }

        private void HideLegacyPopup()
        {
            if (!_isInitialized) InitializeUI();
            if (_overlayBg == null) return;

            _overlayBg.RemoveFromClassList("overlay-bg--active");

            _overlayBg.schedule.Execute(() =>
            {
                if (!_overlayBg.ClassListContains("overlay-bg--active"))
                {
                    _overlayBg.style.display = DisplayStyle.None;
                }
            }).StartingIn(250);
        }

        private void OnConfirmClicked()
        {
            HideLegacyPopup();
            _onConfirmAction?.Invoke();
        }

        private void OnCancelClicked()
        {
            HideLegacyPopup();
            _onCancelAction?.Invoke();
        }

        #endregion
    }
}
