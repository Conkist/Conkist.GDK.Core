using System;

namespace Conkist.GDK
{
    /// <summary>
    /// Event broadcasted to request showing a popup dialog overlay.
    /// </summary>
    public struct ShowPopupEvent
    {
        public string Title;
        public string Message;
        public string ConfirmText;
        public string CancelText;
        public Action OnConfirm;
        public Action OnCancel;
        public bool IsError;

        public ShowPopupEvent(string title, string message, string confirmText = "OK", string cancelText = "", Action onConfirm = null, Action onCancel = null, bool isError = false)
        {
            Title = title;
            Message = message;
            ConfirmText = confirmText;
            CancelText = cancelText;
            OnConfirm = onConfirm;
            OnCancel = onCancel;
            IsError = isError;
        }
    }

    /// <summary>
    /// Event broadcasted to request hiding the currently visible popup dialog overlay.
    /// </summary>
    public struct HidePopupEvent { }
}
