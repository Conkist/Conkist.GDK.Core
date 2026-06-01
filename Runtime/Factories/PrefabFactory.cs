using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Conkist.GDK.Factories
{
    /// <summary>
    /// A specialized Unity factory that instantiates pre-defined prefabs at runtime
    /// and automatically resolves and injects their dependencies using VContainer.
    /// </summary>
    /// <typeparam name="T">The type of the component on the prefab to instantiate.</typeparam>
    public class PrefabFactory<T> : IFactory<T> where T : Component
    {
        private readonly IObjectResolver _container;
        private readonly T _prefab;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefabFactory{T}"/> class.
        /// </summary>
        /// <param name="container">The VContainer object resolver.</param>
        /// <param name="prefab">The prefab component template to instantiate from.</param>
        public PrefabFactory(IObjectResolver container, T prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        /// <summary>
        /// Instantiates the prefab in the scene and automatically resolves and injects all its dependencies.
        /// </summary>
        /// <returns>The instantiated prefab component of type T.</returns>
        public T Create()
        {
            if (_prefab == null)
            {
                Debug.LogError($"[PrefabFactory<{typeof(T).Name}>] Failed to create: Prefab template is null!");
                return null;
            }

            // VContainer Unity extension method 'Instantiate' is used to create the clone 
            // and programmatically inject dependencies into it immediately after instantiation.
            return _container.Instantiate(_prefab);
        }

        /// <summary>
        /// Instantiates the prefab in the scene under a parent Transform and injects all dependencies.
        /// </summary>
        /// <param name="parent">The parent Transform to place the instantiated object under.</param>
        /// <returns>The instantiated prefab component of type T.</returns>
        public T Create(Transform parent)
        {
            if (_prefab == null)
            {
                Debug.LogError($"[PrefabFactory<{typeof(T).Name}>] Failed to create: Prefab template is null!");
                return null;
            }

            return _container.Instantiate(_prefab, parent);
        }
    }
}
