using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Factories
{
    /// <summary>
    /// An asynchronous factory that loads prefabs dynamically via the LoadingManager (Addressables)
    /// and instantiates them with automatic VContainer dependency injection.
    /// This combines dynamic memory/cache management with runtime dependency injection.
    /// </summary>
    /// <typeparam name="T">The type of the component on the prefab to instantiate.</typeparam>
    public class AddressablePrefabFactory<T> : IAsyncFactory<T> where T : Component
    {
        private readonly IObjectResolver _container;
        private readonly string _address;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressablePrefabFactory{T}"/> class.
        /// </summary>
        /// <param name="container">The VContainer object resolver.</param>
        /// <param name="address">The Addressable key or address of the prefab to load.</param>
        public AddressablePrefabFactory(IObjectResolver container, string address)
        {
            _container = container;
            _address = address;
        }

        /// <summary>
        /// Asynchronously loads the prefab using the LoadingManager and instantiates it, resolving all dependencies.
        /// </summary>
        /// <returns>A UniTask returning the instantiated component of type T.</returns>
        public async UniTask<T> CreateAsync()
        {
            if (string.IsNullOrEmpty(_address))
            {
                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Failed to create: Address is null or empty!");
                return null;
            }

            // 1. Asynchronously load the prefab asset using the LoadingManager.
            // This leverages GDK's built-in asset cache, progress tracking, reference counting,
            // and scene-scoped automatic memory cleanup.
            Object asset = await LoadingManager.LoadAssetAsync(_address);
            
            if (asset == null)
            {
                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Failed to load prefab at address: {_address}");
                return null;
            }

            GameObject prefabGo = asset as GameObject;
            if (prefabGo == null)
            {
                // In case the asset returned itself is directly of type T
                if (asset is T componentAsset)
                {
                    return _container.Instantiate(componentAsset);
                }

                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Loaded asset is not a GameObject: {_address}");
                return null;
            }

            T prefabComponent = prefabGo.GetComponent<T>();
            if (prefabComponent == null)
            {
                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Component '{typeof(T).Name}' not found on loaded prefab: {_address}");
                return null;
            }

            // 2. Instantiate and inject dependencies using VContainer
            return _container.Instantiate(prefabComponent);
        }

        /// <summary>
        /// Asynchronously loads the prefab using the LoadingManager and instantiates it under a parent Transform, resolving all dependencies.
        /// </summary>
        /// <param name="parent">The parent Transform to place the instantiated object under.</param>
        /// <returns>A UniTask returning the instantiated component of type T.</returns>
        public async UniTask<T> CreateAsync(Transform parent)
        {
            if (string.IsNullOrEmpty(_address))
            {
                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Failed to create: Address is null or empty!");
                return null;
            }

            Object asset = await LoadingManager.LoadAssetAsync(_address);
            if (asset == null)
            {
                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Failed to load prefab at address: {_address}");
                return null;
            }

            GameObject prefabGo = asset as GameObject;
            if (prefabGo == null)
            {
                if (asset is T componentAsset)
                {
                    return _container.Instantiate(componentAsset, parent);
                }

                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Loaded asset is not a GameObject: {_address}");
                return null;
            }

            T prefabComponent = prefabGo.GetComponent<T>();
            if (prefabComponent == null)
            {
                Debug.LogError($"[AddressablePrefabFactory<{typeof(T).Name}>] Component '{typeof(T).Name}' not found on loaded prefab: {_address}");
                return null;
            }

            return _container.Instantiate(prefabComponent, parent);
        }
    }
}
