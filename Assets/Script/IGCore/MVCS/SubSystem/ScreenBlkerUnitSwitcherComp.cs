using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public class ScreenBlkerUnitSwitcherComp : UnitSwitcherComp
    {
        [SerializeField] Image screenBlocker;
        [SerializeField] float sceneFadeTime = 1.0f;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(screenBlocker);
        }

        public override void SwitchUnit(string nextModuleId)
        {
            StartCoroutine(coOnEventClose(nextModuleId));   
        }

        IEnumerator coOnEventClose(string nextModuleId)
        {
            screenBlocker.enabled = true;
            screenBlocker.color = new Color(.0f, .0f, .0f, .0f);
            float fStart = Time.time;
            while(Time.time - fStart <= sceneFadeTime)
            {
                screenBlocker.color = new Color(.0f, .0f, .0f, Mathf.Lerp(.0f, 1.0f, (Time.time - fStart) / sceneFadeTime));
                yield return null;
            }


            // Turn On/Off Instantly.
            base.SwitchUnit(nextModuleId);


            fStart = Time.time;
            while(Time.time - fStart <= sceneFadeTime)
            {
                screenBlocker.color = new Color(.0f, .0f, .0f, Mathf.Lerp(1.0f, 0.0f, (Time.time - fStart) / sceneFadeTime));
                yield return null;
            }
            screenBlocker.enabled = false;
        }
    }
}
