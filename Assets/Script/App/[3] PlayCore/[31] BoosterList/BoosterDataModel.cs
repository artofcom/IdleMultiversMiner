
using System.Numerics;
using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class BoosterModel : IGCore.MVCS.AModel// IdleMinerModel
    {
        public BoosterData BoosterData { get; private set; }


        public BoosterModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

        public override void Init(object data = null)
        {
            base.Init(data);

            /*
           // var textData = "";// Resources.Load<TextAsset>(GAMEDATA_PATH + "BoosterData");
            BoosterData = "";// JsonUtility.FromJson<BoosterData>(textData.text);

            // Note : We Assume that we don't use mining coin here. - mostly IAP coin.
            for (int q = 0; q < BoosterData.BoostersInfo.Count; ++q)
            {
                Assert.IsTrue(BoosterData.BoostersInfo[q].CostCurrencyType != eCurrencyType.MINING_COIN);
            }*/
        }


        public BoosterInfo GetBoosterInfo(string boosterId)
        {
            for(int q = 0; q < BoosterData.BoostersInfo.Count; ++q)
            {
                if(BoosterData.BoostersInfo[q].Id == boosterId)
                    return BoosterData.BoostersInfo[q];
            }
            return null;
        }

        public bool UseBooster(string boosterId)
        {
            for (int q = 0; q < BoosterData.BoostersInfo.Count; ++q)
            {
             //   if (BoosterData.BoostersInfo[q].Id == boosterId)
             //       return PlayerData.UseBooster(BoosterData.BoostersInfo[q]);
            }

            return false;
        }

        public float GetUsingBoosterValueByType(eBoosterType eType, int planetId)
        {
            if(planetId <= 0)
            {
                Assert.IsTrue(false, "Planet Id should be valid.");
                return 1.0f;
            }

            var usingBoosterInfo = GetUsingBoosterByType(eType);
            if(usingBoosterInfo == null)
                return 1.0f;

            var info = GetBoosterInfo(usingBoosterInfo.BoosterId);
            if(info == null)
                return 1.0f;

            float fRet = 1.0f;
            if(info.Type == eType)
            {
                if((usingBoosterInfo.PlanetId > 0 && usingBoosterInfo.PlanetId == planetId ) ||     // need to match.
                    usingBoosterInfo.PlanetId <= 0 )                                                // don't care.
                {
                    fRet *= info.BoostingRate;
                }
            }
            return fRet;
        }

        public UsingBoosterInfo GetUsingBoosterByType(eBoosterType eType)
        {
            /*string boosterId;
            for(int q = 0; q < PlayerData.UsingBoostersInfo.Count; ++q)
            {
                UsingBoosterInfo usingBooster = PlayerData.UsingBoostersInfo[q];
                boosterId = usingBooster.BoosterId;
                var info = GetBoosterInfo(boosterId);
                if(info == null)
                    continue;

                if(info.Type == eType)      // returns only one.
                    return usingBooster;
            }*/
            return null;
        }
    }
}
