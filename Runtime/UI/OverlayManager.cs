using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Conkist.GDK
{
    /// <summary>
    /// Persistent Overlay and Popup Manager.
    /// Pre-instantiates overlays in DontDestroyOnLoad and displays them immediately when events are triggered.
    /// </summary>
    [AddComponentMenu("Conkist/UI/OverlayManager")]
    [RequireComponent(typeof(UIDocument))]
    public class OverlayManager : SingletonBehaviour<OverlayManager>,
        EventListener<ShowPopupEvent>,
        EventListener<HidePopupEvent>
    {
        [Header("Assets")]
        [Tooltip("The default visual tree template for the popup layout.")]
        [SerializeField] private VisualTreeAsset defaultPopupTemplate;

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

            // Guard in case Singleton destroyed the object
            if (_instance != this) return;

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIDocument>();
            }

            // Load default PanelSettings if none set
            if (_uiDocument.panelSettings == null)
            {
                _uiDocument.panelSettings = Resources.Load<PanelSettings>("PanelSettings");
            }

            // Assign default template if none already assigned in Inspector
            if (_uiDocument.visualTreeAsset == null && defaultPopupTemplate != null)
            {
                _uiDocument.visualTreeAsset = defaultPopupTemplate;
            }

            InitializeUI();
        }

        private void OnEnable()
        {
            // Register to global event listener system
            this.Subscribe<ShowPopupEvent>();
            this.Subscribe<HidePopupEvent>();
        }

        private void OnDisable()
        {
            // Unregister from event system
            this.Unsubscribe<ShowPopupEvent>();
            this.Unsubscribe<HidePopupEvent>();
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

            // Register button events
            if (_confirmBtn != null) _confirmBtn.clicked += OnConfirmClicked;
            if (_cancelBtn != null) _cancelBtn.clicked += OnCancelClicked;

            // Start completely hidden and inactive
            if (_overlayBg != null)
            {
                _overlayBg.style.display = DisplayStyle.None;
                _overlayBg.RemoveFromClassList("overlay-bg--active");
            }

            _isInitialized = true;
        }

        #region Event Callbacks

        public void OnEventCallback(ShowPopupEvent eventData)
        {
            ShowPopup(eventData);
        }

        public void OnEventCallback(HidePopupEvent eventData)
        {
            Hide();
        }

        #endregion

        #region Overlay Methods

        private void ShowPopup(ShowPopupEvent data)
        {
            if (!_isInitialized) InitializeUI();
            if (_overlayBg == null)
            {
                Debug.LogWarning("[OverlayManager] Cannot show popup: OverlayBackground is not initialized yet.");
                return;
            }

            // Bind values
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

            // Set Error accent styling
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
            }

            // Ensure Popup UI is shown
            if (_popupCard != null) _popupCard.style.display = DisplayStyle.Flex;

            PlayFadeIn();
        }

        private void PlayFadeIn()
        {
            if (_overlayBg == null) return;

            // Enable element display and activate CSS smooth transition in the next frame
            _overlayBg.style.display = DisplayStyle.Flex;
            _overlayBg.schedule.Execute(() =>
            {
                _overlayBg.AddToClassList("overlay-bg--active");
            }).StartingIn(1);
        }

        private void Hide()
        {
            if (!_isInitialized) InitializeUI();
            if (_overlayBg == null) return;

            _overlayBg.RemoveFromClassList("overlay-bg--active");

            // Wait for smooth opacity fade-out transition to complete (~250ms) before hiding display
            _overlayBg.schedule.Execute(() =>
            {
                if (!_overlayBg.ClassListContains("overlay-bg--active"))
                {
                    _overlayBg.style.display = DisplayStyle.None;
                }
            }).StartingIn(250);
        }

        #endregion

        #region Buttons Handles

        private void OnConfirmClicked()
        {
            Hide();
            _onConfirmAction?.Invoke();
        }

        private void OnCancelClicked()
        {
            Hide();
            _onCancelAction?.Invoke();
        }

        #endregion
    }
}
