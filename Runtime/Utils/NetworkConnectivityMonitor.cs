using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK
{
    /// <summary>
    /// Persistent monitor component that periodically checks network connectivity
    /// and broadcasts changes via EventManager.
    /// </summary>
    [AddComponentMenu("Conkist/Network/NetworkConnectivityMonitor")]
    public class NetworkConnectivityMonitor : SingletonBehaviour<NetworkConnectivityMonitor>
    {
        [Header("Configuration")]
        [Tooltip("Interval in seconds between active connectivity checks.")]
        [SerializeField] private float pollInterval = 5f;

        [Tooltip("The URL to ping to check for actual internet connectivity.")]
        [SerializeField] private string pingUrl = "https://clients3.google.com/generate_204";

        [Tooltip("Timeout in seconds for the connection ping request.")]
        [SerializeField] private int timeoutSeconds = 3;

        private bool? _lastConnectedState = null;
        private bool _isMonitoring = false;

        /// <summary>
        /// Gets the last known connectivity status.
        /// </summary>
        public bool IsConnected => _lastConnectedState ?? false;

        protected override void Awake()
        {
            base.Awake();
            
            // If this is the active singleton instance, start monitoring
            if (_instance == this)
            {
                StartMonitoring();
            }
        }

        private void Start()
        {
            // Fallback start if awake didn't initialize it (e.g., in editor/pre-instantiated scenarios)
            if (_instance == this)
            {
                StartMonitoring();
            }
        }

        public void StartMonitoring()
        {
            if (_isMonitoring) return;
            
            _isMonitoring = true;
            MonitorConnectivityAsync().Forget();
        }

        private async UniTaskVoid MonitorConnectivityAsync()
        {
            var cancelToken = this.GetCancellationTokenOnDestroy();

            while (_isMonitoring && this != null && gameObject != null)
            {
                bool currentConnection = await NetworkPollingUtil.CheckActualConnectivityAsync(pingUrl, timeoutSeconds);

                if (!_lastConnectedState.HasValue || _lastConnectedState.Value != currentConnection)
                {
                    _lastConnectedState = currentConnection;
                    Debug.Log($"[NetworkConnectivityMonitor] Connection state changed: {currentConnection}");
                    EventManager.TriggerEvent(new NetworkStatusEvent(currentConnection));
                }

                // Wait for the specified interval before the next poll
                await UniTask.Delay(
                    TimeSpan.FromSeconds(pollInterval), 
                    delayType: DelayType.Realtime, 
                    cancellationToken: cancelToken
                );
            }
        }
    }
}
