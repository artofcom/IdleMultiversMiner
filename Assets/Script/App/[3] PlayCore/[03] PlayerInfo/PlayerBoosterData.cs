
using System.Numerics;
using UnityEngine;
using Core.Events;
using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class OwnedBoosterInfo
    {
        [SerializeField] string boosterId;
        [SerializeField] int count;

        // Accessor.
        public string BoosterId => boosterId;
        public int Count => count;
    }

    [Serializable]
    public class UsingBoosterInfo
    {
        [SerializeField] string boosterId;
        [SerializeField] float remainTime;
        [SerializeField] int planetId = 0;  // 0 : don't care. valid : 1 ~ 
        
        // Accessor.
        public string BoosterId { get => boosterId; set => boosterId = value;  }
        public float RemainTime { get => remainTime; set => remainTime = value; }
        public int PlanetId { get => planetId; set => planetId = value; }
        public UsingBoosterInfo(string _boosterId, float _duration)
        {
            boosterId = _boosterId;     remainTime = _duration;
        }
    }


    public partial class PlayerData
    {
        // Serialize Fields.
        List<OwnedBoosterInfo> ownedBoosters = new List<OwnedBoosterInfo>();
        List<UsingBoosterInfo> usingBoostersInfo = new List<UsingBoosterInfo>();
        //[SerializeField] BoostValueInfo boostingInfo;

        // Accessor.
        public List<UsingBoosterInfo> UsingBoostersInfo => usingBoostersInfo;
        //public BoostValueInfo BoostingInfo => boostingInfo;


        void SaveBoosterData()
        {

        }

        void LoadBoosterData()
        {

        }


        //==========================================================================
        //
        // Boosters Control.
        //
        //
        public bool UseBooster(BoosterInfo boosterData)
        {
            if (boosterData == null)
                return false;
            /*
            if (!IsAffordable(boosterData.Cost, boosterData.CostCurrencyType))
                return false;

            UpdateMoney(-boosterData.Cost, boosterData.CostCurrencyType);

            UsingBoosterInfo newBooster = new UsingBoosterInfo(boosterData.Id, boosterData.Duration);
            usingBoostersInfo.Add(newBooster);
            */
            
            return true;
        }

        public bool ExpireBooster(BoosterInfo boosterData, int idxUsingBooster)
        {
            if (boosterData == null)
                return false;
            if (idxUsingBooster < 0 || idxUsingBooster >= UsingBoostersInfo.Count)
                return false;

            Assert.IsTrue(boosterData.Id == UsingBoostersInfo[idxUsingBooster].BoosterId);

            // Remove using booster info from the list.
            UsingBoostersInfo.RemoveAt(idxUsingBooster);
            return true;
        }

        public int GetOwnedBoosterCount(string boosterId)
        {
            for(int q = 0; q < ownedBoosters.Count; ++q)
            {
                if (ownedBoosters[q].BoosterId == boosterId)
                    return ownedBoosters[q].Count;
            }
            return 0;
        }

        //==========================================================================
        // Internal Helpers.
        //
        /*PlanetBoostValueInfo GetPlanetBoostInfo(string planetId)
        {
            for(int q = 0; q < boostingInfo.PlanetBoosts.Count; ++q)
            {
                if (planetId == boostingInfo.PlanetBoosts[q].PlanetId)
                    return boostingInfo.PlanetBoosts[q];
            }
            return null;
        }*/


#if UNITY_EDITOR

        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/Booster")]
        private static void ClearBoosterData()
        {
            string account = string.Empty; // PlayerPrefs.GetString(PREFAB_ACCOUNT, string.Empty);
            if (!string.IsNullOrEmpty(account))
            {
                //PlayerPrefs.SetInt($"{GameKey}_{account}_ManagerSlotDataCount", 0);
                //PlayerPrefs.SetInt($"{GameKey}_{account}_ManagerCollectionDataCount", 0);

                Debug.Log("Deleting All Booster PlayerPrefab...");
            }
            else
                Debug.Log("Could not find player account.");
        }
#endif
    }
}



/*
    [Serializable]
    public class PlanetBoostValueInfo
    {
        [SerializeField] float boostRate = 1.0f;
        [SerializeField] string planetId;

        public float BoostRate { get { return boostRate; } set { boostRate = value;  } }
        public string PlanetId { get { return planetId; }  set { planetId = value;  } }
    }

    [Serializable]
    public class BoostValueInfo
    {
        [SerializeField] float miningBoost = 1.0f;
        [SerializeField] float deliverySpeedBoost = 1.0f;
        [SerializeField] float cargoSizeBoost = 1.0f;
        [SerializeField] List<PlanetBoostValueInfo> planetBoosts;
        [SerializeField] float craftStage0Boost = 1.0f;
        [SerializeField] float craftStage1Boost = 1.0f;

        public float MiningBoost { get { return miningBoost; } set { miningBoost = value; } }
        public float DeliverySpeedBoost {  get { return deliverySpeedBoost; } set { deliverySpeedBoost = value;  } }
        public float CargoSizeBoost { get { return cargoSizeBoost; } set { cargoSizeBoost = value; } }
        public List<PlanetBoostValueInfo> PlanetBoosts { get { return planetBoosts; } set { planetBoosts = value; } }
        public float CraftStage0Boost { get { return craftStage0Boost; } set { craftStage0Boost = value; } }
        public float CraftStage1Boost { get { return CraftStage1Boost; } set { CraftStage1Boost = value; } }

    }

*/

