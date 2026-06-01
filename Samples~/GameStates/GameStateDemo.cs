using UnityEngine;
using UnityEngine.UI;

namespace Conkist.GDK
{
    /// <summary>
    /// Demo component showcasing state machine transition triggers, event handling,
    /// and dynamic UI state sync under the Conkist GDK KISS architecture.
    /// </summary>
    [AddComponentMenu("Conkist/Demos/GameStateDemo")]
    public class GameStateDemo : MonoBehaviour, EventListener<GameStateChangedEvent>
    {
        [Header("UI Text Display")]
        [SerializeField] private Text stateText;

        [Header("State Panels (Optional)")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject pausedPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("IMGUI Helper Panel")]
        [SerializeField] private bool showLegacyOnGUI = true;

        private void OnEnable()
        {
            this.Subscribe<GameStateChangedEvent>();
        }

        private void OnDisable()
        {
            this.Unsubscribe<GameStateChangedEvent>();
        }

        private void Start()
        {
            // Sync UI with current manager state on start
            if (GameStateManager.HasInstance)
            {
                UpdateUI(GameStateManager.Instance.CurrentState);
            }
        }

        /// <summary>
        /// Callback triggered by EventManager whenever a state change occurs.
        /// </summary>
        public void OnEventCallback(GameStateChangedEvent eventData)
        {
            Debug.Log($"[GameStateDemo] Received state change: {eventData.PreviousState} -> {eventData.NewState}");
            UpdateUI(eventData.NewState);
        }

        /// <summary>
        /// Updates the text label and activates the corresponding UI panel.
        /// </summary>
        private void UpdateUI(GameState activeState)
        {
            if (stateText != null)
            {
                stateText.text = $"Current State: {activeState}";
            }

            // Toggle panels depending on active state
            if (mainMenuPanel != null) mainMenuPanel.SetActive(activeState == GameState.MainMenu || activeState == GameState.Boot);
            if (gameplayPanel != null) gameplayPanel.SetActive(activeState == GameState.Gameplay);
            if (pausedPanel != null) pausedPanel.SetActive(activeState == GameState.Paused);
            if (gameOverPanel != null) gameOverPanel.SetActive(activeState == GameState.GameOver);
            if (victoryPanel != null) victoryPanel.SetActive(activeState == GameState.Victory);
            if (defeatPanel != null) defeatPanel.SetActive(activeState == GameState.Defeat);
        }

        #region Public Interface for UI Buttons

        public void TransitionToMainMenu()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.TransitionTo(GameState.MainMenu);
            }
        }

        public void TransitionToGameplay()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.TransitionTo(GameState.Gameplay);
            }
        }

        public void TransitionToPaused()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.TransitionTo(GameState.Paused);
            }
        }

        public void TransitionToGameOver()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.TransitionTo(GameState.GameOver);
            }
        }

        public void TransitionToVictory()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.TransitionTo(GameState.Victory);
            }
        }

        public void TransitionToDefeat()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.TransitionTo(GameState.Defeat);
            }
        }

        #endregion

        #region IMGUI Visualizer / Interaction Overlay

        private void OnGUI()
        {
            if (!showLegacyOnGUI) return;

            // Simple styled debug layout on top-left of the screen
            int width = 240;
            int height = 300;
            Rect windowRect = new Rect(20, 20, width, height);

            GUILayout.BeginArea(windowRect, "GDK GameState Demo", GUI.skin.box);
            GUILayout.Space(25);

            GameState currentState = GameStateManager.HasInstance ? GameStateManager.Instance.CurrentState : GameState.Boot;
            GUILayout.Label($"<b>Active State:</b> {currentState}", new GUIStyle(GUI.skin.label) { richText = true });

            GUILayout.Space(10);
            GUILayout.Label("Trigger State Transition:");

            if (GUILayout.Button("Go to MainMenu")) TransitionToMainMenu();
            if (GUILayout.Button("Go to Gameplay")) TransitionToGameplay();
            if (GUILayout.Button("Go to Paused")) TransitionToPaused();
            if (GUILayout.Button("Go to GameOver")) TransitionToGameOver();
            if (GUILayout.Button("Go to Victory")) TransitionToVictory();
            if (GUILayout.Button("Go to Defeat")) TransitionToDefeat();

            GUILayout.EndArea();
        }

        #endregion
    }
}
