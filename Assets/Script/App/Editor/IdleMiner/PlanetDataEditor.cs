using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.GamePlay;
using Core.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[CustomEditor(typeof(PlanetControllerComp))]
public class PlanetDataEditor : Editor
{
    const string DATA_SUB_PATH = "/EditorData";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();     // 기본 인스펙터 표시

        
        GUILayout.Label("");
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            GUILayout.Label("(1) [Obtainable Resource Param]");
            GUILayout.Label(".");
            if (GUILayout.Button("<< Auto Assign 'Obtainable Resources' >>", GUILayout.Height(50.0f)))
                AssignObtainableResource();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("(2) [Open Cost Param]");
            GUILayout.Label("Ret = Base + Base * index * Rarity");
            if (GUILayout.Button("<< Generate Planet 'Open Cost' >>", GUILayout.Height(50.0f)))
                GenerateOpenCost();
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();


        GUILayout.Label("");
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            GUILayout.Label("(3) [Upgrade Stats Param]");
            GUILayout.Label("Ret = Base + Base * index * Rarity");
            if (GUILayout.Button("<< Generate Stats Upgrade-Param >>", GUILayout.Height(50.0f)))
                GenerateStatUpgradeParam();
            GUILayout.EndVertical();

        
            GUILayout.BeginVertical();
            GUILayout.Label("(4) [Upgrade Costs Param]");
            GUILayout.Label("Ret = Base + Base * index * Rarity");
            if (GUILayout.Button("<< Generate Stats Upgrade-Cost >>", GUILayout.Height(50.0f)))
                GenerateStatUpgradeCost();
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        PlanetControllerComp comp = (PlanetControllerComp)target;

        GUILayout.Label("");
        if (GUILayout.Button("<< Resource Planet Assign Editor >>", GUILayout.Height(50.0f)))
             ResourcePlanetEditor.ShowWindow(comp);

        GUILayout.Label("");
        GUILayout.Label("");
        GUILayout.Label("[ Data File Write ]");
        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/PlanetData.json");
        if (GUILayout.Button("<< Write Planet Data >>", GUILayout.Height(50.0f)))
            comp.ExportPlanetData();
        
