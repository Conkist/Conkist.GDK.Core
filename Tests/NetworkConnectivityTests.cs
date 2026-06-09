using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Tests
{
    public class NetworkConnectivityTests
    {
        private class TestNetworkStatusListener : EventListener<NetworkStatusEvent>
        {
            public bool? LastStatus = null;
            public int ReceiveCount = 0;

            public void OnEventCallback(NetworkStatusEvent eventData)
            {
                LastStatus = eventData.IsConnected;
                ReceiveCount++;
            }
        }

        private class TestPopupListener : EventListener<ShowPopupEvent>, EventListener<HidePopupEvent>
        {
            public ShowPopupEvent? LastShowEvent = null;
            public int ShowCount = 0;
            public int HideCount = 0;

            public void OnEventCallback(ShowPopupEvent eventData)
            {
                LastShowEvent = eventData;
                ShowCount++;
            }

            public void OnEventCallback(HidePopupEvent eventData)
            {
                HideCount++;
            }
        }

        [UnityTest]
        public IEnumerator NetworkPollingUtil_LocalReachabilityCheck()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Verify local reachability doesn't crash and returns a boolean value
                bool isReachable = NetworkPollingUtil.IsLocallyReachable;
                Assert.Pass($"Local reachability is: {isReachable}");
                await UniTask.Yield();
            });
        }

        [UnityTest]
        public IEnumerator NetworkStatusEvent_BroadcastsCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var listener = new TestNetworkStatusListener();
                listener.Subscribe();

                // Act
                EventManager.TriggerEvent(new NetworkStatusEvent(true));
                await UniTask.Yield();

                // Assert
                Assert.AreEqual(1, listener.ReceiveCount, "Listener should receive the event exactly once.");
                Assert.IsTrue(listener.LastStatus, "Listener should receive true status.");

                // Act 2
                EventManager.TriggerEvent(new NetworkStatusEvent(false));
                await UniTask.Yield();

                // Assert 2
                Assert.AreEqual(2, listener.ReceiveCount, "Listener should receive second event.");
                Assert.IsFalse(listener.LastStatus, "Listener should receive false status.");

                // Cleanup
                listener.Unsubscribe();
            });
        }

        [UnityTest]
        public IEnumerator PopupEvents_BroadcastCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var listener = new TestPopupListener();
                listener.Subscribe<ShowPopupEvent>();
                listener.Subscribe<HidePopupEvent>();

                // Act
                var showEvent = new ShowPopupEvent(
                    title: "Test Error",
                    message: "A test message",
                    confirmText: "Retry",
                    cancelText: "Close",
                    isError: true
                );
                EventManager.TriggerEvent(showEvent);
                await UniTask.Yield();

                // Assert Show
                Assert.AreEqual(1, listener.ShowCount, "Show event count should be 1.");
                Assert.IsNotNull(listener.LastShowEvent, "Last show event should not be null.");
                Assert.AreEqual("Test Error", listener.LastShowEvent.Value.Title);
                Assert.AreEqual("A test message", listener.LastShowEvent.Value.Message);
                Assert.AreEqual("Retry", listener.LastShowEvent.Value.ConfirmText);
                Assert.AreEqual("Close", listener.LastShowEvent.Value.CancelText);
                Assert.IsTrue(listener.LastShowEvent.Value.IsError);

                // Act Hide
                EventManager.TriggerEvent(new HidePopupEvent());
                await UniTask.Yield();

                // Assert Hide
                Assert.AreEqual(1, listener.HideCount, "Hide event count should be 1.");

                // Cleanup
                listener.Unsubscribe<ShowPopupEvent>();
                listener.Unsubscribe<HidePopupEvent>();
            });
        }
    }
}
