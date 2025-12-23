using UnityEngine;
using App.GamePlay.IdleMiner;
using System;
using System.Collections.Generic;
using App.GamePlay.IdleMiner.Common;

[Serializable]
public class SkillInfoWithIcon
{
    [SerializeField] Sprite icon;
    [SerializeField] SkillInfo skillInfo;

    public Sprite Icon => icon;
    public SkillInfo SkillInfo => skillInfo;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SkillTreeDataSetting", order = 1)]
public class SkillTreeDataSetting : ScriptableObject
{
    [Header("=== Skill Tree Category Data")]
    
    [SerializeField] string categoryId;
    [SerializeField] List<SkillInfoWithIcon> skillInfoList;


    public string CategoryId => categoryId;
    public List<SkillInfoWithIcon> SkillInfoWithList => skillInfoList;


    public Sprite GetSprite(string key)
    {
        key = key.ToLower();
        for(int q = 0; q < skillInfoList.Count; ++q)
        {
            if(skillInfoList[q].SkillInfo.Id.ToString() == key)
                return skillInfoList[q].Icon;
        }
        return null;
    }
}
