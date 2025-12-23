using UnityEngine;
using System;
using System.Collections;

namespace Core.Util
{
    public class DelayedAction 
    {
        MonoBehaviour coRunner { get; set; }
        public DelayedAction(MonoBehaviour runner)
        {
            coRunner = runner;
        }

        
        public Coroutine TriggerActionWithDelay(float delay, Action action)
        {
            return coRunner.StartCoroutine(coTriggerActionWithDelay(delay, action));
        }
        protected IEnumerator coTriggerActionWithDelay(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }


        public Coroutine TriggerActionWithDelay(float delay, Action<object> action, object data)
        {
            return coRunner.StartCoroutine(coTriggerActionWithDelay(delay, action, data));
        }
        protected IEnumerator coTriggerActionWithDelay(float delay, Action<object> action, object data)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke(data);
        }

        static DelayedAction instance = null;
        static public Coroutine TriggerActionWithDelay(MonoBehaviour actor, float delay, Action action)
        {
            if(instance == null)
                instance = new DelayedAction(actor);
            instance.coRunner = actor;
            return instance.TriggerActionWithDelay(delay, action);
        }
        static public Coroutine TriggerActionWithDelay(MonoBehaviour actor, float delay, Action<object> action, object data)
        {
            if(instance == null)
                instance = new DelayedAction(actor);
            instance.coRunner = actor;
            return instance.TriggerActionWithDelay(delay, action, data);
        }
    }
}
