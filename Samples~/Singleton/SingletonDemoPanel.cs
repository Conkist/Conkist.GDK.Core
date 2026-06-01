using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Demo
{
    /// <summary>
    /// Interactive visual panel that lets users inspect, duplicate, and reload scenes
    /// to observe various GDK Singleton patterns in real-time.
    /// Supports both standard scene loading and modern Addressables-based scene loading via AssetReference.
    /// </summary>
    [AddComponentMenu("Conkist/Demo/SingletonDemoPanel")]
    public class SingletonDemoPanel : MonoBehaviour
    {
        [Header("Addressables Settings")]
        [SerializeField] private AssetReferenceScene demoSceneReference; // Scene as Addressable Reference

        private void OnGUI()
        {
            // Position the singleton debug panel to the right of the Audio debug panel
            GUILayout.BeginArea(new Rect(300, 10, 300, 460));
            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>Conkist GDK - Singleton Patterns Demo</b>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 });
            GUILayout.Space(10);

            // 1. Persistent Singleton (Keep Oldest)
            GUILayout.Label("<b>1. Persistent Singleton (Keep Oldest)</b>");
            if (PersistentSingletonDemo.HasInstance)
            {
                var pInstance = PersistentSingletonDemo.Instance;
                GUILayout.Label($"Instance ID: <color=green>{pInstance.instanceId}</color>");
                GUILayout.Label($"Persistent: {pInstance.persistent} | Keep Oldest: {pInstance.keepOldest}");
            }
            else
            {
                GUILayout.Label("Instance ID: <color=red>None</color>");
            }

            if (GUILayout.Button("Spawn Duplicate Persistent"))
            {
                GameObject go = new GameObject("PersistentSingletonDemo_Duplicate");
                go.AddComponent<PersistentSingletonDemo>();
                Debug.Log("[SingletonDemo] Spawned a duplicate PersistentSingletonDemo. Check console to see conflict resolution.");
            }

            GUILayout.Space(15);

            // 2. Overriding Singleton (Non-Persistent)
            GUILayout.Label("<b>2. Overriding Singleton (Replace Oldest)</b>");
            if (OverridingSingletonDemo.HasInstance)
            {
                var oInstance = OverridingSingletonDemo.Instance;
                GUILayout.Label($"Instance ID: <color=cyan>{oInstance.instanceId}</color>");
                GUILayout.Label($"Persistent: {oInstance.persistent} | Keep Oldest: {oInstance.keepOldest}");
            }
            else
            {
                GUILayout.Label("Instance ID: <color=red>None</color>");
            }

            if (GUILayout.Button("Spawn Duplicate Overriding"))
            {
                GameObject go = new GameObject("OverridingSingletonDemo_Duplicate");
                go.AddComponent<OverridingSingletonDemo>();
                Debug.Log("[SingletonDemo] Spawned a duplicate OverridingSingletonDemo. It should override and replace the existing one.");
            }

            GUILayout.Space(15);

            // 3. Pure C# Singleton
            GUILayout.Label("<b>3. Pure C# Singleton (Lazy Instantiated)</b>");
            var pure = PureSingletonDemo.Instance;
            GUILayout.Label($"Call Count: <color=yellow>{pure.CallCount}</color>");
            if (GUILayout.Button("Call Pure Increment"))
            {
                pure.Increment();
            }

            GUILayout.Space(20);

            // Scene Loading Utilities
            GUILayout.Label("<b>Scene State Testing</b>");
            
            if (demoSceneReference != null && demoSceneReference.RuntimeKeyIsValid())
            {
                if (GUILayout.Button("Reload via Addressables (Ref)"))
                {
                    Debug.Log("[SingletonDemo] Reloading scene via Addressable AssetReference.");
                    Addressables.LoadSceneAsync(demoSceneReference, LoadSceneMode.Single).ToUniTask().Forget();
                }
            }

            if (GUILayout.Button("Reload Scene (Standard)"))
            {
                Debug.Log("[SingletonDemo] Reloading scene via standard SceneManager.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
