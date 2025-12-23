
using System.Numerics;
using UnityEngine;
//using Core.Events;
using Core.Utils;
using System.Collections.Generic;
using Core.Events;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Types;
using Unity.Mathematics.Geometry;

namespace App.GamePlay.IdleMiner.Common.Model
{
    // PLANET
    [Serializable]
    public class PlanetData : PlanetBaseData
    {
        public const int MAX_MINING = 3;
        public const string KEY = "Planet";

        //public static readonly float WORLD_DIST_ADJUSTMENTS = 100.0f;

        [SerializeField] List<ObtainStat> obtainables = new List<ObtainStat>();

        // Going to use distinguishable names due to readibility. 
        [Header("[Mining Stat]")]
        [SerializeField] string miningRate;
        [SerializeField] string shipSpeed;
        [SerializeField] string cargoSize;
        [SerializeField] string shotAccuracy;
        [SerializeField] string shotInterval;
        [Header("[Upgrade Cost]")]
        [SerializeField] string miningCost;
        [SerializeField] string shipCost;
        [SerializeField] string cargoCost;
        [SerializeField] string shotAccuracyCost;
        [SerializeField] string shotIntervalCost;

        public override string Type { get => KEY; }

        // constructor.
        public PlanetData()  {}
        public PlanetData(int zoneId, int id, string name, string openCost, List<ObtainStat> obtainables, 
            string miningRate, string shipSpeed, string cargoSize, string shotAccuracy, string shotInterval, 
            string miningCost, string shipCost, string cargoCost, string shotAccuracyCost, string shotIntervalCost)
        {
            this.zoneId = zoneId;
            this.id = id;           this.name = name;       this.openCost = openCost;
            this.obtainables = obtainables;
            
            this.miningRate = miningRate;               this.shipSpeed = shipSpeed; this.cargoSize = cargoSize;
            this.shotAccuracy = shotAccuracy;           this.shotInterval = shotInterval;
            
            this.miningCost = miningCost;               this.shipCost = shipCost;   this.cargoCost = cargoCost;
            this.shotAccuracyCost = shotAccuracyCost;   this.shotIntervalCost = shotIntervalCost;

            this.Convert();
        }

        public List<ObtainStat> Obtainables => obtainables;

        LevelBasedFloat[] stats = new LevelBasedFloat[(int)eABILITY.MAX];
        LevelBasedBigInteger[] costs = new LevelBasedBigInteger[(int)eABILITY.MAX];


        // All stats have their own bandwidth.
        //
        public float Stat(eABILITY eType, int level)
        {
            Assert.IsTrue(level > 0);
            if (level < 0) level = 1;

            return stats[(int)eType].Value(level);

            //float Stat() { return stats[(int)eType].Value(level); }
        }
        public LevelBasedFloat LBStat(eABILITY ability)
        {
            if (ability >= 0 && ability < eABILITY.MAX)
                return stats[(int)ability];

            return null;
        }
        public LevelBasedBigInteger LBCost(eABILITY ability)
        {
            if (ability >= 0 && ability < eABILITY.MAX)
                return costs[(int)ability];

            return null;
        }
        public BigInteger Cost(eABILITY eType, int level)
        {
            Assert.IsTrue(level > 0);
            if (eType >= 0 && eType < eABILITY.MAX)
                return costs[(int)eType].Value(level);
            Assert.IsTrue(false);
            return 0;
        }


        // Runtime values.
        //
        //public bool IsOpened { get; set; } = false;
        //public bool IsVisible { get; set; } = false;
        //public int[] Level { get; set; } = new int[(int)eSTAT.MAX] { 1, 1, 1 };
        //public Sprite SpriteIcon { get; set; } = null;
        
