using App.GamePlay.IdleMiner.Common.Model;
using System.Collections.Generic;
using System.Numerics;
using System;
using UnityEngine;
using Core.Events;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Types;

namespace App.GamePlay.IdleMiner.Common.PlayerModel
{
    #region ===> Serializable classes

    [Serializable]
    public class DeliveryInfo
    {
        [SerializeField] string amountx1000;
        [SerializeField] float posRate;
        [SerializeField] bool isHeadingToPlanet;

        public BigInteger Amount => Amountx1000/1000;
        BigInteger _amountx1000 = BigInteger.MinusOne;
        public BigInteger Amountx1000
        {
            get
            {
                if(_amountx1000 == BigInteger.MinusOne)
                   _amountx1000 = string.IsNullOrEmpty(amountx1000) ? BigInteger.Zero : BigInteger.Parse(amountx1000);
                return _amountx1000;
            }
        }
        public float PosRate => posRate;
        public bool IsHeadingToPlanet => isHeadingToPlanet;
        public void Set(BigInteger amountx1000, float _posRate, bool _isHeadingPlanet)
        {
            this._amountx1000 = amountx1000;   posRate = _posRate; isHeadingToPlanet = _isHeadingPlanet;
            this.amountx1000 = amountx1000.ToString();
        }
    }

    [Serializable]
    public class MinedResourceInfo
    {
        public static readonly string EVENT_RSC_MINED = "OnResourceMined";

        [SerializeField] string resourceId;
        [SerializeField] string countF3;

        public string ResourceId => resourceId;
        public BigInteger BICountF3
        {
            get => biCountF3;
            set
            {
                EventSystem.DispatchEvent(EVENT_RSC_MINED);
                biCountF3 = value;

                countF3 = biCountF3.ToString();
            }
        }
        public BigInteger BICount => biCountF3 / 1000;

        BigInteger biCountF3;

