using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Model;
using Core.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class PlanetControllerComp : MonoBehaviour
    {
        [SerializeField] List<MiningZoneComp> zoneList;

        MonoBehaviour coRunnerCache;
        GameObject mainCharCache;
        GameObject flyMonCache;

#if UNITY_EDITOR
        [Header("========== Editor Area ============")]
        [Header("[(1) Obtainable Resource Param ]")]
        [SerializeField] public ResourceDataSetting resourceDataSetting;
        
        [Header("[(2) Open Cost Param ]")]
        [SerializeField] public int OpenCostBaseValue = 10;
        [SerializeField] public float OpenCostRarity = 1.0f;
        
        
        [Header("[(3) Upgrade Stats Param ]")]
        [Header("ex) Base:IncRarity")]
        [SerializeField] public string miningRateParam = "1:0";           // "1:0.5:100:0:1:0";
        [SerializeField] public string shipSpeedParam = "0.3:0.01";       // 0.3:0.1:2:0.1:1:0.2";
        [SerializeField] public string cargoSizeParam = "1:0.01";         // "1:0.5:100:0:1:0.2";
        [SerializeField] public string shotAccuracyParam = "0.2:0.01";    // "0.2:0.1:100:0.1:0.05:0.5";
        [SerializeField] public string shotIntervalParam = "5:-0.01";     // "5:0:10:0:-0.01:0";
        

        [Header("[(4) Upgrade Costs Param ]")]
        [Header("ex) BaseBase:BaseRarity:IncBase:IncRarity:IncBaseBase:IncBaseRarity")]
        [SerializeField] public string costUpgradeParam = "100:1.0:1:0.1:40:0";

        public List<MiningZoneComp> ZoneList => zoneList;
#endif


        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Assertions.Assert.IsTrue(zoneList != null && zoneList.Count > 0);
        }


        public void Init(MonoBehaviour coroutineRunner, GameObject mainCharObj, GameObject flyMonObjCache,
            Dictionary<int, List<MiningZoneComp.BaseInfo>> dictZoneInfo)
        {
            coRunnerCache = coroutineRunner;
            mainCharCache = mainCharObj;
            flyMonCache = flyMonObjCache;

            for (int q = 0; q < zoneList.Count; ++q)   // zone list.
            {
                int zId = zoneList[q].ZoneId;
                zoneList[q].gameObject.SetActive( dictZoneInfo.ContainsKey( zId ) );
                
                if(zoneList[q].gameObject.activeSelf) 
                    zoneList[q].Init(mainCharObj, flyMonObjCache, dictZoneInfo[zId]);
            }
        }

        public void UpdateArea(Dictionary<int, List<MiningZoneComp.BaseInfo>> dictZonenfo)
        {
            for (int q = 0; q < zoneList.Count; ++q)
            {
                int zId = zoneList[q].ZoneId;
                if(dictZonenfo.ContainsKey(zId))
                    zoneList[q].Init(mainCharCache, flyMonCache, dictZonenfo[zId]);
            }
        }

        public PlanetBaseComp GetPlanetComp(int zoneId, int planetId)
        {
            var miningZone = GetMiningZoneComp(zoneId);
            if(miningZone != null)
                return miningZone.GetPlanetComp(planetId);
            
            return null;
        }

        //public void Refresh(int townId)
        //{}
        public Sprite GetPlanetSprite(int zoneId, int planetId)
        {
            var miningZone = GetMiningZoneComp(zoneId);
            if(miningZone != null)
            {
                var townComp = miningZone.GetPlanetComp(planetId);
                if (townComp != null)
                    return townComp.GetComponent<PlanetBaseComp>().GetIcon();
            }
            return null;
        }

        public TwoSpotRunner GetDeliverer(int zoneId, int townId)
        {
            var miningZone = GetMiningZoneComp(zoneId);
            if(miningZone != null)
            {
                var townComp = miningZone.GetPlanetComp(townId);
                if (townComp != null)
                {
                    var planetComp = townComp.GetComponent<PlanetComp>();
                    return planetComp!=null ? planetComp.Deliverer : null;
                }
            }
            return null;
        }

        public Vector2 GetPlanetPos(int zoneId, int planetId)
        {
            var miningZone = GetMiningZoneComp(zoneId);
            if(miningZone != null)
            {
                var townComp = miningZone.GetPlanetComp(planetId);
                if (townComp != null)
                    return townComp.transform.position;
            }
            return Vector2.zero;
        }

        public List<Vector2> GetPlanetsPos(int zoneId)
        {
            var miningZone = GetMiningZoneComp(zoneId);
            if(miningZone != null)
                return miningZone.GetPlanetsPos();
            
            return new List<Vector2>();
        }
 



        // Local Helper.
        MiningZoneComp GetMiningZoneComp(int zoneId)
        {
            for (int q = 0; q < zoneList.Count; ++q)
            {
                if(zoneList[q].ZoneId == zoneId)
                    return zoneList[q];
            }
            return null;
        }

