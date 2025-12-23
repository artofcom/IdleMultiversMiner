using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Tween;
using System;

public class MinerMover : Mover
{
    [SerializeField] Vector3 vMoveTo = Vector3.zero;
    [SerializeField] bool isOffset = true;
    [SerializeField] bool TriggerWhenEnable = false;
    [SerializeField] float MinDuration = 1.0f, MaxDuration = 1.0f;
    [SerializeField] bool isLoop = false;
    //[SerializeField] bool isPingPong = false;
    [SerializeField] float MinLoopInterval = 0.0f, MaxLoopInterval = 0.001f;


    // Should be all local Positions.
    Vector3 vTargetPos;
    Vector3 vStartPos, vEndPos;
    bool IsHeadingForward = true;
    float Duration;

    // Start is called before the first frame update
    protected override void Start()
    {
        IsHeadingForward = true;
        Duration = UnityEngine.Random.Range(MinDuration, MaxDuration);
        vStartPos = transform.localPosition;
        vEndPos = isOffset ? transform.localPosition + vMoveTo : vMoveTo;
        vTargetPos = IsHeadingForward ? vEndPos : vStartPos;
    }


    private void OnEnable()
    {
        if (TriggerWhenEnable)
            StartCoroutine(coTriggerActionWithDelay(UnityEngine.Random.Range(MinLoopInterval, MaxLoopInterval), TriggerMovement) );
    }


    void TriggerMovement()
    {
        base.Trigger(transform.localPosition, vTargetPos, Duration, this, (param) =>
        {
            MinerMover mover = (MinerMover)param;

            if (mover.isLoop)
            {
                IsHeadingForward = !IsHeadingForward;
                vTargetPos = IsHeadingForward ? vEndPos : vStartPos;

                StartCoroutine(coTriggerActionWithDelay(UnityEngine.Random.Range(MinLoopInterval, MaxLoopInterval), TriggerMovement) );
            }
        });
    }

    





    IEnumerator coTriggerActionWithDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
