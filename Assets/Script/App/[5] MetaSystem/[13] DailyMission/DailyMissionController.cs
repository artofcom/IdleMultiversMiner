using App.GamePlay.Common;
using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using Core.Utils;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DailyMissionController : AController
{
    DailyMissionModel DailyMissionModel => model as DailyMissionModel;
    EventsGroup events = new EventsGroup();

    DailyMissionView View => view as DailyMissionView;

    public SpriteConfig CommonSpriteConfigCache { get; set; }

    public DailyMissionController(AUnit unit, AView view, AModel model, AContext context) : base(unit, view, model, context)
    { }

    public override void Init() 
    { 
        events.RegisterEvent(EventID.SKILL_LEARNED, OnSkillLearned);
        events.RegisterEvent(EventID.CRAFT_SUCCESSED, OnCraftSuccessed);
        events.RegisterEvent(EventID.MINING_STAT_UPGRADED, OnMiningStatUpgraded);
        events.RegisterEvent(EventID.ADS_WATCHED, OnAdsWatched);
        events.RegisterEvent(EventID.DAILY_MISSION_GOAL_ACHIEVED, OnGoalAchieved);
        events.RegisterEvent(EventID.DAILY_MISSION_RESET, OnDailyMissionReset);

        View.EventOnBtnClaimClicked += OnBtnClaimClicked;
        View.EventOnBtnResetClicked += OnBtnResetClicked;

        RefreshNotificator();

        unit.StartCoroutine( coUpdate() );
    }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  
    { 
        DailyMissionModel.DMPlayerModel.SeenAllNotification();

        RefreshView();
        RefreshNotificator();
    }
    protected override void OnViewDisable() { }

    public override void Dispose() 
    {
        base.Dispose();

        events.UnRegisterAll();
        View.EventOnBtnClaimClicked -= OnBtnClaimClicked;
        View.EventOnBtnResetClicked -= OnBtnResetClicked;
    }

    int SortByProgStatus(DailyMissionPlayerModel.ProgressInfo a, DailyMissionPlayerModel.ProgressInfo b)
    {
        if(a.IsClaimed != b.IsClaimed)
            return a.IsClaimed ? 1 : -1;
        
        if(!a.IsClaimed) 
            return a.Count > b.Count ? -1 : 1;
        
        return 0;
    }  

    void RefreshView()
    {
        var progressPlayerInfo = DailyMissionModel?.DMPlayerModel?.ProgressBundleInfo;
        if(!view.gameObject.activeSelf || progressPlayerInfo == null)
            return;

        DailyMissionConfig.Mission missionData;
        var listItems = new List<DailyMissionListItemComp.Presentor>();
        var listProgInfo = new List<DailyMissionPlayerModel.ProgressInfo>(progressPlayerInfo.ListProgressInfos);

        listProgInfo.Sort((a, b) => { return SortByProgStatus(a, b); });

        for(int q = 0; q < listProgInfo.Count; ++q)
        {
            DailyMissionPlayerModel.ProgressInfo progInfo = listProgInfo[q];
            
            missionData = DailyMissionModel.GetData(progInfo.GoalType);
            Assert.IsNotNull(missionData);

            long collected = progInfo.Count;    // Random.Range(0, (int)missionData.GoalCount+1); //  progInfo.Count;
            bool isClaimable = missionData.GoalCount <= collected;
            DailyMissionListItemComp.Presentor presentor;
            Sprite sprReward = CommonSpriteConfigCache?.GetSprite(missionData.RewardType);
            if(isClaimable)
            {
                presentor = new DailyMissionListItemComp.Presentor((int)progInfo.GoalType, 
                    missionData.Icon, missionData.Name, missionData.Desc, sprReward, missionData.RewardAmount, progInfo.IsClaimed);
            }
            else
            {
                presentor = new DailyMissionListItemComp.Presentor((int)progInfo.GoalType, 
                    missionData.Icon, missionData.Name, missionData.Desc, sprReward, missionData.RewardAmount, missionData.GoalCount, collected);
            }

            listItems.Add( presentor );
        }

        DateTime now = DateTime.UtcNow;
        DateTime tomorrowMidnight = now.Date.AddDays(1);
        TimeSpan diff = tomorrowMidnight - now;
        string resetTime = TimeExt.ToTimeString((int)diff.TotalSeconds, TimeExt.UnitOption.NO_USE, TimeExt.TimeOption.HOUR);
        string msg = $"Reset in {resetTime}.";

        view.Refresh(new DailyMissionView.Presentor(msg, listItems));
    }


    IEnumerator coUpdate()
    {
        var waitASec = new WaitForSeconds(1.0f);
        while(true)
        {
            yield return waitASec;

            DailyMissionModel.Pump();

            if(unit.IsAttached)
                RefreshView();
        }
    }

    void RefreshNotificator()
    {
        var notiInfo = DailyMissionModel.DMPlayerModel.NotificatorInfo;
        if(notiInfo == null)
            return;

        View.DailyMissionNotificator.Reset();

        if(notiInfo.SeenReasons != null)
        {
            for(int q = 0; q < notiInfo.SeenReasons.Count; ++q)
                View.DailyMissionNotificator.EnableNotification(notiInfo.SeenReasons[q]);
            
            View.DailyMissionNotificator.DisableNotification();
        }
        if(notiInfo.UnseenReasons != null)
        {
            for(int q = 0; q < notiInfo.UnseenReasons.Count; ++q)
                View.DailyMissionNotificator.EnableNotification(notiInfo.UnseenReasons[q]);
        }
    }

    void OnBtnClaimClicked(int nGoalType)
    {
        var goalType = (DailyMissionConfig.GoalType)nGoalType;
        DailyMissionModel.Claim(goalType);

        RefreshView();
    }

    void OnBtnResetClicked()
    {
        DailyMissionModel.Reset();
    }

    void OnSkillLearned(object data)
    {
        if (data == null) return;

        Tuple<string, string, string, bool> skill_id_n_ability_id_param = (Tuple< string, string, string, bool>)data;
        bool isPartOfInitProcess = (bool)skill_id_n_ability_id_param.Item4;

        if(!isPartOfInitProcess)
            DailyMissionModel.IncreaseGoalCount(DailyMissionConfig.GoalType.LearnSkill);

        RefreshView();
    }

    void OnCraftSuccessed(object data) 
    {
        eRscStageType stageType = (eRscStageType)data;
        DailyMissionConfig.GoalType goalType = stageType==eRscStageType.COMPONENT ? DailyMissionConfig.GoalType.CraftComp : DailyMissionConfig.GoalType.CraftItem;
        DailyMissionModel.IncreaseGoalCount(goalType);

        RefreshView();
    }

    void OnMiningStatUpgraded(object data)
    {
        DailyMissionModel.IncreaseGoalCount(DailyMissionConfig.GoalType.UpgradeStat);

        RefreshView();
    }

    void OnAdsWatched(object data)
    {
        DailyMissionModel.IncreaseGoalCount(DailyMissionConfig.GoalType.WatchAds);

        RefreshView();
    }

    void OnGoalAchieved(object data)
    {
        DailyMissionConfig.GoalType goalType = (DailyMissionConfig.GoalType)data;
        
        if(DailyMissionModel.DMPlayerModel.AddNotificationReason(goalType.ToString()))
            RefreshNotificator();
    }

    void OnDailyMissionReset(object data)
    {
        DailyMissionModel.DMPlayerModel.Reset();
        
        RefreshNotificator();
        RefreshView();
    }
}
