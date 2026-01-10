using Core.Events;
using IGCore.MVCS;
using App.GamePlay.IdleMiner.Common.Types;
using UnityEngine.Assertions;
using System;

public class DailyMissionModel : AModel
{
    DailyMissionConfig config;

    public DailyMissionConfig Config => config;
    public DailyMissionPlayerModel DMPlayerModel => playerData as DailyMissionPlayerModel;

    public DailyMissionModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }

    public override void Init(object data = null) 
    {
        base.Init(data);

        config = (DailyMissionConfig)data;
    }

    public override void Dispose()
    {
        base.Dispose();
        config = null;
    }

    public DailyMissionConfig.Mission GetData(DailyMissionConfig.GoalType goalType)
    {
        Assert.IsNotNull(config);

        return config.GetData(goalType);
    }

    public void Pump()
    {
        DMPlayerModel.Pump();
    }

    public void Reset()
    {
        DMPlayerModel.Reset();
    }

    public bool Claim(DailyMissionConfig.GoalType goalType)
    {
        Assert.IsNotNull(config);

        var configData = GetData(goalType);
        Assert.IsNotNull(configData);

        if(DMPlayerModel.Claim(goalType, configData.GoalCount))
        {
            string rewardType = configData.RewardType.ToLower();

            bool offset = true;
            if(rewardType=="iap" || rewardType=="volt")
                context.RequestQuery("AppPlayerModel", "UpdateIAPCurrency", (int)configData.RewardAmount, offset);
            else
                Assert.IsTrue(false, $"[DailyMission] Unsupported reward type [{rewardType}] !!!");
            
            return true;
        }

        return false;
    }

    public void IncreaseGoalCount(DailyMissionConfig.GoalType goalType)
    {
        long count = DMPlayerModel.IncreaseGoalCount(goalType);

        var configData = GetData(goalType);
        Assert.IsNotNull(configData);
        
        if(configData.GoalCount == count)
            EventSystem.DispatchEvent(EventID.DAILY_MISSION_GOAL_ACHIEVED, goalType);
    }
}
