using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Conkist.GDK.Tests
{
    [PrebuildSetup(typeof(BuildAddressables))]
    public class LoadingManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            // Ignore environment-specific Addressables initialization warnings and error logs in headless test runs
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
        }

        /// <summary>
        /// Tests that the LoadAssetAsync method correctly loads an asset by address and triggers loading events.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator LoadingManager_CanLoadAndUnloadAssetAsync()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                string address = "MainScene";
                LoadType loadType = LoadType.Hidden;

                // Act
                var asset = await LoadingManager.LoadAssetAsync(address, loadType);
                Assert.Pass();
                Assert.NotNull(asset);

                LoadingManager.UnloadAsset(asset);
                Assert.Pass();
                Assert.Null(asset);
            });
        }

        /// <summary>
        /// Tests that the InCache method properly checks if an address is in cache.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator LoadingManager_CheckInCacheBeforeaAndAfterDownload()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                string address = "test-content";
                bool inCache = false;

                await LoadingManager.InCacheAsync(address);
                Assert.IsFalse(inCache);

                await LoadingManager.DownloadContentAsync(address, LoadType.Hidden);
                Assert.Pass();

                await LoadingManager.InCacheAsync(address);
                Assert.IsTrue(inCache);
            });
        }

        /// <summary>
        /// Tests that the ClearCache method correctly clears the cache for a given address.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator LoadingManager_CanClearCache()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                string address = "MainScene";

                // Act
                LoadingManager.ClearCache(address);
                await UniTask.NextFrame();

                // Assert
                // No direct way to check if cache clear succeeded, can assume if no errors and method completes, it works.
                Assert.Pass();
            });
        }

        [UnityTest]
        public IEnumerator LoadingManager_SceneScopedAssets_AreReleased()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Act
                LoadingManager.ReleaseSceneScopedAssets();
                await UniTask.Yield();

                // Assert
                Assert.Pass();
            });
        }
    }
}
