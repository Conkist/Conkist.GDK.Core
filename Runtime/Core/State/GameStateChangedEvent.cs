namespace Conkist.GDK
{
    /// <summary>
    /// Broadcast when the active GameState changes.
    /// Subscribe via EventManager.AddListener<GameStateChangedEvent>(listener).
    /// </summary>
    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState NewState;

        public GameStateChangedEvent(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }
}
