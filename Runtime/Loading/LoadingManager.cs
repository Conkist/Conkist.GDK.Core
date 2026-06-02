using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using Conkist.GDK.Loading;
using System.Collections.Generic;

namespace Conkist.GDK
{
    [CreateAssetMenu(menuName = "Conkist/Managers/LoadingManager", fileName = "LoadingManager")]
    /// <summary>
    /// Central asset loading broker with automatic caching, reference counting,
    /// and scene-scoped automatic memory cleanup.
    /// </summary>
    public class LoadingManager : ScriptableObject
    {
        private static LoadingManager _instance;
        public static LoadingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LoadingManager>("LoadingManager");
                    if (_instance == null)
                    {
                        // Safe fallback for tests / headless environments where Resources asset doesn't exist
                        _instance = ScriptableObject.CreateInstance<LoadingManager>();
                    }
                }
                return _instance;
            }
        }

        internal static bool _isLoading;
        public static bool IsLoading => _isLoading;
        internal static string _loadAddress;
        internal static bool _ignoreEventsOnHidden;

        private float _loadingProgress;
        public float LoadingProgress => _loadingProgress;

        private LoadingStates _loadingStates;
        public LoadingStates LoadingStates => _loadingStates;
        private LoadType _loadType;

        // Central caching and reference-counting registries
        private static readonly Dictionary<string, AsyncOperationHandle> _loadedHandles = new Dictionary<string, AsyncOperationHandle>();
        private static readonly Dictionary<string, int> _refCounts = new Dictionary<string, int>();
        private static readonly List<string> _sceneScopedKeys = new List<string>();

        /// <summary>
        /// Loads an asset asynchronously with automatic handle caching and reference counting.
        /// </summary>
        public static async UniTask<Object> LoadAssetAsync(string address, LoadType loadType = LoadType.Hidden)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[LoadingManager] Manager is currently loading something else.");
                return null;
            }
            StartupLoading(address, loadType);

            Object asset = null;
            try
            {
                if (_loadedHandles.TryGetValue(address, out var existingHandle))
                {
                    _refCounts[address]++;
                    asset = existingHandle.Result as Object;
                }
                else
                {
                    var handle = Addressables.LoadAssetAsync<Object>(address);
                    _loadedHandles.Add(address, handle);
                    _refCounts.Add(address, 1);
                    _sceneScopedKeys.Add(address);

                    asset = await handle;
                }

                ChangeLoadingState(LoadingStates.InterpolatedLoadProgressComplete);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LoadingManager] Exception caught while loading asset at address '{address}': {ex.Message}");
                _loadedHandles.Remove(address);
                _refCounts.Remove(address);
                _sceneScopedKeys.Remove(address);
                asset = null;
            }
            finally
            {
                _isLoading = false;
            }

            return asset;
        }

        /// <summary>
        /// Loads an AssetReference asynchronously with automatic handle caching and reference counting.
        /// </summary>
        public static async UniTask<Object> LoadAssetReferenceAsync(AssetReference assetReference, LoadType loadType = LoadType.Hidden)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[LoadingManager] Manager is currently loading something else.");
                return null;
            }
            if (assetReference == null) return null;

            string key = assetReference.AssetGUID;
            StartupLoading(key, loadType);

            Object asset = null;
            try
            {
                if (_loadedHandles.TryGetValue(key, out var existingHandle))
                {
                    _refCounts[key]++;
                    asset = existingHandle.Result as Object;
                }
                else
                {
                    var handle = Addressables.LoadAssetAsync<Object>(assetReference);
                    _loadedHandles.Add(key, handle);
                    _refCounts.Add(key, 1);
                    _sceneScopedKeys.Add(key);

                    asset = await handle;
                }

                ChangeLoadingState(LoadingStates.InterpolatedLoadProgressComplete);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LoadingManager] Exception caught while loading asset reference '{key}': {ex.Message}");
                _loadedHandles.Remove(key);
                _refCounts.Remove(key);
                _sceneScopedKeys.Remove(key);
                asset = null;
            }
            finally
            {
                _isLoading = false;
            }
            return asset;
        }

        /// <summary>
        /// Unloads an asset identified by its address, decrementing its reference count.
        /// Fully releases memory only when its reference count drops to 0.
        /// </summary>
        /// <param name="address">The address of the asset to unload.</param>
        public static void UnloadAsset(string address)
        {
            if (string.IsNullOrEmpty(address)) return;

            if (_refCounts.TryGetValue(address, out int count))
            {
                count--;
                if (count <= 0)
                {
                    if (_loadedHandles.TryGetValue(address, out var handle))
                    {
                        if (handle.IsValid()) Addressables.Release(handle);
                        _loadedHandles.Remove(address);
                    }
                    _refCounts.Remove(address);
                    _sceneScopedKeys.Remove(address);
                }
                else
                {
                    _refCounts[address] = count;
                }
            }
            else
            {
                Addressables.Release(address);
            }
        }

        /// <summary>
        /// Unloads a given asset, searching for its registered key.
        /// </summary>
        /// <param name="asset">The asset to unload.</param>
        public static void UnloadAsset(Object asset)
        {
            if (asset == null) return;

            string keyToRelease = null;
            foreach (var kvp in _loadedHandles)
            {
                if (kvp.Value.IsValid() && kvp.Value.Result == asset)
                {
                    keyToRelease = kvp.Key;
                    break;
                }
            }

            if (keyToRelease != null)
            {
                UnloadAsset(keyToRelease);
            }
            else
            {
                Addressables.Release(asset);
            }
        }

        /// <summary>
        /// Unloads an instantiated game object.
        /// </summary>
        /// <param name="instance">The game object instance to unload.</param>
        public static void UnloadInstance(GameObject instance)
        {
            Addressables.ReleaseInstance(instance);
        }

        /// <summary>
        /// Automatically releases all dynamically loaded scene-level assets.
        /// Typically called before transition to a new scene starts.
        /// </summary>
        public static void ReleaseSceneScopedAssets()
        {
            Debug.Log($"[LoadingManager] Releasing {_sceneScopedKeys.Count} scene-scoped assets...");
            
            var keysToUnload = new List<string>(_sceneScopedKeys);
            foreach (var key in keysToUnload)
            {
                if (_loadedHandles.TryGetValue(key, out var handle))
                {
                    if (handle.IsValid()) Addressables.Release(handle);
                    _loadedHandles.Remove(key);
                }
                _refCounts.Remove(key);
            }
            _sceneScopedKeys.Clear();
        }

        /// <summary>
        /// Asynchronously downloads content from the given address, showing progress and triggering load events.
        /// </summary>
        /// <param name="address">The address to download content from.</param>
        /// <param name="loadType">The type of load.</param>
        public static async UniTask DownloadContentAsync(string address, LoadType loadType = LoadType.FullScreen)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[LoadingManager] Manager is currently loading something else.");
                return;
            }
            StartupLoading(address, loadType);

            try
            {
                long size = await Addressables.GetDownloadSizeAsync(address);

                if (size > 0)
                {
                    await UniTask.NextFrame();

                    var download = Addressables.DownloadDependenciesAsync(address)
                        .ToUniTask(Progress.Create<float>(LoadProgress));
                    await download;

                    if (!download.Status.IsCompleted())
                    {
                        download = Addressables.DownloadDependenciesAsync(address)
                        .ToUniTask(Progress.Create<float>(LoadProgress));
                        await download;
                    }

                    ChangeLoadingState(LoadingStates.LoadProgressComplete);
                    await UniTask.Delay(300, DelayType.Realtime);
                    Addressables.Release(address);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LoadingManager] Exception caught while downloading content at address '{address}': {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Asynchronously downloads content from multiple asset labels, showing progress and triggering load events.
        /// </summary>
        /// <param name="assetLabels">Asset labels representing the content to download.</param>
        public static async UniTask DownloadContentAsync(LoadType loadType = LoadType.FullScreen, params AssetLabelReference[] assetLabels)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[LoadingManager] Manager is currently loading something else.");
                return;
            }
            StartupLoading(assetLabels.ToString(), loadType);

            AssetLabelsDownloadPack downloadPack = null;
            try
            {
                downloadPack = new AssetLabelsDownloadPack(assetLabels);

                downloadPack.TrackProgress(Progress.Create<AssetsDownloadStatus>(DownloadProgress));
                var result = await downloadPack.StartDownloadAsync();

                if (!result.IsSuccess)
                {
                    result = await downloadPack.StartDownloadAsync();
                }

                ChangeLoadingState(LoadingStates.LoadProgressComplete);
                await UniTask.Delay(300, DelayType.Realtime);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LoadingManager] Exception caught while downloading content by label pack: {ex.Message}");
            }
            finally
            {
                if (downloadPack != null) downloadPack.Dispose();
                _isLoading = false;
            }
        }

        /// <summary>
        /// Preloads an asset asynchronously.
        /// </summary>
        public static async UniTask PreloadAssetAsync(string address)
        {
            _isLoading = true;
            try
            {
                if (!_loadedHandles.TryGetValue(address, out var handle))
                {
                    handle = Addressables.LoadAssetAsync<Object>(address);
                    _loadedHandles.Add(address, handle);
                    _refCounts.Add(address, 1);
                    _sceneScopedKeys.Add(address);
                }
                await handle;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LoadingManager] Exception caught while preloading asset at address '{address}': {ex.Message}");
                _loadedHandles.Remove(address);
                _refCounts.Remove(address);
                _sceneScopedKeys.Remove(address);
            }
            finally
            {
                _isLoading = false;
            }
        }

        internal static void DownloadProgress(AssetsDownloadStatus status)
        {
            LoadingEvents.DownloadStatusUpdateEvent.Trigger(status);
            LoadProgress(status.PercentProgress);
        }

        internal static void LoadProgress(float progress)
        {
            Instance._loadingProgress = progress;
            LoadingEvents.LoadProgressUpdateEvent.Trigger(progress);
        }

        internal static void ChangeLoadType(LoadType loadType)
        {
            if (Instance._loadType == loadType) return;

            Instance._loadType = loadType;
            LoadingEvents.LoadTypeChangeEvent.Trigger(loadType);
        }

        internal static void ChangeLoadingState(LoadingStates states)
        {
            Instance._loadingStates = states;
            Debug.Log("[LoadingEvent] ChangeState " + states);
            LoadingEvents.LoadingStateChangeEvent.Trigger(_loadAddress, states);
        }

        internal static void StartupLoading(string address, LoadType loadType, string loadingCanvasKey = null, bool ignoreEventsOnHidden = true)
        {
            _isLoading = true;
            _loadAddress = address;
            _ignoreEventsOnHidden = ignoreEventsOnHidden;

            ChangeLoadType(loadType);
            ChangeLoadingState(LoadingStates.LoadStarted);
            EventManager.TriggerEvent(new LoadingEvents.LoadingCanvasSetEvent(loadingCanvasKey));
            EventManager.TriggerEvent(new LoadingEvents.LoadingStartEvent(address, loadType));
        }

        #region EXTENSIONS
        /// <summary>
        /// Clears the cache for a given address.
        /// </summary>
        /// <param name="address">The address to clear the cache for.</param>
        public static void ClearCache(string address)
        {
            Addressables.ClearDependencyCacheAsync(address);
        }

        /// <summary>
        /// Asynchronously checks if a given address is in the cache.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <returns>True if in cache, otherwise false.</returns>
        public static async UniTask<bool> InCacheAsync(string address)
        {
            try
            {
                var getSizeAsyncOp = Addressables.GetDownloadSizeAsync(address).Task.AsUniTask();
                long result = await getSizeAsyncOp;
                return result > 0;
            }
            catch (UnityEngine.AddressableAssets.InvalidKeyException)
            {
                Debug.LogWarning("The address:" + address + " is not listed in the addressable groups. Try to make a new build if does.");
                return false;
            }
        }
        #endregion
    }
}