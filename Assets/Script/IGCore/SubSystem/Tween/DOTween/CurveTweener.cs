using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Core.Utils;

namespace Core.Tween
{
    public class CurveTweener : ATweener
    {
        [SerializeField] protected float midPointStrength = 5.0f;
        [SerializeField] protected bool useInnerCurve = true;


        protected override void ProcessTweener(Transform transformProjectile, Vector3 vLocalFrom, Vector3 vLocalTo, float durationInSec, Action<object> onFinished)
        {
            transformProjectile.localPosition = vLocalFrom;

            const int numOfPoints = 3;
            Vector3[] arrPoints = new Vector3[numOfPoints];
            arrPoints[0] = vLocalFrom;
            arrPoints[numOfPoints - 1] = vLocalTo;

            // Calculate Mid pos and its perpendicular vector, so we can get a proper mid path point.
            //
            Vector3 vDir = vLocalTo - vLocalFrom;
            vDir.Normalize();
            Vector3 vRhs = useInnerCurve ? (vLocalTo.x > vLocalFrom.x ? Vector3.back : Vector3.forward) : (vLocalTo.x > vLocalFrom.x ? Vector3.forward : Vector3.back);
            Vector3 vMidPath = Vector3.Cross(vDir, vRhs);
            vMidPath.Normalize();

            //
            //
            //                                arrPoints[2]
            //                                   
            //                   arrPoints[1]   /
            //                                 /
            //                        \       /
            //                         \     /
            //                          \   /
            //                           \ /
            //                            /
            //                           /
            //                          / 
            //                         /
            //                        /
            //
            //                      arrPoints[0]
            //
            //

            const float fMid = 0.5f;
            arrPoints[1] = Vector3.Lerp(arrPoints[0], arrPoints[2], fMid);
            arrPoints[1] = arrPoints[1] + vMidPath * midPointStrength;

            transformProjectile.DOLocalPath(arrPoints, durationInSec, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.Linear).OnComplete(() =>
            {
                onFinished?.Invoke(dictObjectCache[transformProjectile.gameObject]);

                ReleaseProjectile(transformProjectile.gameObject);

            });
        }



        protected override GameObject CreateProjectile()
        {
            GameObject objProjectile;
            if (Pooler != null)
                objProjectile = GameObjectPooler.GetPoolItem(Pooler, transformParent);
            else
                objProjectile = Instantiate(projectilePrefab, transformParent, false);

            Aimer aimer = objProjectile.GetComponent<Aimer>();
            if (aimer != null)
                aimer.SetAimTarget(transformTo);

            return objProjectile;
        }
    }
}