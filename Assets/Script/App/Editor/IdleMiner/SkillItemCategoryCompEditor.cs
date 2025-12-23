using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.SkillTree;
using Core.Utils;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SkillItemCategoryComp))]
public class SkillItemCategoryCompEditor : Editor
{
    private GUIStyle styleRed;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();     // 기본 인스펙터 표시
     
        if(styleRed == null)
        {
            styleRed = new GUIStyle(GUI.skin.button);
            styleRed.fixedHeight = 50.0f;
            styleRed.normal.textColor = Color.red;
        }

        
        SkillItemCategoryComp comp = (SkillItemCategoryComp)target;

        GUILayout.Label("");
        GUILayout.Label("");
        GUILayout.Label("========== Editor Area ============");
        GUILayout.Label("");
        if (GUILayout.Button("Open Resource Craft Link Editor", GUILayout.Height(50.0f)))
             ResourceSkillTreeEditor.ShowWindow(comp);

       /* GUILayout.Label("");
        GUILayout.Label("Ret = Base + Base * index * Rarity");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<< Generate Duration >>", GUILayout.Height(50.0f)))
             GenerateDurationData();
        if (GUILayout.Button("<< Generate Cost >>", GUILayout.Height(50.0f)))
             GenerateCostData();
        GUILayout.EndHorizontal();


        GUILayout.Label("");
        GUILayout.Label("[Note] : THIS ACTION WILL CLEAR THE CURRENT LIST !!!");
        if (GUILayout.Button("[ Generate Basic Recipe List ]", styleRed))
        {
            GenerateRecipeList();
        }

        GUILayout.Label("");
        string fileName = comp.Type == CraftDataBuildComp.TargetResourceType.COMP ? "Craft_Comp.json" : "Craft_Item.json";
        GUILayout.Label($"Output : Assets/EditorData/{fileName}");
        GUILayout.Label("");
        if (GUILayout.Button("<< Export Craft Data >>", GUILayout.Height(50.0f)))
            WriteResourceData(comp.CraftData, fileName);
        

       // GUILayout.Label("");
      //  GUILayout.Label("Output : Assets/EditorData/Resource_Comp.json");
      //  GUILayout.Label("");
     //   if (GUILayout.Button("<< Write Craft To Item >>", GUILayout.Height(50.0f)))
    //        WriteResourceData(comp.ToItem, "Craft_Item.json");
        

        GUILayout.Label("");*/
        
    }

    void GenerateDurationData()
    {
        CraftDataBuildComp comp = (CraftDataBuildComp)target;
        for(int q = 0; q < comp.CraftData.Recipes.Count; ++q)
        {
            comp.CraftData.Recipes[q].Duration = comp.DurationBaseValue + (int)( ((float)comp.DurationBaseValue) * ((float)q) * comp.DurationRarity);
        }

        Debug.Log($"[GenerateDurationData] : {comp.CraftData.Recipes.Count} has been generated.");
    }
    void GenerateCostData()
    {
        CraftDataBuildComp comp = (CraftDataBuildComp)target;
        for(int q = 0; q < comp.CraftData.Recipes.Count; ++q)
        {
            long cost = comp.OpenCostBaseValue + (long)( ((float)comp.OpenCostBaseValue) * ((float)q) * comp.OpenCostRarity);
            comp.CraftData.Recipes[q].Cost = cost.ToString();
        }

        Debug.Log($"[GenerateCostData] : {comp.CraftData.Recipes.Count} has been generated.");
    }

    void GenerateRecipeList()
    {
        CraftDataBuildComp comp = (CraftDataBuildComp)target;

        //comp.MaterialSet.ResourceSets.Count;
        //comp.ComponentSet.ResourceSets.Count = 0;
        //comp.ItemSet.ResourceSets.Count;

        ResourceDataSetting srcData = comp.Type == CraftDataBuildComp.TargetResourceType.COMP ? comp.MaterialSet : comp.ComponentSet;
        ResourceDataSetting dstData = comp.Type == CraftDataBuildComp.TargetResourceType.COMP ? comp.ComponentSet : comp.ItemSet;

        comp.CraftData.Recipes.Clear();
        
        string id = comp.Type == CraftDataBuildComp.TargetResourceType.COMP ? "Comp_" : "Item_";
        for(int q = 0; q < dstData.ResourceSets.Count; ++q)
            comp.CraftData.Recipes.Add(new RecipeInfo(id+q.ToString(), dstData.ResourceSets[q].ResourceInfo.Id, null));


        Debug.Log($"[GenerateRecipList] : {comp.CraftData.Recipes.Count} has been generated.");

    }


    void WriteResourceData(CraftData craftData, string fileName)
    {
        const string DATA_SUB_PATH = "/EditorData/";

        string content = JsonUtility.ToJson(craftData, prettyPrint:true);
        TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + fileName, content);
    }
}
