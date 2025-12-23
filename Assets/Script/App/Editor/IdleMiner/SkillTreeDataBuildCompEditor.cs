using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common;
using Core.Utils;
using System.Data;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SkillTreeDataBuildComp))]
public class SkillTreeDataBuildCompEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();     // 기본 인스펙터 표시
     
        
        SkillTreeDataBuildComp comp = (SkillTreeDataBuildComp)target;

        GUILayout.Label("");
        GUILayout.Label("");
        GUILayout.Label("========== Editor Area ============");
        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Skill_Mining.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write SkillTree MiningData >>", GUILayout.Height(50.0f)))
            WriteResourceData(comp.MiningData, "Skill_Mining.json");
        

        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Skill_CompCraft.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write SkillTree CompCraftData >>", GUILayout.Height(50.0f)))
            WriteResourceData(comp.CompCraftData, "Skill_CompCraft.json");


        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Skill_ItemCraft.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write SkillTree ItemCraftData >>", GUILayout.Height(50.0f)))
            WriteResourceData(comp.ItemCraftData, "Skill_ItemCraft.json");

        
        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Skill_MobHunt.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write SkillTree MobHuntData >>", GUILayout.Height(50.0f)))
            WriteResourceData(comp.MobHuntData, "Skill_MobHunt.json");


        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Skill_Market.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write SkillTree MarketData >>", GUILayout.Height(50.0f)))
            WriteResourceData(comp.MarketData, "Skill_Market.json");
        

        GUILayout.Label("");
        
    }



    void WriteResourceData(SkillTreeDataSetting targetSetting, string fileName)
    {
        const string DATA_SUB_PATH = "/EditorData/";

/*        SkillCategory skillCategory = new SkillCategory();
        skillCategory.SetCategoryId(targetSetting.CategoryId);

        for(int q = 0; q < targetSetting.SkillInfoWithList.Count; ++q)
        {
            skillCategory.AddSkillInfo( targetSetting.SkillInfoWithList[q].SkillInfo );
        }

        string content = JsonUtility.ToJson(skillCategory, prettyPrint:true);
        TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + fileName, content);*/
    }
}
