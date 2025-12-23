
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
    public class ColonizationProcInfo
    {
        [SerializeField] int planetId;
        [SerializeField] int indexLevel = -1;    // current level.

        public int PlanetId => planetId;
        public int IndexLevel => indexLevel;

        public void Set(int _planetId, int idx)
        {
            planetId = _planetId;   indexLevel = idx;
        }
    }


    [Serializable]
    public class ColonizationProcData
    {
        [SerializeField] List<ColonizationProcInfo> colonizationProcInfoList;

        public List<ColonizationProcInfo> ColonizationProcInfoList => colonizationProcInfoList;
    }


    public partial class PlayerData
    {
        // Serialize Fields.
        [SerializeField] ColonizationProcData colonizationData;
        

        // Accessor.
        public ColonizationProcData ColonizationData => colonizationData;
        


        //==========================================================================
        //
        // Colonization Control.
        //
        //
        public ColonizationProcInfo GetCurrentColonizationProcInfo(int planetId)
        {
            for(int q = 0; q < colonizationData.ColonizationProcInfoList.Count; ++q)
            {
                if (colonizationData.ColonizationProcInfoList[q].PlanetId == planetId)
                    return colonizationData.ColonizationProcInfoList[q];
            }
            return null;
        }

        public int GetCurrentColonizationIndex(int planetId)
        {
            var procInfo = GetCurrentColonizationProcInfo(planetId);
            if (procInfo == null)
                return -1;

            return procInfo.IndexLevel;
        }

        public void UpgradeColonization(int planetId)
        {
            var procData = GetCurrentColonizationProcInfo(planetId);
            if(procData == null)
            {
                procData = new ColonizationProcInfo();
                procData.Set(planetId, 0);
                colonizationData.ColonizationProcInfoList.Add(procData);
            }
            else
            {
                procData.Set(planetId, procData.IndexLevel + 1);
            }
        }

    }
}
