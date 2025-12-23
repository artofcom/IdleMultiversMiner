
using System.Numerics;
using UnityEngine;
using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner
{

    public class ManagerModel : IGCore.MVCS.AModel
    {
        public ManagerData ManagerData { get; private set; }

        public ManagerModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

        public override void Init()
        {
            /*
            var textData = Resources.Load<TextAsset>(GAMEDATA_PATH + "ManagerData");
            ManagerData = JsonUtility.FromJson<ManagerData>(textData.text);
            //Assert.IsTrue(ManagerData.ManagerInfo.Count == 3);
            //Assert.IsTrue(ManagerData.ManagerInfo[0].Id_ == "ManagerA");
            Assert.IsTrue(ManagerData.RecruitInfo.Products.Count > 0);

            ManagerData.Convert();*/
        }



        public int GetRequiredOtherCardToPromote(int level)
        {
            Assert.IsTrue(level>=1 && level<=5);

            switch(level)
            {
            case 1: return ManagerData.RequiredManagerCountToPromoteLv2;
            case 2: return ManagerData.RequiredManagerCountToPromoteLv3;
            case 3: return ManagerData.RequiredManagerCountToPromoteLv4;
            case 4: return ManagerData.RequiredManagerCountToPromoteLv5;
            default:
                break;
            }
            Assert.IsTrue(false, "Unsupported level to promote !!! " + level);
            return 0;
        }

        public bool Promote(string ownedMngId, List<string> sourceOwnedManageIds)
        {
            /*
            var ownedManager = PlayerData.GetOwnedManagerInfo(ownedMngId);
            if(ownedManager == null)
            {
                Debug.LogWarning("Can't find manager in the Player owned Manager group...." + ownedMngId);
                return false;
            }
            for(int q = 0; q < sourceOwnedManageIds.Count; ++q)
            {
                if(sourceOwnedManageIds[q] == ownedMngId)
                {
                    Debug.LogWarning("Can't use the manager want to promote as a source.! " + ownedMngId);
                    return false;
                }
                if(PlayerData.GetOwnedManagerInfo(sourceOwnedManageIds[q]) == null)
                {
                    Debug.LogWarning("Don't have the source manager ! " + sourceOwnedManageIds[q]);
                    return false;
                }
            }

            var managerInfo = GetManagerInfo( ownedManager.ManagerId );
            Assert.IsNotNull(managerInfo, "Invalid Manager Id ! " + ownedMngId);

            int reqSrcCount;
            switch(ownedManager.Level)
            {
                case 1: reqSrcCount = ManagerData.RequiredManagerCountToPromoteLv2; break;
                case 2: reqSrcCount = ManagerData.RequiredManagerCountToPromoteLv3; break;
                case 3: reqSrcCount = ManagerData.RequiredManagerCountToPromoteLv4; break;
                case 4: reqSrcCount = ManagerData.RequiredManagerCountToPromoteLv5; break;
                default:
                    return false;
            }

            if(sourceOwnedManageIds.Count < reqSrcCount)
            {
                Debug.Log("Source Manager count is Not sufficient!");
                return false;
            }

            for(int q = 0; q < reqSrcCount; ++q)
                PlayerData.DiscardManager(sourceOwnedManageIds[q]);

            PlayerData.PromoteManager(ownedMngId);*/
            return true;
        }


        public ManagerInfo GetManagerInfo(string managerId)
        {
            for(int q = 0; q < ManagerData.ManagerInfo.Count; ++q)
            {
                if (ManagerData.ManagerInfo[q].Id_ == managerId)
                    return ManagerData.ManagerInfo[q];
            }
            return null;
        }

        public OwnedManagerInfo RecruitManager(CurrencyAmount cost, int minLevel, int maxLevel)
        {
            /*
            bool canBuy = PlayerData.IsAffordable(cost);
            if(!canBuy)
            {
                Debug.Log("Not enough money to recruit manager...");
                return null;
            }

            // Select Target Index within target level.
            // 
            List<int> indices = new List<int>();
            for(int q = 0; q < ManagerData.ManagerInfo.Count; ++q)
            {
                if(ManagerData.ManagerInfo[q].Level_>=minLevel && ManagerData.ManagerInfo[q].Level_<=maxLevel)
                    indices.Add(q);
            }
            if(indices.Count == 0)
            {
                Assert.IsTrue(false, $"Couldn't find manager in the target range...[{minLevel}]-[{maxLevel}]");
                return null;
            }

            int idx = indices[ UnityEngine.Random.Range(0, indices.Count) ];
            int level = ManagerData.ManagerInfo[idx].Level_;
            return PlayerData.RecruiteManager(cost, ManagerData.ManagerInfo[idx].Id_, level);
            */
            return null;// false;
        }

        public RecruitProductInfo GetRecruitProductInfo(string productId)
        {
            for(int q = 0; q < ManagerData.RecruitInfo.Products.Count; ++q)
            {
                if (ManagerData.RecruitInfo.Products[q].Id == productId)
                    return ManagerData.RecruitInfo.Products[q];
            }
            return null;
        }

        public bool TryPurchaseManagerSlot(CurrencyAmount cost)
        {
            /*
            bool canBuy = PlayerData.IsAffordable(cost);
            if(!canBuy)
            {
                Debug.Log("Not enough money to buy manager slot...");
                return false;
            }

            PlayerData.PurchaseManagerSlot(cost);*/
            return true;
        }    
        
       /// public float GetManagerBuff(int planetId, eABILITY ability)
       // {
            // Manager.
           /* OwnedManagerInfo ownedMngInfo = PlayerData.GetAssignedManagerInfoForPlanet(planetId);
            ManagerInfo mngInfo = ownedMngInfo==null ? null : GetManagerInfo(ownedMngInfo.ManagerId);
            int levelidx = mngInfo==null ? -1 : ownedMngInfo.Level - 1;
            if(levelidx >= 0)
            {
                if(levelidx >= ManagerInfo.MAX_LEVEL)
                {
                    Assert.IsTrue(false, "Invalid Level index ! : " + levelidx.ToString());
                    return 1.0f;
                }
            }

            switch(ability)
            {
            case eABILITY.MINING_RATE:
                return levelidx>=0 ? mngInfo.BuffMiningRate_[levelidx] : 1.0f;
            case eABILITY.DELIVERY_SPEED:
                return levelidx>=0 ? mngInfo.BuffShipSpeedRate_[levelidx] : 1.0f;
            case eABILITY.CARGO_SIZE:
                return levelidx>=0 ? mngInfo.BuffCargoRate_[levelidx] : 1.0f;
            case eABILITY.SHOT_INTERVAL:
                return levelidx>=0 ? mngInfo.BuffFireIntervalRate_[levelidx] : 1.0f;
            case eABILITY.SHOT_ACCURACY:
                return levelidx>=0 ? mngInfo.BuffFireAccuracyRate_[levelidx] : 1.0f;
            default:
                return 1.0f;
            }
           */
       //    return .0f;
       // }
    }
}
