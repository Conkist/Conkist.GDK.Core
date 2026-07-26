using System;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK
{
    /// <summary>
    /// Static API facade for showing, hiding, and querying persistent scene-based overlays.
    /// Adapted to the parameter structure configured in OverlayManager (ShowPopupEvent).
    /// Example usage: Overlay.Show(Const.OverlayName.LoadingScreen);
    /// Example usage: Overlay.Show(Const.OverlayName.PopupOverlay, "Title", "Message", "OK", "Cancel", onConfirmAction);
    /// </summary>
    public static class Overlay
    {
        /// <summary>
        /// Shows the specified overlay without popup parameters.
        /// </summary>
        /// <param name="overlayName">The scene name or Addressable address of the overlay.</param>
        public static void Show(string overlayName)
        {
            OverlayManager.Instance.ShowOverlay(overlayName, new ShowPopupEvent());
        }

        /// <summary>
        /// Shows the specified overlay using a ShowPopupEvent parameter struct.
        /// </summary>
        /// <param name="overlayName">The scene name or Addressable address of the overlay.</param>
        /// <param name="popupData">The popup parameter data.</param>
        public static void Show(string overlayName, ShowPopupEvent popupData)
        {
            OverlayManager.Instance.ShowOverlay(overlayName, popupData);
        }

        /// <summary>
        /// Shows the specified overlay with full popup parameters as configured in OverlayManager.
        /// </summary>
        public static void Show(
            string overlayName,
            string title,
            string message,
            string confirmText = "OK",
            string cancelText = "",
            Action onConfirm = null,
            Action onCancel = null,
            bool isError = false)
        {
            var data = new ShowPopupEvent(title, message, confirmText, cancelText, onConfirm, onCancel, isError);
            OverlayManager.Instance.ShowOverlay(overlayName, data);
        }

        /// <summary>
        /// Asynchronously shows the specified overlay without popup parameters.
        /// </summary>
        public static UniTask ShowAsync(string overlayName)
        {
            return OverlayManager.Instance.ShowOverlayAsync(overlayName, new ShowPopupEvent());
        }

        /// <summary>
        /// Asynchronously shows the specified overlay using a ShowPopupEvent parameter struct.
        /// </summary>
        public static UniTask ShowAsync(string overlayName, ShowPopupEvent popupData)
        {
            return OverlayManager.Instance.ShowOverlayAsync(overlayName, popupData);
        }

        /// <summary>
        /// Asynchronously shows the specified overlay with full popup parameters as configured in OverlayManager.
        /// </summary>
        public static UniTask ShowAsync(
            string overlayName,
            string title,
            string message,
            string confirmText = "OK",
            string cancelText = "",
            Action onConfirm = null,
            Action onCancel = null,
            bool isError = false)
        {
            var data = new ShowPopupEvent(title, message, confirmText, cancelText, onConfirm, onCancel, isError);
            return OverlayManager.Instance.ShowOverlayAsync(overlayName, data);
        }

        /// <summary>
        /// Hides and deactivates the specified overlay.
        /// </summary>
        /// <param name="overlayName">The scene name or Addressable address of the overlay.</param>
        public static void Hide(string overlayName)
        {
            OverlayManager.Instance.HideOverlay(overlayName);
        }

        /// <summary>
        /// Hides and deactivates all currently active overlays.
        /// </summary>
        public static void HideAll()
        {
            OverlayManager.Instance.HideAllOverlays();
        }

        /// <summary>
        /// Checks whether the specified overlay scene has been loaded and placed in DontDestroyOnLoad.
        /// </summary>
        /// <param name="overlayName">The scene name or Addressable address of the overlay.</param>
        public static bool IsLoaded(string overlayName)
        {
            return OverlayManager.Instance.IsOverlayLoaded(overlayName);
        }

        /// <summary>
        /// Checks whether the specified overlay is currently visible.
        /// </summary>
        /// <param name="overlayName">The scene name or Addressable address of the overlay.</param>
        public static bool IsVisible(string overlayName)
        {
            return OverlayManager.Instance.IsOverlayVisible(overlayName);
        }
    }
}
