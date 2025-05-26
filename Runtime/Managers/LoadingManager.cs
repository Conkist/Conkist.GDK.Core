using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using Conkist.GDK.Loading;

namespace Conkist.GDK
{
    [CreateAssetMenu(menuName = "Game/ScriptableManagers/LoadingManager", fileName = "LoadingManager")]
    /// <summary>
    /// Manages loading operations and broadcasts loading events using LoadingEvent.
    /// </summary>
    public class LoadingManager : ScriptableObject
    {
        private static LoadingManager _instance;
        public static LoadingManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<LoadingManager>("LoadingManager");
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

        public static async UniTask<Object> LoadAssetAsync(string address, LoadType loadType = LoadType.Hidden)
        {
            if(IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return null;
            }
            StartupLoading(address, loadType);

            var locations = await Addressables.LoadResourceLocationsAsync(address, typeof(Object));
            
            if (locations.Count == 0) return null;

            var asset = await Addressables.LoadAssetAsync<Object>(address);
            _isLoading = false;
            ChangeLoadingState(LoadingStates.InterpolatedLoadProgressComplete);

            return asset;
        }

        public static async UniTask<Object> LoadAssetReferenceAsync(AssetReference assetReference, LoadType loadType = LoadType.Hidden)
        {
            if(IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return null;
            }
            StartupLoading(assetReference.AssetGUID, loadType);

            IAssetsReferenceLoader<Object> loader = new AssetsReferenceLoader<Object>();
            await loader.PreloadAssetAsync(assetReference as AssetReferenceT<Object>);

            if (loader.TryGetAsset(assetReference as AssetReferenceT<Object>, out Object result))
            {
                ChangeLoadingState(LoadingStates.InterpolatedLoadProgressComplete);
                _isLoading = false;
                return result;
            }
            else
            {
                _isLoading = false;
                Debug.LogWarning("No asset loaded");
            }
            return null;
        }

        /// <summary>
        /// Unloads an asset identified by its address.
        /// </summary>
        /// <param name="address">The address of the asset to unload.</param>
        public static void UnloadAsset(string address)
        {
            Addressables.Release(address);
        }

        /// <summary>
        /// Unloads a given asset.
        /// </summary>
        /// <typeparam name="T">Type of the asset.</typeparam>
        /// <param name="asset">The asset to unload.</param>
        public static void UnloadAsset(Object asset)
        {
            Addressables.Release(asset);
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
        /// Asynchronously downloads content from the given address, showing progress and triggering load events.
        /// </summary>
        /// <param name="address">The address to download content from.</param>
        /// <param name="loadType">The type of load.</param>
        public static async UniTask DownloadContentAsync(string address, LoadType loadType = LoadType.FullScreen)
        {
            if(IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return;
            }
            StartupLoading(address, loadType);

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
                _isLoading = false;
                await UniTask.Delay(300, DelayType.Realtime);
                Addressables.Release(address);
            }
        }

        /// <summary>
        /// Asynchronously downloads content from multiple asset labels, showing progress and triggering load events.
        /// </summary>
        /// <param name="assetLabels">Asset labels representing the content to download.</param>
        public static async UniTask DownloadContentAsync(LoadType loadType = LoadType.FullScreen, params AssetLabelReference[] assetLabels)
        {
            if(IsLoading){
                Debug.LogWarning("Manager is currently loading something");
                return;
            }
            StartupLoading(assetLabels.ToString(), loadType);

            var downloadPack = new AssetLabelsDownloadPack(assetLabels);

            downloadPack.TrackProgress(Progress.Create<AssetsDownloadStatus>(DownloadProgress));
            var result = await downloadPack.StartDownloadAsync();

            if (!result.IsSuccess)
            {
                result = await downloadPack.StartDownloadAsync();
            }

            ChangeLoadingState(LoadingStates.LoadProgressComplete);
            _isLoading = false;
            await UniTask.Delay(300, DelayType.Realtime);
            downloadPack.Dispose();
        }

        /// <summary>
        /// Preloads an asset asynchronously.
        /// </summary>
        /// <param name="assetReference">The asset reference.</param>
        /// <returns>A UniTask representing the async operation.</returns>
        public UniTask GetAssetPreloader(AssetReferenceT<Object> assetReference)
        {
            _isLoading = true;
            var loader = new AssetsReferenceLoader<Object>().PreloadAssetAsync(assetReference);
            _isLoading = false;
            return loader;
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
            if(Instance._loadType == loadType) return;
            
            Instance._loadType = loadType;
            LoadingEvents.LoadTypeChangeEvent.Trigger(loadType);
        }

        /// <summary>
        /// Fires a loading event and returns a task based on the load type.
        /// </summary>
        /// <param name="address">The address related to the event.</param>
        /// <param name="states">The load status.</param>
        /// <param name="type">The type of load.</param>
        /// <returns>A UniTask representing the event task.</returns>
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
            catch (InvalidKeyException)
            {
                Debug.LogWarning("The address:" + address + " is not listed in the addressable groups. Try to make a new build if does.");
                return false;
            }
        }
#endregion
    }
}