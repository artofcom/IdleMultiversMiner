
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using Core.Utils;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Resouces
{

    internal class ResourcePlayerModel : GatewayWritablePlayerModel
    {
        [Serializable]
        public class ListResourceCollections
        {
            [SerializeField] List<ResourceCollectInfo> resourceCollectInfos;

            public ListResourceCollections()
            {
                resourceCollectInfos = new List<ResourceCollectInfo>();
            }
            public List<ResourceCollectInfo> ResourceCollectInfos => resourceCollectInfos;

            public void Dispose()
            {
                if(resourceCollectInfos != null)
                    resourceCollectInfos.Clear();
                resourceCollectInfos = null;
            }
        }

        ListResourceCollections resourceCollections;
        Dictionary<string, ResourceCollectInfo> RscCollections = new Dictionary<string, ResourceCollectInfo>();  // id, count.
        
        static string DataKey_ResourceData => $"{nameof(ResourcePlayerModel)}_ResourceData";//$"{IdleMinerContext.GameKey}_{IdleMinerContext.AccountName}_ResourceData_";
        EventsGroup Events = new EventsGroup();
        #region ===> Interfaces

        public ResourcePlayerModel(AContext ctx, IDataGatewayService gatewayService) : base(ctx, gatewayService) { }

        public void WriteData()
        {
            SaveResourceData();
        }


        public override void Init()
        {
            base.Init();

            LoadResourceData();

            InitResourceCollection();

            Events.RegisterEvent(EventID.SKILL_RESET_GAME_INIT, ResetGamePlay_InitGame);
            IsInitialized = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            resourceCollections?.Dispose();
            RscCollections?.Clear();         RscCollections = null;

            Events.UnRegisterEvent(EventID.SKILL_RESET_GAME_INIT, ResetGamePlay_InitGame);
            IsInitialized = false;
        }

        public Dictionary<string, ResourceCollectInfo>.KeyCollection GetResourceCollectionInfoKeys()
        {
            return RscCollections.Keys;
        }        

        public ResourceCollectInfo GetResourceCollectInfo(string id)
        {
            id = id.ToLower();
            if (RscCollections.ContainsKey(id))
                return RscCollections[id];

            return null;
        }

        public ResourceCollectInfo GetResourceCollectionInfoByResourceId(string resourceId)
        {
            Assert.IsNotNull(resourceCollections);

            for(int q = 0; q < resourceCollections.ResourceCollectInfos.Count; ++q)
            {
                if (resourceCollections.ResourceCollectInfos[q].RscId_ == resourceId)
                    return resourceCollections.ResourceCollectInfos[q];
            }
            return null;
        }

        public bool SellResource(string id, BigInteger amount)
        {
            UpdateResourceX1000(id, -amount * 1000, offset: true);
            return true;
        }

        // _countx1000 should be the number : acutal Num x 1000.
        public void UpdateResourceX1000(string rscId, BigInteger _countx1000, bool offset = true)
        {
            BigInteger oldValue = BigInteger.Zero;
            ResourceCollectInfo info;
            rscId = rscId.ToLower();
            if (!RscCollections.ContainsKey(rscId))
            {
                info = new ResourceCollectInfo();
                info.RscId_ = rscId;
                info.BICountX1000 = _countx1000;
                RscCollections.Add(rscId, info);

                resourceCollections.ResourceCollectInfos.Add(info); // for data serialization.
            }
            else
            {
                info = RscCollections[rscId];
                oldValue = info.BICount;
                
                if (!offset) info.BICountX1000 = _countx1000;
                else         info.BICountX1000 += _countx1000;
            }

            BigInteger newValue = info.BICount;
            
            if(oldValue < newValue)
                EventSystem.DispatchEvent(EventID.RESOURCE_UPDATED, new Tuple<string, BigInteger>(rscId, newValue));
        }

        public void UpdateResource(string rscId, int count, bool offset = true)
        {
            UpdateResourceX1000(rscId, count * 1000, offset);
        }

        public BigInteger TryAutoSell(string srcId)
        {
            var collectInfo = GetResourceCollectInfo(srcId);
            if (collectInfo == null || !collectInfo.AutoSell_)
                return BigInteger.Zero;

            BigInteger soldCount = collectInfo.BICount;
            SellResource(srcId, collectInfo.BICount);
            return soldCount;
        }

        public BigInteger SellResource(string srcId)
        {
            var collectInfo = GetResourceCollectInfo(srcId);
            if (collectInfo == null)
                return BigInteger.Zero;

            BigInteger soldCount = collectInfo.BICount;
            SellResource(srcId, collectInfo.BICount);
            return soldCount;
        }

        #endregion





        #region ===> Helpers


        void SaveResourceData()
        {
           // Assert.IsNotNull(ResourceCollectInfos);
            //WriteFileInternal(DataKey_ResourceDataCount, ResourceCollectInfos.Count, false);
            //for(int q = 0; q < ResourceCollectInfos.Count; ++q)
            //{
            //    WriteFileInternal(DataKey_ResourceData + q.ToString(), ResourceCollectInfos[q]);
            //}
        }
        void LoadResourceData()
        {
            if(context.IsSimulationMode())
                resourceCollections = new ListResourceCollections();
            else 
                FetchData(DataKey_ResourceData, out resourceCollections, fallback: new ListResourceCollections());
        }

        void ResetGamePlay_InitGame(object data)
        {
            LoadResourceData();
            InitResourceCollection();
        }


        //==========================================================================
        //
        // Resource Collection Control.
        //
        //
        void InitResourceCollection()
        {
            if (resourceCollections!=null && resourceCollections.ResourceCollectInfos != null)
            {
                RscCollections.Clear();
                for (int q = 0; q < resourceCollections.ResourceCollectInfos.Count; ++q)
                {
                    string rscId = resourceCollections.ResourceCollectInfos[q].RscId_.ToLower();
                    Assert.IsTrue(!RscCollections.ContainsKey( rscId ));
                    resourceCollections.ResourceCollectInfos[q].Convert();
                    RscCollections.Add( rscId, resourceCollections.ResourceCollectInfos[q]);
                }

                // Note: ResourceCollectionInfos & RscCollections dictionary only shares it's data container as reference.
                //       So they'll be updated at the same time. 
            }
        }


        #endregion

        #region IWritableModel

        public override List<Tuple<string, string>> GetSaveDataWithKeys()
        {
            List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
            
            Assert.IsNotNull(resourceCollections);
            listDataSet.Add(new Tuple<string, string>(DataKey_ResourceData, JsonUtility.ToJson(resourceCollections)));
            
            return listDataSet;
        }
        
        #endregion



#if UNITY_EDITOR

        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/Resource")]
        public static void EditorClearResourceData()
        {
            // ClearResourceData();
        }
#endif
    }
}
