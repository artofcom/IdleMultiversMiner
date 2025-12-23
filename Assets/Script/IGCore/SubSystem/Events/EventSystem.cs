using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    public class EventSystem
    {
        private static EventSystem _instance;

        protected MonoBehaviour coroutineProxy;
        protected Dictionary<string, List<Coroutine>> delayedCoroutines;
        private static EventSystem instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventSystem();
                }
                return _instance;
            }
        }

        private GenericEvents events;
        private EventSystem()
        {
            events = new GenericEvents();
        }

        public static void setCoroutineProxy(MonoBehaviour coroutineProxy)
        {
            instance.coroutineProxy = coroutineProxy;
        }

        public static void RegisterEvent(string EventName, EventDelegate del)
        {
            instance.events.RegisterEvent(EventName, del);
        }

        public static void RegisterEvent(Enum EventEnumName, EventDelegate del)
        {
            RegisterEvent(EventEnumName.ToString(), del);
        }

        public static void UnRegisterEvent(string EventEnumName, EventDelegate del)
        {
            instance.events.UnRegisterEvent(EventEnumName, del);
        }

        public static void UnRegisterEvent(Enum EventEnumName, EventDelegate del)
        {
            UnRegisterEvent(EventEnumName.ToString(), del);
        }

        public static void UnRegisterAll(object target)
        {
            instance.events.UnRegisterAll(target);
        }

        public static void DispatchEvent(string EventEnumName, object data = null)
        {
            instance.events.DispatchEvent(EventEnumName, data);
        }

        public static void DispatchEvent(Enum EventEnumName, object data = null)
        {
            DispatchEvent(EventEnumName.ToString(), data);
        }

        public static bool HasEventRegistered(string EventName)
        {
            return instance.events.HasEventRegistered(EventName);
        }

        #region Delays
        public static void DispatchEventAfterDelay(string EventEnumName, object data = null, float delay = 0.0f)
        {
            if (instance.coroutineProxy != null)
            {
                Coroutine co = instance.coroutineProxy.StartCoroutine(instance.coDispatchEventAfterDelay(EventEnumName, data, delay));

                if (instance.delayedCoroutines.ContainsKey(EventEnumName))
                {
                    instance.delayedCoroutines[EventEnumName].Add(co);
                }
                else
                {
                    List<Coroutine> newLst = new List<Coroutine>();
                    newLst.Add(co);

                    instance.delayedCoroutines.Add(EventEnumName, newLst);
                }
            }
        }

        public static void DispatchEventAfterDelay(Enum EventEnumName, object data = null, float delay = 0.0f)
        {
            if (instance.coroutineProxy != null)
            {
                DispatchEventAfterDelay(EventEnumName.ToString(), data, delay);
            }
        }

        public static void RemoveDelayedEvent(string EventEnumName)
        {

        }

        protected IEnumerator coDispatchEventAfterDelay(string EventEnumName, object data, float delay)
        {
            yield return new WaitForSeconds(delay);

            DispatchEvent(EventEnumName, data);
        }
        #endregion
    }

    public class EventPacket
    {
        public string eventName { get; private set; }
        public object eventData { get; private set; }
        public float delay { get; private set; }

        public EventPacket(string name, object data, float delay)
        {
            this.eventName = name;
            this.eventData = data;
            this.delay = delay;
        }
    }

}