using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DailyMissonConfig", menuName = "ScriptableObjects/DailyMissionConfig")]
public class DailyMissionConfig : ScriptableObject
{
    public enum GoalType
    {
        UpgradeStat, CraftComp, CraftItem, LearnSkill, WatchAds, MAX
    };

    [Serializable]
    public class Mission
    {
        [SerializeField] Sprite icon;
        [SerializeField] string name;
        [SerializeField] string desc;
        [SerializeField] GoalType goalType;
        [SerializeField] long goalCount;
        [SerializeField] string rewardType;
        [SerializeField] long rewardAmount;

        public Sprite Icon => icon;
        public string Name => name;
        public string Desc => desc;
        public GoalType GoalType => goalType;
        public long GoalCount => goalCount;
        public string RewardType => rewardType;
        public long RewardAmount => rewardAmount;
    }

    [SerializeField] List<Mission> missions;
    
    void OnEnable() {}

    public Mission GetData(GoalType goalType)
    {
        for(int q = 0; q < missions.Count; ++q)
        {
            if(missions[q].GoalType == goalType)    
                return missions[q];
        }
        return null;
    }

}
