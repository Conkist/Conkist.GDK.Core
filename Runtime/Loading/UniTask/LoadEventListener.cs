using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Loading
{
    public abstract class LoadEventListener : MonoBehaviour, 
        EventListener<LoadingEvents.LoadingStartEvent>, 
        EventListener<LoadingEvents.LoadProgressUpdateEvent>,
        EventListener<LoadingEvents.DownloadStatusUpdateEvent>,
        EventListener<LoadingEvents.LoadingStateChangeEvent>
    {

        protected LoadType loadType;

        protected virtual void OnEnable()
        {
            this.Subscribe<LoadingEvents.LoadingStartEvent>();
            this.Subscribe<LoadingEvents.LoadProgressUpdateEvent>();
            this.Subscribe<LoadingEvents.DownloadStatusUpdateEvent>();
            this.Subscribe<LoadingEvents.LoadingStateChangeEvent>();
        }
        protected virtual void OnDisable()
        {
            this.Unsubscribe<LoadingEvents.LoadingStartEvent>();
            this.Unsubscribe<LoadingEvents.LoadProgressUpdateEvent>();
            this.Unsubscribe<LoadingEvents.DownloadStatusUpdateEvent>();
            this.Unsubscribe<LoadingEvents.LoadingStateChangeEvent>();
        }

        protected abstract UniTask OnLoadStarted(string address);
        public abstract void LoadProgress(float progress);
        public virtual void DownloadProgress(AssetsDownloadStatus status)
        {
            LoadProgress(status.PercentProgress);
        }
        protected virtual async UniTask OnStateChange(string address, LoadingStates state)
        {
            switch (state)
            {
                case LoadingStates.ExitFade:
                {
                    await OnFadeOut(address);
                }break;
            }
        }
        protected abstract UniTask OnFadeOut(string address);

        public void OnEventCallback(LoadingEvents.LoadingStartEvent ev)
        {
            loadType = ev.loadType;
            OnLoadStarted(ev.loadingAssetAddress).Forget();
        }
        public void OnEventCallback(LoadingEvents.LoadProgressUpdateEvent ev) => LoadProgress(ev.progress);
        public void OnEventCallback(LoadingEvents.DownloadStatusUpdateEvent ev) => DownloadProgress(ev.status);
        public void OnEventCallback(LoadingEvents.LoadingStateChangeEvent ev)
        {
            OnStateChange(ev.loadingAssetAddress, ev.loadingState).Forget();
        }
    }
}
