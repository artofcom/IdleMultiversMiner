
using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class DailyAttendItemData
    {
        [SerializeField] int id;
        [SerializeField] CurrencyAmount reward = null;
        
        public int Id => id;
        public CurrencyAmount Reward => reward;
        
        public void Convert()
        {
            reward?.Init();
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
    }


    public class DailyAttendModel : IGCore.MVCS.AModel
    {
        public const string EVENT_ON_DAILY_ATTEND_CLAIMED = "EVENT_ON_DAILY_ATTEND_CLAIMED";

        public DailyAttendData DailyAttendData { get; private set; }

        public DailyAttendModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

        public override void Init(){}

        /*
        void InitDailyAttend()
        {
            var textData = Resources.Load<TextAsset>(GAMEDATA_PATH + "DailyAttendData");
            DailyAttendData = JsonUtility.FromJson<DailyAttendData>(textData.text);
            DailyAttendData.Convert();

            Assert.IsNotNull(MeteorData);
        }

        public void ResumeDailyAttend()
        {
            PlayerData.ResumeDailyReward();
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
