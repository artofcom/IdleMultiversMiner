using App.GamePlay.IdleMiner;
using Core.Utils;
using System.Data;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ResourceDataBuildComp))]
public class ResourceDataBuildCompEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();     // 기본 인스펙터 표시
     
        
        ResourceDataBuildComp comp = (ResourceDataBuildComp)target;

        GUILayout.Label("");
        GUILayout.Label("");
        GUILayout.Label("========== Editor Area ============");
        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Resource_Mat.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write Resource Mat Data >>", GUILayout.Height(50.0f)))
            comp.ExportData(comp.MaterialData, "Resource_Mat.json");
        

        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Resource_Comp.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write Resource Comp Data >>", GUILayout.Height(50.0f)))
            comp.ExportData(comp.ComponentData, "Resource_Comp.json");
        

        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/Resource_Item.json");
        GUILayout.Label("");
        if (GUILayout.Button("<< Write Resource Item Data >>", GUILayout.Height(50.0f)))
            comp.ExportData(comp.ItemData, "Resource_Item.json");
        
    }

}