/*switch (boosterData.Type )
            {
                case eBoosterType.MINING_RATE:
                    boostingInfo.MiningBoost *= boosterData.BoostingRate;
                    break;
                case eBoosterType.DELIVERY_SPEED:
                    boostingInfo.DeliverySpeedBoost *= boosterData.BoostingRate;
                    break;
                case eBoosterType.CARGO_SIZE:
                    boostingInfo.CargoSizeBoost *= boosterData.BoostingRate;
                    break;
                case eBoosterType.CASH:
                    //
                    //
                    //
                    break;
                case eBoosterType.PLANET_BOOST:
                    {
                        Assert.IsTrue(!string.IsNullOrEmpty(planetId), "Should select planet first!");
                        if (string.IsNullOrEmpty(planetId))
                            break;

                        var planetBoost = GetPlanetBoostInfo(planetId);
                        if (planetBoost == null)
                        {
                            planetBoost = new PlanetBoostValueInfo();
                            planetBoost.PlanetId = planetId;
                            planetBoost.BoostRate *= boosterData.BoostingRate;
                            boostingInfo.PlanetBoosts.Add(planetBoost);
                        }
                        else 
                            planetBoost.BoostRate *= boosterData.BoostingRate;
                    }
                    break;

                case eBoosterType.CRAFT_ALL_BOOST:
                    boostingInfo.CraftStage0Boost *= boosterData.BoostingRate;
                    boostingInfo.CraftStage1Boost *= boosterData.BoostingRate;
                    break;
                case eBoosterType.CRAFT_STAGE0_BOOST:
                    boostingInfo.CraftStage0Boost *= boosterData.BoostingRate;
                    break;
                case eBoosterType.CRAFT_STAGE1_BOOST:
                    boostingInfo.CraftStage1Boost *= boosterData.BoostingRate;
                    break;
                case eBoosterType.TIME_JUMP:
                    //
                    //
                    //
                    break;
                default:
                    Assert.IsTrue(false, "Not supported booster type! " + boosterData.Type.ToString());
                    return false;
            }

            if (boosterData.Duration > 0)
            {
                newBooster.SetData(boosterData.Id, boosterData.Duration);
                UsingBoostersInfo.Add(newBooster);
            }*/
 /*
        public bool ExpireBooster(BoosterInfo boosterData, int idxUsingBooster, string planetId = "")
        {
            if (boosterData == null)
                return false;
            if (idxUsingBooster < 0 || idxUsingBooster >= UsingBoostersInfo.Count)
                return false;

            Assert.IsTrue(boosterData.Id == UsingBoostersInfo[idxUsingBooster].BoosterId);

            switch (boosterData.Type)
            {
                case eBoosterType.MINING_RATE:
                    boostingInfo.MiningBoost /= boosterData.BoostingRate;
                    break;
                case eBoosterType.DELIVERY_SPEED:
                    boostingInfo.DeliverySpeedBoost /= boosterData.BoostingRate;
                    break;
                case eBoosterType.CARGO_SIZE:
                    boostingInfo.CargoSizeBoost /= boosterData.BoostingRate;
                    break;
                case eBoosterType.CASH:
                    break;
                case eBoosterType.PLANET_BOOST:
                    {
                        / *Assert.IsTrue(!string.IsNullOrEmpty(planetId), "Should select planet first!");
                        if (string.IsNullOrEmpty(planetId))
                            break;

                        var planetBoost = GetPlanetBoostInfo(planetId);
                        if (planetBoost == null)
                        {
                            planetBoost = new PlanetBoostValueInfo();
                            planetBoost.PlanetId = planetId;
                            planetBoost.BoostRate *= boosterData.BoostingRate;
                            boostingInfo.PlanetBoosts.Add(planetBoost);
                        }
                        else
                            planetBoost.BoostRate *= boosterData.BoostingRate;* /

                    }
                    break;

                case eBoosterType.CRAFT_ALL_BOOST:
                    boostingInfo.CraftStage0Boost /= boosterData.BoostingRate;
                    boostingInfo.CraftStage1Boost /= boosterData.BoostingRate;
                    break;
                case eBoosterType.CRAFT_STAGE0_BOOST:
                    boostingInfo.CraftStage0Boost /= boosterData.BoostingRate;
                    break;
                case eBoosterType.CRAFT_STAGE1_BOOST:
                    boostingInfo.CraftStage1Boost /= boosterData.BoostingRate;
                    break;
                case eBoosterType.TIME_JUMP:
                    //
                    //
                    //
                    break;
                default:
                    Assert.IsTrue(false, "Not supported booster type! " + boosterData.Type.ToString());
                    return false;
            }

            // Remove using booster info from the list.
            UsingBoostersInfo.RemoveAt(idxUsingBooster);
            return true;
        }*/
