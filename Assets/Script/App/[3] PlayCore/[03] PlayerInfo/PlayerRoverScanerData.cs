
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
    public class UsingRoverScanerInfo
    {
        [SerializeField] string missionId = string.Empty;
        [SerializeField] float remainTime = 0;


        // Accessor.
        public string MissionId => missionId;
        public float RemainTime { get { return remainTime; } set { remainTime = value; } }
        public void SetData(string idMission, float time)
        {
            missionId = idMission;     remainTime = time;
        }
        public bool IsInMission()
        {
            return !string.IsNullOrEmpty(missionId) && remainTime>0;
        }
    }

    

    public partial class PlayerData
    {
        // Serialize Fields.
        [SerializeField] UsingRoverScanerInfo usingRoverScanerInfo;
        [SerializeField] string unclaimedMissionId;

        // Accessor.
        public UsingRoverScanerInfo UsingRoverScanerInfo => usingRoverScanerInfo;
        public string UnclaimedMissionId => unclaimedMissionId;
        


        //==========================================================================
        //
        // Rovers Control.
        //
        //
        public bool StartRoverScan(RoverMission mission)
        {
            if (usingRoverScanerInfo.IsInMission())
                return false;

            usingRoverScanerInfo.SetData(mission.Id, mission.Duration);
            return true;
        }

        //
        public void FinishRoverScan(RoverMission mission)
        {
            if (!usingRoverScanerInfo.IsInMission())
                return;

            Assert.IsTrue(mission.Id == usingRoverScanerInfo.MissionId);

            unclaimedMissionId = mission.Id;

            // Clear.
            usingRoverScanerInfo.SetData(string.Empty, .0f);
        }

        //
        public void ClaimRoverMissionReward(RoverMission mission)
        {
            if (string.IsNullOrEmpty(unclaimedMissionId))
                return;

            Assert.IsTrue(unclaimedMissionId == usingRoverScanerInfo.MissionId);

            // Take Rewards.
            for (int q = 0; q < mission.Rewards.Count; ++q)
            {
               // if (mission.Rewards[q].Money.Type == eCurrencyType.MINING_COIN)
               //     UpdateMoney(mission.Rewards[q].Money.BIAmount, mission.Rewards[q].Money.Type);
               // else
               //     UpdateMoney(mission.Rewards[q].Money.Amount, mission.Rewards[q].Money.Type);
            }

            unclaimedMissionId = string.Empty;
        }

    }
}
