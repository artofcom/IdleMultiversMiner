
using System.Numerics;
using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;

namespace App.GamePlay.IdleMiner
{
    // Material -> Component -> Item 
    public enum eRscStageType { MATERIAL = 0, COMPONENT, ITEM, eMax };

    [Serializable]
    public class ResourceInfo : IBigIntegerConverter
    {
        [SerializeField] string id;
        [SerializeField] string price;
        

        // Accessor.
        public string Id => id;
        public string Price { get => price; set => price = value; }


        // Runtime Data.
        // public Sprite SpriteIcon { get; set; } = null;
        public BigInteger BIPrice { get; private set; }
        public eRscStageType eLevel { get; set; }

        public ResourceInfo(string id)
        {
            this.id = id;
            this.price = "0";
        }
#if UNITY_EDITOR
        public void SetId(string id) { this.id = id; }
#endif
        public void Convert()
        {
            BigInteger biPrice;
            bool ret = BigInteger.TryParse(Price, out biPrice);
            UnityEngine.Assertions.Assert.IsTrue(ret);
            if (ret)    BIPrice = biPrice;
        }
        public string GetSpriteGroupId()
        {
            switch(eLevel)
            {
                case eRscStageType.MATERIAL:    return "RSC-material";
                case eRscStageType.COMPONENT:   return "RSC-component";
                case eRscStageType.ITEM:        return "RSC-item";
                default:
                    UnityEngine.Assertions.Assert.IsTrue(false, "Undefinded resource type !!! " + eLevel.ToString());
                    break;
            }
            return string.Empty;
        }
        public string GetClassKey()
        {
            switch (eLevel)
            {
                case eRscStageType.MATERIAL:    return "M";
                case eRscStageType.COMPONENT:   return "C";
                case eRscStageType.ITEM:        return "I";
                default:
                    UnityEngine.Assertions.Assert.IsTrue(false, "Undefinded resource type !!! " + eLevel.ToString());
                    break;
            }
            return string.Empty;
        }
    }


    [Serializable]
    public class ResourceData
    {
        [SerializeField] List<ResourceInfo> data = new List<ResourceInfo>();

        // Accessor.
        public List<ResourceInfo> Data => data;


        public void Convert(eRscStageType _lv)
        {
            for (int q = 0; q < Data.Count; ++q)
            {
                Data[q].eLevel = _lv;
                Data[q].Convert();
            }
        }

#if UNITY_EDITOR
        public void AddResourceInfo(ResourceInfo info)
        {
            data.Add(info);
        }
#endif
    }
}
