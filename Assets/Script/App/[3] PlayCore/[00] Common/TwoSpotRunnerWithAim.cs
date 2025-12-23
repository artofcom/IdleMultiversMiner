using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class TwoSporRunnerWithAim : TwoSpotRunner
    {
        enum Type { XFLIP, FULL_ROT };
        [SerializeField] Type aimType = Type.XFLIP;
        Vector3 vOldPos;
        Vector3 vInitScale;

        public override void Init(Transform target1, Transform target2, float _duration = .0f)
        {
            base.Init(target1, target2, _duration);
            vInitScale = transform.localScale;
        }

        protected override void Update()
        {
            base.Update();

            Vector3 vNewDir = transform.localPosition - vOldPos;
            Vector2 vDir = new Vector2(vNewDir.x, vNewDir.y);
            if (aimType == Type.XFLIP)
            {
                if (vDir.x > 0)
                    transform.localScale = new Vector3(-vInitScale.x, vInitScale.y, vInitScale.z);
                else
                    transform.localScale = vInitScale;
            }
            else
            {
                vDir.Normalize();
                transform.rotation = Quaternion.FromToRotation(Vector3.up, vDir);
            }

            vOldPos = transform.localPosition;
        }
    }
}