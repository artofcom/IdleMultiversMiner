
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
    public class MeteorInfo
    {
        [SerializeField] string lastRewardTime;


        public string LastRewardTime { get => lastRewardTime; set => lastRewardTime = value; }
        public MeteorInfo(string tick)
        {
            lastRewardTime = tick;
        }
    }


    public partial class PlayerData
    {
        
        // Serialize Fields.
        MeteorInfo meteorInfo;
        

        // Accessor.
        public MeteorInfo MeteorInfo => meteorInfo;
        


        void LoadMeteorData()
        {
           // ReadFileInternal<MeteorInfo>($"{mAccount}_MeteorInfo", ref meteorInfo);
          //  if(meteorInfo == null || string.IsNullOrEmpty(meteorInfo.LastRewardTime))
          //      meteorInfo = new MeteorInfo(PlayerData.UTCNowTick.ToString());
        }

        void SaveMeteorData()
        {
          //  WriteFileInternal($"{mAccount}_MeteorInfo", meteorInfo);
        }

        public void RecordMeteorRewardedTime()
        {
           // if(meteorInfo == null)
           //     meteorInfo = new MeteorInfo(PlayerData.UTCNowTick.ToString());
           // else 
          //      meteorInfo.LastRewardTime = PlayerData.UTCNowTick.ToString();
        }
    }
}
