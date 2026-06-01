using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK
{
    /// <summary>
    /// Demo component showcasing scene loading, asset downloads,
    /// simulated transitions, and loading event tracking under the Conkist GDK framework.
    /// </summary>
    [AddComponentMenu("Conkist/Demos/LoadingTransitionDemo")]
    public class LoadingTransitionDemo : MonoBehaviour,
        EventListener<LoadingEvents.LoadingStartEvent>,
        EventListener<LoadingEvents.LoadProgressUpdateEvent>,
        EventListener<LoadingEvents.LoadingStateChangeEvent>
    {
        [Header("Scene Transition Settings")]
        [SerializeField] private AssetReferenceScene targetSceneAddress;
        [SerializeField] private LoadType defaultLoadType = LoadType.FullScreen;

        [Header("Asset Loading Settings")]
        [SerializeField] private UnityEngine.AddressableAssets.AssetReference prefabReference;
        [SerializeField] private UnityEngine.AddressableAssets.AssetLabelReference targetLabel;

        [Header("Simulated Load Settings")]
        [Range(1f, 10f)]
        [SerializeField] private float simulatedLoadDuration = 3f;

        [Header("IMGUI Helper Panel")]
        [SerializeField] private bool showLegacyOnGUI = true;

        private bool _isSimulating;

        private void OnEnable()
        {
            this.Subscribe<LoadingEvents.LoadingStartEvent>();
            this.Subscribe<LoadingEvents.LoadProgressUpdateEvent>();
            this.Subscribe<LoadingEvents.LoadingStateChangeEvent>();
        }

        private void OnDisable()
        {
            this.Unsubscribe<LoadingEvents.LoadingStartEvent>();
            this.Unsubscribe<LoadingEvents.LoadProgressUpdateEvent>();
            this.Unsubscribe<LoadingEvents.LoadingStateChangeEvent>();
        }

        #region Transition Execution Methods

        /// <summary>
        /// Triggers a simulated loading cycle. Handy for testing the UI and transition screen
        /// without needing multi-scene setups or addressable assets configured.
        /// </summary>
        public void StartSimulatedLoad()
        {
            if (_isSimulating || LoadingManager.IsLoading)
            {
                Debug.LogWarning("[LoadingTransitionDemo] A load or simulation is already in progress.");
                return;
            }

            SimulateLoadAsync(simulatedLoadDuration, defaultLoadType).Forget();
        }

        private async UniTaskVoid SimulateLoadAsync(float duration, LoadType loadType)
        {
            _isSimulating = true;
            Debug.Log($"[LoadingTransitionDemo] Starting simulated load of {duration}s using {loadType}...");

            // Step 1: Fire Start event
            EventManager.TriggerEvent(new LoadingEvents.LoadingStartEvent("SimulatedAsset", loadType));
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent("SimulatedAsset", LoadingStates.LoadStarted));
            
            await UniTask.Delay(300, DelayType.Realtime); // Brief delay matching entry transition

            // Step 2: Loop to simulate progress loading
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                // Broadcast progress
                EventManager.TriggerEvent(new LoadingEvents.LoadProgressUpdateEvent(progress));
                await UniTask.Yield();
            }

            // Ensure 100% is dispatched
            EventManager.TriggerEvent(new LoadingEvents.LoadProgressUpdateEvent(1f));
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent("SimulatedAsset", LoadingStates.LoadProgressComplete));

            await UniTask.Delay(400, DelayType.Realtime); // Brief pause at 100%

            // Step 3: Trigger exit fade sequence
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent("SimulatedAsset", LoadingStates.ExitFade));
            
            await UniTask.Delay(500, DelayType.Realtime); // Wait for exit fade to finish
            
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent("SimulatedAsset", LoadingStates.LoadTransitionComplete));

            _isSimulating = false;
            Debug.Log("[LoadingTransitionDemo] Simulated load completed.");
        }

        /// <summary>
        /// Reloads the currently active scene using the SceneManager system.
        /// </summary>
        public void ReloadCurrentScene()
        {
            if (_isSimulating || LoadingManager.IsLoading)
            {
                Debug.LogWarning("[LoadingTransitionDemo] Cannot reload scene: a load operation is active.");
                return;
            }

            Debug.Log("[LoadingTransitionDemo] Reloading active scene...");
            SceneManager.ReloadScene(defaultLoadType).Forget();
        }

        /// <summary>
        /// Transitions to the target scene using the SceneManager.
        /// Requires the target scene to be configured/registered in Addressables if address-based.
        /// </summary>
        public void LoadTargetScene()
        {
            if (targetSceneAddress == null || !targetSceneAddress.RuntimeKeyIsValid())
            {
                Debug.LogError("[LoadingTransitionDemo] Target Scene Address is invalid or not assigned.");
                return;
            }

            if (_isSimulating || LoadingManager.IsLoading)
            {
                Debug.LogWarning("[LoadingTransitionDemo] Cannot load scene: a load operation is active.");
                return;
            }

            Debug.Log($"[LoadingTransitionDemo] Transitioning to: {targetSceneAddress.RuntimeKey}...");
            SceneManager.LoadSceneAsync(targetSceneAddress, defaultLoadType).Forget();
        }

        /// <summary>
        /// Loads the referenced scene using standard Unity SceneManager (requires scene in Build Settings, but works in Editor without Addressables build!).
        /// </summary>
        public void LoadReferencedSceneStandard()
        {
            if (targetSceneAddress == null || !targetSceneAddress.RuntimeKeyIsValid())
            {
                Debug.LogError("[LoadingTransitionDemo] Target Scene Address is invalid or not assigned.");
                return;
            }

            if (_isSimulating || LoadingManager.IsLoading)
            {
                Debug.LogWarning("[LoadingTransitionDemo] Cannot load scene: a load operation is active.");
                return;
            }

#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(targetSceneAddress.AssetGUID);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[LoadingTransitionDemo] Could not resolve GUID to asset path in Editor. Make sure the scene asset exists.");
                return;
            }
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            LoadSceneStandardAsync(sceneName).Forget();
#else
            Debug.LogError("[LoadingTransitionDemo] Standard scene load from AssetReference is only supported in Editor mode in this demo. For builds, please use Addressables loading.");
