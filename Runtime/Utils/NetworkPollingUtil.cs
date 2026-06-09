using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK
{
    /// <summary>
    /// Utility class for active network reachability and internet connection status checking.
    /// </summary>
    public static class NetworkPollingUtil
    {
        /// <summary>
        /// Gets whether the application reports any local network reachability (WiFi or Carrier).
        /// Note that local reachability does not guarantee actual internet access.
        /// </summary>
        public static bool IsLocallyReachable => Application.internetReachability != NetworkReachability.NotReachable;

        /// <summary>
        /// Performs an active check using a web request to verify actual internet connectivity.
        /// </summary>
        /// <param name="pingUrl">The URL to make a request to (should return a successful HTTP code, preferably 204 or 200).</param>
        /// <param name="timeoutSeconds">The timeout duration in seconds for the web request.</param>
        /// <returns>True if the request succeeds, false otherwise.</returns>
        public static async UniTask<bool> CheckActualConnectivityAsync(string pingUrl = "https://clients3.google.com/generate_204", int timeoutSeconds = 3)
        {
            if (!IsLocallyReachable)
            {
                return false;
            }

            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(pingUrl))
                {
                    request.timeout = timeoutSeconds;
                    
                    // Send request asynchronously using UniTask
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Request failed, meaning no real internet connectivity
            }

            return false;
        }
    }
}
