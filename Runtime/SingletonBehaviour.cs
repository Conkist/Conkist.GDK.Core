using UnityEngine;

namespace Conkist.GDK
{
    /// <summary>
    /// A generic Singleton base class for Unity MonoBehaviour components.
    /// Ensures only one instance of the component exists in the scene.
    /// </summary>
    /// <typeparam name="T">Type of the component inheriting from this Singleton class.</typeparam>
    [DefaultExecutionOrder(-100)]
    public class SingletonBehaviour<T> : MonoBehaviour where T : Component
    {
        // The singleton instance
        protected static T _instance;

        // Flags to control the singleton behavior
        public bool persistent = true;
        public bool keepOldest = true;

        /// <summary>
        /// Gets a value indicating whether an instance of the Singleton exists.
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Attempts to get the instance of the Singleton. Returns null if no instance exists.
        /// </summary>
        public static T TryGetInstance() => HasInstance ? _instance : null;

        /// <summary>
        /// Gets the singleton instance, creating it if it doesn't already exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    InitializeInstance();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the singleton instance, if necessary.
        /// </summary>
        protected static void InitializeInstance()
        {
            _instance = FindObjectOfType<T>();
            if (_instance == null)
            {
                GameObject obj = new GameObject { name = $"{typeof(T).Name}_AutoCreated" };
                _instance = obj.AddComponent<T>();
            }
        }

        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            HandleSingletonInstance();
            SetPersistency(persistent);
        }

        /// <summary>
        /// Manages the singleton instance, ensuring only one exists based on the configuration.
        /// </summary>
        protected void HandleSingletonInstance()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                if (_instance is SingletonBehaviour<T> existingInstance && existingInstance.keepOldest)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(_instance.gameObject);
                    _instance = this as T;
                }
            }
        }

        /// <summary>
        /// Sets the singleton object to persist across scenes, if configured to do so.
        /// </summary>
        /// <param name="shouldPersist">If true, the object will persist between scene loads.</param>
        protected void SetPersistency(bool shouldPersist)
        {
            if (shouldPersist && _instance != null)
            {
                _instance.transform.SetParent(null);
                DontDestroyOnLoad(_instance.gameObject);
            }
        }
    }
}
