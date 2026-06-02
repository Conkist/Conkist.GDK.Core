using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Tests
{
    public class CoreUtilitiesTests
    {
        [SetUp]
        public void SetUp()
        {
            ResetSingletons();
        }

        [TearDown]
        public void TearDown()
        {
            ResetSingletons();
        }

        private void ResetSingletons()
        {
            ResetStaticInstance<AudioManager>();
            ResetStaticInstance<GameStateManager>();
            ResetStaticInstance<TestPersistentClass>();
        }

        private void ResetStaticInstance<T>() where T : Component
        {
            var field = typeof(SingletonBehaviour<T>).GetField("_instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(null, null);
            }
        }

        private class TestPureClass : PureSingleton<TestPureClass>
        {
            public int Value = 42;
        }

        private class TestPersistentClass : SingletonBehaviour<TestPersistentClass>
        {
            public string Data = "Persistent";
        }

        private class TestStateListener : EventListener<GameStateChangedEvent>
        {
            public GameState? LastNewState = null;
            public GameState? LastOldState = null;

            public void OnEventCallback(GameStateChangedEvent eventData)
            {
                LastNewState = eventData.NewState;
                LastOldState = eventData.PreviousState;
            }
        }

        [UnityTest]
        public IEnumerator PureSingleton_CreatesInstanceCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Act
                var instance = TestPureClass.Instance;

                // Assert
                Assert.IsNotNull(instance);
                Assert.AreEqual(42, instance.Value);

                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator PersistentSingleton_CreatesAndPersists()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject go = new GameObject();
                var comp = go.AddComponent<TestPersistentClass>();

                // Act
                await UniTask.Yield();

                // Assert
                Assert.IsTrue(TestPersistentClass.HasInstance);
                Assert.AreEqual("Persistent", TestPersistentClass.Instance.Data);

                // Clean up
                Object.DestroyImmediate(go);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator GameStateManager_BroadcastsTransitions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject go = new GameObject();
                var manager = go.AddComponent<GameStateManager>();
                var listener = new TestStateListener();

                // Act
                await UniTask.Yield(); // Allow Awake and Start
                listener.Subscribe();

                manager.TransitionTo(GameState.Gameplay);
                await UniTask.Yield();

                // Assert
                Assert.AreEqual(GameState.Gameplay, manager.CurrentState);
                Assert.AreEqual(GameState.Gameplay, listener.LastNewState);

                // Clean up
                listener.Unsubscribe();
                Object.DestroyImmediate(go);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator AudioManager_PoolInitializesAndAllocates()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject go = new GameObject("AudioManager", typeof(AudioManager));
                var manager = go.GetComponent<AudioManager>();
                await UniTask.Yield();

                // Act
                var source = manager.GetPooledSFXSource();

                // Assert
                Assert.IsNotNull(source);
                Assert.IsFalse(source.isPlaying);

                // Clean up
                Object.DestroyImmediate(go);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator AudioTrigger_TriggersCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject managerGo = new GameObject("AudioManager", typeof(AudioManager));
                GameObject triggerGo = new GameObject("AudioTrigger", typeof(AudioTrigger));
                
                var trigger = triggerGo.GetComponent<AudioTrigger>();
                await UniTask.Yield();

                // Act & Assert (Should complete without exception)
                trigger.Trigger();
                Assert.Pass();

                // Clean up
                Object.DestroyImmediate(managerGo);
                Object.DestroyImmediate(triggerGo);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator MusicController_CreatesSourcesUnderMusicChild()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject managerGo = new GameObject("AudioManager", typeof(AudioManager));
                GameObject musicControllerGo = new GameObject("MusicController", typeof(MusicController));
                var controller = musicControllerGo.GetComponent<MusicController>();

                await UniTask.Yield();

                // Act - Trigger active source retrieval to initialize the default music source
                AudioSource activeSource = controller.GetActiveSource();

                // Assert child "Music" is created under AudioManager
                Transform musicChild = managerGo.transform.Find("Music");
                Assert.IsNotNull(musicChild, "Expected a child named 'Music' to be created under AudioManager");

                // Assert default source is created under "Music" child
                Transform defaultSource = musicChild.Find("DefaultMusicSource");
                Assert.IsNotNull(defaultSource, "Expected DefaultMusicSource to exist under child Music");
                Assert.AreEqual(activeSource.transform, defaultSource);

                // Clean up
                Object.DestroyImmediate(managerGo);
                Object.DestroyImmediate(musicControllerGo);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator AudioTrigger_PlaysOnTargetAudioSource()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject triggerGo = new GameObject("AudioTrigger", typeof(AudioTrigger));
                GameObject targetGo = new GameObject("TargetSource", typeof(AudioSource));
                
                var trigger = triggerGo.GetComponent<AudioTrigger>();
                var source = targetGo.GetComponent<AudioSource>();

                // Assign targetAudioSource via reflection
                var targetField = typeof(AudioTrigger).GetField("targetAudioSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                targetField.SetValue(trigger, source);

                // Use the new public SetRuntimeClip method
                AudioClip clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
                trigger.SetRuntimeClip(clip);

                await UniTask.Yield();

                // Act
                trigger.Trigger();

                // Assert target source is configured with clip and played
                Assert.AreEqual(clip, source.clip, "Target AudioSource clip should be the triggered clip");
                Assert.IsTrue(source.isPlaying, "Target AudioSource should be playing the clip");

                // Clean up
                Object.DestroyImmediate(triggerGo);
                Object.DestroyImmediate(targetGo);
                Object.DestroyImmediate(clip);
                await UniTask.Yield();
            });
        }
    }
}
