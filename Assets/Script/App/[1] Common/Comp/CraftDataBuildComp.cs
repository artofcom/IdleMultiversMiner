using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.GamePlay;
using Core.Utils;
using System.Collections.Generic;
using UnityEngine;

public class CraftDataBuildComp : MonoBehaviour
{
#if UNITY_EDITOR
    public enum TargetResourceType { COMP, ITEM };

    [Header("[(1) Resource Section ]")]
    [SerializeField] ResourceDataSetting materialSet;
    [SerializeField] ResourceDataSetting componentSet;
    [SerializeField] ResourceDataSetting itemSet;

    [Header("[(2) Recipe Pre-set Section ]")]
    [SerializeField] TargetResourceType type;
    [SerializeField] public int DurationBaseValue = 10;
    [SerializeField] public float DurationRarity = 1.0f;
    [SerializeField] public int OpenCostBaseValue = 10;
    [SerializeField] public float OpenCostRarity = 1.0f;
    
    [Header("[(3) ZoneData - Optional ]")]
    [SerializeField] PlanetControllerComp zoneDataComp;

    [Header("[(4) Recipe List ]")]
    [SerializeField] CraftData craftData;
    
    
    

    Dictionary<string, int> dictTargetSelections = new Dictionary<string, int>();

    public ResourceDataSetting MaterialSet => materialSet;
    public ResourceDataSetting ComponentSet => componentSet;
    public ResourceDataSetting ItemSet => itemSet;
    
    public PlanetControllerComp ZoneDataComp => zoneDataComp;

    public TargetResourceType Type => type;
    public CraftData CraftData => craftData;
    public Dictionary<string, int> DictTargetSelections => dictTargetSelections;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public RecipeInfo GetRecipeInfo(string id)
    {
        id = id.ToLower();
        for(int q = 0; q < craftData.Recipes.Count; ++q)
        {
            if(craftData.Recipes[q].Id.ToLower() == id)
                return craftData.Recipes[q];
        }
        return null;
    }


    public void SetRecipiOutcomeId(string recipi_Id, string resourceId)
    {
        RecipeInfo info = GetRecipeInfo(recipi_Id);
        if (info == null)   return;
        
        info.OutcomeId = resourceId;

    }
    public void SetRecipiSourceId(string recipi_Id, string resourceId)
    {
        RecipeInfo info = GetRecipeInfo(recipi_Id);
        if (info == null)   return;
        
        if(info.Sources == null)
            info.Sources = new List<ResourceRequirement>();

        for(int q = 0; q < info.Sources.Count; ++q)
        {
            // Already there?
            if(info.Sources[q].ResourceId.ToLower() == resourceId.ToLower())
                return;
        }
        info.Sources.Add(new ResourceRequirement(resourceId, count:100));
    }
    public void RemoveRecipiSourceId(string recipi_Id, string resourceId)
    {
        RecipeInfo info = GetRecipeInfo(recipi_Id);
        if (info == null)   return;
        
        if(info.Sources == null)
            return;

        for(int q = 0; q < info.Sources.Count; ++q)
        {
            // Already there?
            if(info.Sources[q].ResourceId.ToLower() == resourceId.ToLower())
            {
                info.Sources.RemoveAt(q);
                return;
            }
        }
    }


    public void ExportCraftData(CraftData craftData, string fileName)
    {
        const string DATA_SUB_PATH = "/EditorData/";

        string content = JsonUtility.ToJson(craftData, prettyPrint:true);
        TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + fileName, content);
    }
#endif
}
