//#define EVENTROUTER_THROWEXCEPTIONS 
#if EVENTROUTER_THROWEXCEPTIONS
#endif

using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace MoreMountains.Tools
{
	public struct MMGameEvent
	{
		public string EventName;
		public MMGameEvent(string newName)
		{
			EventName = newName;
        }
        static MMGameEvent e;
        public static void Trigger(string newName)
        {
            e.EventName = newName;
            MMEventManager.TriggerEvent(e);
        }
    }
    [ExecuteAlways]
	public static class MMEventManager 
	{
	    private static Dictionary<Type, List<MMEventListenerBase>> _subscribersList;

		static MMEventManager()
	    {
	        _subscribersList = new Dictionary<Type, List<MMEventListenerBase>>();
	    }
	    public static void AddListener<MMEvent>( MMEventListener<MMEvent> listener ) where MMEvent : struct
	    {
	        Type eventType = typeof( MMEvent );

	        if( !_subscribersList.ContainsKey( eventType ) )
	            _subscribersList[eventType] = new List<MMEventListenerBase>();

	        if( !SubscriptionExists( eventType, listener ) )
	            _subscribersList[eventType].Add( listener );
	    }
	    public static void RemoveListener<MMEvent>( MMEventListener<MMEvent> listener ) where MMEvent : struct
	    {
	        Type eventType = typeof( MMEvent );

	        if( !_subscribersList.ContainsKey( eventType ) )
	        {
				#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
				#else
					return;
				#endif
	        }

			List<MMEventListenerBase> subscriberList = _subscribersList[eventType];

            #if EVENTROUTER_THROWEXCEPTIONS
	            bool listenerFound = false;
            #endif

            for (int i = 0; i<subscriberList.Count; i++)
			{
				if( subscriberList[i] == listener )
				{
					subscriberList.Remove( subscriberList[i] );
                    #if EVENTROUTER_THROWEXCEPTIONS
					    listenerFound = true;
                    #endif

                    if ( subscriberList.Count == 0 )
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
	    public static void TriggerEvent<MMEvent>( MMEvent newEvent ) where MMEvent : struct
	    {
	        List<MMEventListenerBase> list;
	        if( !_subscribersList.TryGetValue( typeof( MMEvent ), out list ) )
#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
#else
			                return;
#endif
			
			for (int i=0; i<list.Count; i++)
			{
				( list[i] as MMEventListener<MMEvent> ).OnMMEvent( newEvent );
			}
	    }
	    private static bool SubscriptionExists( Type type, MMEventListenerBase receiver )
	    {
	        List<MMEventListenerBase> receivers;

	        if( !_subscribersList.TryGetValue( type, out receivers ) ) return false;

	        bool exists = false;

			for (int i=0; i<receivers.Count; i++)
			{
				if( receivers[i] == receiver )
				{
					exists = true;
					break;
				}
			}

	        return exists;
	    }
	}
	public static class EventRegister
	{
	    public delegate void Delegate<T>( T eventType );

	    public static void MMEventStartListening<EventType>( this MMEventListener<EventType> caller ) where EventType : struct
	    {
			MMEventManager.AddListener<EventType>( caller );
	    }

		public static void MMEventStopListening<EventType>( this MMEventListener<EventType> caller ) where EventType : struct
	    {
			MMEventManager.RemoveListener<EventType>( caller );
	    }
	}
	public interface MMEventListenerBase { };
	public interface MMEventListener<T> : MMEventListenerBase
	{
	    void OnMMEvent( T eventType );
	}
}