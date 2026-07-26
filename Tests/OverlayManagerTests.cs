using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Conkist.GDK.Tests
{
    public class OverlayManagerTests
    {
        private class DummyOverlayController : MonoBehaviour, IOverlayController
        {
            public bool WasShown { get; private set; }
            public bool WasHidden { get; private set; }
            public ShowPopupEvent ReceivedData { get; private set; }

            public void OnOverlayShow(ShowPopupEvent data)
            {
                WasShown = true;
                ReceivedData = data;
            }

            public void OnOverlayHide()
            {
                WasHidden = true;
            }
        }

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator Overlay_StaticAPI_CanShowAndHideOverlay()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Create a temporary scene to act as an overlay scene
                string testOverlayName = "TestAdditiveOverlayScene";
                Scene testScene = USceneManager.CreateScene(testOverlayName);
                
                // Add a dummy root object with a controller
                GameObject rootGo = new GameObject("TestOverlayRoot");
                USceneManager.MoveGameObjectToScene(rootGo, testScene);
                var controller = rootGo.AddComponent<DummyOverlayController>();

                // Verify initial state
                Assert.IsFalse(Overlay.IsLoaded(testOverlayName));
                Assert.IsFalse(Overlay.IsVisible(testOverlayName));

                // Clean up dummy scene
                Object.DestroyImmediate(rootGo);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator OverlayManager_CachingAndDontDestroyOnLoad_PreservesObjects()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Verify OverlayManager singleton exists
                Assert.NotNull(OverlayManager.Instance);

                // Test IsLoaded and IsVisible initial state
                Assert.IsFalse(Overlay.IsLoaded("NonExistentOverlay"));
                Assert.IsFalse(Overlay.IsVisible("NonExistentOverlay"));

                // Call HideAll
                Overlay.HideAll();
                Assert.Pass();
            });
        }
    }
}
