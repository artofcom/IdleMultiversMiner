using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner.Resouces
{
    //  Town Management.-------------------------------------
    //
    public class ResourceController : IGCore.MVCS.AController
    {
        ResourceView View => (ResourceView)view;
        
        eRscStageType CurRscLevel = eRscStageType.MATERIAL;
        string SelectedRscId = string.Empty;
        BigInteger BISelectedRscCount = BigInteger.Zero;
        float ProgressRate = 1.0f;
        EventsGroup events = new EventsGroup();
        ResourceModel Model => (ResourceModel)model;
        IdleMinerContext IMContext => (IdleMinerContext)context;

        #region ===> Core

        public ResourceController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { }

        public override void Init() 
        {
            View?.EventSellCountSliderChanged.AddListener(OnRSCListPnlSellCountSliderChanged);
            View?.EventBtnAutoClicked.AddListener(OnRSCListPnlClickBtnAuto);
            View?.EventBtnSellClicked.AddListener(OnRSCListPnlClickBtnSell);
            View?.EventTabIndexChanged.AddListener(OnRSCListPnlTabSelectionChanged);

            events.RegisterEvent(ResourceItemComp.EVENT_PANEL_CLICKED, ElementItemComp_OnPnlClicked);
            events.RegisterEvent(ResourceItemComp.EVENT_AUTOSELL_CLICKED, ElementItemComp_OnAutoSellClicked);
            events.RegisterEvent(EventID.RESOURCE_UPDATED, ResourceListView_OnResourceUpdated);

            events.RegisterEvent(EventID.GAME_RESET_REFRESH, OnRefreshView);
        }

        public override void Dispose()
        {
            base.Dispose();

            View?.EventSellCountSliderChanged.RemoveAllListeners();
            View?.EventBtnAutoClicked.RemoveAllListeners();
            View?.EventBtnSellClicked.RemoveAllListeners();
            View?.EventTabIndexChanged.RemoveAllListeners();

            events.UnRegisterEvent(ResourceItemComp.EVENT_PANEL_CLICKED, ElementItemComp_OnPnlClicked);
            events.UnRegisterEvent(ResourceItemComp.EVENT_AUTOSELL_CLICKED, ElementItemComp_OnAutoSellClicked);
            events.UnRegisterEvent(EventID.RESOURCE_UPDATED, ResourceListView_OnResourceUpdated);

            events.UnRegisterEvent(EventID.GAME_RESET_REFRESH, OnRefreshView);
        }

        public override void Resume(int duration)
        {
            if(context.IsSimulationMode())
            {
                SellResourcesIfPossible();

                LogSimulationStat();
            }
            else
            {
                // Game Mode Auto Sell.
                foreach(string srcId in Model.PlayerData.GetResourceCollectionInfoKeys())
                    Model.TryAutoSell(srcId);
            }

        }

        public override void Pump() { }

        public override void WriteData()
        {
            Model.PlayerData.WriteData();
        }

        protected override void OnViewEnable() 
        { 
            view.StartCoroutine( coTriggerActionWithDelay(-1.0f, () => RefreshView()) );
        }
        protected override void OnViewDisable() { }

        #endregion




        #region ===> Event Handler

        void OnRSCListPnlSellCountSliderChanged(float value)
        {
            if (string.IsNullOrEmpty(SelectedRscId))
                return;

            ProgressRate = value;
            RefreshView();
        }

        void OnRSCListPnlClickBtnAuto()
        {
            if (string.IsNullOrEmpty(SelectedRscId))
                return;

            var rscInfo = Model.PlayerData.GetResourceCollectInfo(SelectedRscId);
            if (rscInfo == null)
                return;

            rscInfo.AutoSell_ = !rscInfo.AutoSell_;
            if(rscInfo.AutoSell_)
            {
                Model.SellResource(rscInfo.RscId_, rscInfo.BICount);
                Debug.Log($"AutoSell turned on for {rscInfo.RscId_}, sold:[{rscInfo.BICount.ToAbbString()}].");
            }

            RefreshView();
        }

        void OnRSCListPnlClickBtnSell()
        {
            var rscInfo = Model.PlayerData.GetResourceCollectInfo(SelectedRscId);
            if (rscInfo == null || rscInfo.BICount <= BigInteger.Zero)
                return;

            // BISelectedRscCount = (rscInfo.BICount * (int)((1.0f-ProgressRate) * 1000.0f)) / 1000;
            //
            BISelectedRscCount = Math.Max(1, (int)(rscInfo.BICount * (int)(ProgressRate * 1000.0f)) / 1000);
            Model.SellResource(SelectedRscId, BISelectedRscCount);
            RefreshView();
        }

        void OnRSCListPnlTabSelectionChanged(int idx)
        {
            SelectedRscId = string.Empty;
            CurRscLevel = (eRscStageType)idx;
            RefreshView();
        }

        void ElementItemComp_OnPnlClicked(object data)
        {
            if (data == null)
                return;

            SelectedRscId = (string)data;

            var rscInfo = Model.PlayerData.GetResourceCollectInfo(SelectedRscId);
            if (rscInfo == null)
                return;
         
            RefreshView();
        }

        void ElementItemComp_OnAutoSellClicked(object data)
        {
            if (data == null)
                return;

            string rscId = (string)data;
            var rscInfo = Model.PlayerData.GetResourceCollectInfo(rscId);
            if (rscInfo == null)
                return;
           
            rscInfo.AutoSell_ = !rscInfo.AutoSell_;
            if(rscInfo.AutoSell_)
            {
                Model.SellResource(rscInfo.RscId_, rscInfo.BICount);
                Debug.Log($"AutoSell turned on for {rscInfo.RscId_}, sold:[{rscInfo.BICount.ToAbbString()}].");
            }
            RefreshView();
        }

        void ResourceListView_OnResourceUpdated(object data)
        {      
            var rscId_value = (Tuple<string, BigInteger>)data;
            Debug.Log($"<color=green>[SIM][Action] Resource {rscId_value.Item1} count has been increased to {rscId_value.Item2}.</color>");
            
            RefreshView();
        }

        void OnRefreshView(object data)
        {
            RefreshView();
        }

        IEnumerator coTriggerActionWithDelay(float delay, Action action)
        {
            if(delay < .0f) yield return null;
            else            yield return new WaitForSeconds(delay);

            action?.Invoke();
        }

        #endregion




        #region ===> View Refresher & Helper

        void RefreshView()
        {
#if UNITY_EDITOR
            if(context.IsSimulationMode())
                return;
#endif

            //const string RuneIcon = "<sprite name=\"Rune\">";
            List<ResourceItemComp.PresentInfo> presentList = new List<ResourceItemComp.PresentInfo>();
            foreach (var key in Model.PlayerData.GetResourceCollectionInfoKeys())
            {
                var mat = Model.GetResourceInfo(key);
                if (mat == null || mat.eLevel != CurRscLevel)
                    continue;

                var collectinoInfo = Model.PlayerData.GetResourceCollectInfo(key);
                Assert.IsNotNull(collectinoInfo);

                Sprite iconSprite = IMContext.GetSprite(mat.GetSpriteGroupId(), spriteKey:mat.Id);
                ResourceItemComp.PresentInfo present = new ResourceItemComp.PresentInfo(
                    mat.Id, iconSprite, mat.GetClassKey(), mat.Id,
                    collectinoInfo.BICount.ToAbbString(),
                    "$"+mat.BIPrice.ToAbbString(),
                    collectinoInfo.AutoSell_,
                !string.IsNullOrEmpty(SelectedRscId) && SelectedRscId == mat.Id);
                presentList.Add(present);

                if (SelectedRscId == mat.Id)
                {
                    BISelectedRscCount = (collectinoInfo.BICount * (int)(ProgressRate * 1000.0f)) / 1000;
                    // Debug.Log($"Updated Progress Rate [{ProgressRate}] : count [{BISelectedRscCount}]");
                }
            }

            View.Refresh(new ResourceView.PresentInfo(presentList, BISelectedRscCount.ToAbbString()));
        }

        #endregion


        #region SIMULATOR


       
        void SellResourcesIfPossible()
        {
            Assert.IsTrue(context.IsSimulationMode());
                
            HashSet<string> setReqResources = (HashSet<string>)context.GetData(SimDefines.SIM_SKILLTREE_EASIEST_NODES_REQ_RESOURCES, null);

            foreach(string srcId in Model.PlayerData.GetResourceCollectionInfoKeys())
            {
                if(setReqResources!=null && (setReqResources.Contains(srcId) || setReqResources.Contains(srcId.ToLower())))     // should not sell if craft needs this.
                {
                    Debug.Log($"<color=red>[SIM] Can't sell {srcId} because CRAFT/SkillTree needs this. !</color>");
                    continue;
                }
                
                BigInteger biSoldCount = Model.SellResource(srcId);
                if(biSoldCount > BigInteger.Zero)
                    Debug.Log($"<color=green>[SIM] : Sold ResourceId[{srcId}], Sold_Amount[{biSoldCount.ToAbbString()}]</color>");
            }
        }

        void LogSimulationStat()
        {
            // Debug.Log("[SIM] : Collecting Resource Info...");

            foreach(string srcId in Model.PlayerData.GetResourceCollectionInfoKeys())
            {
                var mat = Model.GetResourceInfo(srcId);
                if (mat == null)
                    continue;
                
                float productionRate = .0f;
                context.RequestQuery("GamePlay", "GetProductionRate", (errMsg, ret) => 
                { 
                    productionRate = (float)ret;
                }, srcId);

                var collectinoInfo = Model.PlayerData.GetResourceCollectInfo(srcId);
                Debug.Log($"[SIM][Status] : ResourceId:[{srcId}], CollectedAmount:[{collectinoInfo.BICount.ToAbbString()}], PDR:[{productionRate}]");
            }
        }

        #endregion
    }
}
