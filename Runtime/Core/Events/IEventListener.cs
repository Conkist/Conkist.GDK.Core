namespace Conkist.GDK
{
    /// <summary>
    /// Base interface for event listeners.
    /// </summary>
    public interface EventListenerBase { }

    /// <summary>
    /// Interface for event listeners of a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    public interface EventListener<TEvent> : EventListenerBase
    {
        /// <summary>
        /// Callback method when the event is triggered.
        /// </summary>
        /// <param name="eventType">The event that was triggered.</param>
        void OnEventCallback(TEvent eventType);
    }
}
