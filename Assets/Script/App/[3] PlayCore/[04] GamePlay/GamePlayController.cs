using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using Core.Events;
using Core.Util;
using Core.Utils;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class GamePlayController : AController, ISkillLeaner
    {
        class ResumeBlockWithBoostInfo
        {
            public ResumeBlockWithBoostInfo(float duration, float MRRate, float SIRate, float SARate)
            {
                this.MRRate = MRRate; this.SIRate = SIRate; this.SARate = SARate;
                this.duration = duration;
            }
            public float duration;
            public float MRRate, SIRate, SARate;
        }

        class SimpleBoosterInfo
        {
            public SimpleBoosterInfo(eBoosterType _eType, float _remainTime, float _rate)
            {
                type = _eType; remainTime = _remainTime;
                this.fRate = _rate;
            }
            eBoosterType type;
            float fRate;
            float remainTime;

            public eBoosterType BoostType => type;
            public float Rate => fRate;
            public float RemainTime => remainTime;
        }


        #region SKILL_BEHAVIOR

        // Should be nested classes to access all of controller's members.
        class ZoneOpenSkill : ISkillBehavior
        {
            public const string Id = "UNLOCK_ZONE";
            public void Learn(AController ctrler, string abilityParam) 
            {
                uint zoneId;
                bool ret = uint.TryParse(abilityParam, out zoneId);
                Assert.IsTrue(ret, "Ability Param Parsing Has been Failed.. .! " + abilityParam);
                if(!ret)        return;

                GamePlayController gameCtrler = (GamePlayController)ctrler;
                var unlockZone = gameCtrler.Model.UnlockZone((int)zoneId, gameCtrler.View.GetDistanceToPlanet((int)zoneId));
                if(!unlockZone)
                    Debug.LogError("Learning Zone Unlock Skill has been failed....." + abilityParam);
            }
        }

        // zoneId=100:duration=60:rate=10:cooltime=10
        class ZoneBuffSkill : ISkillBehavior
        {
            public const string Id = "MINING_ZONE_BUFF";
            public void Learn(AController ctrler, string abilityParam) 
            {
                string[] data = abilityParam.Split(':');
                Assert.IsTrue(data!=null && data.Length==4);

                uint zoneId;
                bool ret = uint.TryParse(data[0], out zoneId);
                Assert.IsTrue(ret, "Ability Param Parsing Has been Failed.. .! " + data[0]);
                if(!ret)        return;

                float duration;
                ret = float.TryParse(data[1], out duration);
                Assert.IsTrue(ret, "Ability Param Parsing Has been Failed.. .! " + data[1]);
                if(!ret)        return;

                float fBuffRate;
                ret = float.TryParse(data[2], out fBuffRate);
                Assert.IsTrue(ret, "Ability Param Parsing Has been Failed.. .! " + data[2]);
                if(!ret)        return;

                float coolTimeDuration;
                ret = float.TryParse(data[3], out coolTimeDuration);
                Assert.IsTrue(ret, "Ability Param Parsing Has been Failed.. .! " + data[3]);
                if(!ret)        return;

                GamePlayController gameCtrler = (GamePlayController)ctrler;

                // To Do : should add event 
                gameCtrler.Model.UnlockZoneBooster((int)zoneId, duration, fBuffRate, coolTimeDuration);
                // Debug.Log($"<color=green>[LearnSkill] Unlocked Zone Booster {zoneId}! {strTime} </color>");
                /*
                if(gameCtrler.View.gameObject.activeInHierarchy && gameCtrler.View.gameObject.activeSelf)
                {
                    const float shortDelay = 0.1f;
                    gameCtrler.View.StartCoroutine(gameCtrler.View.coTriggerActionWithDelay(shortDelay, () =>
                    {
                        gameCtrler.View.PlanetsComp.UpdateArea(gameCtrler.BuildPlanetBaseInfoDictionary());

                    }) );
                }*/
            }
        }

        class GameResetSkill : ISkillBehavior
        {
            int starAmount = 0;
            public int StarAmount => starAmount;

            public const string Id = "GAME_RESET";
            public void Learn(AController ctrler, string abilityParam) 
            {
                string[] data = abilityParam.Split(':');
                Assert.IsTrue(data!=null && data.Length==2);

                Assert.IsTrue(data[0].ToLower() == "star");
                int cnt = 0;
                if(int.TryParse(data[1], out cnt))
                {
                    starAmount = cnt;
                    ((GamePlayController)ctrler).EnableGameReset(true);
                }
            }
        }

        #endregion



        const float FIRE_TIME_FLEXIBILITY = 0.5f;

        GamePlayView View => (GamePlayView)view;
        GamePlayModel Model => (GamePlayModel)model;
        EventsGroup events = null;

        IdleMinerContext IMContext => (IdleMinerContext)context;

        int ResetStarAmount => (dictSkillBehaviors[GameResetSkill.Id.ToLower()] as GameResetSkill).StarAmount;

        protected bool bZoneInitizlized = false;
        protected Dictionary<string, ISkillBehavior> dictSkillBehaviors = new Dictionary<string, ISkillBehavior>();

        bool bEnableMiningProcess = true;

        #region ===> CORE.

        public GamePlayController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
            : base(view, model, ctx)
        { }


        public override void Init()
        {
            ((ISkillLeaner)this).CreateSkillBehaviors();
            InitPlayGround();
            InitZone();
        }

        protected override void OnViewEnable()
        {
            Debug.Log("OnViewEnable : GamePlayController.");

            View.StartCoroutine( coStartView() );

            View.EventOnBtnGameResetClicked += EventOnBtnGameResetClicked;
            SettingDialogView.EventOnShowGameReset += EventOnBtnShowGameResetClicked;
        }
        protected override void OnViewDisable()
        {
            Debug.Log("OnViewDisable : GamePlayController.");

            View.EventOnBtnGameResetClicked -= EventOnBtnGameResetClicked;
            SettingDialogView.EventOnShowGameReset -= EventOnBtnShowGameResetClicked;
        }

        public override void Dispose()
        {
            base.Dispose();

            events.UnRegisterAll();
            
            PlanetComp[] planetComps = View.transform.GetComponentsInChildren<PlanetComp>(includeInactive:true);
            Assert.IsTrue(planetComps!=null && planetComps.Length>0);
            for(int q = 0; q < planetComps.Length; q++) 
            {
                planetComps[q].EventOnClicked -= PlanetComp_OnClicked;
                planetComps[q].EventOnArrivalAtTown -= PlanetComponent_EventOnArrivalAtTown;
                planetComps[q].EventOnArrivalAtStation -= PlanetComponent_EventOnArrivalAtStation;
                planetComps[q].EventOnBoosterClicked -= PlanetComponent_EventOnManualBuffClicked;
            }

            context.RemoveRequestDelegate("GamePlay", "GetPlanetSprite");
        }

        public override void Resume(int duration)
        {
            ResumeMining(duration);

            ResumeShipping(duration);

            CleanUpBooster(duration);

#if UNITY_EDITOR
            if(context.IsSimulationMode())
            {
                SIM_OpenPlanetIfPossible();

                SIM_UpgradeStatIfPossible();

                SIM_LogSimulationStat();
            }
#endif
        }

        public override void Pump()
        {
            if(!bEnableMiningProcess)
                return;

            Model?.Pump();

            PumpMiningTargets();
        }

        public override void WriteData()
        {
        }

        public void EnableGameReset(bool enable)
        {
            if(!context.IsSimulationMode())
                View.EnableGameResetButton(enable);
        }

        void EventOnBtnShowGameResetClicked()
        {
            EnableGameReset(true);
        }

        void ResetGame()
        {
            Debug.Log("<color=red> Resetting All of this Game Systems... </color>");
            
            IMContext.ResetPlayerData();
            EventSystem.DispatchEvent(EventID.SKILL_RESET_GAME_INIT);

            // Add some effect or so.
            DelayedAction.TriggerActionWithDelay(View, 0.05f, () =>
            {
                View.CleanUp();

                InitZone();

                View.Init( new GamePlayView.ViewIniter( BuildPlanetBaseInfoDictionary() ) );

                EventSystem.DispatchEvent(EventID.GAME_RESET_REFRESH, IdleMinerContext.GameKey);

                EnableGameReset(enable:false);

                Debug.Log("<color=red> Refreshing All Views... </color>");
            });

            // Hold off mining process for a while for game-reset.
            bEnableMiningProcess = false;
            DelayedAction.TriggerActionWithDelay(view, delay:3.0f, () => bEnableMiningProcess = true);
        }

        void EventOnBtnGameResetClicked()
        {
            var presentInfo = new GameResetDialog.PresentInfo( message :  
                $"Reset the Level and Earn {ResetStarAmount} Stars as a reward.", 
                () => 
                {
                    bool offset = true;
                    context.RequestQuery("AppPlayerModel", "UpdateStarCurrency", ResetStarAmount, offset);
                    this.ResetGame();
                });

            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GAME_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                "GameResetDialog", 
                presentInfo,
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("GameReset Dialog has been closed.");

                } ) ); 
        }

        protected void RefreshView(object data)
        {
            if(data == null)            return;
            if(!bEnableMiningProcess)   return;


#if UNITY_EDITOR
            if(context.IsSimulationMode())
                return;
#endif

            Tuple<int, int> tupleZonePlanet = (Tuple<int, int>)data;
            if(tupleZonePlanet == null)
                return;

            RefreshPlanetComp(tupleZonePlanet.Item1, tupleZonePlanet.Item2);
        }
        #endregion




        #region Initiailization

        protected virtual void InitPlayGround()
        {
            Debug.Log("[InitSeq]:[GamePlayController] InitPlayGround.");

            events = new EventsGroup();

            events.RegisterEvent(EventID.PLANET_BATTLE_CLEARED, OnPlanetBattleCleared);
            events.RegisterEvent(EventID.PLANET_UNLOCKED, PlayerData_OnPlanetUnlocked);
            events.RegisterEvent(EventID.PLANET_CLOSED, PlayerData_OnPlanetClosed);
            //events.RegisterEvent("EVENT_ONVISIBLE_PLANET_ADDED", OnVisiblePlanetAdded);
            events.RegisterEvent(EventID.MINING_STAT_UPGRADED, PlayerData_OnStatupgraded);
            events.RegisterEvent(EventID.SKILL_LEARNED, EventOnSkillLearned);
            events.RegisterEvent(EventID.GAME_RESET_REFRESH, EventOnRefreshAllView);
            events.RegisterEvent(EventID.ZONE_UNLOCKED, EventOnZoneUnlocked);

            PlanetComp[] planetComps = View.transform.GetComponentsInChildren<PlanetComp>(includeInactive:true);
            Assert.IsTrue(planetComps!=null && planetComps.Length>0);
            for(int q = 0; q < planetComps.Length; q++) 
            {
                planetComps[q].EventOnClicked += PlanetComp_OnClicked;
                planetComps[q].EventOnArrivalAtTown += PlanetComponent_EventOnArrivalAtTown;
                planetComps[q].EventOnArrivalAtStation += PlanetComponent_EventOnArrivalAtStation;
                planetComps[q].EventOnBoosterClicked += PlanetComponent_EventOnManualBuffClicked;
            }

            context.AddRequestDelegate("GamePlay", "GetPlanetSprite", getPlanetSprite);

#if UNITY_EDITOR
            TestMingPumpResumeLogic();
#endif
        }

        void InitZone()
        {
            Debug.Log("[InitSeq]:[GamePlayController] InitZone...");

            int defaultZoneId = (int)IMContext.GetData("DefaultZoneId", 100);

            if(Model.PlayerData.UnlockedZoneGroup.Zones.Count == 0)
                Model.UnlockZone(defaultZoneId, View.GetDistanceToPlanet(defaultZoneId));
            
            else
            {
                for(int q = 0; q < Model.PlayerData.UnlockedZoneGroup.Zones.Count; q++)
                {
                    ZoneStatusInfo status = Model.PlayerData.UnlockedZoneGroup.Zones[q];
                    List<float> listDists = View.GetDistanceToPlanet(status.ZoneId);

                    if(listDists.Count != status.Planets.Count)
                    {
                        Debug.LogWarning($"[Warning] Planet Count[{status.Planets.Count}] is not identical with distance Count [{listDists.Count}] !!!");

                        // This usually happends, due to edit planet/zone view data after initial released. - Planet Data not the same as before.
                        // Clear all.
                        Model.PlayerData.ForceClearData();
                        Model.UnlockZone(defaultZoneId, View.GetDistanceToPlanet(defaultZoneId));
                    }
                    else 
                        status.SetDistanceInfo( listDists );
                }
            }

            bZoneInitizlized = true;
        }

        IEnumerator coStartView()
        {
            // Wait until zone initialized.
            yield return new WaitUntil( () => bZoneInitizlized && Model.IsInitialized );

            View.Init( new GamePlayView.ViewIniter( BuildPlanetBaseInfoDictionary() ) );

            InitiateFiringBullets();
        }

        #endregion ===> Initiailization.

        
        




        #region ===> Core Pump & Resume Helpers

        protected virtual void ResumeMining(float duration)
        {
            for(int q = 0; q < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++q)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[q];

                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                {
                    PlanetInfo planetInfo = zoneInfo.Planets[k];
                    if (planetInfo == null || !planetInfo.IsUnlocked)
                        continue;

                    PlanetData townData = Model.GetPlanetData(zoneInfo.ZoneId, planetInfo.PlanetId);
                    if (townData == null)
                        continue;

                    // 2 Step Resume Required due to the Booster.
                    float remainedDuration = duration;
                    if(planetInfo.IsBoosterUnlocked && planetInfo.BoostState==PlanetInfo.BOOST_STATE.Boosting)
                    {
                        float boostTime = Math.Min(duration, planetInfo.BoostRemainTimeInSec);
                        ResumeMiningZone(boostTime, planetInfo, zoneInfo, townData.Type, planetInfo.BoosterRate);
                        remainedDuration -= boostTime;
                    }
                    
                    ResumeMiningZone(remainedDuration, planetInfo, zoneInfo, townData.Type);
                }
            }
        }

        protected virtual void ResumeShipping(float duration)
        {
            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];

                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                {
                    PlanetInfo planetInfo = zoneInfo.Planets[k];
                    if (planetInfo == null || !planetInfo.IsUnlocked)
                        continue;

                    PlanetData planetData = Model.GetPlanetData(zoneInfo.ZoneId, planetInfo.PlanetId);
                    if (planetData == null)
                        continue;

                    float resetDuration = duration;
                    if(planetInfo.IsBoosterUnlocked && planetInfo.BoostState==PlanetInfo.BOOST_STATE.Boosting)
                    {
                        float shipTime = Math.Min(duration, planetInfo.BoostRemainTimeInSec);
                        ResumeZoneShipping(shipTime, zoneInfo.ZoneId, planetData, planetInfo, planetInfo.BoosterRate);
                        resetDuration -= shipTime;   
                    }

                    ResumeZoneShipping(resetDuration, zoneInfo.ZoneId, planetData, planetInfo);
                }
            }
        }

        protected virtual void CleanUpBooster(float duration)
        {
            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];

                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                {
                    PlanetInfo planetInfo = zoneInfo.Planets[k];
                    if (planetInfo == null || !planetInfo.IsUnlocked)
                        continue;

                    PlanetData planetData = Model.GetPlanetData(zoneInfo.ZoneId, planetInfo.PlanetId);
                    if (planetData == null)
                        continue;

                    // Process Internal data.
                    planetInfo.Resume(duration);
                }
            }
        }

        protected virtual void PumpMiningTargets()
        {
            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];

                for(int q = 0; q < zoneInfo.Planets.Count; q++)
                {
                    PlanetInfo info = zoneInfo.Planets[q];
                    PlanetData planetData = Model.GetPlanetData(zoneInfo.ZoneId, info.PlanetId);
                    if (planetData == null) continue;
                    if (planetData as PlanetBaseData == null) continue;

                    _refreshPlanetComp(zoneInfo.ZoneId, info, planetData, View.PlanetsComp.GetPlanetComp(zoneInfo.ZoneId, info.PlanetId), GetManagerSpriteOnThePlanet(info.PlanetId));
                }
            }
        }





        
        int SortByRemainTime(SimpleBoosterInfo a, SimpleBoosterInfo b)
        {
            if(a.RemainTime < b.RemainTime) return -1;
            if(a.RemainTime > b.RemainTime) return 1;
            return 0;
        }        

        // Note : Core Logic - Should be taking care of any edge cases.
        List<ResumeBlockWithBoostInfo> SplitResumeSectorFromBooster(float duration, List<SimpleBoosterInfo> listBoosters)
        {
            Assert.IsTrue(listBoosters!=null && listBoosters.Count == 3);   // Needs miningRate, shotInterval, shotAccuracy boost info.

            // Ascending Order. ( 0, 2, 5, .... )
            listBoosters.Sort((a, b) => { return SortByRemainTime(a, b); });

            float mineRateBooster = 1.0f;
            float shotIntervalBooster = 1.0f;
            float shotAccuracyBooster = 1.0f;
            float curDuration = duration;

            List<ResumeBlockWithBoostInfo> listOutcomes = new List<ResumeBlockWithBoostInfo>();

            // Loops as many of (listBooster.coun + 1)
            for (int q = listBoosters.Count - 1; q >= -1; --q)
            {
                SimpleBoosterInfo curBooster = q >= 0 ? listBoosters[q] : null;

                float sectionDuration = curBooster == null ? curDuration : curDuration - curBooster.RemainTime;
                if (sectionDuration > .0f)
                    listOutcomes.Add(new ResumeBlockWithBoostInfo(sectionDuration, mineRateBooster, shotIntervalBooster, shotAccuracyBooster));
                
                if (q < 0) break;

                switch (listBoosters[q].BoostType)
                {
                    case eBoosterType.MINING_RATE:   mineRateBooster *= curBooster.Rate; break;
                    case eBoosterType.SHOT_ACCURACY: shotAccuracyBooster *= curBooster.Rate; break;
                    case eBoosterType.SHOT_INTERVAL: shotIntervalBooster *= curBooster.Rate; break;
                    default:
                        Assert.IsTrue(false, "Unsupported booster type.");
                        continue;
                }

                if (sectionDuration > .0f)
                {
                    curDuration -= sectionDuration;
                    if (curDuration <= float.Epsilon)
                        break;
                }
            }
            return listOutcomes;
        }

        void ResumeMiningZone(float duration, PlanetInfo planetInfo, ZoneStatusInfo zoneInfo, string planetType, float buffRate = 1.0f)
        {
            if(duration <= float.Epsilon)
                return;

            float miningRatePerTime = Model.GetStatValue(zoneInfo.ZoneId, planetInfo.PlanetId, eABILITY.MINING_RATE);
            float shotInterval = Model.GetStatValueWithExtraBuff(zoneInfo.ZoneId, planetInfo.PlanetId, eABILITY.SHOT_INTERVAL, buffRate);
            float accuracy = Model.GetStatValueWithExtraBuff(zoneInfo.ZoneId, planetInfo.PlanetId, eABILITY.SHOT_ACCURACY, buffRate);
            int shotCount = 1 + (int)(((float)duration) / shotInterval);    // try not to miss any cast values.
            // Debug.Log($"Resume mining.... PlanetId[{planetInfo.PlanetId}] : count[{shotCount}]");

            if (planetType == PlanetBossData.KEY)
            {
                // BATTLE at the Planet.
                //
                AttackAtPlanetBattle(zoneInfo.ZoneId, planetInfo.PlanetId, miningRatePerTime * accuracy, shotCount);
            }
            else
            {
                // MINING at the Planet.
                //    
                MineResourcesAtPlanet(zoneInfo.ZoneId, planetInfo.PlanetId, miningRatePerTime * accuracy, shotCount);
            }
        }

        void ResumeZoneShipping(float duration, int zoneId, PlanetData planetData, PlanetInfo planetInfo, float buffRate = 1.0f)
        {
            if(duration < float.Epsilon)
                return;


            // SHIPPING for the Planet.
            //
            float shipSpeed = Model.GetStatValue(zoneId, planetInfo.PlanetId, eABILITY.DELIVERY_SPEED) * buffRate;
            float cargoSize = Model.GetStatValue(zoneId, planetInfo.PlanetId, eABILITY.CARGO_SIZE) * buffRate;

            for (int q = 0; q < planetData.Obtainables.Count; ++q)
            {
                // Debug.Log($"[Resume] ZoneShipping..ZoneId:{zoneId}-PlanetId:{planetData.Id}-RSC:{planetData.Obtainables[q].ResourceId}");

                float cargoCapacity = cargoSize * planetData.Obtainables[q].Yield;
                BigInteger minedAmountF3 = GetMinedResourceAmount(zoneId, planetInfo.PlanetId, planetData.Obtainables[q].ResourceId, isX1000: true);
                BigInteger deliveredAmountF3 = CalculateDeliveredAmountx1000(shipSpeed, planetInfo.Distance, second:duration, cargoCapacity, minedAmountF3);

                // Debug.Log($"[Resume] shipSpeed{shipSpeed}, dist:{planetInfo.Distance}, duration:{duration}, cargoCapacity:{cargoCapacity}, minedAmountF3:{minedAmountF3}, cargoCap:{cargoCapacity}, deliveredAmountF3:{deliveredAmountF3}, buffRate:{buffRate}");
                Model.PlayerData.UpdateMiningResourceAmountX1000(zoneId, planetInfo.PlanetId, planetData.Obtainables[q].ResourceId, deliveredAmountF3 * -1);
                   
                const bool isOffset = true;
                context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => { }, 
                    planetData.Obtainables[q].ResourceId, deliveredAmountF3, isOffset);
            }
        }

        #endregion ===> Resume







        #region ===> Helpers

        void AttackAtPlanetBattle(int zoneId, int planetId, float offsetDamage, int count = 1)
        {
            BigInteger biAmountF3 = (long)(1000.0f * offsetDamage * ((float)count));
            Model.UpdateDamageX1000(zoneId, planetId, biAmountF3);
           
            Debug.Log($"Planet[{planetId}] : DamageF3[{biAmountF3}]]");
        }

        void MineResourcesAtPlanet(int zoneId, int planetId, float miningRateWithBooster, int count = 1)
        {
            var planetData = Model.GetPlanetData(zoneId, planetId);
            if (planetData == null)
                return;

            for (int q = 0; q < planetData.Obtainables.Count; ++q)
            {
                BigInteger biAmountF3 = (long)(1000.0f * miningRateWithBooster * planetData.Obtainables[q].Yield * ((float)count));
                Model.PlayerData.UpdateMiningResourceAmountX1000(zoneId, planetId, planetData.Obtainables[q].ResourceId, biAmountF3);

                // Debug.Log($"Planet[{planetData.Id}] : RSC[{planetData.Obtainables[q].ResourceId}]=AmountF3[{biAmountF3}] has mined with Rate[{miningRateWithBooster}], times[{count}]");
            }
        }

        void PlanetComp_OnClicked(int zoneId, int planetId)
        {
            Debug.Log($"On Planet Clicked!! - [{zoneId}]-[{planetId}]");
            if (View == null)
                return;

            var planetProcInfo = Model.PlayerData.GetPlanetInfo(zoneId, planetId);
            if((planetProcInfo!=null && planetProcInfo.IsUnlocked) || Model.UnlockPlanet(zoneId, planetId))
                Context.RequestQuery("MiningStat", "Attach", (errMsg, ret) => { }, zoneId, planetId);
            else
            {
                var presentInfo = new ToastMessageDialog.PresentInfo( message :  "Insufficient GOLD.", duration:1.5f );
                context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GAME_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                    "ToastMessageDialog", presentInfo,
                    new Action<APopupDialog>( (popupDlg) => 
                    { 
                        Debug.Log("ToastMessage Dialog has been closed.");
                    } ) ); 
            }
        }

        // Arrival Callback.
        // => Obeys Mining Pump logic.
        //
        void PlanetComponent_EventOnArrivalAtTown(int zoneId, int planetId)
        {
            if(!bEnableMiningProcess)   return;

            var townId = planetId;
            var townData = Model.GetPlanetData(zoneId, townId);
            if (townData == null)
            {
                Assert.IsTrue(false, "Couldn't find a town data ... " + townId.ToString());
                return;
            }

            PlanetInfo visiblePlanet = Model.PlayerData.GetPlanetInfo(zoneId, townId);
            Assert.IsNotNull(visiblePlanet);

            // reset. 
            visiblePlanet.DeliveryInfo.Clear();


            // Mining Mats in the town => Loading into Delivery Cargo.
            DeliveryInfo deliveryInfo = new DeliveryInfo();
            for (int q = 0; q < townData.Obtainables.Count; ++q)
            {
                float cargo = Model.GetPlanetStat(zoneId, townId, eABILITY.CARGO_SIZE) * townData.Obtainables[q].Yield;
                BigInteger deliveredAmountF3 = CalculateDeliveredAmountx1000(cargo, GetMinedResourceAmount(zoneId, townId, townData.Obtainables[q].ResourceId, isX1000: true));

                BigInteger biDeliveredAmountF3 = deliveredAmountF3 * -1;
                Model.PlayerData.UpdateMiningResourceAmountX1000(zoneId, townId, townData.Obtainables[q].ResourceId, biDeliveredAmountF3);
                deliveryInfo.Set(deliveredAmountF3, .0f, _isHeadingPlanet: false);
                visiblePlanet.DeliveryInfo.Add(deliveryInfo);
            }
        }
        //
        //
        void PlanetComponent_EventOnArrivalAtStation(int zoneId, int townId)
        {
            if(!bEnableMiningProcess)   return;

            var townData = Model.GetPlanetData(zoneId, townId);
            if (townData == null)
            {
                Assert.IsTrue(false, "Couldn't find a town data ... " + townId.ToString());
                return;
            }

            PlanetInfo visiblePlanet = Model.PlayerData.GetPlanetInfo(zoneId, townId);
            Assert.IsNotNull(visiblePlanet);

            // Cargo Rscs => Unload onto the main-demon.
            Assert.IsTrue(visiblePlanet.DeliveryInfo!=null && visiblePlanet.DeliveryInfo.Count>0);
            for (int q = 0; q < visiblePlanet.DeliveryInfo.Count; ++q)
            {
                context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => { }, 
                    townData.Obtainables[q].ResourceId, (BigInteger)visiblePlanet.DeliveryInfo[q].Amountx1000, true);
                
                context.RequestQuery("Resource", "TryAutoSell", (errMsg, ret) => { }, townData.Obtainables[q].ResourceId);
            }
        }

        void PlanetComponent_EventOnManualBuffClicked(int zoneId, int planetId)
        {
            // Need to Get Param Data from Skill Tree.
            //
            //
            Model.TriggerManualBooster(zoneId, planetId);
        }

        long CalculateMiningAmountx1000(float second, float miningRatePerSec, float yield)
        {
            float miningAmountF3 = 1000.0f * miningRatePerSec * second * yield;
            return (long)(miningAmountF3);
        }

        BigInteger CalculateDeliveredAmountx1000(float speed, float distance, float second, float cargoCapacity, BigInteger minedAmountF3)
        {
            // v = s/t : t = s/v
            // 
            float shipDuration = (distance * 2.0f) / speed;
            float totalShipCount = second / shipDuration;

            BigInteger deliverableAmountF3 = (BigInteger)(cargoCapacity * totalShipCount * 1000.0f);

            // Debug.Log($"shipSpeed[{speed}], duration:[{second}], shipCount[{totalShipCount}], cargo[{deliverableAmountF3}], obtainedCountx1000[{minedAmountF3}]");

            return deliverableAmountF3<minedAmountF3 ? deliverableAmountF3 : minedAmountF3;
        }

        BigInteger CalculateDeliveredAmountx1000(float cargoCapacity, BigInteger minedAmountF3)
        {
            BigInteger deliverableAmountF3 = (BigInteger)(cargoCapacity * 1000.0f);
            return deliverableAmountF3<minedAmountF3 ? deliverableAmountF3 : minedAmountF3;
        }

        List<SimpleBoosterInfo> BuildCompareBoosterData()
        {   
            /*UsingBoosterInfo MRBooster = Model.GetUsingBoosterByType(eBoosterType.MINING_RATE);
            UsingBoosterInfo SIBooster = Model.GetUsingBoosterByType(eBoosterType.SHOT_INTERVAL);
            UsingBoosterInfo SABooster = Model.GetUsingBoosterByType(eBoosterType.SHOT_ACCURACY);

            List<SimpleBoosterInfo> listBoosters = new List<SimpleBoosterInfo>();
            if (MRBooster != null)  listBoosters.Add(new SimpleBoosterInfo(eBoosterType.MINING_RATE, MRBooster.RemainTime, Model.GetBoosterInfo(MRBooster.BoosterId).BoostingRate));
            else                    listBoosters.Add(new SimpleBoosterInfo(eBoosterType.MINING_RATE, 0, 1.0f));
            if (SIBooster != null)  listBoosters.Add(new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, SIBooster.RemainTime, Model.GetBoosterInfo(SIBooster.BoosterId).BoostingRate));
            else                    listBoosters.Add(new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, 0, 1.0f));
            if (SABooster != null)  listBoosters.Add(new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, SABooster.RemainTime, Model.GetBoosterInfo(SABooster.BoosterId).BoostingRate));
            else                    listBoosters.Add(new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, 0, 1.0f));

            return listBoosters;
            */
            return null;
        }

        void InitiateFiringBullets()
        {
            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];

                List<PlanetInfo> listPlanets = zoneInfo.Planets;
                for(int q = 0; q < listPlanets.Count; ++q)
                {
                    PlanetInfo visiblePlanet = listPlanets[q];
                    if (!visiblePlanet.IsUnlocked)
                        continue;

                    TriggerBullets(zoneInfo.ZoneId, visiblePlanet.PlanetId);
                }
            }
        }

        void TriggerBullets(int zoneId, object data)
        {
            if(!bEnableMiningProcess)    return;

            int planetId = (int)data;
            var planetComp = View.PlanetsComp.GetPlanetComp(zoneId, planetId);
            Assert.IsNotNull(planetComp);

            PlanetInfo planetInfo = Model.PlayerData.GetPlanetInfo(zoneId, planetId);
            if (planetInfo == null || !planetInfo.IsUnlocked)
                return;

            if(Model.IsPlanetBattleMode(zoneId, planetId))
            {
                Assert.IsNotNull(planetInfo.BattleInfo);
                if (planetInfo.BattleInfo.IsCleared)
                    return;
            }

            float shotInterval = Model.GetPlanetLevelStat(zoneId, planetId, eABILITY.SHOT_INTERVAL);
            shotInterval = UnityEngine.Random.Range(shotInterval - FIRE_TIME_FLEXIBILITY, shotInterval + FIRE_TIME_FLEXIBILITY);
            shotInterval /= Model.GetBuffRatioByAbility(zoneId, planetId, eABILITY.SHOT_INTERVAL);
            //float shotInterval = Model.GetStatValue(planetId, eABILITY.SHOT_INTERVAL);

            shotInterval = UnityEngine.Random.Range(shotInterval - FIRE_TIME_FLEXIBILITY, shotInterval + FIRE_TIME_FLEXIBILITY);

            DelayedAction.TriggerActionWithDelay( view, shotInterval, (data) =>
            {
                if(!bEnableMiningProcess)    return;

                const float viewAdjustments = 0.01f;                
                UnityEngine.Vector3 vTo = new UnityEngine.Vector3(
                                planetComp.transform.position.x + viewAdjustments * UnityEngine.Random.Range(-1.0f, 1.0f),
                                planetComp.transform.position.y + viewAdjustments * UnityEngine.Random.Range(-1.0f, 1.0f),
                                planetComp.transform.position.z);
                
                int _planetId = (int)data;
                var gameConfig = (GameConfig)context.GetData("GameConfig", null);
                Assert.IsNotNull(gameConfig);

                View.TriggerMagicBall(vTo, velocity:gameConfig.SpeedOfMiningBall, _planetId, (sndData) =>
                {
                    int planetIddd = (int)sndData;

                    PlanetInfo info = Model.PlayerData.GetPlanetInfo(zoneId, planetIddd);
                    if (info == null || !info.IsUnlocked)
                        return;

                    float accuracy = Model.GetStatValue(zoneId, planetId, eABILITY.SHOT_ACCURACY);
                    float mineRate = Model.GetStatValue(zoneId, planetId, eABILITY.MINING_RATE);      // 0 ~ 
                    if (Model.IsPlanetBattleMode(zoneId, planetIddd))
                    {
                        Assert.IsNotNull(info.BattleInfo);
                        if(!info.BattleInfo.IsCleared)
                            this.AttackAtPlanetBattle(zoneId, planetIddd, mineRate * accuracy);
                    }
                    else
                        this.MineResourcesAtPlanet(zoneId, planetIddd, mineRate * accuracy);
                });

                // Reserve the next shot for the planet. 
                TriggerBullets(zoneId, _planetId);

            }, planetId);
        }

        // play ground stuff.
        void RefreshPlanetComp(int zoneId, int planetId)
        {
            // How to Refresh Town Comp....
            var planetComp = View.PlanetsComp.GetPlanetComp(zoneId, planetId);
            Assert.IsNotNull(planetComp);

            PlanetData planetData = Model.GetPlanetData(zoneId, planetId);
            Assert.IsNotNull(planetData);

            PlanetInfo visiblePlanet = Model.PlayerData.GetPlanetInfo(zoneId, planetData.Id);
            if (visiblePlanet == null)
                return;

            _refreshPlanetComp(zoneId, visiblePlanet, planetData, planetComp, GetManagerSpriteOnThePlanet(planetData.Id));
        }

        // Visible planet info.
        Dictionary<int, List<MiningZoneComp.BaseInfo>> BuildPlanetBaseInfoDictionary()
        {
            var dictPlanetData = new Dictionary<int, List<MiningZoneComp.BaseInfo>>();

            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                List<MiningZoneComp.BaseInfo> listPlanetsInfo = new List<MiningZoneComp.BaseInfo>();
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];    
                for(int q = 0; q < zoneInfo.Planets.Count; ++q)
                {
                    PlanetData planetData = Model.GetPlanetData(zoneInfo.ZoneId, zoneInfo.Planets[q].PlanetId);
                    Assert.IsNotNull(planetData, $"Invalid Zone/Planet Id....[{zoneInfo.ZoneId}], [{zoneInfo.Planets[q].PlanetId}]");

                    PlanetInfo visiblePlanet = Model.PlayerData.GetPlanetInfo(zoneInfo.ZoneId, planetData.Id);
                    if (visiblePlanet == null)
                        continue;

                    // WHY ?
                    MiningZoneComp.BaseInfo baseInfo;
                    if (visiblePlanet.IsUnlocked)
                    {
                        float shipSpeed = Model.GetStatValue(zoneInfo.ZoneId, planetData.Id, eABILITY.DELIVERY_SPEED);
                        baseInfo = new MiningZoneComp.BaseInfo(planetData.Name, shipSpeed, GetManagerSpriteOnThePlanet(planetData.Id));
                    }
                    else
                    {
                        baseInfo = new MiningZoneComp.BaseInfo(planetData.BIOpenCost.ToAbbString());
                    }
                    listPlanetsInfo.Add(baseInfo);
                    //dictPlanetData.Add(planetData.Id, baseInfo);
                }
                dictPlanetData.Add(zoneInfo.ZoneId, listPlanetsInfo);
            }
            return dictPlanetData;
        }

        void _refreshPlanetComp(int zoneId, PlanetInfo planetInfo, PlanetData planetData, PlanetBaseComp planetComp, Sprite managerSprite)
        { 
            Assert.IsNotNull(planetInfo);
            Assert.IsNotNull(planetData);
            Assert.IsNotNull(planetComp);

            PlanetBossData bossData = planetData as PlanetBossData;
            bool isMiningPlanet = bossData == null;
            
            IGCore.MVCS.AView.APresentor presentInfo;
            if (isMiningPlanet)
            {
                float shipSpeed = Model.GetStatValue(zoneId, planetInfo.PlanetId, eABILITY.DELIVERY_SPEED);
                if (planetInfo.IsUnlocked)
                {
                    BoosterComp.PresentInfo boostPresentInfo = null;
                    if(planetInfo.IsBoosterUnlocked)
                    {
                        if(planetInfo.IsBoosterReadyToRun)
                            boostPresentInfo = new BoosterComp.PresentInfo();
                        else
                        {
                            float remainSec = planetInfo.BoostRemainTimeInSec;
                            float durationSec = planetInfo.BoostState == PlanetInfo.BOOST_STATE.Boosting ? planetInfo.BoostingDuration : planetInfo.BoostCoolTimeDuration;
                            boostPresentInfo = new BoosterComp.PresentInfo(remainSec, durationSec, planetInfo.BoostState == PlanetInfo.BOOST_STATE.Boosting);
                        }
                    }

                    presentInfo = new PlanetComp.PresentInfo($"[{zoneId}-{planetInfo.PlanetId}]\n{planetData.Name}", shipSpeed * View.SpeedVisualAdjustmentRate, managerSprite, boostPresentInfo);
                }
                else
                    presentInfo = new PlanetComp.PresentInfo(planetData.BIOpenCost.ToAbbString());
            }
            else
            {
                if (planetInfo.IsUnlocked)
                {
                    if (planetInfo.BattleInfo.IsCleared)
                    {
                        presentInfo = new PlanetBossComp.PresentInfo(planetData.Name, dontCare: true);
                    }
                    else
                    {
                        long remain = (long)bossData.BattleDuration - planetInfo.GetSecondFromEvent();
                        BigInteger biTotal = new BigInteger(bossData.Life);
                        float lifeRate = ((float)(biTotal - planetInfo.BattleInfo.BIDamage)) / (float)biTotal;
                        presentInfo = new PlanetBossComp.PresentInfo(planetData.Name, TimeExt.ToTimeString(remain, TimeExt.UnitOption.NO_USE, TimeExt.TimeOption.HOUR), lifeRate, managerSprite);
                    }
                }
                else
                    presentInfo = new PlanetBossComp.PresentInfo(planetData.BIOpenCost.ToAbbString());
            }
            planetComp.Refresh(presentInfo);
        }
        

        BigInteger GetMinedResourceAmount(int zoneId, int planetId, string resourceId, bool isX1000=false)
        {
            MinedResourceInfo info = null;
            context.RequestQuery("GamePlay", "PlayerData.GetMinedResourceInfo", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                info = (MinedResourceInfo) ret; 

            }, zoneId, planetId, resourceId);
 
            if (info == null)
                return BigInteger.Zero;

            return isX1000 ? info.BICountF3 : info.BICount;
        }


        Sprite GetManagerSpriteOnThePlanet(int planetId)
        {
            Sprite spriteMng = null;
         /*   OwnedManagerInfo ownedMngInfo = Model.PlayerData.GetAssignedManagerInfoForPlanet(planetId);
            if(ownedMngInfo != null)
            {
            //    ManagerInfo mngInfo = Model.GetManagerInfo(ownedMngInfo.ManagerId);   
          //      spriteMng = mngInfo != null ? View.GetManagerSprite(mngInfo.SpriteKey) : null;
            }
            return spriteMng;*/

            return null;
        }
        #endregion ===> Helpers


        #region ===> Events

        void OnPlanetBattleCleared(object data)
        {
            RefreshView(data);
        }

        void PlayerData_OnPlanetUnlocked(object data)
        {
#if UNITY_EDITOR
            if(context.IsSimulationMode())
                return;
#endif
            // Release Holding Process when any planet unlock.
            bEnableMiningProcess = true;

            int zoneId = ((Tuple<int, int>)data).Item1;
            int planetId = ((Tuple<int, int>)data).Item2;
            
            TriggerBullets(zoneId, planetId);
            RefreshView(data);
        }

        void PlayerData_OnPlanetClosed(object data)
        {
            RefreshView(data);
        }

        /*
        void OnVisiblePlanetAdded(object data)
        {
            int zoneId = ((Tuple<int, int>)data).Item1;
            int planetId = ((Tuple<int, int>)data).Item2;

            // Planets Visibility Update.
            View.StartCoroutine(View.coTriggerActionWithDelay(0.3f, () =>
            {
                View.PlanetsComp.UpdateArea(BuildPlanetBaseInfoDictionary());
            }) );
        }*/

        void PlayerData_OnStatupgraded(object data)
        {
            RefreshView(data);
        }

        #endregion


        #region SKILL_RELATED.

        // ### : This helps interface's accessorbility to like private.
        void ISkillLeaner.CreateSkillBehaviors()
        {
            createSkillBehaviorInternal();
        }

        void ISkillLeaner.LearnSkill(string skill_id, string ability_id, string ability_param)
        {
            if(dictSkillBehaviors.ContainsKey(ability_id))
            {
                dictSkillBehaviors[ability_id].Learn(this, ability_param);

                string strTime = $"===> [{(string)context.GetData("SimTime", string.Empty)}]";
                Debug.Log($"<color=green>[Skill] Learning Skill...{skill_id}:{ability_id}:{ability_param} {strTime}</color> ");
            }
        }

        protected virtual void createSkillBehaviorInternal()
        {
            dictSkillBehaviors.Clear();

            dictSkillBehaviors.Add(ZoneOpenSkill.Id.ToLower(), new ZoneOpenSkill());
            dictSkillBehaviors.Add(ZoneBuffSkill.Id.ToLower(), new ZoneBuffSkill());
            dictSkillBehaviors.Add(GameResetSkill.Id.ToLower(), new GameResetSkill());
        }

        void EventOnSkillLearned(object data)
        {
            if (data == null) return;

            Tuple<string, string, string, bool> skill_id_n_ability_id_param = (Tuple< string, string, string, bool>)data;
            string skill_id = skill_id_n_ability_id_param.Item1;
            string abilityId = skill_id_n_ability_id_param.Item2.ToLower();
            string abilityParam = skill_id_n_ability_id_param.Item3.ToLower();
            bool isSaveData = skill_id_n_ability_id_param.Item4;

            ((ISkillLeaner)this).LearnSkill(skill_id, abilityId, abilityParam);

            if(isSaveData)  WriteData();
        }
        
        void EventOnRefreshAllView(object data)
        {
            // RefreshView(data);
        }
        void EventOnZoneUnlocked(object data)
        {
            int zoneId = (int)data;
            Debug.Log($"<color=green>[LearnSkill] Zone {zoneId} Unlocked ! </color>");

            if(!context.IsSimulationMode())
            {
                const float shortDelay = 0.1f;
                View.StartCoroutine(View.coTriggerActionWithDelay(shortDelay, () =>
                {
                    View.PlanetsComp.UpdateArea(BuildPlanetBaseInfoDictionary());
                }) );
            }
        }

        #endregion

        #region ===> Requests.

        object getPlanetSprite(params object[] data)
        {
            if(data.Length < 2)
                return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];

            return View.GetPlanetSprite(zoneId, planetId);
        }

        #endregion


        #region SIMULATOR

        void SIM_OpenPlanetIfPossible()
        {
            // Debug.Log("[SIM] : Collecting Planet Info...");
            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];

                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                {
                    PlanetInfo planetInfo = zoneInfo.Planets[k];
                    if (planetInfo == null || (planetInfo!=null && planetInfo.IsUnlocked))
                        continue;
                    
                    bool ret = Model.UnlockPlanet(zoneInfo.ZoneId, planetInfo.PlanetId);
                    string strColor = ret ? "<color=green>" : "<color=white>";
                    Debug.Log($"{strColor}[SIM][Action] : Trying unlock planet... Id[{planetInfo.PlanetId}], Outcome[{ret}]</color>");
                }
            }
        }

        class SimLevelStat
        {
            public SimLevelStat(eABILITY ability, int level) { this.ability = ability; this.level = level; }
            public int level;
            public eABILITY ability;

        }

        void SIM_UpgradeStatIfPossible()
        {
            float desiredProductionRate = (float)context.GetData("DesiredPDR", 0.1f);
            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];

                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                {
                    PlanetInfo planetInfo = zoneInfo.Planets[k];
                    if (planetInfo == null || (planetInfo!=null && false==planetInfo.IsUnlocked))
                        continue;
                    
                    var planetData = Model.GetPlanetData(zoneInfo.ZoneId, planetInfo.PlanetId);
                    if (planetData == null)
                        continue;

                    // Check PDR to see if this planet still needs to be upgraded. 
                    bool needUpgrade = false;
                    for (int q = 0; q < planetData.Obtainables.Count; ++q)
                    {
                        float productionRate = .0f;
                        context.RequestQuery("GamePlay", "GetProductionRate", (errMsg, ret) => 
                        { 
                            productionRate = (float)ret;

                        }, planetData.Obtainables[q].ResourceId);

                        if(productionRate < desiredProductionRate)
                        {
                            needUpgrade = true;
                            break;
                        }
                    }
                    if(!needUpgrade)        continue;

                    // Sort ability by level.- Acending Order so that small level can have priority. 
                    List<SimLevelStat> stats = new List<SimLevelStat>();
                    for(int q = 0; q < (int)eABILITY.MAX; ++q)
                        stats.Add(new SimLevelStat((eABILITY)q, planetInfo.Level[q]));
                    stats.Sort((a, b) =>
                    {
                        if(a.level < b.level) return -1;
                        else if(a.level > b.level) return 1;
                        return 0;
                    });


                    for(int q = 0; q < (int)eABILITY.MAX; ++q)
                    {
                        eABILITY curAbility = stats[q].ability;

                        // Debug.Log($"[SIM] : zoneId:[{zoneInfo.ZoneId}], planetId:[{planetInfo.PlanetId}], stat:[{curAbility}], level:[{stats[q].level}]");
                        bool upgraded = Model.UpgradePlanet(zoneInfo.ZoneId, planetInfo.PlanetId, curAbility);
                        if(upgraded)
                            Debug.Log($"<color=green>[SIM] : zoneId:[{zoneInfo.ZoneId}], planetId:[{planetInfo.PlanetId}], stat:[{curAbility}] upgraded.{planetInfo.Level[q]} </color>");
                    }
                }
            }
        }

        void SIM_LogSimulationStat()
        {
            // Debug.Log("[SIM] : Collecting Planet Info...");
            for(int w = 0; w < Model.PlayerData.UnlockedZoneGroup.Zones.Count; ++w)
            {
                ZoneStatusInfo zoneInfo = Model.PlayerData.UnlockedZoneGroup.Zones[w];

                for (int k = 0; k < zoneInfo.Planets.Count; ++k)
                {
                    PlanetInfo planetInfo = zoneInfo.Planets[k];
                    if (planetInfo == null)
                        continue;

                    PlanetData planetData = Model.GetPlanetData(zoneInfo.ZoneId, planetInfo.PlanetId);
                    if (planetData == null)
                        continue;

                    Debug.Log($"[SIM][Status] : ZoneId[{zoneInfo.ZoneId}], PlanetId[{planetData.Id}], Unlocked:[{planetInfo.IsUnlocked}]");
                }
            }
        }

        #endregion





        #region ===> UNIT_TEST_PART

        //==========================================================================
        //
        // Unit Test Codes.
        //
        //
        void TestMingPumpResumeLogic()
        {
            Assert.IsTrue(CalculateMiningAmountx1000(second: 10.0f, miningRatePerSec: 1.0f, yield: 1.0f) == 10000);
            Assert.IsTrue(CalculateMiningAmountx1000(second: 10.0f, miningRatePerSec: 0.5f, yield: 1.0f) == 5000);
            Assert.IsTrue(CalculateMiningAmountx1000(second: 10.0f, miningRatePerSec: 0.5f, yield: 0.5f) == 2500);

            Assert.IsTrue(CalculateDeliveredAmountx1000(speed: 1.0f, distance: 10.0f, second: 20.0f, cargoCapacity: 1.0f, minedAmountF3: 1) == 1);
            Assert.IsTrue(CalculateDeliveredAmountx1000(speed: 2.0f, distance: 10.0f, second: 10.0f, cargoCapacity: 1.0f, minedAmountF3: 1) == 1);
            Assert.IsTrue(CalculateDeliveredAmountx1000(speed: 1.0f, distance: 10.0f, second: 20.0f, cargoCapacity: 1.0f, minedAmountF3: 50) == 50);
            Assert.IsTrue(CalculateDeliveredAmountx1000(speed: 1.0f, distance: 10.0f, second: 20.0f, cargoCapacity: 2.0f, minedAmountF3: 50000) == 2000);
            Assert.IsTrue(CalculateDeliveredAmountx1000(speed: 1.0f, distance: 10.0f, second: 20.0f, cargoCapacity: 10.0f, minedAmountF3: 500) == 500);
            Assert.IsTrue(CalculateDeliveredAmountx1000(speed: 1.0f, distance: 10.0f, second: 40.0f, cargoCapacity: 3.0f, minedAmountF3: 8888500) == 6000);

            TestMiningSections();


            Debug.Log("===== Mining Pump Resume Logic Test has been PASSED Successfully !!! =====");
        }

        void TestMiningSections()
        { 
            // Case 1. Using buff evenly.
            List<SimpleBoosterInfo> listBoosters = new List<SimpleBoosterInfo>()
            {
                new SimpleBoosterInfo(eBoosterType.MINING_RATE,   _remainTime:2.0f, 2.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, _remainTime:4.0f, 3.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, _remainTime:8.0f, 4.0f)
            };
            List<ResumeBlockWithBoostInfo> listRet = SplitResumeSectorFromBooster(duration:10.0f, listBoosters);
            Assert.IsTrue(listRet.Count == 4);
            Assert.IsTrue(listRet[0].duration == 2.0f && listRet[0].MRRate == 1.0f && listRet[0].SIRate == 1.0f && listRet[0].SARate == 1.0f);
            Assert.IsTrue(listRet[1].duration == 4.0f && listRet[1].MRRate == 1.0f && listRet[1].SIRate == 1.0f && listRet[1].SARate == 4.0f);
            Assert.IsTrue(listRet[2].duration == 2.0f && listRet[2].MRRate == 1.0f && listRet[2].SIRate == 3.0f && listRet[2].SARate == 4.0f);
            Assert.IsTrue(listRet[3].duration == 2.0f && listRet[3].MRRate == 2.0f && listRet[3].SIRate == 3.0f && listRet[3].SARate == 4.0f);


            // Case 2. Using only MiningRate & SInterval.
            listBoosters = new List<SimpleBoosterInfo>()
            {
                new SimpleBoosterInfo(eBoosterType.MINING_RATE,   _remainTime:8.0f, 2.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, _remainTime:4.0f, 3.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, _remainTime:0.0f, 1.0f)     // don't use accuracy buff.
            };
            listRet = SplitResumeSectorFromBooster(duration: 10.0f, listBoosters);
            Assert.IsTrue(listRet.Count == 3);
            Assert.IsTrue(listRet[0].duration == 2.0f && listRet[0].MRRate == 1.0f && listRet[0].SIRate == 1.0f && listRet[0].SARate == 1.0f);
            Assert.IsTrue(listRet[1].duration == 4.0f && listRet[1].MRRate == 2.0f && listRet[1].SIRate == 1.0f && listRet[1].SARate == 1.0f);
            Assert.IsTrue(listRet[2].duration == 4.0f && listRet[2].MRRate == 2.0f && listRet[2].SIRate == 3.0f && listRet[2].SARate == 1.0f);


            // Case 3. No Buff is in use.
            listBoosters = new List<SimpleBoosterInfo>()
            {
                new SimpleBoosterInfo(eBoosterType.MINING_RATE,   _remainTime:0.0f, 1.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, _remainTime:0.0f, 1.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, _remainTime:0.0f, 1.0f)  
            };
            listRet = SplitResumeSectorFromBooster(duration: 10.0f, listBoosters);
            Assert.IsTrue(listRet.Count == 1);
            Assert.IsTrue(listRet[0].duration == 10.0f && listRet[0].MRRate == 1.0f && listRet[0].SIRate == 1.0f && listRet[0].SARate == 1.0f);


            // Case 4. using 3 buffs and remain time is bigger than duration itself.
            listBoosters = new List<SimpleBoosterInfo>()
            {
                new SimpleBoosterInfo(eBoosterType.MINING_RATE,   _remainTime:12.0f, 2.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, _remainTime:15.0f, 3.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, _remainTime:58.0f, 4.0f)
            };
            listRet = SplitResumeSectorFromBooster(duration: 10.0f, listBoosters);
            Assert.IsTrue(listRet.Count == 1);
            Assert.IsTrue(listRet[0].duration == 10.0f && listRet[0].MRRate == 2.0f && listRet[0].SIRate == 3.0f && listRet[0].SARate == 4.0f);


            // Case 5. using 1 buff and remain time is bigger than duration itself.
            listBoosters = new List<SimpleBoosterInfo>()
            {
                new SimpleBoosterInfo(eBoosterType.MINING_RATE,   _remainTime:0.0f, 1.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, _remainTime:0.0f, 1.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, _remainTime:58.0f, 4.0f)
            };
            listRet = SplitResumeSectorFromBooster(duration: 10.0f, listBoosters);
            Assert.IsTrue(listRet.Count == 1);
            Assert.IsTrue(listRet[0].duration == 10.0f && listRet[0].MRRate == 1.0f && listRet[0].SIRate == 1.0f && listRet[0].SARate == 4.0f);


            // Case 6. 
            listBoosters = new List<SimpleBoosterInfo>()
            {
                new SimpleBoosterInfo(eBoosterType.MINING_RATE,   _remainTime:0.0f, 1.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, _remainTime:4.0f, 2.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, _remainTime:58.0f, 4.0f)
            };
            listRet = SplitResumeSectorFromBooster(duration: 10.0f, listBoosters);
            Assert.IsTrue(listRet.Count == 2);
            Assert.IsTrue(listRet[0].duration == 6.0f && listRet[0].MRRate == 1.0f && listRet[0].SIRate == 1.0f && listRet[0].SARate == 4.0f);
            Assert.IsTrue(listRet[1].duration == 4.0f && listRet[1].MRRate == 1.0f && listRet[1].SIRate == 2.0f && listRet[1].SARate == 4.0f);


            // Case 7. 
            listBoosters = new List<SimpleBoosterInfo>()
            {
                new SimpleBoosterInfo(eBoosterType.MINING_RATE,   _remainTime:0.0f, 1.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_INTERVAL, _remainTime:22.0f, 2.0f),
                new SimpleBoosterInfo(eBoosterType.SHOT_ACCURACY, _remainTime:2.0f, 4.0f)
            };
            listRet = SplitResumeSectorFromBooster(duration: 10.0f, listBoosters);
            Assert.IsTrue(listRet.Count == 2);
            Assert.IsTrue(listRet[0].duration == 8.0f && listRet[0].MRRate == 1.0f && listRet[0].SIRate == 2.0f && listRet[0].SARate == 1.0f);
            Assert.IsTrue(listRet[1].duration == 2.0f && listRet[1].MRRate == 1.0f && listRet[1].SIRate == 2.0f && listRet[1].SARate == 4.0f);
        }
        #endregion
    }
}