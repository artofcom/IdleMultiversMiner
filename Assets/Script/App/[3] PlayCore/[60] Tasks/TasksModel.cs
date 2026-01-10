using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

namespace App.GamePlay.IdleMiner
{
    /*
    [Serializable]
    public class DailyAttendItemData
    {
        [SerializeField] int id;
        [SerializeField] CurrencyAmount reward = null;
        
        public int Id => id;
        public CurrencyAmount Reward => reward;
        
        public void Convert()
        {
            reward?.Convert();
        }
    }

    [Serializable]
    public class DailyAttendData
    {
        [SerializeField] List<DailyAttendItemData> listItemData;
     
        public List<DailyAttendItemData> ListItemData => listItemData;

        public void Convert()
        {
            for(int q = 0; q < listItemData.Count; ++q)
                listItemData[q].Convert();
        }
    }*/

    [Serializable]
    public class TaskData
    {
        public enum Type
        {
            COLLECT_MINING_MONEY, 
            HIT_METEOR
        };

        [SerializeField] Type type;
        [SerializeField] int count;
    }

    [Serializable]
    public class TaskBundle
    {
        public enum Type { DAILY, USUAL };

        

    }

    public class TaskModel : IGCore.MVCS.AModel
    {
        
        //public const string EVENT_ON_DAILY_ATTEND_CLAIMED = "EVENT_ON_DAILY_ATTEND_CLAIMED";

        public TaskBundle TaskBundle { get; private set; }

        public TaskModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

        public override void Init(object data = null)
        {
            base.Init(data);
        }

        /*
        void InitDailyAttend()
        {
            var textData = Resources.Load<TextAsset>(GAMEDATA_PATH + "DailyAttendData");
            DailyAttendData = JsonUtility.FromJson<DailyAttendData>(textData.text);
            DailyAttendData.Convert();

            Assert.IsNotNull(MeteorData);
        }

        public void ResumeDailyAttend(int durationInSec)
        {
            PlayerData.ResumeDailyReward(durationInSec);
        }

        public void PumpDailyAttend()
        {
            PlayerData.PumpDailyAttend();
        }

        public void ClaimDailyAttendReward(int idxDay)
        {
            if(PlayerData.ClaimDailyReward(idxDay))
            {
                // reward.
                Assert.IsTrue(idxDay>=0 && idxDay<DailyAttendData.ListItemData.Count);
                PlayerData.AddMoney(DailyAttendData.ListItemData[idxDay].Reward);
                // event.
                Core.Events.EventSystem.DispatchEvent(EVENT_ON_DAILY_ATTEND_CLAIMED, idxDay);
                // log.
                Debug.Log("Daily Attend Calimed..! " + idxDay.ToString());
            }
        }*/
    }
}
