using System;

namespace Conkist.GDK
{
    /// <summary>
    /// Wrapper for handling event listeners with a callback and owner context.
    /// </summary>
    /// <typeparam name="TOwner">The owner type of the event listener.</typeparam>
    /// <typeparam name="TTarget">The target type of the event listener.</typeparam>
    /// <typeparam name="TEvent">The event type.</typeparam>
    public class EventListenerWrapper<TOwner, TTarget, TEvent> : EventListener<TEvent>, IDisposable where TEvent : struct
    {
        private Action<TTarget> _callback;

        private TOwner _owner;
        public EventListenerWrapper(TOwner owner, Action<TTarget> callback)
        {
            _owner = owner;
            _callback = callback;
            SetEventSubscription(true);
        }

        public void Dispose()
        {
            SetEventSubscription(false);
            _callback = null;
        }

        protected virtual TTarget OnEvent(TEvent eventType) => default;
        public void OnEventCallback(TEvent eventType)
        {
            var item = OnEvent(eventType);
            _callback?.Invoke(item);
        }

        private void SetEventSubscription(bool register)
        {
            if (register)
            {
                this.Subscribe<TEvent>();
            }
            else
            {
                this.Unsubscribe<TEvent>();
            }
        }
    }
}
