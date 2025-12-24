using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;




[CreateAssetMenu(fileName = "DailyMissonConfig", menuName = "ScriptableObjects/DailyMissionConfig")]
public class DailyMissionConfig : ScriptableObject
{
    public enum GoalType
    {
        UpgradeStat, CraftComp, CraftItem, LearnSkill, WatchAds
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
    }

    [SerializeField] List<Mission> missions;
    
    void OnEnable() {}

}
