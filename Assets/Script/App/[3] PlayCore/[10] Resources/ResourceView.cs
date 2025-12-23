using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameCore.UI;
// using App.GamePlay.IdleGame.SubSystem;
using Core.Utils;
using Core.Events;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Events;

namespace App.GamePlay.IdleMiner.Resouces
{
    public class ResourceView : IGCore.MVCS.AView// ARefreshable
    {
        // Const values.
        public const string EVENT_ON_ENABLED = "ElementDialog_OnEnable";
        public const string EVENT_ON_DISABLED = "ElementDialog_OnDisable";

        // Events from this view.
        [HideInInspector] public UnityEvent<float> EventSellCountSliderChanged;
        [HideInInspector] public UnityEvent EventBtnAutoClicked;
        [HideInInspector] public UnityEvent EventBtnSellClicked;
        [HideInInspector] public UnityEvent<int> EventTabIndexChanged;

        // Serialize Field.
        [SerializeField] TabButtons MainMenuTabs;
        [SerializeField] TabButtons RscTabBtns;
        [SerializeField] GameObject ItemCache;
        [SerializeField] Transform Content;
        [SerializeField] GameObject PnlSelectRsc, PnlRscSelected;
        [SerializeField] TMP_Text TxtBtnAutoSell;
        [SerializeField] TMP_Text TxtProgressItemCount;
        [SerializeField] Slider SellCountSlider;
        [SerializeField] Transform PoolerParent;

        public class PresentInfo : APresentor
        {
            public PresentInfo(List<ResourceItemComp.PresentInfo> itemInfo, string selectedSellCnt)
            {
                ItemPresentInfo = itemInfo;
                SelectedSellCount = selectedSellCnt;
            }

            public List <ResourceItemComp.PresentInfo> ItemPresentInfo { get; private set; }
            public string SelectedSellCount { get; private set; }
        }

        // Members.
        bool IsStarted = false;
        GameObjectPooler ListItemPooler = new GameObjectPooler();
        List<ResourceItemComp> Items = new List<ResourceItemComp>();
        

        // Start is called before the first frame update
        void Start()
        {
            if (IsStarted)  return;

            Assert.IsNotNull(MainMenuTabs);
            Assert.IsNotNull(RscTabBtns);
            Assert.IsNotNull(ItemCache);
            Assert.IsNotNull(Content);
            Assert.IsNotNull(PnlSelectRsc);
            Assert.IsNotNull(PnlRscSelected);
            Assert.IsNotNull(TxtBtnAutoSell);
            Assert.IsNotNull(TxtProgressItemCount);
            Assert.IsNotNull(SellCountSlider);
            Assert.IsNotNull(PoolerParent);

            IsStarted = true;
            RscTabBtns.SelectedIndex = 0;
            RscTabBtns.OnSelectionChanged.AddListener( OnTabSelectionChanged );
            ItemCache.SetActive(false);
            ListItemPooler.Create(ItemCache, PoolerParent);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!IsStarted) Start();    // Start() gets called AFTER OnEnable().

            EventSystem.DispatchEvent(EVENT_ON_ENABLED, this);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            for (int q = 0; q < Items.Count; ++q)
                GameObjectPooler.ReleasePoolItem(ListItemPooler, Items[q].gameObject);
            Items.Clear();

            EventSystem.DispatchEvent(EVENT_ON_DISABLED, this);
        }

        public override void Refresh(APresentor presentor)
        {
            if(!IsStarted)  Start();

            PresentInfo info = (PresentInfo)presentor;
            if (info == null) return;

            bool rebuildList = info.ItemPresentInfo.Count != Items.Count;
            if(rebuildList)
            {
                for (int q = 0; q < Items.Count; ++q)
                    GameObjectPooler.ReleasePoolItem(ListItemPooler, Items[q].gameObject);
                Items.Clear();
            }

            bool IsRscSelected = false;
            bool IsSelectedAusoSellOn = false;
            int idx = 0;
            for (int q = 0; q < info.ItemPresentInfo.Count; ++q)
            {
                var obj = rebuildList ? GameObjectPooler.GetPoolItem(ListItemPooler, Content, Vector3.zero) : Items[q].gameObject;
                obj.SetActive(true);
                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, .0f);
                var itemComp = obj.GetComponent<ResourceItemComp>();
                Assert.IsNotNull(itemComp);

                itemComp.Init(info.ItemPresentInfo[q].resourceId);
                itemComp.Refresh(info.ItemPresentInfo[q]);
                if (rebuildList) Items.Add(itemComp);

                if (!IsRscSelected && info.ItemPresentInfo[q].IsSelected)
                {
                    IsRscSelected = true;
                    IsSelectedAusoSellOn = info.ItemPresentInfo[q].autoSell;
                }

                ++idx;
            }

            PnlRscSelected.SetActive(IsRscSelected);
            PnlSelectRsc.SetActive(!IsRscSelected);
            if(IsRscSelected)
            {
                TxtBtnAutoSell.text = IsSelectedAusoSellOn ? "AUTO SELL OFF" : "AUTO SELL ON";
                TxtProgressItemCount.text = info.SelectedSellCount;
            }
        }

        


        //
        // Event Recvr.
        //
        public void OnSellCountSliderChanged()
        {
            EventSellCountSliderChanged.Invoke(SellCountSlider.value);
        }
        public void OnClickBtnAuto()
        {
            EventBtnAutoClicked.Invoke();
        }
        public void OnClickBtnSell()
        {
            EventBtnSellClicked.Invoke();
        }
        void OnTabSelectionChanged(int idx)
        {
            EventTabIndexChanged.Invoke(idx);
        }

        public void OnBtnCloseClicked()
        {
            MainMenuTabs.CloseAll();
            gameObject.SetActive(false);
        }
    }
}
