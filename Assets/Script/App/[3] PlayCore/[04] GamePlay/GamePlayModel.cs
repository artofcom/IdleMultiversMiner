using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.PlayerModel;
// using System.Data;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using Core.Utils;
using IGCore.MVCS;
using IGCore.Simulator.GameData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.GamePlay
{
    internal class GamePlayModel : AModel
    {
        #region ===> Properties

        PlanetZoneGroup planetDataGroup;
        PlanetZoneBossGroup planetBossDataGroup;

        IdleMinerContext IMContext => context as IdleMinerContext;

        public GamePlayPlayerModel PlayerData => (GamePlayPlayerModel)playerData;

        // Runtime.
        //Dictionary<int, PlanetData> dictPlanetData = new Dictionary<int, PlanetData>();
        //public Dictionary<int, PlanetData> DictPlanetData => dictPlanetData;

        //  Events ----------------------------------------
        //
        EventsGroup Events = new EventsGroup();

        #endregion ===> Properties

        
        #region ===> Interfaces

        public GamePlayModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }

        public override void Init()
        {
            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);
            _InitModel();
        }

        void _InitModel()
        {
            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);

            string gamePath = (string)IMCtx.GetData("gamePath");
            
            InitModel(gamePath + "/Data/PlanetData", gamePath + "/Data/PlanetBossData");

            Events.RegisterEvent(EventID.PLANET_BOOSTER_TRIGGERED, EventOnPlanetBoosterTrigger);
            Events.RegisterEvent(EventID.PLANET_BOOSTER_FINISHED, EventOnPlanetBoosterFinished);

            RegisterRequestables();

            _isInitialized = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            Events.UnRegisterEvent(EventID.PLANET_BOOSTER_TRIGGERED, EventOnPlanetBoosterTrigger);
            Events.UnRegisterEvent(EventID.PLANET_BOOSTER_FINISHED, EventOnPlanetBoosterFinished);

            UnregisterRequestables();

            _isInitialized = false;
        }

        public PlanetData GetPlanetData(int zoneId, int id)
        {
            var zoneData = planetDataGroup.GetZoneData(zoneId);
            if(zoneData != null)
            {
                var planetData = zoneData.GetPlanetData(id);
                if(planetData != null)  return planetData;
            }
            return null;
        }

        public bool IsZoneUnlocked(int zoneId)
        {
            var zoneInfo = PlayerData.GetUnlockedZoneStatusInfo(zoneId);
            return zoneInfo!=null;
        }

        // zone unlock should be called from skill-tree.
        // cost can be null.
        public bool UnlockZone(int zoneId, List<float> distanceList)
        {
            // eligibility check.
            if(IsZoneUnlocked(zoneId))      return true;

            // fill data.
            List<int> planetIds = new List<int>();
            PlanetZoneData zoneData = planetDataGroup.GetZoneData(zoneId);
            if(zoneData != null)
            {
                for(int q = 0; q < zoneData.Planets.Count; q++) 
                    planetIds.Add(zoneData.Planets[q].Id);
            }
            else
                Debug.LogWarning("Couldn't find the zone..." + zoneId);

            // 
            if(PlayerData.UnlockZone(zoneId, planetIds, distanceList))
                return true;
            
            return false;
        }

        public bool UnlockPlanet(int zoneId, int planetId)
        {
            var data = GetPlanetData(zoneId, planetId);
            if (data == null)
            {
                Debug.Log("[UnlockPlanet] : Couldn't find the planet id..." + planetId.ToString());
                return false;
            }

            if(!IsZoneUnlocked(zoneId))
            {
                Debug.Log("[UnlockPlanet] : Should unlock zone first." + zoneId.ToString());
                return false;
            }

            var planetProcInfo = PlayerData.GetPlanetInfo(zoneId, planetId);
            if(planetProcInfo==null || planetProcInfo.IsUnlocked)
            {
                Debug.Log($"[UnlockPlanet] : Invalid Unlock Target Planet...{zoneId}, {planetId}");
                return false;
            }

            bool isAffordable = false;
            context.RequestQuery("IdleMiner", "IsAffordableCurrency", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                isAffordable = (bool)ret;

            }, data.BIOpenCost, eCurrencyType.MINING_COIN);

            if(!isAffordable)
            {
                Debug.Log($"[UnlockPlanet] : Not enought money..., [{data.BIOpenCost.ToAbbString()}]");
                return false;
            }

            bool ret = PlayerData.UnlockPlanet(zoneId, planetId);//, data.Type==PlanetBossData.KEY);
            if(ret)
            {
                context.RequestQuery("IdleMiner", "AddMoney", (errMsg, ret) =>{}, new CurrencyAmount((-1*data.BIOpenCost).ToString(), eCurrencyType.MINING_COIN));
                EventSystem.DispatchEvent(EventID.PLANET_UNLOCKED, new Tuple<int, int>(zoneId, planetId));
                return true;
            }
            return false;
        }

        public void UnlockZoneBooster(int zoneId, float duration, float buffRate, float coolTimeDuration)
        {
            PlayerData.UnlockZoneBooster(zoneId, duration, buffRate, coolTimeDuration);
        }
        
        public bool TriggerManualBooster(int zoneId, int planetId)
        {
            var data = GetPlanetData(zoneId, planetId);
            if (data == null)
            {
                Debug.Log("Couldn't find the planet id..." + planetId.ToString());
                return false;
            }

            if(PlayerData.TriggerPlanetBooster(zoneId, planetId))
                return true;
            
            return false;
        }

        void EventOnPlanetBoosterTrigger(object data)
        {
            Debug.Log("<color=#56F8C4>>>>[Planet-Booster] Booster Started...Model</color>");
        }

        void EventOnPlanetBoosterFinished(object data)
        {
            Debug.Log("<color=#56F8C4>>>>[Planet-Booster] Booster Finished..Model</color>");
        }

        internal bool UpgradePlanet(int zoneId, int planetId, eABILITY eStat, int offsetUpLevel = 1)
        {
            var townData = GetPlanetData(zoneId, planetId);
            if (townData == null)
                return false;

            BigInteger upgradeCost = GetPlanetOpenCost(zoneId, planetId, eStat);

            bool isAffordable = false;
            context.RequestQuery("IdleMiner", "IsAffordableCurrency", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                isAffordable = (bool)ret;

            }, upgradeCost, eCurrencyType.MINING_COIN);

            if (!isAffordable)
                return false;
            
            bool ret = PlayerData.UpgradePlanet(zoneId, planetId, eStat, upgradeCost, offsetUpLevel);
            if (!ret)   return false;
            
            context.RequestQuery("IdleMiner", "AddMoney", (errMsg, ret) =>{}, new CurrencyAmount((-1*upgradeCost).ToString(), eCurrencyType.MINING_COIN));
            return true;
        }
        
        internal void ResetPlanetStat(int zoneId, int planetId, eABILITY eStat)
        {
            var townData = GetPlanetData(zoneId, planetId);
            if (townData == null)
                return;

            PlayerData.ResetPlanetStat(zoneId, planetId, eStat);
        }

        internal float GetPlanetStat(int zoneId, int planetId, eABILITY statType)
        {
            return GetStatValue(zoneId, planetId, statType, withBuff:false);
        }

        public BigInteger GetPlanetOpenCost(int zoneId, int planetId, eABILITY statType)
        {            
            var planetData = GetPlanetData(zoneId, planetId);
            Assert.IsNotNull(PlayerData);

            PlanetInfo info = PlayerData.GetPlanetInfo(zoneId, planetId);
            if (info == null)
                return BigInteger.Zero;     // Invisible Planet.

            if (statType < eABILITY.MAX)
                return planetData.Cost(statType, 1 + info.Level[(int)statType]); 
            
            return BigInteger.Zero;
        }

        public Tuple<int, int> GetNeighborPlanetId(int zoneId, int planetId, bool isUpper, bool isIncludeBattleCleared = false)
        {
            PlanetInfo planetInfo = PlayerData.GetPlanetInfo(zoneId, planetId);
            if(planetInfo == null)
            {
                Assert.IsTrue(false, $"Unavaliable planet data...{planetId}");
                return null;
            }

            int startZone = zoneId, startPlanetId = planetId;
            int zone = zoneId, planet = planetId;
            do
            {
                (zone, planet) = PlayerData.GetNeighborData(zone, planet, isUpper);
                
                // No other planet?
                if(zone==startZone && planet==startPlanetId)
                    break;

                planetInfo = PlayerData.GetPlanetInfo(zone, planet);
                
            }while(!planetInfo.IsUnlocked);

            return new Tuple<int, int>(zone, planet);   
        }

        internal float GetPlanetLevelStat(int zoneId, int planetId, eABILITY eAbility)
        {
            if (eAbility >= eABILITY.MAX)
                return .0f;

            PlanetData planetData = GetPlanetData(zoneId, planetId);
            if(planetData == null)
            {
                Debug.Log($"Couln't find planet data...{planetId}");
                return .0f;
            }

            PlanetInfo visiblePlanet = PlayerData.GetPlanetInfo(zoneId, planetId);
            if(visiblePlanet==null || !visiblePlanet.IsUnlocked)
            {
                Debug.Log($"Unavaliable planet data...{planetId}");
                return .0f;
            }

            int level = visiblePlanet.Level[(int)eAbility];
            return planetData.Stat(eAbility, level);
        }

        //public void ExtendPlanetVisibility(List<int> planetIds)
        //{
        //    for (int q = 0; q < planetIds.Count; ++q)
        //        PlayerData.AddPlanetVisibility(planetIds[q]);
        //}

        public bool IsPlanetBattleMode(int zoneId, int planetId)
        {
            PlanetData data = GetPlanetData(zoneId, planetId);
            if (data == null)
                return false;

            return data.Type == PlanetBossData.KEY;
        }

        // true - if the battle has been finished. 
        // false - still on fight.
        public bool UpdateDamageX1000(int zoneId, int planetId, BigInteger offsetDamageX1000)
        {            
            PlanetData planetData = GetPlanetData(zoneId, planetId);
            if (planetData == null || planetData.Type != PlanetBossData.KEY)
            {
                Debug.Log($"Invalid planet Id..{planetId}");
                return false;
            }
            PlanetBossData bossData = planetData as PlanetBossData;
            Assert.IsNotNull(bossData);

            PlanetInfo info = PlayerData.GetPlanetInfo(zoneId, planetId);
            Assert.IsTrue(info != null && info.BattleInfo != null);

            if (info.BattleInfo.IsCleared)
                return true;

            PlayerData.UpdateDamageX1000(zoneId, planetId, offsetDamageX1000);

            if(bossData.Life <= info.BattleInfo.BIDamage)
            {
                info.ClearBattle();
       //         UpdateSkillState();
                
                EventSystem.DispatchEvent(EventID.PLANET_BATTLE_CLEARED, new Tuple<int, int>(zoneId, planetId));
                return true;
            }
            return false;
        }

        public void Pump()
        {
            ClosePlanetWhenTheDurationExpires();

            PlayerData.Pump();
        }

        void ClosePlanetWhenTheDurationExpires()
        {
            /*
            // Close Boss Battle if time is over.
            for (int q = 0; q < PlayerData.PlanetStatus.VisiblePlanets_.Count; ++q)
            {
                PlanetInfo info = PlayerData.PlanetStatus.VisiblePlanets_[q];
                PlanetData data = GetPlanetData( info.PlanetId);
                Assert.IsNotNull(data);
                if (data == null || data.Type != PlanetBossData.KEY)
                    continue;

                PlanetBossData bossData = data as PlanetBossData;
                Assert.IsNotNull(bossData);
                if (bossData == null || !info.IsOpened || info.BattleInfo == null)
                    continue;

                if (info.GetSecondFromEvent() > bossData.BattleDuration)
                    info.ClosePlanet();
            }*/
        }

        public float GetStatValue(int zoneId, int planetId, eABILITY ability, bool withBuff=true)
        {
            float fBuff = withBuff ? GetBuffRatioByAbility(zoneId, planetId, ability) : 1.0f;
            return sanitizeStat(ability, getStatValueInternal(zoneId, planetId, ability) * fBuff);
        }

        public float GetStatValueWithExtraBuff(int zoneId, int planetId, eABILITY ability, float extraBuff)
        {
            return sanitizeStat(ability, getStatValueInternal(zoneId, planetId, ability) * extraBuff);
        }

        public float GetBuffRatioByAbility(int zoneId, int planetId, eABILITY ability)
        {   
            PlanetInfo info = PlayerData.GetPlanetInfo(zoneId, planetId);
            if (info != null && info.BoostState == PlanetInfo.BOOST_STATE.Boosting)
                return info.BoosterRate;

            return 1.0f;
        }
        
        
        public void SetMiningSkillBuff(eABILITY ability, float rate)
        {
            // PlayerData.SkillAbilityInfo.SetMiningBuff(ability, rate);
        }

        public BigInteger GetGoldProductionRate(float duraionInSec = 60.0f)
        {   
            Dictionary<string, float> dictRscPDR = new Dictionary<string, float>();
            for(int z = 0; z < PlayerData.UnlockedZoneGroup.Zones.Count; ++z)
            {
                ZoneStatusInfo zoneInfo = PlayerData.UnlockedZoneGroup.Zones[z];
                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                {
                    var planetData = GetPlanetData(zoneInfo.ZoneId, zoneInfo.Planets[k].PlanetId);
                    if (planetData == null)
                        continue;
                
                    for (int q = 0; q < planetData.Obtainables.Count; ++q)
                    {
                        string targetId = planetData.Obtainables[q].ResourceId;
                        if(dictRscPDR.ContainsKey(targetId))
                            dictRscPDR[targetId] += GetProductionRatePerSec(zoneInfo.ZoneId, zoneInfo.Planets[k].PlanetId, targetId);
                        else 
                            dictRscPDR.Add(targetId , GetProductionRatePerSec(zoneInfo.ZoneId, zoneInfo.Planets[k].PlanetId, targetId));
                    }
                }
            }

            int loop = 0;
            BigInteger totalGoldPDR = BigInteger.One;
            foreach(string rsc_id in dictRscPDR.Keys) 
            {
                ResourceInfo rscInfo = null;
                context.RequestQuery("Resource", endPoint:"GetResourceInfo", (errorMsg, ret) => 
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    rscInfo = (ResourceInfo)ret;

                }, rsc_id);

                float fRatex1000 = 1000.0f;
                float duration_PDR = duraionInSec * dictRscPDR[rsc_id] * fRatex1000;
                if(duration_PDR < 1.0f)
                    continue;               // discard this one cuz its too small.
                
                BigInteger goldPDR = (BigInteger)(rscInfo.BIPrice * (int)duration_PDR);
                goldPDR /= 1000;
                totalGoldPDR = loop==0 ? goldPDR : totalGoldPDR + goldPDR;
                loop++;
            }

            return totalGoldPDR;
        }

        public float GetProductionRatePerSec(int zoneId, int planetId, string resourceId)
        { 
            PlanetInfo planetInfo = PlayerData.GetPlanetInfo(zoneId, planetId);
            if (planetInfo == null || !planetInfo.IsUnlocked)
                return .0f;

            var planetData = GetPlanetData(zoneId, planetId);
            if (planetData == null)
                return .0f;

            const float buffRate    = 1.0f;
            float miningRatePerSec  = GetStatValue(zoneId, planetInfo.PlanetId, eABILITY.MINING_RATE);
            float accuracy          = GetStatValueWithExtraBuff(zoneId, planetInfo.PlanetId, eABILITY.SHOT_ACCURACY, buffRate);
            float shotCountPerSec   = 1.0f / GetStatValueWithExtraBuff(zoneId, planetInfo.PlanetId, eABILITY.SHOT_INTERVAL, buffRate);
            float shipSpeed         = GetStatValue(zoneId, planetInfo.PlanetId, eABILITY.DELIVERY_SPEED) * buffRate;
            float cargoSize         = GetStatValue(zoneId, planetInfo.PlanetId, eABILITY.CARGO_SIZE) * buffRate;

            for (int q = 0; q < planetData.Obtainables.Count; ++q)
            {
                if(0 != string.Compare(resourceId, planetData.Obtainables[q].ResourceId, ignoreCase:true))
                    continue;
                
                miningRatePerSec *= planetData.Obtainables[q].Yield * shotCountPerSec;
                cargoSize *= planetData.Obtainables[q].Yield;

                float shipDuration = (planetInfo.Distance * 2.0f) / shipSpeed;
                float shipCountPerSec = 1.0f / shipDuration;

                float deliverableCountPerSec = cargoSize * shipCountPerSec;
                return Mathf.Min(miningRatePerSec, deliverableCountPerSec);
            }
            return .0f;
        }
        
        public float GetProductionRatePerSec(string resourceId)
        {
            float PRS = .0f;
            for(int z = 0; z < PlayerData.UnlockedZoneGroup.Zones.Count; ++z)
            {
                ZoneStatusInfo zoneInfo = PlayerData.UnlockedZoneGroup.Zones[z];
                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                    PRS += GetProductionRatePerSec(zoneInfo.ZoneId, zoneInfo.Planets[k].PlanetId, resourceId);                
            }
            return PRS;
        }

        // ZoneId, Planetid
        List<Tuple<int, int>> FindCollectableZonePlanetId(string resourceId)
        {
            List<Tuple<int, int>> zonePlanets = new List<Tuple<int, int>>();

            for(int q = 0; q < planetDataGroup.Data.Count; ++q)
            {
                var planetZoneData = planetDataGroup.Data[q];
                for(int k = 0; k < planetZoneData.Planets.Count; ++k)
                {
                    var planetData = planetZoneData.Planets[k];
                    for(int p = 0; p < planetData.Obtainables.Count; ++p)
                    {
                        var obtainStat = planetData.Obtainables[p];
                        if(0 == string.Compare(obtainStat.ResourceId, resourceId, ignoreCase:true))
                            zonePlanets.Add( new Tuple<int, int>(planetZoneData.ZoneId, planetData.Id) );
                    }
                }
            }
            return zonePlanets;
        }

        #endregion ===> Interfaces









        #region ===> Helpers

        void InitModel(string planetDataPath, string bossDataPath)
        {
            var textData = Resources.Load<TextAsset>(planetDataPath);   // gmSetting.PlanetsDataPath);
            planetDataGroup = JsonUtility.FromJson<PlanetZoneGroup>(textData.text);
            Assert.IsNotNull(planetDataGroup);
            planetDataGroup.Convert();

            textData = Resources.Load<TextAsset>(bossDataPath);         // gmSetting.PlanetBossDataPath);
            planetBossDataGroup = JsonUtility.FromJson<PlanetZoneBossGroup>(textData.text);
            Assert.IsNotNull(planetBossDataGroup);
            planetBossDataGroup.Convert();


            //---------------------------------------------------------------------//
            //
            // !!! : To have unified zone data, adding boss-planet data to planetDataGroup.
            //       That way, we should only handle the planetDataGroup, but the planetBossGroup.
            //
            for(int q = 0; q < planetBossDataGroup.Data.Count; ++q)
            {
                PlanetZoneBossData bossZone = planetBossDataGroup.Data[q];
                PlanetZoneData zoneData = planetDataGroup.GetZoneData(bossZone.ZoneId);
                if(zoneData == null)
                {
                    zoneData = new PlanetZoneData(zoneData.ZoneId, new List<PlanetData>());
                    planetDataGroup.Data.Add(zoneData);
                }

                for(int k = 0; k < bossZone.Planets.Count; ++k)
                    zoneData.Planets.Add( bossZone.Planets[k] );
            }
            Debug.Log("Total Zone Count : " + planetDataGroup.Data.Count);
            planetBossDataGroup = null;


            // ============= Build Cache Dictionary.
            //
            /*dictPlanetData.Clear();
            for (int q = 0; q < planetDataGroup.Data.Count; ++q)
            {
                Assert.IsTrue(!dictPlanetData.ContainsKey(planetDataGroup.Data[q].Id));
                dictPlanetData[planetDataGroup.Data[q].Id] = planetDataGroup.Data[q];
            }
            for (int q = 0; q < planetBossDataGroup.Data.Count; ++q)
            {
                Assert.IsTrue(!dictPlanetData.ContainsKey(planetBossDataGroup.Data[q].Id));
                dictPlanetData[planetBossDataGroup.Data[q].Id] = planetBossDataGroup.Data[q];
            }*/
        }

        int GetUpperIdxPlanetId(int idx, bool isIncludeBattleCleared=false)
        {
            /*
            List<PlanetInfo> visiblePlanets = PlayerData.PlanetStatus.VisiblePlanets_;
            int q = idx + 1;
            q %= visiblePlanets.Count;

            int cnt = 0;
            while (true)
            {
                ++cnt;
                if(cnt > visiblePlanets.Count*2)
                    break;

                PlanetInfo info = visiblePlanets[q];

                bool keepSearch = !info.IsOpened;
                PlanetData data = GetPlanetData(info.PlanetId);
                if(false)// data.Type )//== App.GamePlay.IdleMiner.PlanetMining.PlanetBossData.KEY && !isIncludeBattleCleared)
                {
                    keepSearch |= info.BattleInfo.IsCleared;
                }

                if (keepSearch)
                {
                    ++q;
                    q %= visiblePlanets.Count;
                    if (q == idx) return visiblePlanets[q].PlanetId;

                    continue;
                }
                return visiblePlanets[q].PlanetId;
            }*/
            return -1;
        }
        
        int GetLowerIdxPlanetId(int idx, bool isIncludeBattleCleared = false)
        {
            /*
            List<PlanetInfo> visiblePlanets = PlayerData.PlanetStatus.VisiblePlanets_;
            int q = idx - 1;
            q = q < 0 ? q + visiblePlanets.Count : q;

            int cnt = 0;
            while (true)
            {
                ++cnt;
                if(cnt > visiblePlanets.Count*2)
                    break;

                PlanetInfo info = visiblePlanets[q];

                bool keepSearch = !info.IsOpened;
                PlanetData data = GetPlanetData(info.PlanetId);
               //  if (data.Type == App.GamePlay.IdleMiner.PlanetMining.PlanetBossData.KEY && !isIncludeBattleCleared)
                {
                    keepSearch |= info.BattleInfo.IsCleared;
                }

                if (keepSearch)
                {
                    --q;
                    q = q < 0 ? q + visiblePlanets.Count : q;
                    if (q == idx) return visiblePlanets[q].PlanetId;

                    continue;
                }
                return visiblePlanets[q].PlanetId;
            }*/
            return -1;
        }
        
        float getStatValueInternal(int zoneId, int planetId, eABILITY ability)
        {
            PlanetData planetData = GetPlanetData(zoneId, planetId);
            if(planetData == null) return 1.0f;

            PlanetInfo visiblePlanet = PlayerData.GetPlanetInfo(zoneId, planetData.Id);
            if (visiblePlanet == null)
                return 1.0f;

            int statLevel = visiblePlanet.Level[(int)ability];
            return planetData.Stat(ability, statLevel);
        }

        float sanitizeStat(eABILITY abillity, float value)
        {
            float ret;
            switch(abillity)
            {
                case eABILITY.MINING_RATE:          // 0.001f => unlimited.
                case eABILITY.DELIVERY_SPEED:
                case eABILITY.CARGO_SIZE:
                    ret = Math.Max(value, 0.001f);
                    break;
                
                case eABILITY.SHOT_INTERVAL:        // unlimit => 0.1f 
                    ret = Math.Max(value, 0.1f);
                    break;

                case eABILITY.SHOT_ACCURACY:        // 0 ~ 1
                    ret = Math.Clamp(value, 0.0f, 1.0f);
                    break;
            
                default:
                    Assert.IsTrue(false, "Invalid Ability Type !!!" + abillity);
                    return .0f;
            }
            return ret;
        }

        #endregion ===> Helpers







        #region ===> Requestables

        void RegisterRequestables()
        {
            context.AddRequestDelegate("GamePlay", "GetPlanetData", getPlanetData);
            context.AddRequestDelegate("GamePlay", "GetPlanetStat", getPlanetStat);
            context.AddRequestDelegate("GamePlay", "GetStatValue", getStatValue);
            context.AddRequestDelegate("GamePlay", "GetNeighborPlanetId", getNeighborPlanetId);
            context.AddRequestDelegate("GamePlay", "UpgradePlanet", upgradePlanet);
            context.AddRequestDelegate("GamePlay", "ResetPlanetStat", resetPlanetStat);
            context.AddRequestDelegate("GamePlay", "CalculateBonusRewardCurrencyAmount", calculateBonusRewardCurrencyAmount);

            context.AddRequestDelegate("GamePlay", "PlayerData.GetMinedResourceInfo", getMinedResourceInfo);
            context.AddRequestDelegate("GamePlay", "GetProductionRate", getProductionRatePerSec);
            context.AddRequestDelegate("GamePlay", "FindCollectableZonePlanetId", findCollectableZonePlanetId);
        }
        void UnregisterRequestables()
        {
            context.RemoveRequestDelegate("GamePlay", "GetPlanetData");
            context.RemoveRequestDelegate("GamePlay", "GetPlanetStat");
            context.RemoveRequestDelegate("GamePlay", "GetStatValue");
            context.RemoveRequestDelegate("GamePlay", "GetNeighborPlanetId");
            context.RemoveRequestDelegate("GamePlay", "UpgradePlanet");
            context.RemoveRequestDelegate("GamePlay", "ResetPlanetStat");
            context.RemoveRequestDelegate("GamePlay", "CalculateBonusRewardCurrencyAmount");

            context.RemoveRequestDelegate("GamePlay", "PlayerData.GetMinedResourceInfo");
            context.RemoveRequestDelegate("GamePlay", "GetProductionRate");
            context.RemoveRequestDelegate("GamePlay", "FindCollectableZonePlanetId");
        }

        object getProductionRatePerSec(params object[] data)
        {
            if(data.Length == 1)
                return GetProductionRatePerSec(resourceId:(string)data[0]);
            else if(data.Length == 3)
                return GetProductionRatePerSec(zoneId:(int)data[0], planetId:(int)data[1], resourceId:(string)data[2]);

            Assert.IsTrue(false, "Invalid parameter count !, " + data.Length);
            return null;
        }

        object findCollectableZonePlanetId(params object[] data)
        {
            if(data==null || data.Length<1)
                return null;

            return FindCollectableZonePlanetId((string)data[0]);
        }

        object getPlanetData(params object[] data)
        {
            if(data.Length < 2)
                return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];

            PlanetData planetData = GetPlanetData(zoneId, planetId);
            return planetData;          //  JsonUtility.ToJson(planetData);
        }
        object getPlanetStat(params object[] data)
        {
            if(data.Length < 3) return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            int statType = (int)data[2];
            return GetPlanetStat(zoneId, planetId, (eABILITY)statType);
        }
        object getStatValue(params object[] data)
        {
            if(data.Length < 4) return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            int statType = (int)data[2];
            bool withBuff = (bool)data[3];
            return GetStatValue(zoneId, planetId, (eABILITY)statType, withBuff);
        }
        object getNeighborPlanetId(params object[] data)
        {
            if(data.Length < 4) return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            bool isUpper = (bool)data[2];
            bool isIncludeBattleCleared = (bool)data[3];

            return GetNeighborPlanetId(zoneId, planetId, isUpper, isIncludeBattleCleared);
        }
        object upgradePlanet(params object[] data)
        {
            if(data.Length < 4) return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            eABILITY statType = (eABILITY)data[2];
            int offsetUpLevel = (int)data[3];

            return UpgradePlanet(zoneId, planetId, statType, offsetUpLevel);
        }
        object resetPlanetStat(params object[] data)
        {
            if(data.Length < 3) return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            eABILITY statType = (eABILITY)data[2];
            ResetPlanetStat(zoneId, planetId, statType);
            
            return null;
        }
        object calculateBonusRewardCurrencyAmount(params object[] data)
        {
            if(data.Length < 1)
                return null;

            float sec = (float)data[0];
            return GetGoldProductionRate(sec);
        }

        object getMinedResourceInfo(params object[] data)
        {
            if(data.Length < 3)
                return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            string rscId = (string)data[2];

            return PlayerData.GetMinedResourceInfo(zoneId, planetId, rscId);
        }
        #endregion

        #region aaaa


        #endregion
    }
}