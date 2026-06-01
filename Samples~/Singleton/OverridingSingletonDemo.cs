using UnityEngine;

namespace Conkist.GDK.Demo
{
    /// <summary>
    /// Demo component for a non-persistent, overriding singleton.
    /// Does not survive scene changes, and overrides old instances if duplicates are spawned.
    /// </summary>
    [AddComponentMenu("Conkist/Demo/OverridingSingletonDemo")]
    public class OverridingSingletonDemo : SingletonBehaviour<OverridingSingletonDemo>
    {
        public string instanceId;

        protected override void Awake()
        {
            // Initialize a unique ID for this instance
            instanceId = System.Guid.NewGuid().ToString().Substring(0, 8);

            // Configure non-persistence and overriding behavior
            persistent = false;
            keepOldest = false;

            base.Awake();
        }
    }
}
