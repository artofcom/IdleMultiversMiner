// using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.Common;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using IGCore.PlatformService;

namespace App.GamePlay.IdleMiner.GamePlay
{
    internal class GamePlayPlayerModel : MultiGatewayWritablePlayerModel
    {
        #region ===> Properties

        const string EnvironmentDataKey = "Environment";
        string DateKey_ZoneInfo => $"{nameof(GamePlayPlayerModel)}_{nameof(ZoneGroupStatusInfo)}";

        ZoneGroupStatusInfo unlockedZoneGroup = new ZoneGroupStatusInfo();

        // Accessor.
        public ZoneGroupStatusInfo UnlockedZoneGroup => unlockedZoneGroup;
        //static string DataKey_PlanetData => $"{IdleMinerContext.GameKey}_{IdleMinerContext.AccountName}_PlanetData";
        
        EventsGroup Events = new EventsGroup();

        #endregion




        #region ===> Interfaces

        public GamePlayPlayerModel(AContext ctx, List<IDataGatewayService> gatewayService) : base(ctx, gatewayService) { }


        public override void Init()
        {
            base.Init();

            InitPlayerData();
            RegisterRequestables();

            ZoneStatusInfo.OnManualBoosterFinished += ZoneStatusInfo_OnManualBoosterFinished;
            
            IsInitialized = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            unlockedZoneGroup?.Dispose();
            unlockedZoneGroup = null;
            UnregisterRequestables();

            ZoneStatusInfo.OnManualBoosterFinished -= ZoneStatusInfo_OnManualBoosterFinished;

            IsInitialized = false;
        }

        public void Pump()
        {
            unlockedZoneGroup.Pump();
        }

        public bool UnlockPlanet(int zoneId, int planetId)//, float distance)
        {
            var zoneInfo = GetUnlockedZoneStatusInfo(zoneId);
            if (zoneInfo == null)  return false;
            
            var planetInfo = zoneInfo.GetPlanetInfo(planetId);
            if(planetInfo == null) return false;

            if(!planetInfo.IsUnlocked)
            {
                planetInfo.OpenPlanet();
                SetDirty();
                (context as IdleMinerContext).SavePlayerDataInstantly();
                return true;
            }

            // Fire Event ?
            return false;
        }
        public bool UnlockZone(int zoneId, List<int> planetIds, List<float> distanceList)
        {
            var zoneInfo = GetUnlockedZoneStatusInfo(zoneId);
            if(zoneInfo != null)    // Already Opened.
                return false;
            
            unlockedZoneGroup.Add(zoneId, planetIds, distanceList);
            EventSystem.DispatchEvent(EventID.ZONE_UNLOCKED, zoneId);
            SetDirty();
            (context as IdleMinerContext).SavePlayerDataInstantly();
            return true;
        }

        public bool UpgradePlanet(int zoneId, int planetId, eABILITY stat, BigInteger cost, int offsetUpLevel)
        {   
            var info = GetPlanetInfo(zoneId, planetId);
            if (info == null)
                return false;

            info.Upgrade(stat, offsetUpLevel);
            EventSystem.DispatchEvent(EventID.MINING_STAT_UPGRADED, new Tuple<int, int>(zoneId, planetId));
            SetDirty();
            (context as IdleMinerContext).SavePlayerDataInstantly();
            return true;
        }

        public void ResetPlanetStat(int zoneId, int planetId, eABILITY stat)
        {
            var info = GetPlanetInfo(zoneId, planetId);
            if (info == null)
                return;

            info.ResetStat(stat);
            SetDirty();
            (context as IdleMinerContext).SavePlayerDataInstantly();
            EventSystem.DispatchEvent(EventID.MINING_STAT_RESET, new Tuple<int, int>(zoneId, planetId));
        }

        public ZoneStatusInfo GetUnlockedZoneStatusInfo(int zoneId)
        {
            return unlockedZoneGroup.GetZoneStatusInfo(zoneId);
        }

