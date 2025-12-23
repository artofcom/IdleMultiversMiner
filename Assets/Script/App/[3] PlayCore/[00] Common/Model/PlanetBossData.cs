using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Core.Utils;
using System.Numerics;

#pragma warning disable CS0108
namespace App.GamePlay.IdleMiner.Common.Model
{
    [Serializable]
    public class PlanetBossData : PlanetData
    {
        public const string KEY = "Boss";

        [SerializeField] protected long life;
        [SerializeField] protected int battleDuration, coolTime;

        // Accessor.
        public long Life => life;
        public int BattleDuration => battleDuration;    
        public int CoolTime => coolTime;

        public override string Type { get => KEY; }
    }


    [Serializable]
    public class PlanetZoneBossData
    {
        [SerializeField] int zoneId;
        [SerializeField] List<PlanetBossData> planets;

        public PlanetZoneBossData() { }
        public PlanetZoneBossData(int zoneId, List<PlanetBossData> data)
        {
            this.zoneId = zoneId;
            this.planets = data;
            this.Convert();
        }

        // Accessor.
        public int ZoneId => zoneId;
        public List<PlanetBossData> Planets => planets;

        public void Convert()
        {
            for (int q = 0; q < Planets.Count; ++q)
                Planets[q].Convert();
        }
        public PlanetBossData GetPlanetData(int planetId)
        {
            for(int q = 0; q < planets.Count; ++q)
            {
                if(planets[q].Id == planetId)
                    return planets[q];  
            }
            return null;
        }
    }

    [Serializable]
    public class PlanetZoneBossGroup
    {
        [SerializeField] List<PlanetZoneBossData> data;

        // Accessor.
        public List<PlanetZoneBossData> Data => data;

        public void Convert()
        {
            for (int q = 0; q < Data.Count; ++q)
                Data[q].Convert();
        }
        public PlanetZoneBossData GetZoneData(int zoneId)
        {
            for(int q = 0; q < data.Count; ++q)
            {
                if(data[q].ZoneId == zoneId) 
                    return data[q];
            }
            return null;
        }


#if UNITY_EDITOR
        public void AddZoneData(PlanetZoneBossData zoneData)
        {
            if(data == null)
                data = new List<PlanetZoneBossData>();

            data.Add(zoneData);
        }
#endif
    }
}
