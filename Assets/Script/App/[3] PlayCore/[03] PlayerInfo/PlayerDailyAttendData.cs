
using System.Numerics;
using UnityEngine;
using Core.Events;
using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class DailyAttendProgInfo
    {
        const int TOTAL_DAY = 8;

        [SerializeField] int daysStreak = 0;            // 0 ~ Total_day.
        [SerializeField] string lastClaimedTimeTick;

        bool canClaim = false;
        int idxCurDay = 0;
        long _lastClaimedTimeTick;
        long LastClaimedTimeTick
        {
            set
            {
                _lastClaimedTimeTick = value;
                lastClaimedTimeTick = _lastClaimedTimeTick.ToString();
            }
            get => _lastClaimedTimeTick;
        }

        // Accessor.
        public int DaysStreak => daysStreak;
        public bool CanClaim => canClaim;
        public int IndexCurDay => idxCurDay;

        public void Resume()
        {
            Pump();
        }

        public bool Claim()
        {
            if(canClaim) 
            {
                canClaim = false;
                daysStreak = 1 + (daysStreak % TOTAL_DAY);          // 1 ~ Total_day.
              //  LastClaimedTimeTick = PlayerData.UTCNowTick;
                return true;
            }
            return false;
        }

        public void Pump()
        {       
            /*
            DateTime lastClaimDate = default(DateTime);
            DateTime today = (new DateTime(PlayerData.UTCNowTick)).Date;

            long lastClaimedTick = 0;
            bool parseRet = long.TryParse(lastClaimedTimeTick, out lastClaimedTick);
            if (parseRet)
                lastClaimDate = new DateTime(lastClaimedTick);
            
            void ResetClaim() { canClaim = true; idxCurDay = 0;  daysStreak = 0; }

            if(!parseRet)
            {
                ResetClaim();
            }
            else
            {
                TimeSpan diff = today - lastClaimDate.Date;
                
                if(diff.Days > 1)           ResetClaim();
                else if(diff.Days == 1)
                {
                    canClaim = true;
                    idxCurDay = daysStreak % TOTAL_DAY;
                }
                else
                {
                    idxCurDay = daysStreak - 1;
                    canClaim = false;
                }
            }

            Debug.Log($"[DailyReward] : {canClaim}, {daysStreak}, {idxCurDay}");*/
        }

        
        public DateTime GetNextRewardTime(bool isLocal)
        {
         /*   DateTime now = new DateTime(PlayerData.UTCNowTick);
            DateTime AM00today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);   //TimeZoneInfo.ConvertTimeToUtc(now);
            DateTime AM00tomorrow = AM00today.AddDays(1); 

            if(isLocal) return AM00tomorrow;
            else return AM00tomorrow;*/
            return DateTime.UtcNow;
        }
    }

    

    public partial class PlayerData
    {
        // Serialize Fields.
        [SerializeField] DailyAttendProgInfo dailyAttendProgInfo;

        // Accessor.
        public DailyAttendProgInfo DailyAttendProgInfo => dailyAttendProgInfo;
        


        //==========================================================================
        //
        // Daily Attend Data Control.
        //
        //
        void SaveDailyAttendData()
        {
         //   WriteFileInternal($"{mAccount}_DailyAttendData", dailyAttendProgInfo);
        }

        void LoadDailyAttendData()
        {
           // ReadFileInternal($"{mAccount}_DailyAttendData", ref dailyAttendProgInfo);
        }

        public void ResumeDailyReward()
        {
            if(dailyAttendProgInfo == null)
                dailyAttendProgInfo = new DailyAttendProgInfo();
            dailyAttendProgInfo.Resume();
        }
        public bool ClaimDailyReward(int idxDay)
        {
            if (dailyAttendProgInfo.IndexCurDay != idxDay)
                return false;

            return dailyAttendProgInfo.Claim();
        }
        public void PumpDailyAttend()
        {
            dailyAttendProgInfo.Pump();
        }

        
        
#if UNITY_EDITOR

        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/DailyAttend")]
        private static void ClearDailyAttendData()
        {
          /* string account = PlayerPrefs.GetString(PREFAB_ACCOUNT, string.Empty);
            if (!string.IsNullOrEmpty(account))
            {
                WriteFileInternal($"{GameKey}_{account}_DailyAttendData", string.Empty, false);
                Debug.Log("Deleting All Daily Attend PlayerPrefab...");
            }
            else
                Debug.Log("Could not find player account.");*/
        }
#endif
    }
}
