using UnityEngine;
using App.GamePlay.IdleMiner;
using System;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CraftDataSetting", order = 1)]
public class CraftDataSetting : ScriptableObject
{
    [Header("=== Craft Area ")]
    
    [SerializeField] CraftData craftData;


    public CraftData CraftData => craftData;

    /*
    public Sprite GetSprite(string key)
    {
        key = key.ToLower();
        for(int q = 0; q < resourceSets.Count; ++q)
        {
            if(resourceSets[q].ResourceInfo.Id.ToLower() == key)
                return resourceSets[q].Icon;
        }
        return null;
    }*/
}
