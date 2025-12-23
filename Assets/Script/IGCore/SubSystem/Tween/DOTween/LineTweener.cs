using UnityEngine;
using DG.Tweening;
using System;

namespace Core.Tween
{
    public class LineTweener : ATweener
    {
        //GameObject curProjectile;

        protected override void ProcessTweener(Transform transformProjectile, Vector3 vLocalFrom, Vector3 vLocalTo, float durationInSec, Action<object> onFinished)
        {
            transformProjectile.localPosition = vLocalFrom;

            transformProjectile.DOLocalMove(vLocalTo, duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                onFinished?.Invoke(dictObjectCache[transformProjectile.gameObject]);

                ReleaseProjectile(transformProjectile.gameObject);
            });
        }
    }
}