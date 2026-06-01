using UnityEngine;

namespace Conkist.GDK.Demo
{
    /// <summary>
    /// Demo component for a persistent, keep-oldest singleton.
    /// Survives scene changes, and destroys duplicates to keep the original instance.
    /// </summary>
    [AddComponentMenu("Conkist/Demo/PersistentSingletonDemo")]
    public class PersistentSingletonDemo : SingletonBehaviour<PersistentSingletonDemo>
    {
        public string instanceId;

        protected override void Awake()
        {
            // Initialize a unique ID for this instance
            instanceId = System.Guid.NewGuid().ToString().Substring(0, 8);

            // Configure persistence and keepOldest behavior
            persistent = true;
            keepOldest = true;

            base.Awake();
        }
    }
}
