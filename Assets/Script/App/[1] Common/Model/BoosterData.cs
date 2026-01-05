
using System.Numerics;
using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner
{
    public enum eBoosterType
    {
        MINING_RATE, DELIVERY_SPEED, CARGO_SIZE, SHOT_INTERVAL, SHOT_ACCURACY,
        CASH, CRAFT_ALL_BOOST, CRAFT_STAGE0_BOOST, CRAFT_STAGE1_BOOST, TIME_JUMP, 
        MAX
    };

    [Serializable]
    public class BoosterInfo
    {
        [SerializeField] string id;
        [SerializeField] string name;
        [SerializeField] string desc;
        [SerializeField] float boostingRate;
        [SerializeField] eBoosterType type;
        [SerializeField] int duration;  // in sec.
        [SerializeField] int cost;
        [SerializeField] eCurrencyType costCurrencyType;
        [SerializeField] string spriteKey;


        // Accessor.
        public string Id => id;
        public string Name => name;
        public string Desc => desc;
        public float BoostingRate => boostingRate;
        public eBoosterType Type => type;
        public int Duration => duration;
        public int Cost => cost;
        public eCurrencyType CostCurrencyType => costCurrencyType;
        public string SpriteKey => spriteKey;
    }



    [Serializable]
    public class BoosterData
    {
        [SerializeField] List<BoosterInfo> boostersInfo;

        // Accessor.
        public List<BoosterInfo> BoostersInfo => boostersInfo;
    }

}
