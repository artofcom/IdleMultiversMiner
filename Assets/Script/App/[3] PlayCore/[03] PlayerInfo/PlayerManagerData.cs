
using System.Numerics;
using UnityEngine;
using Core.Events;
using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class ManagerSlotData
    {
        [SerializeField] string ownedManagerInfo;   // string.Empty means empty slot.
        [SerializeField] int assignedPlanetId;

        // Accessor.
        public string OwnedManagerInfoId => ownedManagerInfo;
        public int AssignedPlanetId => assignedPlanetId;

        public ManagerSlotData()
        {
            ownedManagerInfo = string.Empty;
        }
        public void Assign(string _ownedMngInfoId, int _planetId)
        {
            ownedManagerInfo = _ownedMngInfoId; assignedPlanetId = _planetId;
        }
    }

    [Serializable]
    public class OwnedManagerInfo
    {
        [SerializeField] string id;
        [SerializeField] string managerId;
        [SerializeField] int level;

        // Accessor.
        public string Id => id;
        public string ManagerId { get => managerId; set => managerId = value; }
        public int Level
        {
            get { return level; }
            set
            {
                if (value > ManagerInfo.MAX_LEVEL)
                {
                    Debug.Log("Manager level is too high!! : " + value.ToString());
                    return;
                }
                level = value;
            }
        }
        public OwnedManagerInfo() { }
        public OwnedManagerInfo(string _id, string _managerId, int _lv)
        {
            id = _id;   managerId = _managerId;     level = _lv;
        }
        public static OwnedManagerInfo Create(string mngId, int level)
        {
            string id = "AAA";// PlayerData.UTCNowTick.ToString() + "_" + mngId;
            return new OwnedManagerInfo(id, mngId, level);
        }
    }




    public partial class PlayerData
    {
        // Serialize Fields.
        [SerializeField] List<ManagerSlotData> managerSlots = new List<ManagerSlotData>();
        [SerializeField] List<OwnedManagerInfo> ownedManagers = new List<OwnedManagerInfo>();


        // Accessor.
        public List<ManagerSlotData> ManagerSlots => managerSlots;
        public List<OwnedManagerInfo> OwnedManagers => ownedManagers;




        void SaveManagerData()
        {
            SaveManagerSlotData();
            SaveManagerCollectionData();
        }
        void LoadManagerData()
        {
            LoadManagerSlotData();
            LoadManagerCollectionData();
        }
        

        void SaveManagerSlotData()
        {
            /*Assert.IsNotNull(managerSlots);
            WriteFileInternal($"{mAccount}_ManagerSlotDataCount", managerSlots.Count, false);
            for (int q = 0; q < managerSlots.Count; ++q)
            {
                WriteFileInternal($"{mAccount}_{q}_ManagerSlotData", managerSlots[q]);
            }*/
        }
        void SaveManagerCollectionData()
        {
            /*Assert.IsNotNull(ownedManagers);
            WriteFileInternal($"{mAccount}_ManagerCollectionDataCount", ownedManagers.Count, false);
            for (int q = 0; q < ownedManagers.Count; ++q)
            {
                WriteFileInternal($"{mAccount}_{q}_ManagerCollectionData", ownedManagers[q]);
            }*/
        }
        void LoadManagerSlotData()
        {
            if (managerSlots == null)
                managerSlots = new List<ManagerSlotData>();

            managerSlots.Clear();
            //int count = 0;
         /*   ReadFileInternal<int>($"{mAccount}_ManagerSlotDataCount", ref count);
            for (int q = 0; q < count; ++q)
            {
                ManagerSlotData slotInfo = null;
                ReadFileInternal<ManagerSlotData>($"{mAccount}_{q}_ManagerSlotData", ref slotInfo);
                Assert.IsNotNull(slotInfo);
                managerSlots.Add(slotInfo);
            }*/
        }
        void LoadManagerCollectionData()
        {
            if (ownedManagers == null)
                ownedManagers = new List<OwnedManagerInfo>();

            ownedManagers.Clear();
           // int count = 0;
         /*   ReadFileInternal<int>($"{mAccount}_ManagerCollectionDataCount", ref count);
            for (int q = 0; q < count; ++q)
            {
                OwnedManagerInfo mngInfo = null;
                ReadFileInternal<OwnedManagerInfo>($"{mAccount}_{q}_ManagerCollectionData", ref mngInfo);
                Assert.IsNotNull(mngInfo);
                ownedManagers.Add(mngInfo);
            }*/
        }


        //==========================================================================
        //
        // Manager Control.
        //
        //
        public OwnedManagerInfo GetOwnedManagerInfo(string ownedMgnInfoId)
        {
            if (string.IsNullOrEmpty(ownedMgnInfoId))
                return null;

            for (int q = 0; q < OwnedManagers.Count; ++q)
            {                
                if (OwnedManagers[q].Id == ownedMgnInfoId)
                    return OwnedManagers[q];
            }
            return null;
        }


        public bool PromoteManager(string ownedMgnInfoId)
        {
            OwnedManagerInfo info = GetOwnedManagerInfo(ownedMgnInfoId);
            if (info == null)
                return false;

            if (info.Level >= ManagerInfo.MAX_LEVEL)
                return false;

            info.Level++;
            return true;
        }
        public OwnedManagerInfo RecruiteManager(CurrencyAmount cost, string managerId, int level)
        {
            /*if(cost.Type == eCurrencyType.MINING_COIN)
                UpdateMoney(-cost.BIAmount, eCurrencyType.MINING_COIN);
            else
                UpdateMoney(-cost.Amount, cost.Type);
            */
            Debug.Log($"Recruiting {managerId} with level {level}...");
            OwnedManagers.Add(OwnedManagerInfo.Create(managerId, level));
            return OwnedManagers[OwnedManagers.Count - 1];
        }
        public void DiscardManager(string ownedMngInfoId)
        {
            if (string.IsNullOrEmpty(ownedMngInfoId))
                return;

            // Fire Manager from socket if found. 
            for (int q = 0; q < ManagerSlots.Count; ++q)
            {
                if (ManagerSlots[q].OwnedManagerInfoId == ownedMngInfoId)
                {
                    ManagerSlots[q].Assign(string.Empty, -1);
                    break;
                }
            }

            // Discard Manager from inventory.
            var manager = GetOwnedManagerInfo(ownedMngInfoId);
            if (manager != null)
                OwnedManagers.Remove(manager);
        }
        public bool HireManagerToPlanet(string ownedMngInfoId, int planetId, bool isHiring=true)
        {
            // UnAssign Manager if hired.
            if (!string.IsNullOrEmpty(ownedMngInfoId))
            {
                bool found = false;
                for (int q = 0; q < ManagerSlots.Count; ++q)
                {
                    if (ManagerSlots[q].OwnedManagerInfoId == ownedMngInfoId)
                    {
                        found = true;
                        ManagerSlots[q].Assign(string.Empty, -1);
                        break;
                    }
                }
                if(!isHiring && found)   return true;
            }

            // Find target slot with the planet.
            int idxSlot = -1;
            for(int q = 0; q < ManagerSlots.Count; ++q)
            {
                if(ManagerSlots[q].AssignedPlanetId == planetId)
                {
                    idxSlot = q;
                    break;
                }
            }

            if(!isHiring)
            {
                if(idxSlot < 0)         return false;   // No manager has been assigned for the planet.

                ManagerSlots[idxSlot].Assign(string.Empty, -1);         // Fire.
                return true;
            }

            // find empty slot.
            if (idxSlot < 0)
            {
                idxSlot = FindEmptySlotIndex();
                if (idxSlot < 0)
                {
                    Debug.Log("No Avaliable slot.");
                    return false;
                }
            }

            // Assign.
            OwnedManagerInfo mngInfo = GetOwnedManagerInfo(ownedMngInfoId);
            if(mngInfo == null)
                ManagerSlots[idxSlot].Assign(string.Empty, -1);         // Fire.
            else
                ManagerSlots[idxSlot].Assign(ownedMngInfoId, planetId); // Hire.

            return true;
        }


        public void PurchaseManagerSlot(CurrencyAmount cost)
        {
            //if(cost.Type == eCurrencyType.MINING_COIN)
           //     UpdateMoney(-cost.BIAmount, eCurrencyType.MINING_COIN);
          //  else
           //     UpdateMoney(-cost.Amount, cost.Type);

            Debug.Log("Increasing manager slot to ..." + (ManagerSlots.Count + 1).ToString());
            ManagerSlotData slot = new ManagerSlotData();
            ManagerSlots.Add(slot);
        }

        public (int, int) GetManagerSlotCount()
        {
            int max = ManagerSlots.Count, inUse = 0;
            for(int q = 0; q < ManagerSlots.Count; ++q)
            {
                if (!string.IsNullOrEmpty(ManagerSlots[q].OwnedManagerInfoId))
                    ++inUse;
            }

            return (inUse, max);
        }

        public OwnedManagerInfo GetAssignedManagerInfoForPlanet(int planetId)
        {
            for(int q = 0; q < ManagerSlots.Count; ++q)
            {
                if (ManagerSlots[q].AssignedPlanetId==planetId && !string.IsNullOrEmpty(ManagerSlots[q].OwnedManagerInfoId))
                    return GetOwnedManagerInfo(ManagerSlots[q].OwnedManagerInfoId);
            }
            return null;
        }

        public int GetPlanetIdForManager(string managerInfoId)
        {
            for (int q = 0; q < ManagerSlots.Count; ++q)
            {
                if (ManagerSlots[q].OwnedManagerInfoId == managerInfoId)
                    return ManagerSlots[q].AssignedPlanetId>0 ? ManagerSlots[q].AssignedPlanetId : -1;
                
            }
            return -1;
        }


        int FindEmptySlotIndex()
        {
            for (int q = 0; q < ManagerSlots.Count; ++q)
            {
                if (string.IsNullOrEmpty(ManagerSlots[q].OwnedManagerInfoId))
                    return q;
            }
            return -1;
        }







#if UNITY_EDITOR

        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/Manager")]
        private static void ClearManagerData()
        {
            /*string account = PlayerPrefs.GetString(PREFAB_ACCOUNT, string.Empty);
            if (!string.IsNullOrEmpty(account))
            {
                WriteFileInternal($"{GameKey}_{account}_ManagerSlotDataCount", string.Empty, false);
                WriteFileInternal($"{GameKey}_{account}_ManagerCollectionDataCount", string.Empty, false);

                Debug.Log("Deleting All Manager PlayerPrefab...");
            }
            else
                Debug.Log("Could not find player account.");
            */
        }

#endif
    }
}
