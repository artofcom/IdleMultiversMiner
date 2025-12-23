using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GameDataExporter))]
public class GameDataExporterEditor  : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();     // 기본 인스펙터 표시


        GUILayout.Label("");
        GUILayout.Label("========== Editor Area ============");
        GUILayout.Label("");

        

        if (GUILayout.Button("<< Export All Data ! >>", GUILayout.Height(50.0f)))
        {
            //WriteResourceData(comp.MaterialData, "Resource_Mat.json");
            WriteAllGameData();
        }


        if (GUILayout.Button("<< Run Simulator >>", GUILayout.Height(50.0f)))
        {
            GameEconomyGrokSimulator sim = new GameEconomyGrokSimulator();
            sim.Run();
        }
    }

    void WriteAllGameData()
    {
        GameDataExporter comp = (GameDataExporter)target;

        if(comp.ResourceDataBuildComp != null)
        {
            if(comp.ResourceDataBuildComp.MaterialData != null)
            {
                comp.ResourceDataBuildComp.ExportData(comp.ResourceDataBuildComp.MaterialData, "Resource_Mat.json");
                Debug.Log("Exporting Material Rsc Data....Resource_Mat.json");
            }
            if(comp.ResourceDataBuildComp.ComponentData != null) 
            {
                comp.ResourceDataBuildComp.ExportData(comp.ResourceDataBuildComp.ComponentData, "Resource_Comp.json");
                Debug.Log("Exporting Comp Rsc Data....Resource_Comp.json");
            }
            if(comp.ResourceDataBuildComp.ItemData != null) 
            {
                comp.ResourceDataBuildComp.ExportData(comp.ResourceDataBuildComp.ItemData, "Resource_Item.json");
                Debug.Log("Exporting Item Rsc Data....Resource_Item.json");
            }
        }

        if(comp.ZoneDataComp != null) 
        {
            comp.ZoneDataComp.ExportPlanetData();
            Debug.Log("Exporting zone Data....PlanetData.json");
        }

        if(comp.CraftCompDataBuildComp!=null && comp.CraftCompDataBuildComp.CraftData!=null) 
        {
            comp.CraftCompDataBuildComp.ExportCraftData(comp.CraftCompDataBuildComp.CraftData, "Craft_Comp.json");
            Debug.Log("Exporting craft comp Data....Craft_Comp.json");
        }
        if(comp.CraftItemDataBuildComp != null && comp.CraftItemDataBuildComp.CraftData!=null)
        {
            comp.CraftItemDataBuildComp.ExportCraftData(comp.CraftItemDataBuildComp.CraftData, "Craft_Item.json");
            Debug.Log("Exporting item comp Data....Craft_Item.json");
        }

        if(comp.SkillItemBundleComp != null)
        {
            comp.SkillItemBundleComp.ExportSkillData();
            Debug.Log("Exporting Skill Data....Skill.json");
        }
    }
}