#endif
        }

        private async UniTaskVoid LoadSceneStandardAsync(string sceneName)
        {
            _isSimulating = true;
            Debug.Log($"[LoadingTransitionDemo] Loading scene '{sceneName}' via standard SceneManager...");
            
            // Step 1: Trigger transition start
            EventManager.TriggerEvent(new LoadingEvents.LoadingStartEvent(sceneName, defaultLoadType));
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(sceneName, LoadingStates.LoadStarted));

            await UniTask.Delay(300, DelayType.Realtime);

            // Step 2: Start Unity Async Load
            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[LoadingTransitionDemo] Failed to load scene '{sceneName}'. Make sure it is added to Build Settings!");
                EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(sceneName, LoadingStates.ExitFade));
                _isSimulating = false;
                return;
            }
            
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                EventManager.TriggerEvent(new LoadingEvents.LoadProgressUpdateEvent(op.progress));
                await UniTask.Yield();
            }

            // Progress complete
            EventManager.TriggerEvent(new LoadingEvents.LoadProgressUpdateEvent(1f));
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(sceneName, LoadingStates.LoadProgressComplete));

            await UniTask.Delay(300, DelayType.Realtime);

            // Activate scene
            op.allowSceneActivation = true;
            
            while (!op.isDone)
            {
                await UniTask.Yield();
            }

            // Step 3: Trigger exit fade sequence
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(sceneName, LoadingStates.ExitFade));
            await UniTask.Delay(500, DelayType.Realtime);
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(sceneName, LoadingStates.LoadTransitionComplete));

            _isSimulating = false;
        }

        private GameObject _spawnedPrefab;

        /// <summary>
        /// Loads a prefab by direct AssetReference and instantiates it in the scene.
        /// </summary>
        public void LoadPrefabDirect()
        {
            if (prefabReference == null || !prefabReference.RuntimeKeyIsValid())
            {
                Debug.LogError("[LoadingTransitionDemo] Prefab Reference is invalid or not assigned.");
                return;
            }

            if (_spawnedPrefab != null)
            {
                Debug.LogWarning("[LoadingTransitionDemo] Prefab is already loaded and spawned. Unloading it first...");
                UnloadSpawnedPrefab();
            }

            LoadPrefabDirectAsync().Forget();
        }

        private async UniTaskVoid LoadPrefabDirectAsync()
        {
            Debug.Log($"[LoadingTransitionDemo] Loading prefab directly via AssetReference...");
            
            // Use the loading manager to load the asset reference with a Quick loading overlay
            Object loadedAsset = await LoadingManager.LoadAssetReferenceAsync(prefabReference, LoadType.Quick);
            
            if (loadedAsset is GameObject prefabGo)
            {
                _spawnedPrefab = Instantiate(prefabGo);
                _spawnedPrefab.transform.position = Vector3.up * 2f; // Position it slightly above ground
                Debug.Log("[LoadingTransitionDemo] Prefab loaded and instantiated successfully.");
            }
            else
            {
                Debug.LogError("[LoadingTransitionDemo] Loaded asset is not a valid GameObject.");
            }

            // Explicitly trigger exit fade since asset loading doesn't do it automatically
            await UniTask.Delay(100, DelayType.Realtime);
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(prefabReference.AssetGUID, LoadingStates.ExitFade));
            await UniTask.Delay(500, DelayType.Realtime);
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(prefabReference.AssetGUID, LoadingStates.LoadTransitionComplete));
        }

        /// <summary>
        /// Unloads the instantiated prefab and frees up the memory handle.
        /// </summary>
        public void UnloadSpawnedPrefab()
        {
            if (_spawnedPrefab == null) return;

            Debug.Log("[LoadingTransitionDemo] Unloading and destroying spawned prefab...");
            Destroy(_spawnedPrefab);
            _spawnedPrefab = null;

            // Notify LoadingManager to decrement reference count and release if needed
            LoadingManager.UnloadAsset(prefabReference.AssetGUID);
        }

        /// <summary>
        /// Pre-downloads all dependencies and assets associated with a specific Addressables Label.
        /// </summary>
        public void DownloadAssetsByLabel()
        {
            if (targetLabel == null || string.IsNullOrEmpty(targetLabel.labelString))
            {
                Debug.LogError("[LoadingTransitionDemo] Target Label is empty or invalid.");
                return;
            }

            if (_isSimulating || LoadingManager.IsLoading)
            {
                Debug.LogWarning("[LoadingTransitionDemo] Cannot download: an operation is active.");
                return;
            }

            DownloadAssetsByLabelAsync().Forget();
        }

        private async UniTaskVoid DownloadAssetsByLabelAsync()
        {
            Debug.Log($"[LoadingTransitionDemo] Downloading assets tagged with label '{targetLabel.labelString}'...");
            await LoadingManager.DownloadContentAsync(defaultLoadType, targetLabel);
            Debug.Log($"[LoadingTransitionDemo] Downloads completed for label '{targetLabel.labelString}'.");

            // Explicitly trigger exit fade since download doesn't do it automatically
            await UniTask.Delay(100, DelayType.Realtime);
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(targetLabel.labelString, LoadingStates.ExitFade));
            await UniTask.Delay(500, DelayType.Realtime);
            EventManager.TriggerEvent(new LoadingEvents.LoadingStateChangeEvent(targetLabel.labelString, LoadingStates.LoadTransitionComplete));
        }

        #endregion

        #region Event Callbacks

        public void OnEventCallback(LoadingEvents.LoadingStartEvent eventData)
        {
            Debug.Log($"[LoadingTransitionDemo] Loading Started: Address = '{eventData.loadingAssetAddress}', Type = {eventData.loadType}");
        }

        public void OnEventCallback(LoadingEvents.LoadProgressUpdateEvent eventData)
        {
            Debug.Log($"[LoadingTransitionDemo] Progress Update: {eventData.progress * 100f:F1}%");
        }

        public void OnEventCallback(LoadingEvents.LoadingStateChangeEvent eventData)
        {
            Debug.Log($"[LoadingTransitionDemo] State Change: State = {eventData.loadingState} for '{eventData.loadingAssetAddress}'");
        }

        #endregion

        #region IMGUI Overlay

        private void OnGUI()
        {
            if (!showLegacyOnGUI) return;

            // Simple styled debug layout on top-right of the screen
            int width = 280;
            int height = 500;
            Rect windowRect = new Rect(Screen.width - width - 20, 20, width, height);

            GUILayout.BeginArea(windowRect, "GDK Loading Transition Demo", GUI.skin.box);
            GUILayout.Space(25);

            bool isCurrentlyLoading = LoadingManager.IsLoading || _isSimulating;
            GUILayout.Label($"<b>Status:</b> {(isCurrentlyLoading ? "<color=orange>LOADING</color>" : "<color=green>READY</color>")}", new GUIStyle(GUI.skin.label) { richText = true });

            GUILayout.Space(10);
            GUILayout.Label("Simulated Load Settings:");
            simulatedLoadDuration = GUILayout.HorizontalSlider(simulatedLoadDuration, 1f, 10f);
            GUILayout.Label($"Simulate Duration: {simulatedLoadDuration:F1}s");

            GUILayout.Space(10);
            GUILayout.Label("<b>Trigger Operations:</b>", new GUIStyle(GUI.skin.label) { richText = true });

            if (GUILayout.Button("Simulate Fade + Progress")) StartSimulatedLoad();
            if (GUILayout.Button("Reload Current Scene")) ReloadCurrentScene();
            
            GUILayout.Space(5);
            GUILayout.Label("<b>Scene Load Target:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            string currentAddressName = (targetSceneAddress != null && targetSceneAddress.RuntimeKeyIsValid()) 
                ? targetSceneAddress.RuntimeKey.ToString() 
                : "None (Assign in Inspector)";
            GUILayout.Label($"Selected Scene GUID: {currentAddressName}", new GUIStyle(GUI.skin.label) { wordWrap = true });

            if (GUILayout.Button("Load Referenced Scene (Addressables)")) LoadTargetScene();
            if (GUILayout.Button("Load Ref Scene (Standard / Build Settings)")) LoadReferencedSceneStandard();

            GUILayout.Space(5);
            GUILayout.Label("<b>Asset Reference Spawn:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            string prefabStateName = (prefabReference != null && prefabReference.RuntimeKeyIsValid())
                ? prefabReference.RuntimeKey.ToString()
                : "None (Assign in Inspector)";
            GUILayout.Label($"Prefab: {prefabStateName}", new GUIStyle(GUI.skin.label) { wordWrap = true });
            
            if (_spawnedPrefab == null)
            {
                if (GUILayout.Button("Spawn Prefab Reference")) LoadPrefabDirect();
            }
            else
            {
                if (GUILayout.Button("Unload Spawned Prefab")) UnloadSpawnedPrefab();
            }

            GUILayout.Space(5);
            GUILayout.Label("<b>Label Bundle Download:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            string labelName = (targetLabel != null && !string.IsNullOrEmpty(targetLabel.labelString))
                ? targetLabel.labelString
                : "None (Assign in Inspector)";
            GUILayout.Label($"Label: {labelName}");

            if (GUILayout.Button("Download Label Dependencies")) DownloadAssetsByLabel();

            GUILayout.EndArea();
        }

        #endregion
    }
}
