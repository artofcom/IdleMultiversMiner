
using System.Numerics;
using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;
using IGCore.MVCS;
using App.GamePlay.IdleMiner.Common.Model;
using System.Collections;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Resouces
{
    internal class ResourceModel : IGCore.MVCS.AModel
    {
        public List<ResourceData> ResourceData { get; private set; } = new List<ResourceData>();  // by Level.

        public ResourcePlayerModel PlayerData => (ResourcePlayerModel)playerData;
        
        Dictionary<string, ResourceInfo> dictMatCache = new Dictionary<string, ResourceInfo>();
        Dictionary<string, ResourceInfo> dictCompCache = new Dictionary<string, ResourceInfo>();
        Dictionary<string, ResourceInfo> dictItemCache = new Dictionary<string, ResourceInfo>();

        public ResourceModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel _playerData) : base(ctx, _playerData)  {}

        public override void Init()
        {
            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);
            _InitModel();
        }

        public override void Dispose()
        {
            base.Dispose();

            ResourceData?.Clear();   ResourceData = null;
            dictMatCache?.Clear();   dictMatCache = null;
            dictCompCache?.Clear();  dictCompCache = null;
            dictItemCache?.Clear();  dictItemCache = null;

            UnregisterRequestables();

            _isInitialized = false;
        }
         
        void _InitModel()
        {
            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);

            string gamePath = (string)IMCtx.GetData("gamePath");
            LoadModelData(gamePath);

            RegisterRequestables();

            _isInitialized = true;
        }

        void LoadModelData(string gamePath)
        {
            var textData = Resources.Load<TextAsset>(gamePath + "/Data/Resource_Mat");
            ResourceData.Add(JsonUtility.FromJson<ResourceData>(textData.text));
            ResourceData[ResourceData.Count - 1].Convert(eRscStageType.MATERIAL);
            for(int q = 0; q < ResourceData[ResourceData.Count - 1].Data.Count; ++q)
                dictMatCache.Add(ResourceData[ResourceData.Count - 1].Data[q].Id.ToLower(), ResourceData[ResourceData.Count - 1].Data[q]);

            textData = Resources.Load<TextAsset>(gamePath + "/Data/Resource_Comp");
            ResourceData.Add(JsonUtility.FromJson<ResourceData>(textData.text));
            ResourceData[ResourceData.Count - 1].Convert(eRscStageType.COMPONENT);
            for(int q = 0; q < ResourceData[ResourceData.Count - 1].Data.Count; ++q)
                dictCompCache.Add(ResourceData[ResourceData.Count - 1].Data[q].Id.ToLower(), ResourceData[ResourceData.Count - 1].Data[q]);

            textData = Resources.Load<TextAsset>(gamePath + "/Data/Resource_Item");
            ResourceData.Add(JsonUtility.FromJson<ResourceData>(textData.text));
            ResourceData[ResourceData.Count - 1].Convert(eRscStageType.ITEM);
            for(int q = 0; q < ResourceData[ResourceData.Count - 1].Data.Count; ++q)
                dictItemCache.Add(ResourceData[ResourceData.Count - 1].Data[q].Id.ToLower(), ResourceData[ResourceData.Count - 1].Data[q]);

        }




        //==========================================================================
        //
        // Resource Control
        //
        //
        public ResourceInfo GetResourceInfo(string id)
        {
            id = id.ToLower();
            if(dictMatCache.ContainsKey(id))    return dictMatCache[id];
            if(dictCompCache.ContainsKey(id))   return dictCompCache[id];
            if(dictItemCache.ContainsKey(id))   return dictItemCache[id];

            return null;
        }

        public bool SellResource(string id, BigInteger BICount = default(BigInteger))
        {
            var rsc = GetResourceInfo(id);
            if (rsc == null)
                return false;

            var collectInfo = PlayerData.GetResourceCollectInfo(id);
            if (collectInfo == null)
                return false;

            BigInteger count = BICount == default(BigInteger) ? collectInfo.BICount : BICount;

            // Not enough to sell?
            if (count < 1)      return false;

            
            bool ret    = PlayerData.SellResource(id, count);
            if(!ret)    return false;

            context.RequestQuery("IdleMiner", "AddMoney", (errMsg, ret) =>{}, new CurrencyAmount((BICount*rsc.BIPrice).ToString(), eCurrencyType.MINING_COIN));
            return true;
        }



        public bool TryAutoSell(string id)
        {
            var rsc = GetResourceInfo(id);
            if (rsc == null)
                return false;

            BigInteger biSellCount = PlayerData.TryAutoSell(id);
            if(biSellCount==null || biSellCount==0)
                return false;


            // Debug.Log($"[AutoSell] : [{id}] - [{biSellCount}] sold.");

            BigInteger sellPrice = biSellCount * rsc.BIPrice;
            context.RequestQuery("IdleMiner", "AddMoney", (errMsg, ret) =>{}, new CurrencyAmount(sellPrice.ToString(), eCurrencyType.MINING_COIN));
            return true;
        }

        public BigInteger SellResource(string id)
        {
            var rsc = GetResourceInfo(id);
            if (rsc == null)
                return BigInteger.Zero;

            BigInteger biSellCount = PlayerData.SellResource(id);
            if(biSellCount==null || biSellCount==BigInteger.Zero)
                return BigInteger.Zero;


            // Debug.Log($"[AutoSell] : [{id}] - [{biSellCount}] sold.");

            BigInteger sellPrice = biSellCount * rsc.BIPrice;
            context.RequestQuery("IdleMiner", "AddMoney", (errMsg, ret) =>{}, new CurrencyAmount(sellPrice.ToString(), eCurrencyType.MINING_COIN));
            return biSellCount;
        }



        #region ===> Requestables

        void RegisterRequestables()
        {
            context.AddRequestDelegate("Resource", "GetResourceInfo", getResourceInfo);
            context.AddRequestDelegate("Resource", "TryAutoSell", tryAutoSell);
            context.AddRequestDelegate("Resource", "PlayerData.UpdateResourceX1000", updateResourceX1000);
            context.AddRequestDelegate("Resource", "PlayerData.GetResourceCollectionInfo", getResourceCollectionInfo);
            context.AddRequestDelegate("Resource", "PlayerData.GetResourceCollectInfo", getResourceCollectInfo);
        }

        void UnregisterRequestables()
        {
            context.RemoveRequestDelegate("Resource", "GetResourceInfo");
            context.RemoveRequestDelegate("Resource", "TryAutoSell");
            context.RemoveRequestDelegate("Resource", "PlayerData.UpdateResourceX1000");
            context.RemoveRequestDelegate("Resource", "PlayerData.GetResourceCollectionInfo");
            context.RemoveRequestDelegate("Resource", "PlayerData.GetResourceCollectInfo");
        }

        object getResourceInfo(params object[] data)
        {
            if(data.Length < 1)
                return null;

            string rscId = (string)data[0];
            return GetResourceInfo(rscId);
        }
        object tryAutoSell(params object[] data)
        {
            if(data.Length < 1)
                return null;

            string rscId = (string)data[0];
            return TryAutoSell(rscId);
        }
        object getResourceCollectionInfo(params object[] data)
        {
            if(data.Length < 1)
                return null;

            string rscId = (string)data[0];
            return PlayerData.GetResourceCollectInfo(rscId);
        }
        object updateResourceX1000(params object[] data)
        {
            if(data.Length < 3)
                return null;

            // string rscId, BigInteger _countx1000, bool offset = true)
            string rscId = (string)data[0];
            BigInteger countx1000 = (BigInteger)data[1];
            bool isOffset = (bool)data[2];
            PlayerData.UpdateResourceX1000(rscId, countx1000, isOffset);
            return null;
        }
        object getResourceCollectInfo(params object[] data)
        {
            if(data.Length < 1)
                return null;

            string rscId = (string)data[0];
            return PlayerData.GetResourceCollectInfo(rscId);
        }
        #endregion

    }


}
