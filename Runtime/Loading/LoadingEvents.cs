using Conkist.GDK.Loading;

namespace Conkist.GDK
{
    public class LoadingEvents
    {
        /// <summary>
        /// Represents a change in loading event type.
        /// </summary>
        public struct LoadTypeChangeEvent
        {
            static LoadTypeChangeEvent ev;

            public LoadType loadType;

            public LoadTypeChangeEvent(LoadType type)
            {
                loadType = type;
            }

            /// <summary>
            /// Triggers a loading event.
            /// </summary>
            /// <param name="address">The address related to the event.</param>
            /// <param name="state">The load status.</param>
            /// <param name="type">The type of load.</param>
            public static void Trigger(LoadType type)
            {
                ev.loadType = type;

                EventManager.TriggerEvent(ev);
            }
        }
        
        /// <summary>
        /// Represents a change in loading event status.
        /// </summary>
        public struct LoadingStateChangeEvent
        {
            static LoadingStateChangeEvent ev;

            public string loadingAssetAddress;
            public LoadingStates loadingState;

            public LoadingStateChangeEvent(string address, LoadingStates state)
            {
                loadingAssetAddress = address;
                loadingState = state;
            }

            /// <summary>
            /// Triggers a loading event.
            /// </summary>
            /// <param name="address">The address related to the event.</param>
            /// <param name="state">The load status.</param>
            /// <param name="type">The type of load.</param>
            public static void Trigger(string address, LoadingStates state)
            {
                ev.loadingAssetAddress = address;
                ev.loadingState = state;

                EventManager.TriggerEvent(ev);
            }
        }

        public struct LoadingStartEvent
        {
            static LoadingStartEvent ev;

            public string loadingAssetAddress;
            public LoadType loadType;

            public LoadingStartEvent(string address, LoadType type)
            {
                loadingAssetAddress = address;
                loadType = type;
            }

            /// <summary>
            /// Triggers a loading event when it starts.
            /// </summary>
            /// <param name="address">The address related to the event.</param>
            /// <param name="type">The type of load.</param>
            public static void Trigger(string address, LoadType type)
            {
                ev.loadingAssetAddress = address;
                ev.loadType = type;

                EventManager.TriggerEvent(ev);
            }
        }
        
        public struct DownloadStatusUpdateEvent
        {
            static DownloadStatusUpdateEvent ev;

            public AssetsDownloadStatus status;

            public DownloadStatusUpdateEvent(AssetsDownloadStatus status)
            {
                this.status = status;
            }
            
            public static void Trigger(AssetsDownloadStatus status)
            {
                ev.status = status;

                EventManager.TriggerEvent(ev);
            }
        }

        public struct LoadProgressUpdateEvent
        {
            static LoadProgressUpdateEvent ev;

            public float progress;

            public LoadProgressUpdateEvent(float progress)
            {
                this.progress = progress;
            }
            
            public static void Trigger(float progress)
            {
                ev.progress = progress;

                EventManager.TriggerEvent(ev);
            }
        }

        public struct ReloadSceneEvent
        {
            static ReloadSceneEvent ev;
            
            public static void Trigger() { EventManager.TriggerEvent(ev); }
        }

        public struct VisibleLoadCompletedEvent
        {
            static VisibleLoadCompletedEvent ev;
            
            public static void Trigger() { EventManager.TriggerEvent(ev); }
        }

        public struct LoadingCanvasSetEvent
        {
            public string loadingCanvasKey;

            public LoadingCanvasSetEvent(string loadingCanvasKey)
            {
                this.loadingCanvasKey = loadingCanvasKey;
            }
        }

        public struct SceneRemovedEvent
        {
            public string sceneName;

            public SceneRemovedEvent(string sceneName)
            {
                this.sceneName = sceneName;
            }
        }
    }
}
