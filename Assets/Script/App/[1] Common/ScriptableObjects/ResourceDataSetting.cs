using UnityEngine;
using App.GamePlay.IdleMiner;
using System;
using System.Collections.Generic;

[Serializable]
public class ResourceSetInfo
{
    [SerializeField] Sprite icon;
    [SerializeField] ResourceInfo resourceInfo;

    public Sprite Icon => icon;
    public ResourceInfo ResourceInfo => resourceInfo;   
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ResourceDataSetting", order = 1)]
public class ResourceDataSetting : ScriptableObject
{
    [Header("=== Resource Area - Sell Price")]
    
    [SerializeField] int baseValue = 10;
    [SerializeField] float rarity = 1.0f;

    [SerializeField] List<ResourceSetInfo> resourceSets;

    public List<ResourceSetInfo> ResourceSets => resourceSets;
    public int BaseValue { get => baseValue; set => baseValue = value; }
    public float Rarity {  get => rarity; set => rarity = value; }

    public Sprite GetSprite(string key)
    {
        key = key.ToLower();
        for(int q = 0; q < resourceSets.Count; ++q)
        {
            if(resourceSets[q].ResourceInfo.Id.ToLower() == key)
                return resourceSets[q].Icon;
        }
        return null;
    }
}
