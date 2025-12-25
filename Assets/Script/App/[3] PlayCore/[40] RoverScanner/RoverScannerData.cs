
using System.Numerics;
using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class RoverRewardInfo
    {
        [SerializeField] CurrencyAmount money;
        [SerializeField] float chance;

        public CurrencyAmount Money => money;
        public float Chance => chance;
    }


    [Serializable]
    public class RoverMission
    {
        [SerializeField] string id;
        [SerializeField] List<RoverRewardInfo> rewards;
        [SerializeField] int duration;

        public string Id => id;
        public List<RoverRewardInfo> Rewards => rewards;
        public int Duration => duration;
    }


    [Serializable]
    public class RoverData
    {
        [SerializeField] List<RoverMission> missions;

        public List<RoverMission> Missions => missions;
    }






    public class RoverScannerDataModel : IGCore.MVCS.AModel// IdleMinerModel
    {
        public RoverData RoverData { get; private set; }

        public RoverScannerDataModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

        public override void Init(object data = null)
        {
           // var textData = Resources.Load<TextAsset>(GAMEDATA_PATH + "RoverScanerData");
           // RoverData = JsonUtility.FromJson<RoverData>(textData.text);

           // Assert.IsNotNull(RoverData);
          //  Assert.IsTrue(RoverData.Missions != null && RoverData.Missions.Count > 0);
        }

        public RoverMission GetMission(string missionId)
        {
            if (string.IsNullOrEmpty(missionId))
                return null;

            for(int q = 0; q < RoverData.Missions.Count; ++q)
            {
                if (RoverData.Missions[q].Id == missionId)
                    return RoverData.Missions[q];
            }
            return null;
        }

        public void ClaimRoverMissionReward()
        {
          //  var missionData = GetMission(PlayerData.UnclaimedMissionId);
          //  if (missionData == null)
           //     return;

          //  PlayerData.ClaimRoverMissionReward(missionData);
        }
    }
}
