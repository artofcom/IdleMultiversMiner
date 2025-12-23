using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    public class EventsGroup
    {
        Dictionary<string, List<EventDelegate>> RegisteredEvents = new Dictionary<string, List<EventDelegate>>();

        public void RegisterEventgle(System.Enum EventEnumName, EventDelegate del)
        {
            RegisterEvent(EventEnumName.ToString(), del);
        }

        public void UnRegisterEvent(System.Enum EventEnumName, EventDelegate del)
        {
            UnRegisterEvent(EventEnumName.ToString(), del);
        }

        public void RegisterEvent(string EventName, EventDelegate del)
        {
            if (!RegisteredEvents.ContainsKey(EventName))
            {
                RegisteredEvents[EventName] = new List<EventDelegate>();
                RegisteredEvents[EventName].Add(del);
                EventSystem.RegisterEvent(EventName, del);
            }
            else
            {
                if (!RegisteredEvents[EventName].Contains(del))
                {
                    RegisteredEvents[EventName].Add(del);
                    EventSystem.RegisterEvent(EventName, del);
                }
            }
        }

        public void UnRegisterEvent(string EventName, EventDelegate del)
        {
            if (RegisteredEvents.ContainsKey(EventName) && RegisteredEvents[EventName].Contains(del))
            {
                RegisteredEvents[EventName].Remove(del);
                EventSystem.UnRegisterEvent(EventName, del);

                if (RegisteredEvents[EventName].Count == 0)
                {
                    RegisteredEvents.Remove(EventName);
                }
            }
        }

        public void UnRegisterAll()
        {
            List<KeyValuePair<string, EventDelegate>> cachedEvents = new List<KeyValuePair<string, EventDelegate>>();

            foreach (var events in RegisteredEvents)
            {
                foreach (var individualEvent in events.Value)
                {
                    cachedEvents.Add(new KeyValuePair<string, EventDelegate>(events.Key, individualEvent));
                }
            }

            foreach (var element in cachedEvents)
            {
                UnRegisterEvent(element.Key, element.Value);
            }
        }
    }

}