        GUILayout.Label("");
        GUILayout.Label("Output : Assets/EditorData/PlanetBossData.json");
        if (GUILayout.Button("<< Write Planet Boss Data >>", GUILayout.Height(50.0f)))
            comp.ExportPlanetBossData();
    }


    void GenerateStatUpgradeParam()
    {
        PlanetControllerComp comp = (PlanetControllerComp)target;

        if(comp.resourceDataSetting == null || comp.resourceDataSetting.ResourceSets==null || comp.resourceDataSetting.ResourceSets.Count==0)
        {
            Assert.IsTrue(false, "Invalid Resource Data.");
            return;
        }

        int idx = 0;
        PlanetZoneGroup zoneGroup = new PlanetZoneGroup();
        for(int q = 0; q < comp.ZoneList.Count; ++q)
        {
            var zone = comp.ZoneList[q];
            for(int z = 0; z < zone.Planets.Count; z++) 
            {   
                if((zone.Planets[z] as PlanetBossComp) != null)
                    continue;
                
                PlanetComp planetComp = (zone.Planets[z] as PlanetComp);
                Assert.IsNotNull(planetComp);

                planetComp.PlanetData.SetUpgradeStats(
                    generateUpgradeParam(comp.miningRateParam, idx, isCost:false), 
                    generateUpgradeParam(comp.shipSpeedParam, idx, isCost:false), 
                    generateUpgradeParam(comp.cargoSizeParam, idx, isCost:false),  
                    generateUpgradeParam(comp.shotAccuracyParam, idx, isCost:false), 
                    generateUpgradeParam(comp.shotIntervalParam, idx, isCost:false));
                    
                Debug.Log($"[{q}]-[{z}]-[{idx}] : Stat upgrade stats has been set.");
                ++idx;
            }
        }
    }

    // input => 100:1.0:100:0.1:40:0
    // output => base=1:incPercent=100:incBase=0
    string generateUpgradeParam(string inputCostParam, int index, bool isCost)
    {
        if(string.IsNullOrEmpty(inputCostParam))
        {
            Debug.LogError("Invalid cost input param string.");
            return string.Empty;
        }

        string[] costParams = inputCostParam.Split(':');
        
        if(isCost)
        {
            if(costParams==null || costParams.Length!=6)
            {
                Debug.LogError("Invalid cost input param string.");
                return string.Empty;
            }

            long baseBase = long.Parse(costParams[0]);
            float baseRarity = float.Parse(costParams[1]);
            float incPercentBase = float.Parse(costParams[2]) * 100.0f;
            float incPercentRarity = float.Parse(costParams[3]);
            long incBaseBase = long.Parse(costParams[4]);
            float incBaseRarity = float.Parse(costParams[5]);

            long finalBase = (long)( baseBase + ((float)baseBase) * ((float)index) * baseRarity );
            long finalIncPercent = (long)( incPercentBase + ((float)incPercentBase) * ((float)index) * incPercentRarity);
            long finalIncBase = (long)( incBaseBase + ((float)incBaseBase) * ((float)index) * incBaseRarity);

            return $"base={finalBase}:incPercent={finalIncPercent}:incBase={finalIncBase}";
        }
        else
        {
            if(costParams == null)
            {
                Debug.LogError("Invalid stat input param string.");
                return string.Empty;
            }

            if(costParams.Length == 2)
            {
                float finalBase = float.Parse(costParams[0]);   //  baseBase + baseBase * ((float)index) * baseRarity;
                float finalIncPercent = finalBase * 100.0f;     // incPercentBase + incPercentBase * ((float)index) * incPercentRarity;
                float finalIncBase = float.Parse(costParams[1]);// incBaseBase + incBaseBase * ((float)index) * incBaseRarity;

                return $"base={finalBase.ToString("0.00")}:incPercent={finalIncPercent.ToString("0.00")}:incBase={finalIncBase.ToString("0.00")}";
            }
            else if(costParams.Length == 3) 
            {
                float finalBase = float.Parse(costParams[0]);       //  baseBase + baseBase * ((float)index) * baseRarity;
                float finalIncPercent = float.Parse(costParams[1]) * 100.0f; // incPercentBase + incPercentBase * ((float)index) * incPercentRarity;
                float finalIncBase = float.Parse(costParams[2]);    // incBaseBase + incBaseBase * ((float)index) * incBaseRarity;

                return $"base={finalBase.ToString("0.00")}:incPercent={finalIncPercent.ToString("0.00")}:incBase={finalIncBase.ToString("0.00")}";
            }
            else
            {
                Debug.LogError("Invalid stat input param string.");
                return string.Empty;
            }
        }
    }

    void GenerateStatUpgradeCost()
    {
        PlanetControllerComp comp = (PlanetControllerComp)target;

        if(comp.resourceDataSetting == null || comp.resourceDataSetting.ResourceSets==null || comp.resourceDataSetting.ResourceSets.Count==0)
        {
            Assert.IsTrue(false, "Invalid Resource Data.");
            return;
        }

        int idx = 0;
        PlanetZoneGroup zoneGroup = new PlanetZoneGroup();
        for(int q = 0; q < comp.ZoneList.Count; ++q)
        {
            var zone = comp.ZoneList[q];
            for(int z = 0; z < zone.Planets.Count; z++) 
            {   
                if((zone.Planets[z] as PlanetBossComp) != null)
                    continue;
                
                PlanetComp planetComp = (zone.Planets[z] as PlanetComp);
                Assert.IsNotNull(planetComp);

                string costUpgradeParam = generateUpgradeParam(comp.costUpgradeParam, idx, isCost:true);

                planetComp.PlanetData.SetUpgradeCosts(
                    costUpgradeParam, costUpgradeParam, costUpgradeParam, costUpgradeParam, costUpgradeParam);
                    
                Debug.Log($"[{q}]-[{z}]-[{idx}] : Stat upgrade cost has been set.");
                ++idx;
            }
        }
    }


    void GenerateOpenCost()
    {
        PlanetControllerComp comp = (PlanetControllerComp)target;

        if(comp.resourceDataSetting == null || comp.resourceDataSetting.ResourceSets==null || comp.resourceDataSetting.ResourceSets.Count==0)
        {
            Assert.IsTrue(false, "Invalid Resource Data.");
            return;
        }

        int idx = 0;
        PlanetZoneGroup zoneGroup = new PlanetZoneGroup();
        for(int q = 0; q < comp.ZoneList.Count; ++q)
        {
            var zone = comp.ZoneList[q];
            for(int z = 0; z < zone.Planets.Count; z++) 
            {   
                if((zone.Planets[z] as PlanetBossComp)== null)
                {
                    PlanetComp planetComp = (zone.Planets[z] as PlanetComp);
                    Assert.IsNotNull(planetComp);

                    float price = comp.OpenCostBaseValue + ((float)comp.OpenCostBaseValue) * ((float)idx) * comp.OpenCostRarity;
                    planetComp.PlanetData.SetOpenCost( ((long)price).ToString() );

                    Debug.Log($"[{q}]-[{z}]-[{idx}] : Cost [{price}] has been set.");
                }
                ++idx;
            }
        }
    }

    void AssignObtainableResource()
    {
        PlanetControllerComp comp = (PlanetControllerComp)target;

        if(comp.resourceDataSetting == null || comp.resourceDataSetting.ResourceSets==null || comp.resourceDataSetting.ResourceSets.Count==0)
        {
            Assert.IsTrue(false, "Invalid Resource Data.");
            return;
        }

        int idxRsc = 0;
        PlanetZoneGroup zoneGroup = new PlanetZoneGroup();
        for(int q = 0; q < comp.ZoneList.Count; ++q)
        {
            var zone = comp.ZoneList[q];
            if(zone.PlanetCount != 3)
            {
                Debug.LogWarning("Skipping the zone... the logic should work only when planet count is 3 for now.");
                continue;
            }

            for(int z = 0; z < zone.Planets.Count; z++) 
            {   
                if((zone.Planets[z] as PlanetBossComp)== null)
                {
                    PlanetComp planetComp = (zone.Planets[z] as PlanetComp);
                    Assert.IsNotNull(planetComp);

                    planetComp.PlanetData.Obtainables.Clear();

                    ResourceInfo info0 = comp.resourceDataSetting.ResourceSets[idxRsc].ResourceInfo;
                    if(idxRsc == comp.resourceDataSetting.ResourceSets.Count-1)
                    {   
                        planetComp.PlanetData.Obtainables.Add(new ObtainStat(info0.Id, 0.9f));
                        Debug.Log($"[{q}]-[{z}]-[{idxRsc}] : [{info0.Id}-0.9] has been assigned, also reached resource limit.");
                        continue;
                    }

                    switch(z)
                    {
                    case 1:
                        {
                            info0 = comp.resourceDataSetting.ResourceSets[idxRsc].ResourceInfo;
                            planetComp.PlanetData.Obtainables.Add(new ObtainStat(info0.Id, 0.5f));
                            Debug.Log($"[{q}]-[{z}]-[{idxRsc}] : [{info0.Id}-0.5] has been assigned.");

                            ResourceInfo info1 = comp.resourceDataSetting.ResourceSets[idxRsc+1].ResourceInfo;
                            planetComp.PlanetData.Obtainables.Add(new ObtainStat(info1.Id, 0.4f));
                            Debug.Log($"[{q}]-[{z}]-[{idxRsc}] : [{info1.Id}-0.4] has been assigned.");

                            ++idxRsc;
                            break;
                        }
                    case 0:
                    case 2:
                        {
                            float yieldRate = z == 0 ? 0.65f : 0.45f;
                            info0 = comp.resourceDataSetting.ResourceSets[idxRsc].ResourceInfo;
                            planetComp.PlanetData.Obtainables.Add(new ObtainStat(info0.Id, yieldRate));
                            Debug.Log($"[{q}]-[{z}]-[{idxRsc}] : [{info0.Id}-{yieldRate}] has been assigned.");
                            
                            if(z == 2)      ++idxRsc;
                            break;
                        }
                    default:
                        Assert.IsTrue(false, "Invalid index..." + z);
                        break;
                    }
                }
            }
        }
    }

    
   
}
