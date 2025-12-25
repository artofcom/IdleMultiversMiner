using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameCore.UI;
using UnityEngine.Assertions;
using Core.Events;
using UnityEngine.Events;
using Core.Utils;
using TMPro;
using IGCore.Components;

namespace App.GamePlay.IdleMiner.Craft
{
    public class CraftView : IGCore.MVCS.AView
    {
        // Const Definitions.
        public const string EVENT_ONENABLED = "CraftDialog_OnEnabled";
        public const string EVENT_ONDISBLED = "CraftDialog_OnDisabled";
        const string DIALOG_KEY = "craftDialog";

        // Events from this view.
        [HideInInspector] public UnityEvent<int> EventTabIndexChanged;
        [HideInInspector] public UnityEvent<int> EventOnEmptySlotClicked;
        [HideInInspector] public UnityEvent<int> EventOnBtnShowRecipeClicked;
        [HideInInspector] public UnityEvent<int> EventOnLockedSlotClicked;
        [HideInInspector] public UnityEvent<int> EventOnBtnProgXClicked;


        // Serialize Fields.
       // [SerializeField] TabButtons MainMenuTabs;
        [SerializeField] TabButtons TabBtns;
        [SerializeField] GameObject RecipeItemCache;
        [SerializeField] Transform Content;
        [SerializeField] Transform PoolerParent;
     //   [SerializeField] PanelSlider panelSlider;
     //   [SerializeField] Transform trSliderDirImage;
        [SerializeField] TMP_Text txtStatus;

        [SerializeField] GameObject openedScreen;
        [SerializeField] GameObject lockedScreen;

        [ImplementsInterface(typeof(INotificator))] 
        [SerializeField] MonoBehaviour mainBtnNotificator;

        [ImplementsInterface(typeof(INotificator))] 
        [SerializeField] MonoBehaviour compTabNotificator;
        [ImplementsInterface(typeof(INotificator))] 
        [SerializeField] MonoBehaviour itemTabNotificator;

        public int InnerSingleCraftItemCount => RecipeItemCache!=null ? RecipeItemCache.GetComponent<CraftListMultiItemComp>().SingleItemCount : 0;
        public INotificator NotificatorComp => mainBtnNotificator as INotificator;

        // Members.
        EventsGroup Events = new EventsGroup();
        GameObjectPooler ListItemPooler = new GameObjectPooler();
        List<CraftListMultiItemComp> ListItems = new List<CraftListMultiItemComp>();
        bool IsStarted = false;

        bool isPanelHigh;
        bool IsPanelHigh
        {
            get => isPanelHigh;
            set
            {
                isPanelHigh = value;
                Vector3 vRot = isPanelHigh ? new Vector3(.0f, .0f, 270.0f) : new Vector3(.0f, .0f, 90.0f);
            //    trSliderDirImage.rotation = Quaternion.Euler(vRot);
            }
        }


        // Accessor.
        public int TabIndex => TabBtns.SelectedIndex;

        public class PresentInfo : APresentor
        {
            public PresentInfo()
            {
                infos = null;
            }
            public PresentInfo(List<CraftListMultiItemComp.PresentInfo> _info, string status)
            {
                infos = _info;
                this.status = status;
            }
            public List<CraftListMultiItemComp.PresentInfo> infos { get; private set; }
            public string status;
        }




        // Start is called before the first frame update
        void Start()
        {
            if (IsStarted) return;

        //    Assert.IsNotNull(MainMenuTabs);
            Assert.IsNotNull(TabBtns);
            Assert.IsNotNull(RecipeItemCache);
            Assert.IsNotNull(Content);
            Assert.IsNotNull(PoolerParent);
            Assert.IsNotNull(txtStatus);

            IsStarted = true;
            TabBtns.SelectedIndex = 0;
            TabBtns.OnSelectionChanged.AddListener(OnTabSelectionChanged);
            RecipeItemCache.SetActive(false);
            ListItemPooler.Create(RecipeItemCache, PoolerParent);

            Events.RegisterEvent(CraftSingleItemComp.EVENT_LOCKED_CLICKED, SingleCraftItemComp_OnBtnLockedClicked);
            Events.RegisterEvent(CraftSingleItemComp.EVENT_EMPTY_CLICKED, SingleCraftItemComp_OnBtnEmptySlotClicked);
            Events.RegisterEvent(CraftSingleItemComp.EVENT_SLOT_CLICKED, SingleCraftItemComp_OnBtnRecipeClicked);
            Events.RegisterEvent(CraftSingleItemComp.EVENT_PROG_X_CLICKED, SingleCraftItemComp_OnBtnProgXClicked);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!IsStarted) Start();

            IsPanelHigh = false;
       //     panelSlider.SlidePanel(IsPanelHigh);
            EventSystem.DispatchEvent(EVENT_ONENABLED, this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            for (int q = 0; q < ListItems.Count; ++q)
                GameObjectPooler.ReleasePoolItem(ListItemPooler, ListItems[q].gameObject);
            ListItems.Clear();

            EventSystem.DispatchEvent(EVENT_ONDISBLED);
        }

