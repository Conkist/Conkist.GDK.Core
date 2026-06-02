using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Tests
{
    /// <summary>
    /// Unit tests for the SingletonBehaviour<T> class.
    /// </summary>
    public class SingletonTests
    {
        [SetUp]
        public void SetUp()
        {
            DoCleanup();
        }

        [TearDown]
        public void TearDown()
        {
            DoCleanup();
        }

        private void DoCleanup()
        {
            var singletons = Object.FindObjectsOfType<TestSingleton>();
            foreach (var singleton in singletons)
            {
                Object.DestroyImmediate(singleton.gameObject);
            }
        }

        /// <summary>
        /// A sample derived Singleton class used for testing.
        /// </summary>
        private class TestSingleton : SingletonBehaviour<TestSingleton>
        {
        }

        /// <summary>
        /// Tests that a singleton instance can be instantiated correctly.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator Singleton_CanBeInstantiated()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject go = new GameObject();
                go.AddComponent<TestSingleton>();

                // Act
                TestSingleton instance = TestSingleton.Instance;
                await UniTask.Yield();

                // Assert
                Assert.IsNotNull(instance);
            });
        }

        /// <summary>
        /// Tests that only one instance of the singleton exists,
        /// even when multiple GameObjects with the singleton are created.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator Singleton_OnlyOneInstanceExists()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject go1 = new GameObject();
                GameObject go2 = new GameObject();
                go1.AddComponent<TestSingleton>();
                go2.AddComponent<TestSingleton>();

                // Act
                await UniTask.Yield();

                // Assert
                Assert.IsTrue(go1 == null || go2 == null);
            });
        }

        /// <summary>
        /// Tests that the older instance of the singleton is destroyed
        /// when 'keepOldest' is set to false.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator Singleton_OldInstanceDestroyed_WhenKeepOldestFalse()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject go1 = new GameObject();
                TestSingleton singleton1 = go1.AddComponent<TestSingleton>();
                singleton1.keepOldest = false;
                await UniTask.Yield();

                GameObject go2 = new GameObject();
                TestSingleton singleton2 = go2.AddComponent<TestSingleton>();

                // Allow a frame to pass to let Awake() method execute
                await UniTask.Yield();
                await UniTask.DelayFrame(5); // Wait a few frames for deferred destruction to process

                // Assert
                Assert.IsTrue(go1 == null || go1.Equals(null));
                Assert.IsTrue(go2 != null && !go2.Equals(null));
                await UniTask.Yield();
            });
        }

        /// <summary>
        /// Tests that the singleton instance persists across scene loads
        /// if 'persistent' is set to true.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator Singleton_PersistsAcrossScenes()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject go = new GameObject();
                TestSingleton singleton = go.AddComponent<TestSingleton>();

                // Allow a frame to pass to let Awake() method execute
                await UniTask.Yield();

                // Assert that the instance exists before scene load
                Assert.IsTrue(TestSingleton.HasInstance);

                // Create a new empty scene
                SceneManager.CreateScene("New Empty Scene");

                // Allow a frame to pass to let the scene load completely
                await UniTask.Yield();

                // Assert
                Assert.IsTrue(TestSingleton.HasInstance, "The singleton instance should persist across scenes.");
            });
        }
    }
}
