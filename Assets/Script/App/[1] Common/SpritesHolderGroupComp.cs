using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

/*
[Serializable]
public sealed class SpriteHolderInfo
{
    [SerializeField] string key;
    [SerializeField] SpritesHolderComp spriteHolder;

    public string Key => key;
    public SpritesHolderComp SpriteHolder => spriteHolder;
}*/

public sealed class SpritesHolderGroupComp : MonoBehaviour
{
    [SerializeField] ResourceDataBuildComp resourceEditComp;
    [SerializeField] SkillTreeDataBuildComp skillTreeDataBuildComp;

    Dictionary<string, SpritesHolderComp> holderDict;

    // Start is called before the first frame update
    void Start()
    {
        holderDict = new Dictionary<string, SpritesHolderComp>();
        for(int q = 0; q < transform.childCount; ++q)
        {
            var child = transform.GetChild(q);
            var holderComp = child.GetComponent<SpritesHolderComp>();
            if (holderComp == null)
                continue;

            string key = child.gameObject.name;
            key = key.ToLower();
            Assert.IsTrue(!holderDict.ContainsKey(key));


            holderDict.Add(key, holderComp);
            Debug.Log($"[SHGroup] : [{q}]-[{key}] Group has been added.");
        }
    }


    SpritesHolderComp GetSpriteHolder(string groupKey)
    {
        groupKey = groupKey.ToLower();

        if (holderDict.ContainsKey(groupKey))
            return holderDict[groupKey];

        Debug.LogWarning("Couldn't find the sprite-holder key : " + groupKey);
        return null;
    }






    public Sprite GetSprite(string groupKey, string spriteKey)
    {
        string gkeyL = groupKey.ToLower();

        if(gkeyL.Contains("rsc-"))
            return resourceEditComp.GetSprite(groupKey, spriteKey);
        else if(gkeyL.Contains("skill-"))
            return skillTreeDataBuildComp.GetSprite(groupKey, spriteKey);

        var group = GetSpriteHolder(groupKey);
        if (group != null)
            return group.GetSprite(spriteKey);

        Debug.LogWarning($"Couldn't find the sprite group : {groupKey}");
        return null;
    }

}