        public PlanetInfo GetPlanetInfo(int zoneId, int planetId)
        {
            var zoneGroupInfo = unlockedZoneGroup.GetZoneStatusInfo(zoneId);
            if(zoneGroupInfo == null)   
                return null;        // Locked Zone == Invisible.

            return zoneGroupInfo.GetPlanetInfo(planetId);
        }

        // zoneid, planetId
        public (int, int) GetNeighborData(int zoneId, int planetId, bool isUp)
        {
            var zoneInfo = unlockedZoneGroup.GetZoneStatusInfo(zoneId);
            if(zoneInfo == null)
            {
                Assert.IsTrue(false, "Invalid Zone... " + zoneId);
                return (0, 0);
            }
            
            var nextPlanetInfo = zoneInfo.GetNeighborData(planetId, isUp, isCircle:false);
            if(nextPlanetInfo != null)
                return (zoneId, nextPlanetInfo.PlanetId);

            var nextZoneInfo = unlockedZoneGroup.GetNeighborData(zoneId, isUp, isCircle:true);
            if(nextZoneInfo != null)
            {
                nextPlanetInfo = isUp ? nextZoneInfo.Planets[0] : nextZoneInfo.Planets[ nextZoneInfo.Planets.Count-1 ];
                return (nextZoneInfo.ZoneId, nextPlanetInfo.PlanetId);
            }

            nextPlanetInfo = zoneInfo.GetNeighborData(planetId, isUp, isCircle:true);
            if(nextPlanetInfo != null)
                return (zoneId, nextPlanetInfo.PlanetId);
           
            Assert.IsTrue(false, "Invalid Zone...?? " + zoneId);
            return (0, 0);
        }

        public MinedResourceInfo GetMinedResourceInfo(int zoneId, int planetId, string resourceId)
        {
            PlanetInfo planetInfo = GetPlanetInfo(zoneId, planetId);
            if (planetInfo == null)
                return null;        // Locked Zone == Invisible.

            for (int q = 0; q < planetInfo.MinedRscInfo.Count; ++q)
            {
                if (planetInfo.MinedRscInfo[q].ResourceId == resourceId)
                    return planetInfo.MinedRscInfo[q];
            }
            return null;
        }

        public void UpdateMiningResourceAmountX1000(int zoneId, int planetId, string resourceId, BigInteger offsetX1000)
        {
            MinedResourceInfo info = GetMinedResourceInfo(zoneId, planetId, resourceId);
            if (info != null)
                info.BICountF3 += offsetX1000;
            else
            {
                PlanetInfo planetInfo = GetPlanetInfo(zoneId, planetId);
                if (planetInfo == null)
                    return;

                planetInfo.MinedRscInfo.Add(new MinedResourceInfo(resourceId, offsetX1000));
            }
            SetDirty();
        }
        
        public void UpdateDamageX1000(int zoneId, int planetId, BigInteger offsetDamageX1000)
        {
            PlanetInfo planetInfo = GetPlanetInfo(zoneId, planetId);
            if (planetInfo == null)
                return;

            planetInfo.UpdateDamageX1000(offsetDamageX1000);
            SetDirty();
            EventSystem.DispatchEvent(EventID.PLANET_DAMAGED, planetId);
        }

        //public void WriteData()
        //{
            //SavePlanetData();
        //}

        public void UnlockZoneBooster(int zoneId, float duration, float buffRate, float coolTimeDuration)
        {
            SetDirty();
            (context as IdleMinerContext).SavePlayerDataInstantly();
            unlockedZoneGroup.UnlockMiningBooster(zoneId, duration, buffRate, coolTimeDuration);
        }

        public bool TriggerPlanetBooster(int zoneId, int planetId)
        {
            PlanetInfo planetInfo = GetPlanetInfo(zoneId, planetId);
            if (planetInfo == null)
                return false;

            if(planetInfo.TriggerBooster())
            {
                SetDirty();
                (context as IdleMinerContext).SavePlayerDataInstantly();
                EventSystem.DispatchEvent(EventID.PLANET_BOOSTER_TRIGGERED, new Tuple<int, int>(zoneId, planetId));
                return true;
            }
            return false;
        }

