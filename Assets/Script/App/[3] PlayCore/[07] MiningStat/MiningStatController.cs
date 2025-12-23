using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using Core.Util;
using System;
using UnityEngine.Assertions;
using Core.Utils;
using System.Numerics;
using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.Resouces;

namespace App.GamePlay.IdleMiner.MiningStat
{
    //  Town Management.-------------------------------------
    //
    public class MiningStatController : IGCore.MVCS.AController// AMinerModule
    {
        #region ===> Properties

        enum LV_MODE { LV_1, LV_5, LV_10, MAX };
        const float BIT_OF_DELAY = 1.0f;        
        //const float MIN_SHOT_INTERVAL = 0.1f;

        protected EventsGroup events = new EventsGroup();

        MiningStatModel Model => (MiningStatModel)model;
        MiningStatView View => (MiningStatView)view;
        IdleMinerContext IMContext => (IdleMinerContext)context;

        int curZoneId
        {
            get
            {
                int defaultZoneId       = 0;
                object zoneId           = IMContext.GetData("CurrentZoneId");
                if(zoneId == null)
                {
                    defaultZoneId       = (int)IMContext.GetData("DefaultZoneId", 100);
                    IMContext.AddData("CurrentZoneId", defaultZoneId);
                }
                return zoneId!=null? (int)zoneId : defaultZoneId;
            }
        }
        int curPlanetId = -1;        
        LV_MODE LevelMode = LV_MODE.LV_1;
        EventsGroup Events => IMContext.EventGroup;

        #endregion ===> Properties



        
        #region ===> Initiailization.

        public MiningStatController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
            : base(view, model, ctx)
        { }
        

        public override void Init()
        {
            Events.RegisterEvent(EventID.MINING_STAT_UPGRADED, PlayerData_OnStatupgraded);
            Events.RegisterEvent(EventID.GAME_CURRENCY_UPDATED, PlayerData_OnMondyUpdated);

            Events.RegisterEvent(EventID.MINING_STAT_RESET, PlayerData_OnStatReset);
            Events.RegisterEvent(EventID.PLANET_DAMAGED, PlayerData_OnPlanetAttacked);

            Events.RegisterEvent(EventID.GAME_RESET_REFRESH, OnRefreshAllView);

            MiningStatView.EventOnUpgradeClicked += PlanetPanelView_OnStatUpgradeClicked;
            MiningStatView.EventOnStatResetClicked += PlanetPanelView_OnStatResetClicked;
            MiningStatView.EventOnUpgradeLevelModeClicked += PlanetPanelView_OnUpgradeLevelModeClicked;
            MiningStatView.EventOnBtnResourceShortCutClicked += PlanetPanelView_OnShortCutToResourceClicked;
            MiningStatView.EventOnBtnManagerShortCutClicked += PlanetPanelView_OnShortCutToManagerClicked;

            View.EventOnBtnBrowseToLeft += MiningStatView_OnPlanetBrowseToLeft;
            View.EventOnBtnBrowseToRight += MiningStatView_OnPlanetBrowseToRight;

            Events.RegisterEvent(MinedResourceInfo.EVENT_RSC_MINED, OnMaterialObtained);

            Debug.Log("[InitSeq]:[MiningStatController] InitCtrler...");

#if UNITY_EDITOR
            // TestMingPumpResumeLogic();
#endif
        }

