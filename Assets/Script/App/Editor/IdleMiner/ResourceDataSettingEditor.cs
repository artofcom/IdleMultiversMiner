using App.GamePlay.IdleMiner;
using Core.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ResourceDataSetting))]
public class ResourceDataSettingEditor : Editor
{
    //private SerializedProperty resourceSetsProperty;
    //string baseValue, rarityValue;

    private void OnEnable()
    {
        //resourceSetsProperty = serializedObject.FindProperty("resourceSets");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Label("========== Editor Area ============");
        GUILayout.Label("");
        if (GUILayout.Button("<< Set resource Id from icon file name. >>", GUILayout.Height(50.0f)))
        {
            ResetResourceIdFromIcon();
        }
        GUILayout.Label("");

        SetGoldPrice();
    }

    void SetGoldPrice()
    {
        ResourceDataSetting rscData = (ResourceDataSetting)target;

        GUILayout.Label("Ret = Base + Base * (index * Rarity)");
        if (GUILayout.Button("<< Set Item Gold Price. >>", GUILayout.Height(50.0f)))
        {
            CalculateGoldFromRarity();
        }
    }

    void ResetResourceIdFromIcon()
    {
        ResourceDataSetting rscData = (ResourceDataSetting)target;

        List<ResourceSetInfo> rscSets = rscData.ResourceSets;
        HashSet<string> idList = new HashSet<string>();
        for(int q = 0; q < rscSets.Count; ++q)
        {
            ResourceSetInfo rscSetInfo = rscSets[q];
            rscSetInfo.ResourceInfo.SetId( rscSetInfo.Icon.name.ToLower() );

            if(idList.Contains( rscSetInfo.ResourceInfo.Id ) ) 
            {
                Debug.LogError($"Error : {rscSetInfo.ResourceInfo.Id} has already been defined !!!");
                return;
            }
            idList.Add( rscSetInfo.ResourceInfo.Id );
        }
    }

    void CalculateGoldFromRarity()
    {
        ResourceDataSetting rscData = (ResourceDataSetting)target;

        List<ResourceSetInfo> rscSets = rscData.ResourceSets;
        for(int q = 0; q < rscSets.Count; ++q)
        {
            ResourceSetInfo rscSetInfo = rscSets[q];
            float price = rscData.BaseValue + ((float)rscData.BaseValue) * ((float)q) * rscData.Rarity;
            
            rscSetInfo.ResourceInfo.Price = ((long)price).ToString();
        }
    }




    void WriteResourceData(ResourceDataSetting targetSetting, string fileName)
    {
        const string DATA_SUB_PATH = "/EditorData/";

        App.GamePlay.IdleMiner.ResourceData rscData = new App.GamePlay.IdleMiner.ResourceData();
        for(int q = 0; q < targetSetting.ResourceSets.Count; ++q)
            rscData.AddResourceInfo( targetSetting.ResourceSets[q].ResourceInfo );
            
        string content = JsonUtility.ToJson(rscData, prettyPrint:true);
        TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + fileName, content);
    }
}
