namespace Conkist.GDK
{
    /// <summary>
    /// Represents a general game event with a name.
    /// Used for events such as game started, game ended, life lost, etc.
    /// </summary>
    public struct GameEvent
    {
        public string EventName;
        public GameEvent(string newName)
        {
            EventName = newName;
        }
        static GameEvent ev;

        /// <summary>
        /// Triggers an event with the specified name.
        /// </summary>
        /// <param name="newName">The name of the event to trigger.</param>
        public static void Trigger(string newName)
        {
            ev.EventName = newName;
            EventManager.TriggerEvent(ev);
        }
    }
}
