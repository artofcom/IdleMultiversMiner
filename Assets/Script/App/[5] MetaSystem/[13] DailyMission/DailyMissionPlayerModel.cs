using App.GamePlay.Common;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using App.GamePlay.IdleMiner.Common.Types;

public class DailyMissionPlayerModel : GatewayWritablePlayerModel
{
    [Serializable]
    public class ProgressInfo
    {
        [SerializeField] DailyMissionConfig.GoalType goalType;
        [SerializeField] long count = 0;
        [SerializeField] bool isClaimed = false;

        public ProgressInfo(DailyMissionConfig.GoalType goalType)
        {
            this.goalType = goalType;;
        }
        public void IncreaseCount() { ++count; } 
        public void Claim() { isClaimed = true; }

        public DailyMissionConfig.GoalType GoalType => goalType;
        public long Count => count;
        public bool IsClaimed => isClaimed;
    }

    [Serializable]
    public class ProgressInfoBundle
    {
        [SerializeField] long timeStamp = DateTime.UtcNow.Ticks;
        [SerializeField] List<ProgressInfo> listProgressInfos = new List<ProgressInfo>();

        public List<ProgressInfo> ListProgressInfos => listProgressInfos;
        public long TimeStamp => timeStamp;
        public void Add(ProgressInfo info)  {   ListProgressInfos?.Add(info);   }
    }

    ProgressInfoBundle progressInfo;

    public ProgressInfoBundle ProgressBundleInfo => progressInfo;


    string DataKey => IdleMinerContext.AccountName + "_DailyMissionData";



    public DailyMissionPlayerModel(AContext ctx, IDataGatewayService gatewayService) : base(ctx, gatewayService) { }


    public override void Init()
    {
        base.Init();

        LoadData();

        IsInitialized = true;
    }

    public override void Dispose()
    {
        base.Dispose();

        IsInitialized = false;
    }

    public long IncreaseGoalCount(DailyMissionConfig.GoalType goalType)
    {
        var progressInfo = GetProgressInfo(goalType);
        UnityEngine.Assertions.Assert.IsNotNull(progressInfo);

        progressInfo.IncreaseCount();
        Debug.Log($"[DailyMission] : {goalType} count has been added to {progressInfo.Count}.");

        return progressInfo.Count;
    }

    public bool Claim(DailyMissionConfig.GoalType goalType, long goalCount) 
    {
        var progressInfo = GetProgressInfo(goalType);
        if(progressInfo == null)
            return false;

        if(progressInfo.IsClaimed)
            return false;

        if(progressInfo.Count < goalCount)
            return false;

        progressInfo.Claim();
        return true;
    }

    ProgressInfo GetProgressInfo(DailyMissionConfig.GoalType goalType) 
    {
        for(int q = 0; q < progressInfo.ListProgressInfos.Count; q++) 
        {
            if(progressInfo.ListProgressInfos[q].GoalType == goalType)
                return progressInfo.ListProgressInfos[q];
        }
        return null;
    }

    void LoadData()
    {
        FetchData(DataKey, out progressInfo, null);
        bool resetData = progressInfo==null || progressInfo.ListProgressInfos.Count==0;
        
        if(progressInfo != null)
            resetData |= (false == IsTodayUtc(progressInfo.TimeStamp));
        
        if(resetData)   Reset();
    }

    public override List<Tuple<string, string>> GetSaveDataWithKeys()
    {
        UnityEngine.Assertions.Assert.IsNotNull(progressInfo);
        List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
        listDataSet.Add(new Tuple<string, string>(DataKey, JsonUtility.ToJson(progressInfo)));
        return listDataSet;
    }

    public void Reset()
    {
        progressInfo = new ProgressInfoBundle();
        for(int k = 0; k < (int)DailyMissionConfig.GoalType.MAX; ++k)
            progressInfo.Add(new ProgressInfo((DailyMissionConfig.GoalType)k));

        (context as IdleMinerContext).SaveMetaData();
    }

    public bool Pump()
    {
        if(!IsTodayUtc(progressInfo.TimeStamp))
        {
            Reset();

            EventSystem.DispatchEvent(EventID.DAILY_MISSION_RESET);

            return true;
        }
        return false;
    }




    bool IsTodayUtc(long ticks)
    {
        DateTime inputDate = new DateTime(ticks, DateTimeKind.Utc);
        DateTime nowDate = DateTime.UtcNow;
        return inputDate.Date == nowDate.Date;
    }
}
