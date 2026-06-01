namespace Conkist.GDK
{
    /// <summary>
    /// Base game states under the KISS pattern.
    /// </summary>
    public enum GameState
    {
        Boot,
        MainMenu,
        Loading,
        Gameplay,
        Paused,
        GameOver,
        Victory,
        Defeat
    }

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
