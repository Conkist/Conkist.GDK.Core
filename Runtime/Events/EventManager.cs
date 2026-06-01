using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Conkist.GDK
{
    /// <summary>
    /// Represents a general game event with a name.
    /// Used for events such as game started, game ended, life lost, etc.
    /// </summary>
    public struct GameEvent
    {
        public string EventName;
        public GameEvent(string newName)
        {
            EventName = newName;
        }
        static GameEvent ev;

        /// <summary>
        /// Triggers an event with the specified name.
        /// </summary>
        /// <param name="newName">The name of the event to trigger.</param>
        public static void Trigger(string newName)
        {
            ev.EventName = newName;
            EventManager.TriggerEvent(ev);
        }
    }

    /// <summary>
    /// This class handles event management, and can be used to broadcast events throughout the game, to tell one class (or many) that something's happened.
    /// Events are structs, you can define any kind of events you want. This manager comes with GameEvents, which are 
    /// basically just made of a string, but you can work with more complex ones if you want.
    /// 
    /// To trigger a new event, from anywhere, do YOUR_EVENT.Trigger(YOUR_PARAMETERS)
    /// So GameEvent.Trigger("Save"); for example will trigger a Save GameEvent
    /// 
    /// you can also call EventManager.TriggerEvent(YOUR_EVENT);
    /// For example : EventManager.TriggerEvent(new GameEvent("GameStart")); will broadcast an GameEvent named GameStart to all listeners.
    ///
    /// To start listening to an event from any class, there are 3 things you must do : 
    ///
    /// 1 - tell that your class implements the EventListener interface for that kind of event.
    /// For example: public class GUIManager : SingletonBehaviour<GUIManager>, EventListener<GameEvent>
    /// You can have more than one of these (one per event type).
    ///
    /// 2 - On Enable and Disable, respectively start and stop listening to the event :
    /// void OnEnable()
    /// {
    /// 	this.EventStartListening<GameEvent>();
    /// }
    /// void OnDisable()
    /// {
    /// 	this.EventStopListening<GameEvent>();
    /// }
    /// 
    /// 3 - Implement the EventListener interface for that event. For example :
    /// public void OnEvent(GameEvent gameEvent)
    /// {
    /// 	if (gameEvent.EventName == "GameOver")
    ///		{
    ///			// DO SOMETHING
    ///		}
    /// } 
    /// will catch all events of type GameEvent emitted from anywhere in the game, and do something if it's named GameOver
    /// </summary>
    
    /// <summary>
    /// Manages event subscriptions and broadcasting.
    /// Allows broadcasting events throughout the game to notify one or more classes.
    /// </summary>
    [ExecuteAlways]
    public class EventManager : SingletonBehaviour<EventManager>
    {
        private static Dictionary<Type, List<EventListenerBase>> _subscribersList;

        static EventManager()
        {
            _subscribersList = new Dictionary<Type, List<EventListenerBase>>();
        }

        /// <summary>
        /// Adds a subscriber to a certain event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="listener">The listener to be added.</param>
        public static void AddListener<TEvent>(EventListener<TEvent> listener) where TEvent : struct
        {
            Type eventType = typeof(TEvent);

            if (!_subscribersList.ContainsKey(eventType))
            {
                _subscribersList[eventType] = new List<EventListenerBase>();
            }

            if (!SubscriptionExists(eventType, listener))
            {
                _subscribersList[eventType].Add(listener);
            }
        }

        /// <summary>
        /// Removes a subscriber from a certain event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="listener">The listener to be removed.</param>
        public static void RemoveListener<TEvent>(EventListener<TEvent> listener) where TEvent : struct
        {
            Type eventType = typeof(TEvent);

            if (!_subscribersList.ContainsKey(eventType))
            {
#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
#else
                return;
#endif
            }

            List<EventListenerBase> subscriberList = _subscribersList[eventType];

#if EVENTROUTER_THROWEXCEPTIONS
	            bool listenerFound = false;
#endif

            for (int i = subscriberList.Count - 1; i >= 0; i--)
            {
                if (subscriberList[i] == listener)
                {
                    subscriberList.Remove(subscriberList[i]);
#if EVENTROUTER_THROWEXCEPTIONS
					    listenerFound = true;
#endif

                    if (subscriberList.Count == 0)
                    {
                        _subscribersList.Remove(eventType);
                    }

                    return;
                }
            }

#if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
#endif
        }

        /// <summary>
        /// Triggers an event of the specified type.
        /// All instances subscribed to the event will receive it.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="newEvent">The event to trigger.</param>
        public static void TriggerEvent<TEvent>(TEvent newEvent) where TEvent : struct
        {
            List<EventListenerBase> list;
            if (!_subscribersList.TryGetValue(typeof(TEvent), out list))
#if EVENTROUTER_REQUIRELISTENER
			    throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( Event ).ToString() ) );
#else
                return;
#endif

            for (int i = list.Count - 1; i >= 0; i--)
            {
                (list[i] as EventListener<TEvent>).OnEventCallback(newEvent);
            }
        }

        /// <summary>
        /// Checks if a subscription exists for a certain type of event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="receiver">The receiver of the event.</param>
        /// <returns>True if a subscription exists, false otherwise.</returns>
        private static bool SubscriptionExists(Type eventType, EventListenerBase receiver)
        {
            List<EventListenerBase> receivers;

            if (!_subscribersList.TryGetValue(eventType, out receivers)) return false;

            bool exists = false;

            for (int i = receivers.Count - 1; i >= 0; i--)
            {
                if (receivers[i] == receiver)
                {
                    exists = true;
                    break;
                }
            }

            return exists;
        }

#if UNITY_INCLUDE_TESTS
        public static bool Test_SubscriptionExists(Type eventType, EventListenerBase receiver)
        {
            return SubscriptionExists(eventType, receiver);
        }
#endif

    }

    #region Wrapper and Registration Handling
    /// <summary>
    /// Provides extension methods for event registration and unregistration.
    /// </summary>
    public static class EventRegister
    {
        public delegate void Delegate<T>(T eventType);

        public static void Subscribe<EventType>(this EventListener<EventType> caller) where EventType : struct
        {
            EventManager.AddListener<EventType>(caller);
        }

        public static void Unsubscribe<EventType>(this EventListener<EventType> caller) where EventType : struct
        {
            EventManager.RemoveListener<EventType>(caller);
        }
    }

    /// <summary>
    /// Base interface for event listeners.
    /// </summary>
    public interface EventListenerBase { };

    /// <summary>
    /// Interface for event listeners of a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    public interface EventListener<TEvent> : EventListenerBase
    {
        /// <summary>
        /// Callback method when the event is triggered.
        /// </summary>
        /// <param name="eventType">The event that was triggered.</param>
        void OnEventCallback(TEvent eventType);
    }

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
    #endregion
}
