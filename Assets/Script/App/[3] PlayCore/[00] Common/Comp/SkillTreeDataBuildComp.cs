using UnityEngine;

public class SkillTreeDataBuildComp : MonoBehaviour
{
    [SerializeField] SkillTreeDataSetting miningData;
    [SerializeField] SkillTreeDataSetting compCraftData;
    [SerializeField] SkillTreeDataSetting itemCraftData;
    [SerializeField] SkillTreeDataSetting mobHuntData;
    [SerializeField] SkillTreeDataSetting marketData;
    

    public SkillTreeDataSetting MiningData => miningData;
    public SkillTreeDataSetting CompCraftData => compCraftData;
    public SkillTreeDataSetting ItemCraftData => itemCraftData;
    public SkillTreeDataSetting MobHuntData => mobHuntData;
    public SkillTreeDataSetting MarketData => marketData;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    public Sprite GetSprite(string groupKey, string spriteKey)
    {
        string gKeyL = groupKey.ToLower();

        SkillTreeDataSetting targetSetting = marketData;
        if(gKeyL == "skill-mining")             targetSetting = miningData;
        else if(gKeyL == "skill-compcraft")     targetSetting = compCraftData;
        else if(gKeyL == "skill-itemcraft")     targetSetting = itemCraftData;
        else if(gKeyL == "skill-mobhunt")       targetSetting = MobHuntData;

        return targetSetting.GetSprite(spriteKey);
    }
}
