using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class TwoSpotRunner : MonoBehaviour
    {
        [SerializeField] Transform Target1;
        [SerializeField] Transform Target2;
        [SerializeField] float Target1OffsetRate = .0f;     // 0(target1), 1.0f(target2)
        [SerializeField] float Target2OffsetRate = .0f;     // 0(target2), 1.0f(target1)
        [SerializeField] float Duration;


        [HideInInspector] public UnityEvent OnArrivalAtTarget1;
        [HideInInspector] public UnityEvent OnArrivalAtTarget2;

        Vector3 mPosTarget1, mPosTarget2;

        bool mIsReady = false;
        float mElapsedTime;
        bool mIsToAway;
        float mProgress = .0f;

        protected Vector3 To => mIsToAway ? mPosTarget2 : mPosTarget1;

        public virtual void Init(Transform target1, Transform target2, float _duration = .0f)
        {
            Assert.IsNotNull(target1);
            Assert.IsNotNull(target2);

            Target1 = target1;
            Target2 = target2;

            mIsToAway = true;

            Duration = _duration > Mathf.Epsilon ? _duration : Duration;

            mPosTarget1 = Vector3.Lerp(Target1.position, Target2.position, Target1OffsetRate);
            mPosTarget2 = Vector3.Lerp(Target2.position, Target1.position, Target2OffsetRate);

            transform.position = mIsToAway ? mPosTarget1 : mPosTarget2;

            mElapsedTime = .0f;

            mIsReady = true;
        }

        public void SetDuration(float duration)
        {
            Duration = duration;

            // recalculate current pos by resetting elTime.
            mElapsedTime = Duration * mProgress;
        }
        public float GetDuration() => Duration;

        public void SetVelocity(float _velocity)
        {
            Assert.IsTrue(_velocity > .0f);

            // v = s/t ===> t = s/v
            Vector3 vDist = Target1.position - Target2.position;
            SetDuration( vDist.magnitude / _velocity );
        }

        public void Stop()
        {
            mIsReady = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            mIsReady = Target1 != null && Target2 != null;
            if (!mIsReady)
                return;

            Init(Target1, Target2, Duration);
        }

        // Update is called once per frame
        virtual protected void Update()
        {
            if (!mIsReady)
                return;

           _internalUpdate(Time.deltaTime);
        }

        protected void _internalUpdate(float deltaTime)
        { 
            // Refresh this every frame as we looking for a global pos.
            mPosTarget1 = Vector3.Lerp(Target1.position, Target2.position, Target1OffsetRate);
            mPosTarget2 = Vector3.Lerp(Target2.position, Target1.position, Target2OffsetRate);

            mElapsedTime += deltaTime;

            float fRate = mElapsedTime / Duration;
            bool done = false;
            if (fRate >= 1.0f)
            {
                fRate = 1.0f;
                done = true;
            }

            mProgress = fRate;

            Vector3 vStart = mIsToAway ? mPosTarget1 : mPosTarget2;
            Vector3 vEnd = mIsToAway ? mPosTarget2 : mPosTarget1;

            transform.position = Vector3.Lerp(vStart, vEnd, fRate);

            if (done)
            {
                if (mIsToAway)  OnArrivalAtTarget2?.Invoke();
                else            OnArrivalAtTarget1?.Invoke();

                mIsToAway = !mIsToAway;

                transform.position = vEnd;
                mElapsedTime = .0f;
                mProgress = .0f;
            }
        }
    }
}