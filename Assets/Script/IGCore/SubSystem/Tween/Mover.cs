using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Tween
{
    public class Mover : EaseFuncLib
    {
        object Param;
        AnimationCurve Curve;

        protected virtual void Start() { }
        protected virtual void Update() { }


        // Trigger  -----------------------------------
        //
        public void Trigger(Vector3 vLocalStart, Vector3 vLocalEnd, float duration, object param, Action<object> finAction = null)
        {
            Param = param;

            UpdateEaseFunction();

            Curve = null;
            StartCoroutine(coTween(vLocalStart, vLocalEnd, duration, finAction));
        }

        public void TriggerWithEase(DurationEase easeType, Vector3 vLocalStart, Vector3 vLocalEnd, float duration, object param, Action<object> finAction)
        {
            EaseType = easeType;

            Trigger(vLocalStart, vLocalEnd, duration, param, finAction);
        }

        public void TriggerWithCurve(AnimationCurve curve, Vector3 vLocalStart, Vector3 vLocalEnd, float duration, object param, Action<object> finAction)
        {
            Curve = curve;
            Param = param;

            if(Curve == null)
                UpdateEaseFunction();

            StartCoroutine(coTween(vLocalStart, vLocalEnd, duration, finAction));
        }


        // Member func  -----------------------------------
        //
        IEnumerator coTween(Vector3 vLocalStart, Vector3 vLocalEnd, float duration, Action<object> finAction)
        {
            transform.localPosition = vLocalStart;

            float fStartT = Time.time;
            while (Time.time < fStartT + duration)
            {
                //transform.localScale = Vector3.Lerp(vStart, vTo, Mathf.Clamp01((Time.time - fStartT) / duration));

                float timeDelta = Curve == null ? durationEaseFunc(Time.time - fStartT, duration) :
                                                  Curve.Evaluate((Time.time - fStartT) / duration);

                transform.localPosition = Vector3.LerpUnclamped(vLocalStart, vLocalEnd, timeDelta);

                //float timeDelta = Time.time - fStartT;
                //transform.localPosition = Vector3.Lerp(vStart, vEnd, timeDelta / duration );
                yield return null;
            }
            transform.localPosition = vLocalEnd;

            if (finAction != null)
                finAction.Invoke(Param);
        }

    }

}