        public override void Dispose()
        {
            base.Dispose();

            Events.UnRegisterEvent(EventID.MINING_STAT_UPGRADED, PlayerData_OnStatupgraded);
            Events.UnRegisterEvent(EventID.GAME_CURRENCY_UPDATED, PlayerData_OnMondyUpdated);

            Events.UnRegisterEvent(EventID.MINING_STAT_RESET, PlayerData_OnStatReset);
            Events.UnRegisterEvent(EventID.PLANET_DAMAGED, PlayerData_OnPlanetAttacked);

            Events.UnRegisterEvent(EventID.GAME_RESET_REFRESH, OnRefreshAllView);
 

            MiningStatView.EventOnUpgradeClicked -= PlanetPanelView_OnStatUpgradeClicked;
            MiningStatView.EventOnStatResetClicked -= PlanetPanelView_OnStatResetClicked;
            MiningStatView.EventOnUpgradeLevelModeClicked -= PlanetPanelView_OnUpgradeLevelModeClicked;
            MiningStatView.EventOnBtnResourceShortCutClicked -= PlanetPanelView_OnShortCutToResourceClicked;
            MiningStatView.EventOnBtnManagerShortCutClicked -= PlanetPanelView_OnShortCutToManagerClicked;

            View.EventOnBtnBrowseToLeft -= MiningStatView_OnPlanetBrowseToLeft;
            View.EventOnBtnBrowseToRight -= MiningStatView_OnPlanetBrowseToRight;

            Events.UnRegisterEvent(MinedResourceInfo.EVENT_RSC_MINED, OnMaterialObtained);
        }

        protected override void OnViewEnable()
        {
            object objPlanetId = context.GetData("CurrentPlanetId");
            if(objPlanetId != null)
                curPlanetId = (int)objPlanetId;
            else 
                context.AddData("CurrentPlanetId", -1);

            curPlanetId = curPlanetId < 0 ? 1 : curPlanetId;

            RefreshPanelView(curZoneId, curPlanetId, MiningStatView.SECTION.ALL);
        }
        
        protected override void OnViewDisable()
        {
            curPlanetId = -1;
            context.UpdateData("CurrentPlanetId", -1);
        }
        
        public override void Resume(int duration)
        {
            if(View.gameObject.activeSelf)
                RefreshPanelView(MiningStatView.SECTION.RESOURCE);
        }

        public override void Pump()
        {
            if(View.gameObject.activeSelf)
                RefreshPanelView(MiningStatView.SECTION.ALL);
        }      

        public override void WriteData()
        {
            Model.PlayerData.WriteData();
        }

        #endregion ===> Initiailization.




        //============================================================================================//
        //
        #region ===> Event Handler
        //
        void PlayerData_OnStatupgraded(object data)
        {
            int zoneId = ((Tuple<int, int>)data).Item1;
            int planetId = ((Tuple<int, int>)data).Item2;

            RefreshPanelView(zoneId, planetId);
        }

        void PlayerData_OnStatReset(object data)
        {
            int zoneId = ((Tuple<int, int>)data).Item1;
            int planetId = ((Tuple<int, int>)data).Item2;

            RefreshPanelView(zoneId, planetId, MiningStatView.SECTION.PERFORMANCE);
        }
        void PlayerData_OnMondyUpdated(object data)
        {
            // data should be null.
            RefreshPanelView(curZoneId, curPlanetId, MiningStatView.SECTION.PERFORMANCE);
        }

        void PlayerData_OnPlanetAttacked(object data)
        {
            // No Updates required due to the updates from Pump() that's happening every 1 sec.
        }

        void OnRefreshAllView(object data)
        {
            if(View.gameObject.activeSelf)
                RefreshPanelView(MiningStatView.SECTION.ALL);
        }

