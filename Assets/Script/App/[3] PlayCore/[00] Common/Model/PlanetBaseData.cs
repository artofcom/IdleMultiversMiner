
using System.Numerics;
using UnityEngine;
using Core.Utils;
using System.Collections.Generic;
using Core.Events;
using System;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Common.Model
{
    [Serializable]
    public class ObtainStat
    {
        //public const string EVENT_OBTAINED = "OnOnMaterialObtained";

        [SerializeField] string resourceId;
        [SerializeField] float yield;           // ex) 0.8 ( 0 ~ 1.0 )

        // constructor.
        public ObtainStat() { }
        public ObtainStat(string resourceId, float yield)
        {
            this.resourceId = resourceId;
            this.yield = yield;            
        }

        // Accessor.
        public string ResourceId => resourceId;
        public float Yield => yield;

        public void Convert()
        {
            yield = (float)(Math.Round(yield, 2));
        }
    }

    [Serializable]
    public class PlanetBaseData : IBigIntegerConverter
    {
        // Data Fields.
        [SerializeField] protected int id;                  // 1 ~ 
        [SerializeField] protected string name;             // ABC
        [SerializeField] protected string openCost;
        
        protected int zoneId;                               // 100, 200 ~ 
        protected string type;
        // Accessor.
        public int ZoneId => zoneId;
        public int Id => id;                 
        public string Name => name;          
        public string OpenCost => openCost;

        public virtual string Type { get => "Base"; }

        // RunTime Values.
        public BigInteger BIOpenCost { get; private set; }

#if UNITY_EDITOR
        public void SetOpenCost(string openCost) { this.openCost = openCost; }
#endif

        public virtual void Convert() 
        {
            BigInteger biTempCost;
            if(BigInteger.TryParse(openCost, out biTempCost))
                BIOpenCost =  biTempCost;
            type = Type;
        }
    }
}