        public MinedResourceInfo(string _resourceId, BigInteger biAmountx1000)
        {
            resourceId = _resourceId;
            BICountF3 = biAmountx1000;
        }
        public void Init()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(countF3));
            biCountF3 = BigInteger.Parse(countF3);
        }
    }

    [Serializable]
    public class BattleInfo : ISerializationCallbackReceiver
    {
        [SerializeField] protected long eventStartedTicke;
        [SerializeField] protected string damageX1000;
        [SerializeField] protected bool isCleared;

        BigInteger biDamageX1000;
        public BigInteger BIDamageX1000
        {
            get => biDamageX1000;
            set
            {
                biDamageX1000 = value;
                damageX1000 = biDamageX1000.ToString();
            }
        }
        public BigInteger BIDamage => biDamageX1000 / 1000;

        public long EventStartedTick {  get => eventStartedTicke; set => eventStartedTicke = value; }
        public bool IsCleared { get => isCleared; set => isCleared = value; }
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            BigInteger bigInteger;
            bool ret = BigInteger.TryParse(damageX1000, out bigInteger);
            if (ret)    biDamageX1000 = bigInteger;
            else        biDamageX1000 = 0;

         //   if (EventStartedTick == 0)
          //      EventStartedTick = PlayerData.UTCNowTick;
        }
    }

    [Serializable]
    public class PlanetInfo
    {
        static public Action<int> OnManualBoosterFinished;

        public enum BOOST_STATE { Ready, Boosting, CoolTime };

        [SerializeField] int planetId;
        [SerializeField] bool isUnlocked;
        [SerializeField] List<int> level = new List<int>(); // 0 ~ (int)eABILITY.MAX;
        //[SerializeField] float distance;                    // distance to mainCore.

        [SerializeField] List<MinedResourceInfo> minedRscInfo = new List<MinedResourceInfo>();
        [SerializeField] List<DeliveryInfo> deliveryInfos = new List<DeliveryInfo>();
        [SerializeField] BattleInfo battleInfo = null;

        [SerializeField] bool isBoosterUnlocked = false;
        [SerializeField] int boostState = 0;
        [SerializeField] float boostingDuration = .0f;
        [SerializeField] float boostRemainTimeInSec = .0f;
        [SerializeField] float boostCoolTimeDuration = .0f;
        [SerializeField] float boosterRate = 1.0f;

        // Accessor / Updator.
        public int PlanetId     => planetId;   
        public bool IsUnlocked  => isUnlocked; 
        public List<int> Level  => level;
        public List<DeliveryInfo> DeliveryInfo { get => deliveryInfos; set => deliveryInfos = value; }
        public List<MinedResourceInfo> MinedRscInfo => minedRscInfo;
        public BattleInfo BattleInfo => battleInfo;
        
        public bool IsBoosterUnlocked => isBoosterUnlocked;
        public BOOST_STATE BoostState => (BOOST_STATE)boostState;
        public bool IsBoosterReadyToRun => BoostState==BOOST_STATE.Ready;

        public float BoostingDuration => boostingDuration;
        public float BoostRemainTimeInSec => boostRemainTimeInSec;
        public float BoostCoolTimeDuration => boostCoolTimeDuration;
        public float BoosterRate => boosterRate;

        public float Distance     {  get; set; }  //   { get => distance; set => distance = value; }

        public PlanetInfo(int planetId, float distance)
        {
            this.planetId = planetId;
            this.Distance = distance;
            this.isUnlocked = false;
            Init();
        }

        public void Init()
        {
            if(minedRscInfo != null)
            {
                for (int q = 0; q < minedRscInfo.Count; ++q)
                    minedRscInfo[q].Init();
            }
            if(level.Count != (int)eABILITY.MAX)
            {
                level.Clear();
                for (int q = 0; q < (int)eABILITY.MAX; ++q)
                    level.Add(1);
            }

            if (battleInfo != null)
            {
                if (!isUnlocked)
                    battleInfo.BIDamageX1000 = 0;
            }
            else
                battleInfo = null;
        }        
        public void Resume(float duration)
        {
            if(isBoosterUnlocked)
            {
                float remainTime;
                switch(BoostState)
                {
                case BOOST_STATE.Boosting:
                    remainTime = boostRemainTimeInSec;
                    PumpBooster(duration);
                    
                    if(BoostState == BOOST_STATE.CoolTime)
                        PumpBooster(duration-remainTime);
                    
                    break;

                case BOOST_STATE.CoolTime:
                    PumpBooster(duration);
                    break;

                default:
                    break;
                }
            }
        }
        public void Pump(float duration = 1.0f)
        {
            Assert.IsTrue(duration > .0f, "Duration Error.." + duration);
            if(isBoosterUnlocked)
                PumpBooster(duration);
        }


        void PumpBooster(float duration)
        {
            Assert.IsTrue(isBoosterUnlocked, "Booster should be unlocked.");

            switch(BoostState)
            {
            case BOOST_STATE.Boosting:
                boostRemainTimeInSec -= duration;
                if(boostRemainTimeInSec < .0f)
                {
                    boostRemainTimeInSec = boostCoolTimeDuration;
                    boostState = (int)(BOOST_STATE.CoolTime);
                }
                break;
            case BOOST_STATE.CoolTime:
                boostRemainTimeInSec -= duration;
                if(boostRemainTimeInSec < .0f)
                {
                    boostRemainTimeInSec = boostingDuration;
                    boostState = (int)(BOOST_STATE.Ready);
                }
                break;

            case BOOST_STATE.Ready:
            default:
                break;
            }
        }

        public Sprite ImageSprite { get; set; }
        public void Upgrade(eABILITY type, int offsetLevel = 1)
        {
            Assert.IsTrue((int)eABILITY.MAX == Level.Count);
            Level[(int)type] += offsetLevel;
        }
        public void ResetStat(eABILITY stat)
        {
            Assert.IsTrue((int)eABILITY.MAX == Level.Count);
            Level[(int)stat] = 1;
        }
        public void UpdateDamageX1000(BigInteger damageX1000)
        {
            if (battleInfo == null || battleInfo.IsCleared)
                return;

            battleInfo.BIDamageX1000 += damageX1000;
        }

        public void ClearBattle()
        {
            battleInfo.IsCleared = true;            
          //  EventOnPlanetBattleCleared?.Invoke(planetId);
        }

        public void OpenPlanet()
        {
            Assert.IsTrue(Distance > .0f);
            this.isUnlocked = true;

            if(level != null)           level.Clear();
            if(minedRscInfo != null)    minedRscInfo.Clear();

            if (battleInfo != null)
            {
                // is already cleared ???
                if (battleInfo != null && battleInfo.IsCleared)
                    return;

                battleInfo = new BattleInfo();
         //       battleInfo.EventStartedTick = PlayerData.UTCNowTick;
            }
            
            this.Init();

            Debug.Log($"[Openning-Planet] Planet Id : {planetId}");
            //EventOnPlanetOpened?.Invoke(planetId);  // Unlock.
        }
        public void ClosePlanet()
        {
            Assert.IsNotNull(battleInfo);

            if (battleInfo.IsCleared)
                return;

            this.isUnlocked = false;
            battleInfo.BIDamageX1000 = 0;

            Debug.Log($"[Closing-Planet] Planet Id : {planetId}");
            //EventSystem.DispatchEvent(PlayerPlanetData.EVENT_ON_PLANET_CLOSED, planetId);
            //EventOnPlanetClosed?.Invoke(planetId);
        }
        public long GetSecondFromEvent()    // For now the event is only battle or reset started.
        {
            if (BattleInfo == null)
                return 0;

            long elapsedTick = 0;// PlayerData.UTCNowTick - BattleInfo.EventStartedTick;
            return (long)TimeSpan.FromTicks(elapsedTick).TotalSeconds;
        }

        public void UnlockMiningBuff(float boostRunDuration, float buffRate, float coolTimeDuration)
        {
            isBoosterUnlocked = true;

            boostingDuration = boostRunDuration; 
            boostCoolTimeDuration = coolTimeDuration;
            boosterRate = buffRate;
            
            // Should not change its state - State comes from data.
            // boostState = (int)(BOOST_STATE.Ready);
        }
        public bool TriggerBooster()
        {
            Debug.Log($"[Planet-Booster] Trying to trigger Booster...- " + planetId);
            if(!isBoosterUnlocked)  return false;

            Debug.Log($"<color=#56F8C4>>>>[Planet-Booster] Booster Trigger in Planet. - {planetId} </color>");

            boostRemainTimeInSec = boostingDuration;
            boostState = (int)BOOST_STATE.Boosting;
            return true;
        }
    }

    [Serializable]
    public class ZoneStatusInfo
    {
        static public Action<int, int> OnManualBoosterFinished;

        [SerializeField] int zoneId;
        [SerializeField] List<PlanetInfo> planets = new List<PlanetInfo>();

        public int ZoneId => zoneId;
        public List<PlanetInfo> Planets => planets;

        public ZoneStatusInfo(int zoneId, List<int> planetId, List<float> distanceList)
        {
            Assert.IsTrue(planetId.Count==distanceList.Count);
            this.zoneId = zoneId;

            for(int q = 0; q < planetId.Count; q++)
                planets.Add(new PlanetInfo(planetId[q], distanceList[q]));
        }

        public void Init()
        {
            PlanetInfo.OnManualBoosterFinished += PlanetInfo_OnManualBoosterFinished;

            if(Planets!=null && Planets.Count>0)
            {
                for (int q = 0; q < Planets.Count; ++q)
                    Planets[q].Init();
            }
        }
        public void Pump()
        {
            if(Planets != null)
            {
                for (int q = 0; q < Planets.Count; ++q)
                    Planets[q].Pump();
            }
        }
        public void UnlockMiningBooster(float duration, float buffRate, float cooltimeDuration)
        {
            for(int q = 0; q < Planets.Count; q++)
                Planets[q].UnlockMiningBuff(duration, buffRate, cooltimeDuration);
        }
        public PlanetInfo GetPlanetInfo(int planetId)
        {
            for(int q = 0; q < Planets.Count; q++)
            {
                if(Planets[q].PlanetId == planetId)
                    return Planets[q];
            }
            return null;
        }
        public void SetDistanceInfo(List<float> distanceList)
        {
            Assert.IsTrue(planets.Count == distanceList.Count, $"Planet and Distance count is not matching. [{planets.Count}]/[{distanceList.Count}]" );

            for(int q = 0; q < planets.Count; q++)
                planets[q].Distance = distanceList[q];
        }
        public PlanetInfo GetNeighborData(int planetId, bool up, bool isCircle)
        {
            for(int q = 0; q < Planets.Count; ++q)
            {
                if(Planets[q].PlanetId == planetId)
                {
                    if(up)      
                    {
                        if(isCircle)    return q+1>=Planets.Count ? Planets[0] : Planets[q+1];
                        else            return q+1>=Planets.Count ? null : Planets[q+1];
                    }
                    else
                    {
                        if(isCircle)    return q==0 ? Planets[ Planets.Count-1 ] : Planets[q-1];
                        else            return q==0 ? null : Planets[q-1];
                    }
                }
            }
            return null;
        }

        void PlanetInfo_OnManualBoosterFinished(int planetId)
        {
            OnManualBoosterFinished?.Invoke(zoneId, planetId);
        }
    }

    [Serializable]
    public class ZoneGroupStatusInfo
    {
        [SerializeField] List<ZoneStatusInfo> zones = new List<ZoneStatusInfo>();

        public List<ZoneStatusInfo> Zones => zones;

        public void Init()
        {
            if(zones!=null && zones.Count>0)
            {
                for (int q = 0; q < zones.Count; ++q)
                    zones[q].Init();
            }
        }
        public void Pump()
        {
            if(zones != null)
            {
                for (int q = 0; q < zones.Count; ++q)
                    zones[q].Pump();
            }
        }
        public ZoneStatusInfo GetZoneStatusInfo(int zoneId)
        {
            for(int q = 0; q < zones.Count; q++)
            {
                if(zones[q].ZoneId == zoneId)
                    return zones[q];
            }
            return null;
        }
        public void Add(int zoneId, List<int> planets, List<float> distanceList)
        {
            zones.Add( new ZoneStatusInfo(zoneId, planets, distanceList) );
        }
        public ZoneStatusInfo GetNeighborData(int zoneId, bool up, bool isCircle)
        {
            for(int q = 0; q < zones.Count; ++q)
            {
                if(zones[q].ZoneId == zoneId)
                {
                    if(up)      
                    {
                        if(isCircle)    return q+1>=zones.Count ? zones[0] : zones[q+1];
                        else            return q+1>=zones.Count ? null : zones[q+1];
                    }
                    else
                    {
                        if(isCircle)    return q==0 ? zones[ zones.Count-1 ] : zones[q-1];
                        else            return q==0 ? null : zones[q-1];
                    }
                }
            }
            return null;
        }   
        public void UnlockMiningBooster(int zoneId, float duration, float buffRate, float coolTimeDuration)
        {
            for(int q = 0; q < zones.Count; ++q)
            {
                if(zones[q].ZoneId == zoneId)
                {
                    zones[q].UnlockMiningBooster(duration, buffRate, coolTimeDuration);
                    break;
                }
            }
        }
    }

    #endregion ===> Serializable classes


}