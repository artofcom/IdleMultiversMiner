using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    public class GenericEvents
    {
        Dictionary<string, List<EventDelegate>> RegisteredEvents = new Dictionary<string, List<EventDelegate>>();

        public void RegisterEvent(System.Enum EventEnumName, EventDelegate del)
        {
            RegisterEvent(EventEnumName.ToString(), del);
        }

        public void RegisterEvent(string EventName, EventDelegate del)
        {
            if (!RegisteredEvents.ContainsKey(EventName))
            {
                RegisteredEvents[EventName] = new List<EventDelegate>();
                RegisteredEvents[EventName].Add(del);
            }
            else
            {
                if (!RegisteredEvents[EventName].Contains(del))
                {
                    RegisteredEvents[EventName].Add(del);
                }
            }
        }

        public void UnRegisterEvent(System.Enum EventEnumName, EventDelegate del)
        {
            UnRegisterEvent(EventEnumName.ToString(), del);
        }

        public void UnRegisterEvent(string EventName, EventDelegate del)
        {
            if (RegisteredEvents.ContainsKey(EventName) && RegisteredEvents[EventName].Contains(del))
            {
                RegisteredEvents[EventName].Remove(del);

                if (RegisteredEvents[EventName].Count == 0)
                {
                    RegisteredEvents.Remove(EventName);
                }
            }
        }

        public void UnRegisterAll(object target)
        {
            List<KeyValuePair<string, EventDelegate>> cachedEvents = new List<KeyValuePair<string, EventDelegate>>();

            foreach (var events in RegisteredEvents)
            {
                foreach (var internalEvent in events.Value)
                {
                    if (internalEvent.Target == target)
                    {
                        cachedEvents.Add(new KeyValuePair<string, EventDelegate>(events.Key, internalEvent));
                    }
                }
            }

            foreach (var element in cachedEvents)
            {
                UnRegisterEvent(element.Key, element.Value);
            }
        }

        public void DispatchEvent(System.Enum EventEnumName, object data = null)
        {
            DispatchEvent(EventEnumName.ToString(), data);
        }

        public void DispatchEvent(string EventName, object data = null)
        {
            if (RegisteredEvents.ContainsKey(EventName))
            {
                List<EventDelegate> cachedEventDelegates = new List<EventDelegate>(RegisteredEvents[EventName]);
                foreach (EventDelegate ev in cachedEventDelegates)
                {
                    ev(data);
                }
            }
        }

        public bool HasEventRegistered(string eventName)
        {
            return RegisteredEvents.ContainsKey(eventName);
        }
    }
}