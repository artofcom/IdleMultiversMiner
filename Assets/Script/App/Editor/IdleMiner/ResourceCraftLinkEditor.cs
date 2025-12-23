using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResourceCraftLinkEditor : EditorWindow
{
    const int WIDTH = 1500;
    const int LANE_W = 500;
    private GUIStyle styleBlk, styleRed;

    CraftDataBuildComp craftDataBuildComp;

    // int idxSelected = 0;
    List<List<Rect>> spriteRects = new List<List<Rect>>();
    List<Rect> recipeRect = new List<Rect>();
    Vector2 vScrollPos = Vector2.zero;

    // [MenuItem("PlasticGames/Editor/CraftEditor2")]
    public static void ShowWindow(CraftDataBuildComp craftData)
    {
        ResourceCraftLinkEditor window = GetWindow<ResourceCraftLinkEditor>("Resource Craft Link Editor");

        window.position = new Rect(100, 100, WIDTH, 500);
        window.Init(craftData);
    }

    public void Init(CraftDataBuildComp comp)
    {
        styleRed = new GUIStyle(GUI.skin.button);
        styleRed.normal.textColor = Color.red;
        styleRed.fixedWidth = 100.0f;

        styleBlk = new GUIStyle(GUI.skin.button);
        styleBlk.normal.textColor = GUI.skin.label.normal.textColor;
        styleBlk.fixedWidth = 100.0f;

        craftDataBuildComp = comp;
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
        
        void AddToRefBuffer(string key)
        {
            if(string.IsNullOrEmpty(key)) return;
            if(referenceBuffer.ContainsKey(key))    referenceBuffer[key]++;
            else referenceBuffer[key] = 1;
        }

        for(int q = 0; q < craftDataBuildComp.CraftData.Recipes.Count; ++q)
        {
            RecipeInfo recipe = craftDataBuildComp.CraftData.Recipes[q];
            AddToRefBuffer(recipe.OutcomeId);
            if(recipe.Sources == null)      continue;

            for(int i = 0; i < recipe.Sources.Count; ++i)
                AddToRefBuffer(recipe.Sources[i].ResourceId);
        }


        GUILayout.Label("=== Link Craft Recipe and Resources.");
        GUILayout.Space(10);

        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
     


        // Recipe List.
        GUILayout.BeginVertical();
        recipeRect.Clear();
        GUILayout.Label("[ Recipes List. ]");
        for(int k = 0; k < craftDataBuildComp.CraftData.Recipes.Count; ++k)
        {
            RecipeInfo recipeInfo = craftDataBuildComp.CraftData.Recipes[k];

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"# [{recipeInfo.Id}]-[{recipeInfo.OutcomeId}]");
                Rect rectIcon = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                recipeRect.Add(rectIcon);
                GUILayout.Label("             ");
            }
            GUILayout.EndHorizontal();

            if(recipeInfo.Sources != null)
            {
                for(int q = 0; q < recipeInfo.Sources.Count; ++q)
                {
                    GUILayout.Label($"     {recipeInfo.Sources[q].ResourceId} - {recipeInfo.Sources[q].GetCount()}");    
                }
            }
            GUILayout.Label("        ");
        }
        GUILayout.EndVertical();




        
        // Build Id options first. 
        string[] recipiOptions = new string[ craftDataBuildComp.CraftData.Recipes.Count ];
        for(int q = 0; q < craftDataBuildComp.CraftData.Recipes.Count; ++q)
            recipiOptions[q] = craftDataBuildComp.CraftData.Recipes[q].Id;
                        
        string[] titles = new string[]{"Material", "Component", "Item"};
        ResourceDataSetting[] rscSets = new ResourceDataSetting[]{ craftDataBuildComp.MaterialSet, craftDataBuildComp.ComponentSet, craftDataBuildComp.ItemSet };
        spriteRects.Clear();
        spriteRects.Add(new List<Rect>());  spriteRects.Add(new List<Rect>());  spriteRects.Add(new List<Rect>());
        for(int k = 0; k < rscSets.Length; ++k)
        {
            spriteRects[k].Clear();
            GUILayout.BeginVertical();
            for(int q = 0; q < rscSets[k].ResourceSets.Count; ++q)
            {
                ResourceSetInfo setInfo = rscSets[k].ResourceSets[q];
                string key = setInfo.ResourceInfo.Id.ToLower();
                GUIStyle style = referenceBuffer.ContainsKey(key) && referenceBuffer[key]>0  ? styleRed : styleBlk; 
                int refCount = referenceBuffer.ContainsKey(key) ? referenceBuffer[key] : 0; 

                GUILayout.Label(" ");
                GUILayout.BeginHorizontal();
                string presentZones = FindObtainableMiningTargetZones(craftDataBuildComp.ZoneDataComp, key);
                string title = $"# {titles[k]} {q} : {setInfo.ResourceInfo.Id}";
                if(string.IsNullOrEmpty(presentZones))  title += " => NONE";
                else                                    title += $" => {presentZones}";

                GUILayout.Label(title);
                if(GUILayout.Button("Copy-Id", styleBlk))
                {
                    EditorGUIUtility.systemCopyBuffer = setInfo.ResourceInfo.Id;
                    Debug.Log($"Copied '{setInfo.ResourceInfo.Id}' to clipboard.");
                }
                GUILayout.EndHorizontal();

                Rect rectIcon = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                spriteRects[k].Add(rectIcon);
                DrawIcon(setInfo.Icon, rectIcon);

                GUILayout.BeginHorizontal();

     
                
                if(!craftDataBuildComp.DictTargetSelections.ContainsKey(key))
                    craftDataBuildComp.DictTargetSelections[key] = 0;
                if(GUILayout.Button("Link", styleBlk))
                    craftDataBuildComp.SetRecipiSourceId(recipiOptions[ craftDataBuildComp.DictTargetSelections[key] ], setInfo.ResourceInfo.Id);
                if(GUILayout.Button("Unlink", styleRed))
                    craftDataBuildComp.RemoveRecipiSourceId(recipiOptions[ craftDataBuildComp.DictTargetSelections[key] ], setInfo.ResourceInfo.Id);

                craftDataBuildComp.DictTargetSelections[key] = EditorGUILayout.Popup(craftDataBuildComp.DictTargetSelections[key], recipiOptions);
                
                GUILayout.Label("   ");

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        
        GUILayout.EndHorizontal();
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
            outputZones += $"{listPlanets[q].Item1}_{listPlanets[q].Item2}_{comp.PlanetData.Name}";
            if(q < listPlanets.Count-1)
                outputZones += " ,";
        }
        return outputZones;
    }

    (int, int) GetIndexFromResources(string resourceId)
    {
        ResourceDataSetting[] rscSets = new ResourceDataSetting[]{ craftDataBuildComp.MaterialSet, craftDataBuildComp.ComponentSet, craftDataBuildComp.ItemSet };
        for(int k = 0; k < rscSets.Length; ++k)
        {
            for(int q = 0; q < rscSets[k].ResourceSets.Count; ++q)
            {
                ResourceSetInfo setInfo = rscSets[k].ResourceSets[q];
                if(setInfo.ResourceInfo.Id.ToLower() == resourceId)
                    return (k, q);
            }
        }

        return (0, 0);
    }

    void DrawLine()
    {
        Handles.BeginGUI();

                
        for(int q = 0; q < craftDataBuildComp.CraftData.Recipes.Count; ++q)
        {
            RecipeInfo recipe = craftDataBuildComp.CraftData.Recipes[q];
            (int outClassIdx, int outIdx) = GetIndexFromResources( recipe.OutcomeId );
            Handles.color = Color.green; 
            Handles.DrawLine(recipeRect[q].center, spriteRects[outClassIdx][outIdx].center);
            // Debug.Log($"Rect {q}, X:{recipeRect[q].center.x}, Y:{recipeRect[q].center.y}");
            Handles.color = Color.yellow;

            for(int k = 0; k < recipe.Sources.Count; ++k)
            {    
                (int classIdx, int idx) = GetIndexFromResources(recipe.Sources[k].ResourceId.ToLower());
                Vector2 vtRecipe = new Vector2(recipeRect[q].center.x, recipeRect[q].center.y+30);
                Handles.DrawLine(vtRecipe, spriteRects[classIdx][idx].center);
            }
        }
        
        Handles.EndGUI();
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
