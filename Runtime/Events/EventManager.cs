using System;
using System.Collections.Generic;
using UnityEngine;

namespace Conkist.GDK
{
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
}