        void PlanetPanelView_OnStatUpgradeClicked(Tuple<int, int, eABILITY> tupData)
        {
            if (tupData == null)
                return;

            int offsetLv = 1;
            switch(LevelMode)
            {
                case LV_MODE.LV_5:  offsetLv = 5; break;
                case LV_MODE.LV_10: offsetLv = 10; break;
            }

            int zoneId = tupData.Item1;
            int planetId = tupData.Item2;
            eABILITY ability = tupData.Item3;
            context.RequestQuery("GamePlay", "UpgradePlanet", (errMsg, ret) => { }, zoneId, planetId, ability, offsetLv);
        }
        void PlanetPanelView_OnUpgradeLevelModeClicked()
        {
            switch(LevelMode)
            {
                case LV_MODE.LV_1:  LevelMode = LV_MODE.LV_5;   break;
                case LV_MODE.LV_5:  LevelMode = LV_MODE.LV_10;  break;
                case LV_MODE.LV_10: LevelMode = LV_MODE.LV_1;   break;
                default:    return;
            }
            RefreshPanelView(MiningStatView.SECTION.PERFORMANCE);
        }
        void PlanetPanelView_OnShortCutToResourceClicked()
        {
        }
        void PlanetPanelView_OnShortCutToManagerClicked()
        {
        }
        void PlanetPanelView_OnStatResetClicked(Tuple<int, int, eABILITY> tupData)
        {
            if (tupData == null)
                return;

            int zoneId = tupData.Item1;
            int planetId = tupData.Item2;
            eABILITY ability = tupData.Item3;

            context.RequestQuery("GamePlay", "ResetPlanetStat", (errMsg, ret) => { }, zoneId, planetId, ability);
            // Model.ResetPlanetStat(tupData.Item1, tupData.Item2); 
        }
        void MiningStatView_OnPlanetBrowseToLeft()
        {
            const bool isUpper = false;
            const bool isIncludeBattleCleared = false;
            context.RequestQuery("GamePlay", "GetNeighborPlanetId", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                
                if(ret != null)
                {
                    var id = (Tuple<int, int>)ret;
                    IMContext.UpdateData("CurrentZoneId", id.Item1);
                    curPlanetId = id.Item2;
                }
            }, curZoneId, curPlanetId, isUpper, isIncludeBattleCleared);

            RefreshPanelView(curZoneId, curPlanetId, MiningStatView.SECTION.ALL);
        }
        void MiningStatView_OnPlanetBrowseToRight()
        {
            const bool isUpper = true;
            const bool isIncludeBattleCleared = false;
            context.RequestQuery("GamePlay", "GetNeighborPlanetId", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));

