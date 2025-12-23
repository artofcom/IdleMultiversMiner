
using System.Numerics;
using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class ColonizationLevelInfo
    {
        [SerializeField] int minLevel;
        [SerializeField] List<ResourceRequirement> requirements;
        [SerializeField] List<float> rewardBoostRates;          // Mining, DeliverSpeed, CargoSize

        public int MinLevel => minLevel;
        public List<ResourceRequirement> Requirements => requirements;
        public List<float> RewardBoostRates => rewardBoostRates;
    }

    [Serializable]
    public class ColonizationInfo
    {
        [SerializeField] int planetId;
        [SerializeField] List<ColonizationLevelInfo> coloLevelInfo;

        public int PlanetId => planetId;
        public List<ColonizationLevelInfo> ColoLevelInfo => coloLevelInfo;
    }

    [SerializeField]
    public class ColonizationData
    {
        [SerializeField] List<ColonizationInfo> colonizationInfos;

        Dictionary<int, ColonizationInfo> dictColonizationInfo;

        public Dictionary<int, ColonizationInfo> ColonizationInfoDict => dictColonizationInfo;
        public void Init()
        {
            Assert.IsNotNull(colonizationInfos);

            dictColonizationInfo = new Dictionary<int, ColonizationInfo>();
            for(int q = 0; q < colonizationInfos.Count; ++q)
            {
                Assert.IsTrue(!dictColonizationInfo.ContainsKey(colonizationInfos[q].PlanetId));
                dictColonizationInfo.Add(colonizationInfos[q].PlanetId, colonizationInfos[q]);
            }
        }
    }


    public class ColonizationDataModel : IGCore.MVCS.AModel// IdleMinerModel
    {
        public ColonizationData ColonizationData { get; private set; }

        public ColonizationDataModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

        public override void Init()
        {
          //  var textData = Resources.Load<TextAsset>(GAMEDATA_PATH + "ColonizationData");
         //   ColonizationData = JsonUtility.FromJson<ColonizationData>(textData.text);

         //   Assert.IsNotNull(ColonizationData);
          //  ColonizationData.Init();
        }

        public ColonizationInfo GetColonizationData(int planetId)
        {
            Assert.IsNotNull(ColonizationData);
            Assert.IsNotNull(ColonizationData.ColonizationInfoDict);

            if (ColonizationData.ColonizationInfoDict.ContainsKey(planetId))
                return ColonizationData.ColonizationInfoDict[planetId];

            return null;
        }

        public bool ColonizePlanet(int planetId)
        {
           /* var planetData = GetPlanetData(planetId);
            Assert.IsNotNull(planetData);
            if (planetData == null)
                return false;


            // Req resources check.
            int idxCur = PlayerData.GetCurrentColonizationIndex(planetId);
            Assert.IsTrue(idxCur >= 0);
            if (idxCur < 0) return false;

            var colonization = GetColonizationData(planetId);
            Assert.IsNotNull(colonization);
            if (idxCur >= colonization.ColoLevelInfo.Count)
                return false;

            var coloInfo = colonization.ColoLevelInfo[idxCur];
            for (int q = 0; q < coloInfo.Requirements.Count; ++q)
            {
                var rscCollection = PlayerData.GetResourceCollectInfo(coloInfo.Requirements[q].ResourceId);
                if (rscCollection == null || coloInfo.Requirements[q].Count > (int)rscCollection.BICount)
                {
                    Debug.Log("Resource Insufficient! " + rscCollection == null ? "0" : rscCollection.RscId_);
                    return false;
                }
            }


            // Spend.
            for (int q = 0; q < coloInfo.Requirements.Count; ++q)
            {
                var rscCollection = PlayerData.GetResourceCollectInfo(coloInfo.Requirements[q].ResourceId);
                Assert.IsNotNull(rscCollection);
                PlayerData.UpdateResource(rscCollection.RscId_, -coloInfo.Requirements[q].Count);    
            }


            // Upgrade or Open.
            PlayerData.UpgradeColonization(planetId);*/
            return true;
        }
    }
}
