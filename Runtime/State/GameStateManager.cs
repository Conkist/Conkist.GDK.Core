using UnityEngine;

namespace Conkist.GDK
{
    /// <summary>
    /// A simple and robust GameStateManager under the KISS pattern.
    /// Tracks active state and triggers GameStateChangedEvent via the EventManager.
    /// </summary>
    [AddComponentMenu("Conkist/Managers/GameStateManager")]
    public class GameStateManager : SingletonBehaviour<GameStateManager>
    {
        [SerializeField] private GameState initialState = GameState.Boot;

        private GameState _currentState;
        public GameState CurrentState => _currentState;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            _currentState = initialState;
        }

        private void Start()
        {
            if (Instance == this)
            {
                // Trigger the initial state event at start of gameplay
                EventManager.TriggerEvent(new GameStateChangedEvent(GameState.Boot, _currentState));
            }
        }

        /// <summary>
        /// Transitions the game to a new state and broadcasts a GameStateChangedEvent.
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            if (_currentState == newState) return;

            GameState oldState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameStateManager] State transition: {oldState} -> {newState}");
            EventManager.TriggerEvent(new GameStateChangedEvent(oldState, newState));
        }
    }
}
