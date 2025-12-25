using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.GamePlay;
using App.GamePlay.IdleMiner.SkillTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class ResourceSkillTreeEditor : EditorWindow
{
    const int WIDTH = 1500;
    const int LANE_W = 500;
    private GUIStyle styleBlk, styleRed;

    SkillItemCategoryComp skillCategoryComp;

    // int idxSelected = 0;
    List<List<Rect>> spriteRects = new List<List<Rect>>();
    Dictionary<string, Rect> skillRect = new Dictionary<string, Rect>();
    Dictionary<string, int> targetSkills = new Dictionary<string, int>();
    Vector2 vScrollPos = Vector2.zero;

    // [MenuItem("PlasticGames/Editor/CraftEditor2")]
    public static void ShowWindow(SkillItemCategoryComp craftData)
    {
        ResourceSkillTreeEditor window = GetWindow<ResourceSkillTreeEditor>("Resource SkillTree Editor.");

        window.position = new Rect(100, 100, WIDTH, 500);
        window.Init(craftData);
    }

    public void Init(SkillItemCategoryComp comp)
    {
        styleRed = new GUIStyle(GUI.skin.button);
        styleRed.normal.textColor = Color.red;
        styleRed.fixedWidth = 80.0f;

        styleBlk = new GUIStyle(GUI.skin.button);
        styleBlk.normal.textColor = GUI.skin.label.normal.textColor;
        styleBlk.fixedWidth = 80.0f;

        skillCategoryComp = comp;
    }

    void OnEnable()
    {
    }

    public void OnGUI()
    {
        vScrollPos = GUILayout.BeginScrollView(vScrollPos);
        {
            DrawResourcesGroup();

            //DrawLinesBetweenSprites();
            DrawLine();

            // Dummy and Tests.
            GUILayout.Space(10);
           // GUILayout.Label("=== Numbering for less confusing");
            GUILayout.Label("=== Missing link check");
        }
        GUILayout.EndScrollView();
    }
     
    void DrawResourcesGroup()
    {
        Dictionary<string, int> referenceBuffer = new Dictionary<string, int>();
        
       /* void AddToRefBuffer(string key)
        {
            if(string.IsNullOrEmpty(key)) return;
            if(referenceBuffer.ContainsKey(key))    referenceBuffer[key]++;
            else referenceBuffer[key] = 1;
        }*/

        GUILayout.Label("=== Select Resource to copy its name to the System ClipBoard.");
        GUILayout.Space(10);

        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
     
        
        // Build Id options first. 
        string[] recipiOptions = GetSkillTreeNodeNames(skillCategoryComp.RootNode).ToArray();
           
        // Resources.
        string[] titles = new string[]{"Material", "Component", "Item"};
        ResourceDataSetting[] rscSets = new ResourceDataSetting[]{ skillCategoryComp.MaterialSet, skillCategoryComp.ComponentSet, skillCategoryComp.ItemSet };        
        
        
        spriteRects.Clear();
        spriteRects.Add(new List<Rect>());  spriteRects.Add(new List<Rect>());  spriteRects.Add(new List<Rect>());
        for(int k = 0; k < rscSets.Length; ++k)
        {
            spriteRects[k].Clear();
            GUILayout.BeginVertical();
            //GUILayout.Label(titles[k]);
            for(int q = 0; q < rscSets[k].ResourceSets.Count; ++q)
            {
                ResourceSetInfo setInfo = rscSets[k].ResourceSets[q];
                string key = setInfo.ResourceInfo.Id.ToLower();
                GUIStyle style = referenceBuffer.ContainsKey(key) && referenceBuffer[key]>0  ? styleRed : styleBlk; 
                int refCount = referenceBuffer.ContainsKey(key) ? referenceBuffer[key] : 0; 

                string presentZones = FindObtainableMiningTargetZones(skillCategoryComp.PlanetController, key);
                string strTitle = $"# {titles[k]} {q} : {setInfo.ResourceInfo.Id}";
                if(string.IsNullOrEmpty(presentZones)) strTitle += " => NONE";
                else                                   strTitle += $" => {presentZones}";
                
                GUILayout.Label("   ");
                GUILayout.Label(strTitle);

                Rect rectIcon = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                spriteRects[k].Add(rectIcon);
                DrawIcon(setInfo.Icon, rectIcon);

                GUILayout.BeginHorizontal();

                string idxKey = $"{k}_{q}";
                if(!targetSkills.ContainsKey(idxKey) || targetSkills[idxKey]>=recipiOptions.Length)
                    targetSkills[idxKey] = 0;

                string selectedSkillName = recipiOptions[ targetSkills[idxKey] ];
                
                //if(!craftDataBuildComp.DictTargetSelections.ContainsKey(key))
                //    craftDataBuildComp.DictTargetSelections[key] = 0;
                if(GUILayout.Button("Link", styleBlk))
                    skillCategoryComp.SetToUnlockCost(setInfo.ResourceInfo.Id, GetSkillTreeNode(skillCategoryComp.RootNode, selectedSkillName) ); 
                if(GUILayout.Button("Unlink", styleRed))
                    skillCategoryComp.ClearUnlockCost(setInfo.ResourceInfo.Id, GetSkillTreeNode(skillCategoryComp.RootNode, selectedSkillName) ); 


                targetSkills[idxKey] = EditorGUILayout.Popup(targetSkills[idxKey], recipiOptions);
                //craftDataBuildComp.DictTargetSelections[key] = EditorGUILayout.Popup(craftDataBuildComp.DictTargetSelections[key], recipiOptions);
                
                //GUILayout.BeginVertical();
                GUILayout.Label("   ");
                //GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        


        // Dividers.
        //for(int k = 0; k < rscSets.Length; ++k)
        {
            //spriteRects[k].Clear();
            GUILayout.BeginVertical();
            //GUILayout.Label(titles[k]);
            for(int q = 0; q < rscSets[0].ResourceSets.Count; ++q)
            {
                GUILayout.Label("        ");
            }
            GUILayout.EndVertical();
        }
        

        //skillCategoryComp.RootNode
        // Drawing Skills Here. - BFS
        skillRect.Clear();
        Queue<SkillItemComp> qNodes = new Queue<SkillItemComp>();
        qNodes.Enqueue(skillCategoryComp.RootNode);
        GUILayout.BeginVertical();
        while(qNodes.Count > 0) 
        {
            GUILayout.BeginHorizontal();
            int size = qNodes.Count;
            for(int q = 0; q < size; ++q)
            {
                SkillItemComp item = qNodes.Dequeue();
                // draw item here.
                GUILayout.BeginVertical();
                GUILayout.Label($"# {item.SkillData.Name}");
                Rect rectIcon = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                //spriteRects[k].Add(rectIcon);
                DrawIcon(item.SkillSprite, rectIcon);
                // GUILayout.Label($"# {item.SkillData.Description}");
                skillRect.Add(item.SkillData.Id, rectIcon);
                
                //GUILayout.BeginHorizontal();
                
                for(int t = 0; t < item.SkillData.UnlockCost.Count; ++t)
                    GUILayout.Label($"{item.SkillData.UnlockCost[t].ResourceId} ({item.SkillData.UnlockCost[t].GetCount()}) ");
                
                //GUILayout.EndHorizontal();

                GUILayout.Label("        ");
                GUILayout.EndVertical();

                for(int k = 0; k < item.Children.Count; ++k)
                    qNodes.Enqueue(item.Children[k]);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

       
        /*
        for(int k = 0; k < rscSets.Length; ++k)
        {
            GUILayout.BeginVertical();
            for(int q = 0; q < rscSets[k].ResourceSets.Count; ++q)
            {
                ResourceSetInfo setInfo = rscSets[k].ResourceSets[q];
                string key = setInfo.ResourceInfo.Id.ToLower();
                GUIStyle style = referenceBuffer.ContainsKey(key) && referenceBuffer[key]>0  ? styleRed : styleBlk; 
                int refCount = referenceBuffer.ContainsKey(key) ? referenceBuffer[key] : 0; 

                GUILayout.Label($"SKILL - {titles[k]} {q} : {setInfo.ResourceInfo.Id}");
                Rect rectIcon = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                spriteRects[k].Add(rectIcon);
                DrawIcon(setInfo.Icon, rectIcon);

                GUILayout.BeginHorizontal();

                //if(GUILayout.Button("Link"))
                //    ; 
                GUILayout.Label("AAA");

                GUILayout.Label("   ");

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        */
        
        GUILayout.EndHorizontal();
    }

    (int, int) GetIndexFromResources(string resourceId)
    {
        ResourceDataSetting[] rscSets = new ResourceDataSetting[]{ skillCategoryComp.MaterialSet, skillCategoryComp.ComponentSet, skillCategoryComp.ItemSet };
        for(int k = 0; k < rscSets.Length; ++k)
        {
            for(int q = 0; q < rscSets[k].ResourceSets.Count; ++q)
            {
                ResourceSetInfo setInfo = rscSets[k].ResourceSets[q];
                if(setInfo.ResourceInfo.Id.ToLower() == resourceId)
                    return (k, q);
            }
        }

        return (-1, -1);
    }

    void DrawLine()
    {
        Handles.BeginGUI();
        Handles.color = Color.yellow;
        
        /*for(int q = 0; q < craftDataBuildComp.CraftData.Recipes.Count; ++q)
        {
            RecipeInfo recipe = craftDataBuildComp.CraftData.Recipes[q];
            (int outClassIdx, int outIdx) = GetIndexFromResources( recipe.OutcomeId );
            for(int k = 0; k < recipe.Sources.Count; ++k)
            {
                (int classIdx, int idx) = GetIndexFromResources(recipe.Sources[k].ResourceId.ToLower());

                Handles.DrawLine(spriteRects[classIdx][idx].center, spriteRects[outClassIdx][outIdx].center);
            }
        }*/

        Queue<SkillItemComp> qNodes = new Queue<SkillItemComp>();
        qNodes.Enqueue(skillCategoryComp.RootNode);
        while(qNodes.Count > 0) 
        {
            int size = qNodes.Count;
            for(int q = 0; q < size; ++q)
            {
                SkillItemComp item = qNodes.Dequeue();
                
                Handles.color = q==0 ? Color.yellow : Color.green;

                for(int k = 0; k < item.SkillData.UnlockCost.Count; ++k)
                {
                    (int classIdx, int idx) = GetIndexFromResources(item.SkillData.UnlockCost[k].ResourceId.ToLower());
                    if(classIdx>=0 && idx>=0)
                        Handles.DrawLine(spriteRects[classIdx][idx].center, skillRect[item.SkillData.Id].center);
                }

                for(int k = 0; k < item.Children.Count; ++k)
                    qNodes.Enqueue(item.Children[k]);
            }
        }
        
        Handles.EndGUI();
    }

    List<string> GetSkillTreeNodeNames(SkillItemComp root)
    {
        List<string> names = new List<string>();
        if(root == null)    return names;

        int count = 0;
        Queue<SkillItemComp> qNodes = new Queue<SkillItemComp>();
        qNodes.Enqueue(root);
        while(qNodes.Count > 0) 
        {
            int size = qNodes.Count;
            count += size;
            for(int q = 0; q < size; ++q)
            {
                SkillItemComp item = qNodes.Dequeue();
                names.Add(item.SkillData.Name);
                for(int k = 0; k < item.Children.Count; ++k)
                    qNodes.Enqueue(item.Children[k]);
            }
        }
        return names;
    }
    SkillItemComp GetSkillTreeNode(SkillItemComp root, string skillName)
    {
        if(root == null)    return null;

        Queue<SkillItemComp> qNodes = new Queue<SkillItemComp>();
        qNodes.Enqueue(root);
        while(qNodes.Count > 0) 
        {
            int size = qNodes.Count;
            for(int q = 0; q < size; ++q)
            {
                SkillItemComp item = qNodes.Dequeue();
                if(item.SkillData.Name.ToLower() == skillName.ToLower())
                    return item;

                for(int k = 0; k < item.Children.Count; ++k)
                    qNodes.Enqueue(item.Children[k]);
            }
        }
        return null;
    }

    string FindObtainableMiningTargetZones(PlanetControllerComp zoneDataComp, string resourceId)
    {
        resourceId = resourceId.ToLower();
        
        List<Tuple<int, int>> listPlanets = new List<Tuple<int, int>>();    // zoneId, planetId
        for(int q = 0; q < zoneDataComp.ZoneList.Count; ++q)
        {
            for(int pc = 0; pc < zoneDataComp.ZoneList[q].Planets.Count; ++pc)
            {
                PlanetComp comp = zoneDataComp.ZoneList[q].Planets[pc] as PlanetComp;
                if(comp == null) continue;

                for(int k = 0; k < comp.PlanetData.Obtainables.Count; ++k)
                {
                    if(comp.PlanetData.Obtainables[k].ResourceId.ToLower() == resourceId)
                    {
                        listPlanets.Add(new Tuple<int, int>(q, pc));
                        break;
                    }
                }
            }
        }

        string outputZones = string.Empty;
        for(int q = 0; q < listPlanets.Count; ++q) 
        {
            PlanetComp comp = zoneDataComp.ZoneList[ listPlanets[q].Item1 ].Planets[ listPlanets[q].Item2 ] as PlanetComp;
            outputZones += $"{comp.ZoneId}_{comp.PlanetId}_{comp.PlanetData.Name}";
            if(q < listPlanets.Count-1)
                outputZones += " ,";
        }
        return outputZones;
    }

    void DrawIcon(Sprite mySprite, Rect iconRect)
    {
        // 1. Get the sprite's pixel rect.
        Rect pixelRect = mySprite.rect;
        Texture2D spriteTexture = mySprite.texture;

        // 2. Normalize the pixel rect to get UV coordinates.
        Rect uvRect = new Rect(
            pixelRect.x / spriteTexture.width,
            pixelRect.y / spriteTexture.height,
            pixelRect.width / spriteTexture.width,
            pixelRect.height / spriteTexture.height
        );

        GUI.DrawTextureWithTexCoords(iconRect, spriteTexture, uvRect);
    }

}
