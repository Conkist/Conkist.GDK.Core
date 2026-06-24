namespace Conkist.GDK
{
    /// <summary>
    /// Broadcast when the application's network connectivity status changes.
    /// Subscribe via EventManager.AddListener<NetworkStatusEvent>(listener).
    /// </summary>
    public struct NetworkStatusEvent
    {
        public bool IsConnected;

        public NetworkStatusEvent(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
