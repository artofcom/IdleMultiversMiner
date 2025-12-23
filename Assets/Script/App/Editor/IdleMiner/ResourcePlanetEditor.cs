using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;


public class ResourcePlanetEditor : EditorWindow
{
    const int WIDTH = 1500;
    const int LANE_W = 500;
    private GUIStyle styleBlk, styleRed;

    PlanetControllerComp planetCtronllerComp;

    List<Rect> resourceRects = new List<Rect>();
    Dictionary<string, Rect> planetRect = new Dictionary<string, Rect>();
    Dictionary<string, int> targetPlanets = new Dictionary<string, int>();
    Vector2 vScrollPos = Vector2.zero;

    // [MenuItem("PlasticGames/Editor/CraftEditor2")]
    public static void ShowWindow(PlanetControllerComp planetCtrler)
    {
        ResourcePlanetEditor window = GetWindow<ResourcePlanetEditor>("Resource Planet Editor.");

        window.position = new Rect(100, 100, WIDTH, 500);
        window.Init(planetCtrler);
    }

    public void Init(PlanetControllerComp comp)
    {
        styleRed = new GUIStyle(GUI.skin.button);
        styleRed.normal.textColor = Color.red;
        styleRed.fixedWidth = 80.0f;

        styleBlk = new GUIStyle(GUI.skin.button);
        styleBlk.normal.textColor = GUI.skin.label.normal.textColor;
        styleBlk.fixedWidth = 80.0f;

        planetCtronllerComp = comp;
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
            GUILayout.Label("=== Missing link check");
        }
        GUILayout.EndScrollView();
    }
     
    void DrawResourcesGroup()
    {
        Dictionary<string, int> referenceBuffer = new Dictionary<string, int>();
        
        GUILayout.Label("=== Select Resource to copy its name to the System ClipBoard.");
        GUILayout.Space(10);

        GUILayout.BeginHorizontal(GUILayout.Width(WIDTH));
     
        
        // Build Id options first. 
        string[] planetOptions = GetPlanetNames(planetCtronllerComp.ZoneList).ToArray();
           
        // Resources.
        ResourceDataSetting rscSet = planetCtronllerComp.resourceDataSetting;
        
        resourceRects.Clear();
        GUILayout.BeginVertical();
        //GUILayout.Label(titles[k]);
        for(int q = 0; q < rscSet.ResourceSets.Count; ++q)
        {
            ResourceSetInfo setInfo = rscSet.ResourceSets[q];
            string key = setInfo.ResourceInfo.Id.ToLower();
            GUIStyle style = referenceBuffer.ContainsKey(key) && referenceBuffer[key]>0  ? styleRed : styleBlk; 
            int refCount = referenceBuffer.ContainsKey(key) ? referenceBuffer[key] : 0; 

            GUILayout.Label($"# Mateiral {q} : {setInfo.ResourceInfo.Id}");
            Rect rectIcon = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
            resourceRects.Add(rectIcon);
            DrawIcon(setInfo.Icon, rectIcon);

            GUILayout.BeginHorizontal();

            string idxKey = setInfo.ResourceInfo.Id;
            if(!targetPlanets.ContainsKey(idxKey) || targetPlanets[idxKey]>=planetOptions.Length)
                targetPlanets[idxKey] = 0;

            string selectedPlanetName = planetOptions[ targetPlanets[idxKey] ];
            // 100_1_BtnTownA
            string[] planetParams = selectedPlanetName.Split('_');
            Assert.IsTrue(planetParams.Length==3);

            
            PlanetBaseComp targetComp = GetPlanetCompById(planetCtronllerComp.ZoneList, int.Parse(planetParams[0]), int.Parse(planetParams[1]));

            if(GUILayout.Button("Link", styleBlk))
                planetCtronllerComp.AddCollectableResourceAtPlanet(targetComp, setInfo.ResourceInfo.Id);
            if(GUILayout.Button("Unlink", styleRed))
                planetCtronllerComp.RemoveCollectableResourceAtPlanet(targetComp, setInfo.ResourceInfo.Id);
                
            targetPlanets[idxKey] = EditorGUILayout.Popup(targetPlanets[idxKey], planetOptions);
                
            GUILayout.Label("   ");
            GUILayout.EndHorizontal();
        }
        
        GUILayout.EndVertical();
        //}
        

        // Dividers.
        GUILayout.BeginVertical();
        for(int q = 0; q < rscSet.ResourceSets.Count; ++q)
        {
            GUILayout.Label("        ");
        }
        GUILayout.EndVertical();
        

        //skillCategoryComp.RootNode
        // Drawing Skills Here. - BFS
        planetRect.Clear();
        GUILayout.BeginVertical();

        for(int k = 0; k < planetCtronllerComp.ZoneList.Count; ++k)
        {
            GUILayout.BeginVertical();
            MiningZoneComp zoneComp = planetCtronllerComp.ZoneList[k];

            for(int q = 0; q < zoneComp.Planets.Count; ++q)
            {
                PlanetBaseComp planetComp = zoneComp.Planets[q];
                GUILayout.Label($"# {zoneComp.ZoneId}_{planetComp.PlanetId}_{planetComp.name}");
                Rect rectIcon = GUILayoutUtility.GetRect(30, 30, GUILayout.ExpandWidth(false));
                DrawIcon(planetComp.GetIcon(), rectIcon);
                planetRect.Add($"{zoneComp.ZoneId}_{planetComp.PlanetId}_{planetComp.name}", rectIcon);
                
                PlanetComp plComp = planetComp as PlanetComp;
                if(plComp != null ) 
                {
                    for(int t = 0; t < plComp.PlanetData.Obtainables.Count; ++t)
                         GUILayout.Label($"  > {plComp.PlanetData.Obtainables[t].ResourceId} ({plComp.PlanetData.Obtainables[t].Yield}) ");
                }

                GUILayout.Label("        ");
                
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();
       
        
        GUILayout.EndHorizontal();
    }

    int GetIndexFromResources(string resourceId)
    {
        for(int q = 0; q < planetCtronllerComp.resourceDataSetting.ResourceSets.Count; ++q)
        {
            ResourceSetInfo setInfo = planetCtronllerComp.resourceDataSetting.ResourceSets[q];
            if(setInfo.ResourceInfo.Id.ToLower() == resourceId)
                return q;
        }
        
        return -1;
    }

    void DrawLine()
    {
        Handles.BeginGUI();
        Handles.color = Color.yellow;
        
        for(int k = 0; k < planetCtronllerComp.ZoneList.Count; ++k)
        {
            MiningZoneComp zoneComp = planetCtronllerComp.ZoneList[k];
            for(int q = 0; q < zoneComp.Planets.Count; ++q)
            {
                PlanetComp planetComp = zoneComp.Planets[q] as PlanetComp;
                if(planetComp == null) continue;

                for(int t = 0; t < planetComp.PlanetData.Obtainables.Count; ++t)
                {
                    int idxRsc = GetIndexFromResources(planetComp.PlanetData.Obtainables[t].ResourceId); 
                    string planetKey = $"{zoneComp.ZoneId}_{planetComp.PlanetId}_{planetComp.name}";
                    
                    Assert.IsTrue(idxRsc>=0 && idxRsc<resourceRects.Count);
                    Assert.IsTrue(planetRect.ContainsKey(planetKey));
                    
                    Handles.DrawLine(resourceRects[idxRsc].center, planetRect[ planetKey ].center);
                }
            }
        }

        Handles.EndGUI();
    }

    List<string> GetPlanetNames(List<MiningZoneComp> zoneList)
    {
        List<string> names = new List<string>();
        if(zoneList == null)    return names;

        for(int q = 0; q < zoneList.Count; ++q)
        {
            for(int k = 0; k < zoneList[q].Planets.Count; ++k)
            {
                names.Add( $"{zoneList[q].ZoneId}_{zoneList[q].Planets[k].PlanetId}_{zoneList[q].Planets[k].name}");
            }
        }
        return names;
    }
    PlanetBaseComp GetPlanetCompById(List<MiningZoneComp> zoneList, int zoneId, int planetId)
    {
        for(int q = 0; q < zoneList.Count; ++q)
        {
            if(zoneList[q].ZoneId != zoneId)
                continue;

            for(int k = 0; k < zoneList[q].Planets.Count; ++k)
            {
                if(zoneList[q].Planets[k].PlanetId == planetId)
                    return zoneList[q].Planets[k];  
            }
        }
        return null;
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