        public override void Convert()
        {
            base.Convert();

            string[] performanceData = { miningRate, shipSpeed, cargoSize, shotAccuracy, shotInterval };
            for (int q = 0; q < stats.Length; ++q)
            {
                if (!string.IsNullOrEmpty(performanceData[q]))
                    stats[q] = new LevelBasedFloat(performanceData[q]);
                else
                    stats[q] = new LevelBasedFloat(_defaultValue: 1.0f, _increasePercent: 0, _increaseBase: .0f);
            }

            string[] costData = { miningCost, shipCost, cargoCost, shotAccuracyCost, shotIntervalCost };
            for (int q = 0; q < costs.Length; ++q)
            {
                if (!string.IsNullOrEmpty(costData[q]))
                    costs[q] = new LevelBasedBigInteger(costData[q]);
                else
                    costs[q] = new LevelBasedBigInteger(_defaultValue: BigInteger.Zero, _increasePercent: 0, _increaseBase: 0);
            }

            for (int q = 0; q < obtainables.Count; ++q)
                obtainables[q].Convert();
        }

#if UNITY_EDITOR
        public void SetUpgradeStats(string mingRate, string shipSpeed, string cargoSize, string shotAccuracy, string shotInterval)
        {
            this.miningRate = mingRate;
            this.shipSpeed = shipSpeed;
            this.cargoSize = cargoSize;
            this.shotAccuracy = shotAccuracy;
            this.shotInterval = shotInterval;
        }
        public void SetUpgradeCosts(string mingCost, string shipCost, string cargoCost, string shotAccuracyCost, string shotIntervalCost)
        {
            this.miningCost = mingCost;
            this.shipCost = shipCost;
            this.cargoCost = cargoCost;
            this.shotAccuracyCost = shotAccuracyCost;
            this.shotIntervalCost = shotIntervalCost;
        }
#endif
    }



    [Serializable]
    public class PlanetZoneData
    {
        [SerializeField] int zoneId;
        [SerializeField] List<PlanetData> planets;

        public PlanetZoneData() { }
        public PlanetZoneData(int zoneId, List<PlanetData> data)
        {
            this.zoneId = zoneId;   
            this.planets = data;
            this.Convert();
        }


        // Accessor.
        public int ZoneId => zoneId;
        public List<PlanetData> Planets => planets;

        public void Convert()
        {
            for (int q = 0; q < Planets.Count; ++q)
                Planets[q].Convert();
        }
        public PlanetData GetPlanetData(int planetId)
        {
            for(int q = 0; q < planets.Count; ++q)
            {
                if(planets[q].Id == planetId)
                    return planets[q];  
            }
            return null;
        }
        public PlanetData GetNeighborPlanetData(int planetId, bool up, bool isCircle)
        {
            for(int q = 0; q < planets.Count; ++q)
            {
                if(planets[q].Id == planetId)
                {
                    if(up)      
                    {
                        if(isCircle)    return q+1>=planets.Count ? planets[0] : planets[q+1];
                        else            return q+1>=planets.Count ? null : planets[q+1];
                    }
                    else
                    {
                        if(isCircle)    return q==0 ? planets[ planets.Count-1 ] : planets[q-1];
                        else            return q==0 ? null : planets[q-1];
                    }
                }
            }
            return null;
        }
    }

    [Serializable]
    public class PlanetZoneGroup
    {
        [SerializeField] List<PlanetZoneData> data;

        // Accessor.
        public List<PlanetZoneData> Data => data;

        public void Convert()
        {
            for (int q = 0; q < Data.Count; ++q)
                Data[q].Convert();
        }
        public PlanetZoneData GetZoneData(int zoneId)
        {
            for(int q = 0; q < data.Count; ++q)
            {
                if(data[q].ZoneId == zoneId) 
                    return data[q];
            }
            return null;
        }
        public PlanetZoneData GetNeighborZoneData(int zoneId, bool up, bool isCircle)
        {
            for(int q = 0; q < data.Count; ++q)
            {
                if(data[q].ZoneId == zoneId)
                {
                    if(up)      
                    {
                        if(isCircle)    return q+1>=data.Count ? data[0] : data[q+1];
                        else            return q+1>=data.Count ? null : data[q+1];
                    }
                    else
                    {
                        if(isCircle)    return q==0 ? data[ data.Count-1 ] : data[q-1];
                        else            return q==0 ? null : data[q-1];
                    }
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public void AddZoneData(PlanetZoneData zoneData)
        {
            if(data == null)
                data = new List<PlanetZoneData>();

            data.Add(zoneData);
        }
#endif
    }
}