#if UNITY_EDITOR
        public void ExportPlanetData()
        {
            const string DATA_SUB_PATH = "/EditorData";
            PlanetZoneGroup zoneGroup = new PlanetZoneGroup();
            for(int q = 0; q < ZoneList.Count; ++q)
            {
                var zone = ZoneList[q];

                List<PlanetData> planetDataList = new List<PlanetData>();
                for(int z = 0; z < zone.Planets.Count; z++) 
                {   
                    if((zone.Planets[z] as PlanetBossComp)== null)
                    {
                        PlanetComp planetComp = (zone.Planets[z] as PlanetComp);
                        UnityEngine.Assertions.Assert.IsNotNull(planetComp);

                        planetDataList.Add(planetComp.PlanetData);
                    }
                }
                if(planetDataList.Count > 0) 
                    zoneGroup.AddZoneData(new PlanetZoneData(zone.ZoneId, planetDataList));
            }

            if(zoneGroup.Data!=null && zoneGroup.Data.Count > 0)
            {
                string content = JsonUtility.ToJson(zoneGroup, prettyPrint:true);
                TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + "/PlanetData" + ".json", content);
            }
        }



        public void ExportPlanetBossData()
        {
            const string DATA_SUB_PATH = "/EditorData";
            PlanetZoneBossGroup bossZoneGroup = new PlanetZoneBossGroup();
            for(int q = 0; q < ZoneList.Count; ++q)
            {
                var zone = ZoneList[q];
                List<PlanetBossData> planetBossDataList = new List<PlanetBossData>();
                for(int z = 0; z < zone.Planets.Count; z++) 
                {   
                    if((zone.Planets[z] as PlanetBossComp) != null)
                    {
                        planetBossDataList.Add( (zone.Planets[z] as PlanetBossComp).PlanetBossData );
                    }
                }
                if(planetBossDataList.Count > 0)
                    bossZoneGroup.AddZoneData(new PlanetZoneBossData(zone.ZoneId, planetBossDataList));
            }

            if(bossZoneGroup.Data!=null && bossZoneGroup.Data.Count > 0)
            {
                string content = JsonUtility.ToJson(bossZoneGroup, prettyPrint:true);
                TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + "/PlanetBossData" + ".json", content);
            }
        }


        public void AddCollectableResourceAtPlanet(PlanetBaseComp planet, string resourceId)
        {
            PlanetComp planetComp = planet as PlanetComp;
            if(planetComp != null) 
            {
                for(int q = 0; q < planetComp.PlanetData.Obtainables.Count; ++q)
                {
                    if(planetComp.PlanetData.Obtainables[q].ResourceId == resourceId)
                        return;
                }
                planetComp.PlanetData.Obtainables.Add(new ObtainStat(resourceId, .5f));
            }
        }
        public void RemoveCollectableResourceAtPlanet(PlanetBaseComp planet, string resourceId)
        {
            PlanetComp planetComp = planet as PlanetComp;
            if(planetComp != null) 
            {
                for(int q = 0; q < planetComp.PlanetData.Obtainables.Count; ++q)
                {
                    if(planetComp.PlanetData.Obtainables[q].ResourceId == resourceId)
                    {
                        planetComp.PlanetData.Obtainables.RemoveAt(q);
                        return;
                    }
                }
            }
        }
#endif
    }
}