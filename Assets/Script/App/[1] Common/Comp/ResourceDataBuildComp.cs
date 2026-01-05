using App.GamePlay.IdleMiner;
using Core.Utils;
using UnityEngine;

public class ResourceDataBuildComp : MonoBehaviour
{
    [SerializeField] ResourceDataSetting materialData;
    [SerializeField] ResourceDataSetting componentData;
    [SerializeField] ResourceDataSetting itemData;


    public ResourceDataSetting MaterialData => materialData;
    public ResourceDataSetting ComponentData => componentData;
    public ResourceDataSetting ItemData => itemData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    public Sprite GetSprite(string groupKey, string spriteKey)
    {
        string gKeyL = groupKey.ToLower();

        ResourceDataSetting targetSetting = materialData;
        if(gKeyL == "rsc-component")
            targetSetting = componentData;
        else if(gKeyL == "rsc-item")
            targetSetting = itemData;


        return targetSetting.GetSprite(spriteKey);
    }

#if UNITY_EDITOR

    public void ExportData(ResourceDataSetting targetRscSetting, string targetFileName)
    {
        const string DATA_SUB_PATH = "/EditorData/";

        ResourceData rscData = new ResourceData();
        for(int q = 0; q < targetRscSetting.ResourceSets.Count; ++q)
            rscData.AddResourceInfo( targetRscSetting.ResourceSets[q].ResourceInfo );
            
        string content = JsonUtility.ToJson(rscData, prettyPrint:true);
        TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + targetFileName, content);
    }

#endif

}
