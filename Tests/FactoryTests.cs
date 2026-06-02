using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using Conkist.GDK.Factories;

namespace Conkist.GDK.Tests
{
    public class FactoryTests
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
        // Simple mock service to test VContainer constructor injection
        public class TestService
        {
            public int Value => 123;
        }

        // Simple mock MonoBehaviour to test Prefab dependency injection
        public class TestPrefabComponent : MonoBehaviour
        {
            [Inject]
            public TestService Dependency;
        }

        [UnityTest]
        public IEnumerator VContainerFactory_ResolvesTransientDependency()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var builder = new ContainerBuilder();
                builder.Register<TestService>(Lifetime.Transient);
                var resolver = builder.Build();

                var factory = new VContainerFactory<TestService>(resolver);

                // Act
                TestService instance = factory.Create();

                // Assert
                Assert.IsNotNull(instance, "Factory should resolve the registered transient type");
                Assert.AreEqual(123, instance.Value, "Resolved instance should operate correctly");

                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator PrefabFactory_InstantiatesPrefabAndInjectsDependencies()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject prefabGo = new GameObject("TestPrefabTemplate");
                var templateComponent = prefabGo.AddComponent<TestPrefabComponent>();

                var builder = new ContainerBuilder();
                builder.Register<TestService>(Lifetime.Transient);
                var resolver = builder.Build();

                var factory = new PrefabFactory<TestPrefabComponent>(resolver, templateComponent);

                // Act
                TestPrefabComponent instance = factory.Create();

                // Assert
                Assert.IsNotNull(instance, "PrefabFactory should successfully instantiate a prefab copy");
                Assert.IsNotNull(instance.Dependency, "VContainer should have injected dependencies into the instantiated component");
                Assert.AreEqual(123, instance.Dependency.Value);

                // Clean up
                Object.DestroyImmediate(prefabGo);
                Object.DestroyImmediate(instance.gameObject);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator PrefabFactory_InstantiatesPrefabUnderParent()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                GameObject prefabGo = new GameObject("TestPrefabTemplate");
                var templateComponent = prefabGo.AddComponent<TestPrefabComponent>();

                GameObject parentGo = new GameObject("Parent");

                var builder = new ContainerBuilder();
                builder.Register<TestService>(Lifetime.Transient);
                var resolver = builder.Build();

                var factory = new PrefabFactory<TestPrefabComponent>(resolver, templateComponent);

                // Act
                TestPrefabComponent instance = factory.Create(parentGo.transform);

                // Assert
                Assert.IsNotNull(instance, "PrefabFactory should successfully instantiate a prefab under a parent");
                Assert.AreEqual(parentGo.transform, instance.transform.parent, "Instantiated prefab should be a child of the specified parent transform");

                // Clean up
                Object.DestroyImmediate(prefabGo);
                Object.DestroyImmediate(parentGo);
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator AddressablePrefabFactory_HandlesNullOrEmptyAddress()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var builder = new ContainerBuilder();
                var resolver = builder.Build();
                var factory = new AddressablePrefabFactory<TestPrefabComponent>(resolver, "");

                // Expected error logging from AddressablePrefabFactory guard clause
                LogAssert.Expect(LogType.Error, "[AddressablePrefabFactory<TestPrefabComponent>] Failed to create: Address is null or empty!");

                // Act
                TestPrefabComponent instance = await factory.CreateAsync();

                // Assert
                Assert.IsNull(instance, "Creating with an empty address should return null");
            });
        }

        [UnityTest]
        public IEnumerator AddressablePrefabFactory_HandlesInvalidAddress()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var builder = new ContainerBuilder();
                var resolver = builder.Build();
                var factory = new AddressablePrefabFactory<TestPrefabComponent>(resolver, "NonExistentAddress");
                // Expected errors from Addressables and our custom catch block
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("InvalidKeyException"));
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("ChainOperation failed"));
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Exception caught while loading asset"));
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Failed to load prefab at address"));

                // Act
                // Since this won't load from Addressables, the internal LoadingManager returns null
                TestPrefabComponent instance = await factory.CreateAsync();

                // Assert
                Assert.IsNull(instance, "Creating with an invalid address should fail and return null gracefully");
            });
        }
    }
}
