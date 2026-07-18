namespace Conkist.GDK
{
    /// <summary>
    /// Provides extension methods for event registration and unregistration.
    /// </summary>
    public static class EventRegister
    {
        public delegate void Delegate<T>(T eventType);

        /// <summary>
        /// Subscribes this listener to receive broadcast events of type EventType.
        /// </summary>
        public static void Subscribe<EventType>(this EventListener<EventType> caller) where EventType : struct
        {
            EventManager.AddListener<EventType>(caller);
        }

        /// <summary>
        /// Unsubscribes this listener from receiving broadcast events of type EventType.
        /// </summary>
        public static void Unsubscribe<EventType>(this EventListener<EventType> caller) where EventType : struct
        {
            EventManager.RemoveListener<EventType>(caller);
        }
    }
}
