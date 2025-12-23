
using System.Numerics;
using UnityEngine;
using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class RecruitProductInfo
    {
        [SerializeField] string id;
        [SerializeField] string name;
        [SerializeField] string desc;
        [SerializeField] string level;
        [SerializeField] CurrencyAmount cost;

        public string Id => id;
        public string Name => name;
        public string Desc => desc;
        public CurrencyAmount Cost => cost;
        public int MinLevel => minLevel;
        public int MaxLevel => maxLevel;

        int minLevel, maxLevel;
        public void Convert()
        {
            Cost.Init();
            Assert.IsTrue(!string.IsNullOrEmpty(level));
            if(level.Contains("-"))
            {
                string[] data = level.Split("-");
                Assert.IsTrue(data.Length == 2);

                bool ret = int.TryParse(data[0], out minLevel);
                Assert.IsTrue(ret);
                ret = int.TryParse(data[1], out maxLevel);
                Assert.IsTrue(ret);
            }
            else
            {
                bool ret = int.TryParse(level, out minLevel);
                Assert.IsTrue(ret);
                maxLevel = minLevel;
            }
        }
    }

    [Serializable]
    public class RecruitInfo
    {
        [SerializeField] List<RecruitProductInfo> products;

        public List<RecruitProductInfo> Products => products;

        public void Convert()
        {
            for (int q = 0; q < Products.Count; ++q)
                Products[q].Convert();
        }
    }

    [Serializable]
    public class ManagerInfo
    {
        public const int MAX_LEVEL = 5;

        [SerializeField] string Id;
        [SerializeField] string Name;
        [SerializeField] int Level;
        [SerializeField] string spriteKey;
        [SerializeField] List<float> BuffMiningRate;
        [SerializeField] List<float> BuffFireIntervalRate;
        [SerializeField] List<float> BuffFireAccuracyRate;
        [SerializeField] List<float> BuffShipSpeedRate;
        [SerializeField] List<float> BuffCargoRate;

        // Accessor.
        public string Id_ => Id;
        public string Name_ => Name;
        public int Level_ => Level;
        public List<float> BuffMiningRate_ => BuffMiningRate;
        public List<float> BuffFireIntervalRate_ => BuffFireIntervalRate;
        public List<float> BuffFireAccuracyRate_ => BuffFireAccuracyRate;
        public List<float> BuffShipSpeedRate_ => BuffShipSpeedRate;
        public List<float> BuffCargoRate_ => BuffCargoRate;
        public string SpriteKey => spriteKey;

        // Runtime
        public SpriteRenderer ImageManager { get; set; }
    }

    [Serializable]
    public class ManagerSlotInfo
    {
        [SerializeField] string costData;
        [SerializeField] int costType;

        public int CostType => costType;

        LevelBasedInt cost = null;

        public void Convert()
        {
            cost = new LevelBasedInt(costData);
        }
        public int GetCost(int targetSlotCount)
        {
            Assert.IsNotNull(cost);
            if(targetSlotCount <= 0)
                targetSlotCount = 1;
            
            return cost.Value(targetSlotCount);
        }
    }

    [Serializable]
    public class ManagerData
    {
        [SerializeField] List<ManagerInfo> managerInfo;
        [SerializeField] int requiredManagerCountToPromoteLv2;
        [SerializeField] int requiredManagerCountToPromoteLv3;
        [SerializeField] int requiredManagerCountToPromoteLv4;
        [SerializeField] int requiredManagerCountToPromoteLv5;
        [SerializeField] RecruitInfo recruitInfo;
        [SerializeField] ManagerSlotInfo slotInfo;

        // Accessor.
        public List<ManagerInfo> ManagerInfo => managerInfo;
        public int RequiredManagerCountToPromoteLv2 => requiredManagerCountToPromoteLv2;
        public int RequiredManagerCountToPromoteLv3 => requiredManagerCountToPromoteLv3;
        public int RequiredManagerCountToPromoteLv4 => requiredManagerCountToPromoteLv4;
        public int RequiredManagerCountToPromoteLv5 => requiredManagerCountToPromoteLv5;
        public RecruitInfo RecruitInfo => recruitInfo;
        public ManagerSlotInfo SlotInfo => slotInfo;

        public void Convert()
        {
            recruitInfo.Convert();
            slotInfo.Convert();
        }
    }
}