        // 
        public void ForceClearData()
        {
            // Reset Data Focely.
            Debug.Log("<color=red>[Warnning!] Clearing Planet Data Forceably!!! </color>");
            unlockedZoneGroup = new ZoneGroupStatusInfo();
        }

        #endregion



        #region IWritableModel

        public override List<Tuple<string, string>> GetSaveDataWithKeys()
        {
            Assert.IsNotNull(unlockedZoneGroup);
            List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
            listDataSet.Add(new Tuple<string, string>(DateKey_ZoneInfo, JsonUtility.ToJson(unlockedZoneGroup)));
            return listDataSet;
        }
        
        #endregion




        #region ===> Helpers

        void InitPlayerData()
        {
#if UNITY_EDITOR
            if(context.IsSimulationMode())  LoadSimPlanetData();
            else                            LoadPlanetData();
#else
            LoadPlanetData();
#endif
            unlockedZoneGroup.Init();
        }
        //void SavePlanetData()
        //{
            //Assert.IsNotNull(unlockedZoneGroup);
           // WriteFileInternal(DataKey_PlanetData, unlockedZoneGroup);
        //}
        void LoadPlanetData()
        {
            int idxGatewayService = (context as IdleMinerContext).TargetGameDataGatewayServiceIndex;
            FetchData<ZoneGroupStatusInfo>(idxGatewayService, DateKey_ZoneInfo, out unlockedZoneGroup, fallback:new ZoneGroupStatusInfo());
        }
        void LoadSimPlanetData()
        {
            string gamePath = (string)context.GetData("gamePath");
            
            string jsonText = Resources.Load<TextAsset>(gamePath + "/Data/SimData/Sim_PlanetData").text;
            Assert.IsTrue(!string.IsNullOrEmpty(jsonText));
            unlockedZoneGroup = JsonUtility.FromJson<ZoneGroupStatusInfo>(jsonText);
            Debug.Log($"[SIM]- ZoneCount:{unlockedZoneGroup.Zones.Count}");
        }
        
#endregion




        #region ===> Event Handlers
        
        void ZoneStatusInfo_OnManualBoosterFinished(int zoneId, int planetId)
        {
            Debug.Log("<color=#56F8C4>>>>[Planet-Booster] Booster Finished...zone </color>");
            EventSystem.DispatchEvent(EventID.PLANET_BOOSTER_FINISHED, new Tuple<int, int>(zoneId, planetId));
        }

        // Event. 
        /*void OnEventPlanetOpened(int zoneId, int planetId) 
        { 
            EventSystem.DispatchEvent("EVENT_ON_PLANET_OPENED", new Tuple<int, int>(zoneId, planetId));
        }
        void OnEventPlanetClosed(int zoneId, int planetId)
        { 
            EventSystem.DispatchEvent("EVENT_ON_PLANET_CLOSED", new Tuple<int, int>(zoneId, planetId));
        }
        void OnEventPlanetBattleCleared(int zoneId, int planetId) 
        { 
            EventSystem.DispatchEvent("EVENT_ON_PLANET_BATTLE_CLEARED", new Tuple<int, int>(zoneId, planetId));
        }*/


        

        #endregion

        


        #region ===> Requestables

        void RegisterRequestables()
        {
            context.AddRequestDelegate("GamePlay.PlayerData", "GetVisiblePlanetInfo", getVisiblePlanetInfo);
        }
        void UnregisterRequestables()
        {
            context.RemoveRequestDelegate("GamePlay.PlayerData", "GetVisiblePlanetInfo");
        }

        object getVisiblePlanetInfo(params object[] data)
        {   
            if(data.Length < 2)
                return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            return GetPlanetInfo(zoneId, planetId);
        }

        #endregion

#if UNITY_EDITOR
        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/Planet")]
        public static void EditorClearPlanetData()
        {
            // ClearPlanetData();
        }
#endif
    }
}
