using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Conkist.GDK
{
    /// <summary>
    /// A class for managing the lifecycle of a transient instance of a ScriptableObject loaded via Addressables.
    /// The instance is created on first access and destroyed along with its Addressable handle when disposed.
    /// </summary>
    /// <typeparam name="TAsset">The type of the ScriptableObject.</typeparam>
    [Serializable]
    public class ScopedAsset<TAsset> : IDisposable where TAsset : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The addressable reference to instantiate from.")]
        private AssetReferenceT<TAsset> _assetReference;

        private AsyncOperationHandle<TAsset> _loadHandle;
        private TAsset _instance;

        /// <summary>
        /// Gets the cached clone instance. Returns null if not yet loaded.
        /// </summary>
        public TAsset Instance => _instance;

        /// <summary>
        /// Gets a value indicating whether the scoped asset is loaded and instantiated.
        /// </summary>
        public bool IsLoaded => _instance != null;

        /// <summary>
        /// Asynchronously loads the source asset and retrieves/instantiates the cached scoped instance.
        /// </summary>
        public async UniTask<TAsset> GetAssetAsync()
        {
            if (_instance != null) return _instance;

            if (_assetReference == null)
            {
                Debug.LogError("[ScopedAsset] AssetReference is null!");
                return null;
            }

            if (!_loadHandle.IsValid())
            {
                _loadHandle = Addressables.LoadAssetAsync<TAsset>(_assetReference);
            }

            TAsset source = await _loadHandle;
            if (source != null && _instance == null)
            {
                _instance = Object.Instantiate(source);
            }

            return _instance;
        }

        /// <summary>
        /// Disposes the instance of the asset by destroying it and releasing its Addressable handle.
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
