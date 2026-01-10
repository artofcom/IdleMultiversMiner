using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;

namespace IGCore.MVCS
{
    public class ConditionScreenBlkerUnitSwitcherComp : UnitSwitcherComp
    {
        [SerializeField] Image screenBlocker;
        [SerializeField] float sceneFadeInTime = 1.0f;
        [SerializeField] float sceneFadeOutTime = 1.0f;



        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(screenBlocker);
        }

        public override void SwitchUnit(string nextModuleId, object data)
        {
            StartCoroutine(coOnEventClose(nextModuleId, (Func<bool>)data)); 
        }

        IEnumerator coOnEventClose(string nextModuleId, Func<bool> conditionFunc)
        {
            screenBlocker.enabled = true;
            screenBlocker.color = new Color(.0f, .0f, .0f, .0f);
            float fStart = Time.time;
            while(Time.time - fStart <= sceneFadeInTime)
            {
                screenBlocker.color = new Color(.0f, .0f, .0f, Mathf.Lerp(.0f, 1.0f, (Time.time - fStart) / sceneFadeInTime));
                yield return null;
            }

            if(conditionFunc != null)
                yield return new WaitUntil( () => conditionFunc() );

            // Turn On/Off Instantly.
            base.SwitchUnit(nextModuleId, null);


            fStart = Time.time;
            while(Time.time - fStart <= sceneFadeOutTime)
            {
                screenBlocker.color = new Color(.0f, .0f, .0f, Mathf.Lerp(1.0f, 0.0f, (Time.time - fStart) / sceneFadeOutTime));
                yield return null;
            }
            screenBlocker.enabled = false;
        }
    }
}