                if(ret != null)
                {
                    var id = (Tuple<int, int>)ret;
                    IMContext.UpdateData("CurrentZoneId", id.Item1);
                    curPlanetId = id.Item2;
                }
            }, curZoneId, curPlanetId, isUpper, isIncludeBattleCleared);

            RefreshPanelView(curZoneId, curPlanetId, MiningStatView.SECTION.ALL);
        }

        void OnMaterialObtained(object data)
        {
            // data should be null.
            RefreshPanelView(curZoneId, curPlanetId, MiningStatView.SECTION.RESOURCE);
        }

        #endregion ===> Event Handler






        
        #region ===> View Refrehers.

        protected void RefreshPanelView(MiningStatView.SECTION eSectionType)
        {
            this.RefreshPanelView(curZoneId, curPlanetId, eSectionType);
        }
        protected void RefreshPanelView(int zoneId, int planetId, MiningStatView.SECTION eSectionType = MiningStatView.SECTION.ALL)
        {
            if (View == null || !View.gameObject.activeSelf)
                return;     // Hasn't ready.

            PlanetData planetData = null;
            context.RequestQuery("GamePlay", "GetPlanetData", (errorMsg, objResult) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errorMsg), errorMsg);
                planetData = objResult as PlanetData;

            }, zoneId, planetId);

            PlanetInfo planetInfo = null;
            context.RequestQuery("GamePlay.PlayerData", "GetVisiblePlanetInfo", (errorMsg, objResult) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errorMsg), errorMsg);
                planetInfo = objResult as PlanetInfo;

            }, zoneId, planetId);

            if (planetData == null || planetInfo == null)
                return;     // Hasn't ready.

            bool isBossBattle = planetData.Type == PlanetBossData.KEY;
            bool isPlanetClosed = false;
            bool isBattleCleared = false;
            if(isBossBattle)
            {
                var bossData = planetData as PlanetBossData;
                Assert.IsNotNull(bossData);
                isPlanetClosed = (long)bossData.BattleDuration < planetInfo.GetSecondFromEvent();
                isBattleCleared = planetInfo.BattleInfo!=null ? planetInfo.BattleInfo.IsCleared : false;
            }

            string pnlTitle = isBossBattle ? "BOSS BATLE" : "PLANETS";

            //-------------------------------------------------------//
            //
            // Planet Section
            //
            PlanetSectorComp.PresentInfo planetSection = null;
            if (eSectionType == MiningStatView.SECTION.ALL || eSectionType == MiningStatView.SECTION.PLANET)
            {
                BuildPlanetSectionInfo(ref planetSection, zoneId, planetData, planetInfo.Distance);
            }


            //-------------------------------------------------------//
            //
            // Manager Section
            //
            PlanetManagerCardComp.PresentInfo managerSection = null;
            if (eSectionType == MiningStatView.SECTION.ALL || eSectionType == MiningStatView.SECTION.MANAGER)
            {
                managerSection = BuildManagerSectionCompPresentInfo(zoneId, planetId);
            }


            //-------------------------------------------------------//
            //
            // Mining Resource Section
            //
            List<PlanetResourceItemComp.PresentInfo> miningStatList = null;
            List<PlanetDamageItemComp.PresentInfo> damageStatList = null;
            if (eSectionType == MiningStatView.SECTION.ALL || eSectionType == MiningStatView.SECTION.RESOURCE)
            {
                if(isBossBattle)
                    BuildDamageStats(ref damageStatList, zoneId, planetInfo, planetData as PlanetBossData);
                else
                    BuildMiningResourceStats(ref miningStatList, zoneId, planetInfo, planetData);
            }


            //-------------------------------------------------------//
            //
            // Mining Stat Performance Section
            //
            List<PlanetStatComp.PresentInfo> listUpgrades = new List<PlanetStatComp.PresentInfo>();
            if (eSectionType == MiningStatView.SECTION.ALL || eSectionType == MiningStatView.SECTION.PERFORMANCE)
                BuildPerformanceList(listUpgrades, zoneId, planetInfo, planetData);
            else
            {
                for (int q = 0; q < (int)eABILITY.MAX; ++q)
                    listUpgrades.Add(null);
            }

            string levelMode = "1 x";
            switch (LevelMode)
            {
                case LV_MODE.LV_5:  levelMode = "5 x";  break;
                case LV_MODE.LV_10: levelMode = "10 x"; break;
            }

            // FINALLY.
            MiningStatView.PresentInfo tdPresent = null;
            if (isBossBattle)
            {
                if (isBattleCleared)
                    tdPresent = new MiningStatView.PresentInfo(pnlTitle, planetSection, managerSection, dontcare_4BattleCleared:true);
                
                else if(isPlanetClosed)
                    tdPresent = new MiningStatView.PresentInfo(pnlTitle, planetSection, managerSection);

                else
                {

                    tdPresent = new MiningStatView.PresentInfo(pnlTitle, planetSection, managerSection,
                            miningStatList, damageStatList,
                            levelMode,
                            listUpgrades[(int)eABILITY.MINING_RATE],    // shot intensity
                            listUpgrades[(int)eABILITY.SHOT_INTERVAL],  // interval
                            listUpgrades[(int)eABILITY.SHOT_ACCURACY]); // accuracy
                }
            }
            else
            {
                tdPresent = new MiningStatView.PresentInfo( pnlTitle, planetSection, managerSection,
                        miningStatList, damageStatList,
                        levelMode,
                        listUpgrades[(int)eABILITY.SHOT_INTERVAL],  // interval
                        listUpgrades[(int)eABILITY.SHOT_ACCURACY],  // accuracy
                        listUpgrades[(int)eABILITY.DELIVERY_SPEED], // speed
                        listUpgrades[(int)eABILITY.CARGO_SIZE]);    // cargo size
            }
            
            View.RefreshSction(tdPresent, eSectionType);
            //
        }









        
        // Internal Func for Stat Query.
        float requestStatQuery(int zoneId, int planetId, eABILITY statType)
        {
            float stat = .0f;
            context.RequestQuery("GamePlay", "GetPlanetStat", (errMsg, retObj) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg), errMsg);
                if(retObj != null)
                    stat = (float)retObj;
                
            }, zoneId, planetId, (int)statType);
            return stat;
        }

      
        
        
        void BuildPlanetSectionInfo(ref PlanetSectorComp.PresentInfo planetSection, int zoneId, PlanetData planetData, float dist)
        {
            float fSIBuff = .0f;// Model.GetSkillBuffValue(eABILITY.SHOT_INTERVAL);
            float fSABuff = .0f;// Model.GetSkillBuffValue(eABILITY.SHOT_ACCURACY);
            float fDSBuff = .0f;// Model.GetSkillBuffValue(eABILITY.DELIVERY_SPEED);
            float fCSBuff = .0f;// Model.GetSkillBuffValue(eABILITY.CARGO_SIZE);

            string SIBuff = fSIBuff != 1.0f ? "x " + fSIBuff.ToString("0.00") : string.Empty;
            string SABuff = fSABuff != 1.0f ? "x " + fSABuff.ToString("0.00") : string.Empty;
            string DSBuff = fDSBuff != 1.0f ? "x " + fDSBuff.ToString("0.00") : string.Empty;
            string CSBuff = fCSBuff != 1.0f ? "x " + fCSBuff.ToString("0.00") : string.Empty;
            
            
            Sprite sprPlanet = null;
            context.RequestQuery("GamePlay", "GetPlanetSprite", (errorMsg, objResult) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errorMsg), errorMsg);
                sprPlanet = objResult as Sprite;

            }, zoneId, planetData.Id);

            planetSection = new PlanetSectorComp.PresentInfo(
                sprPlanet,
                $"[{zoneId}-{planetData.Id}]\n{planetData.Name}",
                $"{requestStatQuery(zoneId, planetData.Id, eABILITY.SHOT_INTERVAL)}/sec " + SIBuff,
                $"{requestStatQuery(zoneId, planetData.Id, eABILITY.SHOT_ACCURACY)} % " + SABuff,
                $"{requestStatQuery(zoneId, planetData.Id, eABILITY.DELIVERY_SPEED)} km/s " + DSBuff,
                $"{requestStatQuery(zoneId, planetData.Id, eABILITY.CARGO_SIZE)} ea" + CSBuff,
                //"", "", "", "",
                $"DIST : \n{dist.ToString("0.00")}km AWAY", "");
        }

        void BuildMiningResourceStats(ref List<PlanetResourceItemComp.PresentInfo> miningStatList, int zoneId, PlanetInfo planetInfo, PlanetData planetData)
        {
            Assert.IsNotNull(planetInfo);
            Assert.IsNotNull(planetData);

            // Mining At Planet Rates.
            float miningRatePerSec = requestStatQuery(zoneId, planetInfo.PlanetId, eABILITY.MINING_RATE);
            float shotInterval = requestStatQuery(zoneId, planetInfo.PlanetId, eABILITY.SHOT_INTERVAL);
            float accuracy = requestStatQuery(zoneId, planetInfo.PlanetId, eABILITY.SHOT_ACCURACY);         // 0 ~ 1

            float shotCounterPerSec = 1.0f / shotInterval;
            float miningRate = miningRatePerSec * shotCounterPerSec * accuracy;


            // Total Obtainning Rate.
            float shipSpeed = requestStatQuery(zoneId, planetInfo.PlanetId, eABILITY.DELIVERY_SPEED);
            float cargoSize = requestStatQuery(zoneId, planetInfo.PlanetId, eABILITY.CARGO_SIZE);
            // v = s/t : t = s/v
            float shipDuration = (planetInfo.Distance * 2.0f) / shipSpeed;
            float shipRate = cargoSize * (1.0f / shipDuration);

            miningStatList = new List<PlanetResourceItemComp.PresentInfo>();
            for (int q = 0; q < PlanetData.MAX_MINING; ++q)
            {
                var minedRsc = q < planetData.Obtainables.Count ? planetData.Obtainables[q] : null;
                if (minedRsc == null)
                {
                    miningStatList.Add(new PlanetResourceItemComp.PresentInfo(IMContext.GetSprite("common", "1x1")));
                    continue;
                }

                // minedRsc.ResourceId
                ResourceInfo rscInfo = null;    
                context.RequestQuery("Resource", endPoint:"GetResourceInfo", (errorMsg, ret) => 
                {
                    rscInfo = (ResourceInfo)ret;
                    Assert.IsNotNull(rscInfo, "ResourceInfo cannnot be null! " + minedRsc.ResourceId);

                }, minedRsc.ResourceId);

                if(rscInfo == null)         continue;


                MinedResourceInfo minedResourceInfo = null;
                context.RequestQuery("GamePlay", endPoint:"PlayerData.GetMinedResourceInfo",  (errorMsg, ret) => 
                { 
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    minedResourceInfo = (MinedResourceInfo)ret;

                }, zoneId, planetInfo.PlanetId, minedRsc.ResourceId);

                ResourceCollectInfo resourceCollectInfo = null;
                context.RequestQuery("Resource", endPoint:"PlayerData.GetResourceCollectionInfo", (errorMsg, ret) => 
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    resourceCollectInfo = (ResourceCollectInfo)ret;

                }, minedRsc.ResourceId);

                float rscMiningRate = miningRate * minedRsc.Yield;
                float rscShipRate = shipRate * minedRsc.Yield;
                float rscCollectRate = Math.Min(rscMiningRate, rscShipRate);

                var rscItemPresentor = new PlanetResourceItemComp.PresentInfo(
                    _icon: IMContext.GetSprite(rscInfo.GetSpriteGroupId(), spriteKey: rscInfo.Id),
                    _name: minedRsc.ResourceId,
                    _yield: $"{(int)(minedRsc.Yield * 100.0f)}",       // %
                    _miningRate: rscMiningRate.ToString("0.00"),       // /s
                    _minedCount: minedResourceInfo!=null ? minedResourceInfo.BICount.ToAbbString() : "0",
                    _collectRate: rscCollectRate.ToString("0.00"),     // /s
                    _collectedCnt: resourceCollectInfo!=null ? resourceCollectInfo.BICount.ToAbbString() : "0");

                miningStatList.Add(rscItemPresentor);
            }
        }

        void BuildDamageStats(ref List<PlanetDamageItemComp.PresentInfo> damageStatList, int zoneId, PlanetInfo planetInfo, PlanetBossData bossData)
        {
            Assert.IsNotNull(bossData);
            Assert.IsNotNull(planetInfo);

            string[] statName = new string[] { "LIFE", "DURATION" };//, "DAMAGE" };
            Assert.IsTrue(statName.Length == View.DamageSector.ItemCount);

            damageStatList = new List<PlanetDamageItemComp.PresentInfo>();
            for (int k = 0; k < View.DamageSector.ItemCount; ++k)
            {
                string detail = "aaa";
                float rate = 0.1f;
                switch (k)
                {
                    case 0:
                        {
                            BigInteger biTotal = new BigInteger(bossData.Life);
                            rate = ((float)(biTotal - planetInfo.BattleInfo.BIDamage)) / (float)biTotal;
                            detail = $"{planetInfo.BattleInfo.BIDamage.ToAbbString()}/{biTotal.ToAbbString()}";
                            break;
                        }
                    case 1:
                        {
                            long diff = (long)bossData.BattleDuration - planetInfo.GetSecondFromEvent();
                            rate = (float)diff / ((float)bossData.BattleDuration);
                            detail = $"{diff}s/{bossData.BattleDuration}s";
                            break;
                        }
                    case 2:
                        break;
                    default:
                        continue;
                }

                var damageSectorPresentor = new PlanetDamageItemComp.PresentInfo(statName[k], rate, detail);
                damageStatList.Add(damageSectorPresentor);
            }
        }

        void BuildPerformanceList(List<PlanetStatComp.PresentInfo> listUpgrades, int zoneId, PlanetInfo planetInfo, PlanetData planetData)
        {
            Assert.IsNotNull(listUpgrades);
            Assert.IsNotNull(planetInfo);
            Assert.IsNotNull(planetData);

            string[] statStrings = new string[] { "SHOT INTENSITY"/*"MINING RATE"*/, "SHIP SPEED", "CARGO SIZE", "SHOT ACCURACY", "SHOT INTERVAL", "SHOT COUNT" };
            string[] unitString = new string[] { "/s", "km/s", "/time", "%", "/s", "/s" };
          
            for (int q = 0; q < (int)eABILITY.MAX; ++q)
            {
                int level = planetInfo.Level[q];
                BigInteger nextLevelCost = planetData.Cost((eABILITY)q, level + 1);
                BigInteger cost = BigInteger.Zero;
                if (LevelMode != LV_MODE.LV_1)
                {
                    int count = LevelMode == LV_MODE.LV_5 ? 5 : 10;
                    for (int k = 1; k <= count; ++k)
                        cost += planetData.Cost((eABILITY)q, level + k);
                }
                else cost = nextLevelCost;

                string format = q == (int)eABILITY.CARGO_SIZE ? "0" : "0.00";
                float fUnitRate = 1.0f; // q == (int)eABILITY.SHOT_ACCURACY ? 100.0f : 1.0f;

                float fStat = .0f;
                IMContext.RequestQuery("GamePlay", endPoint:"GetStatValue", (errorMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    fStat = (float)ret;
                }, zoneId, planetInfo.PlanetId, q, true);

                bool isAffordable = false;
                context.RequestQuery("IdleMiner", "IsAffordableCurrency", (errorMsg, ret) => 
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    isAffordable = (bool)ret;

                }, cost ,eCurrencyType.MINING_COIN);

                isAffordable &= planetInfo.IsUnlocked;

                var miningRate = new PlanetStatComp.PresentInfo(zoneId, planetData.Id, (eABILITY)q, statStrings[q], level,
                    //$"{(fUnitRate * planetData.Stat1((eABILITY)q, level)).ToString(format)}{unitString[q]}",
                    $"{(fUnitRate * fStat).ToString(format)}{unitString[q]}",
                    "$" + cost.ToAbbString(),
                    isAffordable);
                listUpgrades.Add(miningRate);
            }
        }
          
        PlanetManagerCardComp.PresentInfo BuildManagerSectionCompPresentInfo(int zoneId, int planetId)
        {            
            //OwnedManagerInfo ownedMngInfo = Model.PlayerData.GetAssignedManagerInfoForPlanet(planetId);
            //if(ownedMngInfo == null)
             //   return new PlanetManagerCardComp.PresentInfo();

            //ManagerInfo mngInfo = Model.GetManagerInfo(ownedMngInfo.ManagerId);
            int levelidx = 1;// ownedMngInfo.Level - 1;
            
            return new PlanetManagerCardComp.PresentInfo(
                _icon: null,// controller.View.GetManagerSprite(mngInfo.SpriteKey), 
                "AAA",// mngInfo.Name_, 
                "AAA",// $"MR:x{mngInfo.BuffMiningRate_[levelidx]} \nDS:x{mngInfo.BuffShipSpeedRate_[levelidx]} \nPS:x{mngInfo.BuffCargoRate_[levelidx]} ", 
                "DDD");
            //$"{ownedMngInfo.Level}/{ManagerInfo.MAX_LEVEL}");
        }
        #endregion ===> View Refrehers.
        
    }
}
