using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.MetaSystem.Bonus
{
    public class TimedBonus : MonoBehaviour, IBonus
    {
        [Header("Configuration (External Inputs)")]
    
        [SerializeField] public int awardingIntervalInMin = 30;
        [SerializeField] GameObject objectView;

        public Action OnBonusClaimAttempt;
        public bool IsEnabled => _isEnable;


        bool _isReadyToClaim = false;
        private bool isReadyToClaim
        {
            set
            {
                objectView?.SetActive(value);
                _isReadyToClaim = value;
            }
            get => _isReadyToClaim;
        }
        
        bool _isEnable = false;
        DateTime nextClaimTime;
        long lastClaimTimeTicks; 

        void Start()
        {
            isReadyToClaim = false;
            _isEnable = false;

            //Initialize();
            StartCoroutine( coTimerUpdate() );
        }

        IEnumerator coTimerUpdate()
        {
            var waitForASec = new WaitForSeconds(1.0f);
            while(true)
            {
                if(_isEnable)
                    CheckBonusReadyState();

                yield return waitForASec;
            }
        }

        
        // ----------------------------------------------------
        // Public Functions
        // ----------------------------------------------------

        public void SetEnable(bool enable)
        {
            if(_isEnable == enable) 
                return;

            _isEnable = enable;

            if(!enable)      return;

            if (lastClaimTimeTicks == 0)
            {
                lastClaimTimeTicks = DateTime.UtcNow.Ticks;
            }
        
            CalculateNextClaimTime();
        
            CheckBonusReadyState(); 
        }

        /// <summary>
        /// UI 버튼이 클릭되었을 때 호출되는 함수
        /// </summary>
        public void ClaimBonus()
        {
            Assert.IsTrue( isReadyToClaim );
            
            OnBonusClaimAttempt?.Invoke();
        
            Debug.Log("Timed Bonus Claimed.");

            UpdateClaimTime();
        }

        // ----------------------------------------------------
        // Private Logic
        // ----------------------------------------------------

        private void CalculateNextClaimTime()
        {
            DateTime lastClaim = new DateTime(lastClaimTimeTicks, DateTimeKind.Utc);
            TimeSpan interval = TimeSpan.FromMinutes( awardingIntervalInMin );
        
            nextClaimTime = lastClaim + interval;
        }

        private void UpdateClaimTime()
        {
            lastClaimTimeTicks = DateTime.UtcNow.Ticks;
     
            CalculateNextClaimTime();
     
            SetReadyState(false);
        }

        private void CheckBonusReadyState()
        {
            TimeSpan timeRemaining = nextClaimTime - DateTime.UtcNow;

            if (timeRemaining <= TimeSpan.Zero)
            {
                if (!isReadyToClaim)
                {
                    SetReadyState(true);
                }
            }
            else
            {
                // Debug.Log(timeRemaining);
                if (isReadyToClaim)
                {
                    SetReadyState(false);
                }
            }
        }

        private void SetReadyState(bool ready)
        {
            isReadyToClaim = ready;

            // 외부의 UI가 버튼을 활성화/비활성화 하도록 알립니다.
            // OnReadyStateChanged.Invoke(ready); 
        }
    }
}