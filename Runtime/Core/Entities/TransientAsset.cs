using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Conkist.GDK
{
    /// <summary>
    /// A class for handling transient addressable assets of a specific type.
    /// Dynamically loads the source asset via Addressables, instantiates a unique clone,
    /// and ensures memory recycling when disposed.
    /// </summary>
    /// <typeparam name="TAsset">The type of the asset.</typeparam>
    [Serializable]
    public class TransientAsset<TAsset> : IDisposable where TAsset : Object
    {
        [SerializeField]
        [Tooltip("The addressable reference to instantiate from.")]
        private AssetReferenceT<TAsset> _assetReference;

        private AsyncOperationHandle<TAsset> _loadHandle;
        private TAsset _instance;

        /// <summary>
        /// Gets the unique instantiated clone. Returns null if not yet loaded.
        /// </summary>
        public TAsset Instance => _instance;

        /// <summary>
        /// Gets a value indicating whether the asset has been loaded and instantiated.
        /// </summary>
        public bool IsInstantiated => _instance != null;

        /// <summary>
        /// Asynchronously loads the source asset via Addressables and instantiates a unique clone.
        /// </summary>
        public async UniTask<TAsset> InstantiateAsync()
        {
            if (_instance != null) return _instance;

            if (_assetReference == null)
            {
                Debug.LogError("[TransientAsset] AssetReference is null!");
                return null;
            }

            if (!_loadHandle.IsValid())
            {
                _loadHandle = Addressables.LoadAssetAsync<TAsset>(_assetReference);
            }

            TAsset source = await _loadHandle;
            if (source != null)
            {
                _instance = Object.Instantiate(source);
            }
            return _instance;
        }

        /// <summary>
        /// Destroys the instantiated clone and releases the Addressable memory handle.
        /// </summary>
        public void Dispose()
        {
            if (_instance != null)
            {
                Object.Destroy(_instance);
                _instance = null;
            }

            if (_loadHandle.IsValid())
            {
                Addressables.Release(_loadHandle);
            }
        }
    }
}
