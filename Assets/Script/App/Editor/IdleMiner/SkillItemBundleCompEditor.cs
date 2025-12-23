using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.SkillTree;
using Core.Utils;
using System.Collections.Generic;
using System.Data;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[CustomEditor(typeof(SkillItemBundleComp))]
public class SkillItemBundleCompEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();     // 기본 인스펙터 표시
     
        
        SkillItemBundleComp comp = (SkillItemBundleComp)target;
        
        GUILayout.Label("");
        GUILayout.Label("========== Editor Area ============");
        GUILayout.Label("> Pre : Childrent data on the comp should be set before click.");
        if (GUILayout.Button("<< Auto Fill Data (children names) >>", GUILayout.Height(50.0f)))
            AutoFillData(comp);
        
        GUILayout.Label("");
        
        if (GUILayout.Button("<< Export SkillTree Data >>", GUILayout.Height(50.0f)))
            comp.ExportSkillData();
    }

    void AutoFillData(SkillItemBundleComp comp)
    {
        Assert.IsNotNull(comp);
        if(comp.CategoryComp==null || comp.CategoryComp.Count==0)
            return;

        for(int q = 0; q < comp.CategoryComp.Count; ++q)
        {
            if(comp.CategoryComp[q].RootNode == null)
            {
                Debug.Log("Error : SkillTree Root Item has not been set.");
                continue;
            }
        
            // DPS. - Going to use just subcall func stack.
            AutoFillItemData(comp.CategoryComp[q].RootNode);
        }
    }

    void AutoFillItemData(SkillItemComp comp)
    {
        Assert.IsNotNull(comp.SkillData);
        comp.SkillData.SetId(comp.gameObject.name);

        comp.SkillData.Children.Clear();
        for(int q = 0; q < comp.Children.Count; ++q)
        {
            comp.SkillData.Children.Add(comp.Children[q].name);

            AutoFillItemData(comp.Children[q]);
        }
    }

    void WriteData(SkillItemBundleComp comp)
    {
        /*
        Assert.IsNotNull(comp);
        if(comp.CategoryRoots==null || comp.CategoryRoots.Count==0)
        {
            Debug.Log("Error : SkillTree Root Item has not been set.");
            return;
        }
        
        const string DATA_SUB_PATH = "/EditorData/";

        for(int q = 0 ; q < comp.CategoryRoots.Count; ++q)
        {
            SkillItemCompInfo info = comp.CategoryRoots[q];
            if(info.Root == null || info.Root.transform.parent == null)
                continue;

            SkillTreeCategoryInfo categoryInfo = new SkillTreeCategoryInfo();
            categoryInfo.SetId( info.CategoryId );
            categoryInfo.SetRootId( info.Root.SkillData.Id );
            List<SkillInfo> listSkillInfoPool = categoryInfo.GetSkillInfoPool();

            Transform trParent = info.Root.transform.parent;
            for(int k = 0; k < trParent.childCount; ++k) 
            {
                Transform child = trParent.GetChild(k);
                SkillItemComp itemComp = child.GetComponent<SkillItemComp>();
                if(itemComp == null || !child.gameObject.activeSelf || itemComp.SkillData==null || string.IsNullOrEmpty(itemComp.SkillData.Id))
                    continue;

                itemComp.SetSkillId( itemComp.SkillData.Id );
                listSkillInfoPool.Add( itemComp.SkillData );
            }

            string fileName = info.FileName.Contains(".json") ? info.FileName : info.FileName + ".json";
            string content = JsonUtility.ToJson(categoryInfo, prettyPrint:true);
            TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + fileName, content);
        }*/
    }
}
