using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using Core.Utils;

namespace Core.Tween
{
    public abstract class ATweener : MonoBehaviour
    {
        [SerializeField] protected Transform transformFrom;
        [SerializeField] protected Transform transformTo;
        [SerializeField] protected float duration = 1.0f;
        [SerializeField] protected bool isTriggerWhenStart = false;
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform transformParent;
        [SerializeField] protected Transform poolerParent;      // Activate GameObjectPool when the parent tranform is valid.

        protected GameObjectPooler Pooler;
        protected Dictionary<GameObject, object> dictObjectCache = new Dictionary<GameObject, object>();

        Transform TransformParent => transformParent;

        public Transform TransformFrom => transformFrom;
        public Transform TransformTo => transformTo;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            Assert.IsNotNull(projectilePrefab);
            Assert.IsNotNull(transformParent);

            if (poolerParent != null)
            {
                Pooler = new GameObjectPooler();
                Pooler.Create(projectilePrefab, poolerParent);
            }

            if (isTriggerWhenStart)
            {
                Assert.IsNotNull(transformFrom);
                Assert.IsNotNull(transformTo);
            
                Trigger( transformFrom.position, transformTo.position, 
                         isWorldPos:true, duration, objCache:null, onFinished:null );
            }
        }


        public virtual void Trigger(Vector3 vFrom, Vector3 vTo, bool isWorldPos, float duration, object objCache, Action<object> onFinished)
        {
            GameObject objProjectile = CreateProjectile();
            Assert.IsTrue(!dictObjectCache.ContainsKey(objProjectile));
            dictObjectCache.Add(objProjectile, objCache);

            duration = duration < .0f ? this.duration : duration;

            if(isWorldPos)
            {
                vFrom = TransformParent.InverseTransformPoint(vFrom);
                vTo = TransformParent.InverseTransformPoint(vTo);
            }
            ProcessTweener(objProjectile.transform, vFrom, vTo, duration, onFinished);
        }

        public virtual void Trigger(Vector3 vTo, bool isWorldPos, float duration, object objCache, Action<object> onFinished)
        {
            GameObject objProjectile = CreateProjectile();
            Assert.IsTrue(!dictObjectCache.ContainsKey(objProjectile));
            dictObjectCache.Add(objProjectile, objCache);

            Vector3 vLocalFrom = TransformParent.InverseTransformPoint(transformFrom.position);
            duration = duration < .0f ? this.duration : duration;

            if(isWorldPos)
                vTo = TransformParent.InverseTransformPoint(vTo);

            ProcessTweener(objProjectile.transform, vLocalFrom, vTo, duration, onFinished);
        }

        public virtual float GetDistance(Vector3 vTo, bool isWorldPos)
        {
            Vector3 vWorldFrom = transformFrom.position;
            if(!isWorldPos)
                vTo = TransformParent.TransformPoint(vTo);

            return Vector3.Distance(vWorldFrom, vTo);
        }


        protected virtual GameObject CreateProjectile()
        {
            if (Pooler != null)
                return GameObjectPooler.GetPoolItem(Pooler, transformParent);

            return Instantiate(projectilePrefab, transformParent, false);
        }
        
        protected virtual void ReleaseProjectile(GameObject objProjectile)
        {
            // double check.
            if (dictObjectCache.ContainsKey(objProjectile))
                dictObjectCache.Remove(objProjectile);

            if (Pooler != null)
                GameObjectPooler.ReleasePoolItem(Pooler, objProjectile);
            else
                Destroy(objProjectile);
        }

        protected virtual void ProcessTweener(Transform transformProjectile, Vector3 vLocalFrom, Vector3 vLocalTo, float duration, Action<object> onFinished)
        {
            // Tween.
            Assert.IsTrue(false, "Should override this func.!");

            onFinished?.Invoke(dictObjectCache[transformProjectile.gameObject]);

            dictObjectCache.Remove(transformProjectile.gameObject);
            ReleaseProjectile(transformProjectile.gameObject);
        }
    }
}
