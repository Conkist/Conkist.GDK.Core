namespace Conkist.GDK
{
    /// <summary>
    /// Interface for components inside an additive overlay scene to receive OverlayManager parameters and lifecycle events.
    /// </summary>
    public interface IOverlayController
    {
        /// <summary>
        /// Called when the overlay is shown, passing the ShowPopupEvent parameter data.
        /// </summary>
        /// <param name="data">The popup parameter data (title, message, buttons, callbacks, error state).</param>
        void OnOverlayShow(ShowPopupEvent data);

        /// <summary>
        /// Called when the overlay is hidden via Overlay.Hide.
        /// </summary>
        void OnOverlayHide();
    }
}