        private void OnDestroy()
        {
            TabBtns.OnSelectionChanged.RemoveListener(OnTabSelectionChanged);

            Events.UnRegisterEvent(CraftSingleItemComp.EVENT_LOCKED_CLICKED, SingleCraftItemComp_OnBtnLockedClicked);
            Events.UnRegisterEvent(CraftSingleItemComp.EVENT_EMPTY_CLICKED, SingleCraftItemComp_OnBtnEmptySlotClicked);
            Events.UnRegisterEvent(CraftSingleItemComp.EVENT_SLOT_CLICKED, SingleCraftItemComp_OnBtnRecipeClicked);
            Events.UnRegisterEvent(CraftSingleItemComp.EVENT_PROG_X_CLICKED, SingleCraftItemComp_OnBtnProgXClicked);
        }

        public void UpdateNotificator(string id, eRscStageType eLevel)
        {
            NotificatorComp?.EnableNotification(id);

            INotificator notificator = eLevel == eRscStageType.COMPONENT ? compTabNotificator as INotificator : itemTabNotificator as INotificator;
            if(notificator != null) 
                notificator.EnableNotification(id);
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;
            if (info == null)
                return;

            openedScreen.SetActive(info.infos != null);
            lockedScreen.SetActive(info.infos == null);

            if(info.infos == null)
                return;

            txtStatus.text = info.status;

            bool rebuildList = ListItems.Count != info.infos.Count;
            if (rebuildList)
            {
                for (int q = 0; q < ListItems.Count; ++q)
                    GameObjectPooler.ReleasePoolItem(ListItemPooler, ListItems[q].gameObject);
                ListItems.Clear();
            }

            for(int q = 0; q < info.infos.Count; ++q)
            {
                var obj = rebuildList ? GameObjectPooler.GetPoolItem(ListItemPooler, Content, Vector3.zero) : ListItems[q].gameObject;
                obj.SetActive(true);
                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, .0f);
                var itemComp = obj.GetComponent<CraftListMultiItemComp>();
                Assert.IsNotNull(itemComp);

                itemComp.Init(q*itemComp.SingleItemCount, DIALOG_KEY);
                itemComp.Refresh(info.infos[q]);

                if(rebuildList)
                    ListItems.Add(itemComp);
            }

            // Update Notificator.
            INotificator notificator = TabIndex == 0 ? compTabNotificator as INotificator : itemTabNotificator as INotificator;
            if(notificator != null) 
                notificator.DisableNotification();
        }

        public void InitAfterReset()
        {
            NotificatorComp?.Reset();

            (compTabNotificator as INotificator)?.Reset();
            (itemTabNotificator as INotificator)?.Reset();

            TabBtns.SelectedIndex = 0;
        }


        // Events Handler.
        //
        void OnTabSelectionChanged(int idx)
        {
            EventTabIndexChanged.Invoke(idx);
        }

        void SingleCraftItemComp_OnBtnRecipeClicked(object data)
        {
            var itemComp = (CraftSingleItemComp)data;
            Assert.IsNotNull(itemComp);
            if (itemComp == null || itemComp.ParentDialogKey!=DIALOG_KEY)
                return;

            EventOnBtnShowRecipeClicked.Invoke(itemComp.SlotIndex);
        }

        void SingleCraftItemComp_OnBtnEmptySlotClicked(object data)
        {
            var itemComp = (CraftSingleItemComp)data;
            Assert.IsNotNull(itemComp);
            if (itemComp == null || itemComp.ParentDialogKey != DIALOG_KEY)
                return;

            EventOnEmptySlotClicked.Invoke(itemComp.SlotIndex);
        }

        void SingleCraftItemComp_OnBtnLockedClicked(object data)
        {
            var itemComp = (CraftSingleItemComp)data;
            Assert.IsNotNull(itemComp);
            if (itemComp == null || itemComp.ParentDialogKey != DIALOG_KEY)
                return;

            EventOnLockedSlotClicked.Invoke(itemComp.SlotIndex);
        }

        void SingleCraftItemComp_OnBtnProgXClicked(object data)
        {
            var itemComp = (CraftSingleItemComp)data;
            Assert.IsNotNull(itemComp);
            if (itemComp == null || itemComp.ParentDialogKey != DIALOG_KEY)
                return;

            EventOnBtnProgXClicked.Invoke(itemComp.SlotIndex);
        }

        public void OnBtnCloseClicked()
        {
        //    MainMenuTabs.CloseAll();
            gameObject.SetActive(false);
        }

        public void OnBtnPanelSlider()
        {
            IsPanelHigh = !IsPanelHigh;
       //     panelSlider.SlidePanel(IsPanelHigh);
        }
    }
}
