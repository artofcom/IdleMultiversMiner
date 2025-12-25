using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Core.Events;
using UnityEngine.Events;
using App.GamePlay.IdleMiner.Craft;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class RecipeListPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //

        // Serialize Fields -------------------------------
        //
        [SerializeField] GameObject RecipeItemCache;
        [SerializeField] Transform Content;


        // Events ----------------------------------------
        //
        [HideInInspector] public UnityEvent<int> EventLockedSlotClicked;


        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        EventsGroup Events = new EventsGroup();
        bool IsStarted = false;
        int mSelectedSlotIndex = -1;


        public int SelectedSlotIndex => mSelectedSlotIndex;

        List<CraftListMultiItemComp> RecipeItems = new List<CraftListMultiItemComp>();

        public class PresentInfo : APresentor
        {
            public PresentInfo(List<CraftListMultiItemComp.PresentInfo> _info)
            {
                infos = _info;
            }
            public List<CraftListMultiItemComp.PresentInfo> infos { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (IsStarted) return;

            Assert.IsNotNull(RecipeItemCache);
            Assert.IsNotNull(Content);

            IsStarted = true;
            RecipeItemCache.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!IsStarted) Start();

            Events.RegisterEvent(RecipeSingleItemComp.EVENT_SLOT_CLICKED, SingleRecipeItemComp_OnBtnSlotClicked);
            Events.RegisterEvent(RecipeSingleItemComp.EVENT_LOCKED_CLICKED, SingleRecipeItemComp_OnBtnLockedSlotClicked);
        }
        protected override void OnDisable()
        {
            Events.UnRegisterEvent(RecipeSingleItemComp.EVENT_SLOT_CLICKED, SingleRecipeItemComp_OnBtnSlotClicked);
            Events.UnRegisterEvent(RecipeSingleItemComp.EVENT_LOCKED_CLICKED, SingleRecipeItemComp_OnBtnLockedSlotClicked);
        }

        public override void InitDialog(string id)
        {
            base.InitDialog(id);
            sID = id;
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;
            if (info == null)
                return;

            bool rebuildList = RecipeItems.Count != info.infos.Count;
            if (rebuildList)
            {
                for (int q = 0; q < RecipeItems.Count; ++q)
                    Destroy(RecipeItems[q].gameObject);
                RecipeItems.Clear();
            }

            int innerItemCount = RecipeItemCache.GetComponent<CraftListMultiItemComp>().SingleItemCount;
            for (int q = 0; q < info.infos.Count; ++q)
            {
                var obj = rebuildList ? Instantiate(RecipeItemCache, Content) : RecipeItems[q].gameObject;
                obj.SetActive(true);
                var itemComp = obj.GetComponent<CraftListMultiItemComp>();
                Assert.IsNotNull(itemComp);

                itemComp.Init(q * innerItemCount, gameObject.name);
                itemComp.Refresh(info.infos[q]);

                if(rebuildList)
                    RecipeItems.Add(itemComp);
            }
        }


        public void OnCloseBtnClicked()
        {
            mSelectedSlotIndex = -1;
            OnClose();
        }

        void SingleRecipeItemComp_OnBtnSlotClicked(object data)
        {
            if (!gameObject.activeSelf)
                return;

            var itemComp = (RecipeSingleItemComp)data;
            Assert.IsNotNull(itemComp);
            if (itemComp == null || itemComp.ParentDialogKey != gameObject.name)
                return;

            mSelectedSlotIndex = itemComp.SlotIndex;
            OnClose();
        }

        void SingleRecipeItemComp_OnBtnLockedSlotClicked(object data)
        {
            if (!gameObject.activeSelf)
                return;

            var itemComp = (RecipeSingleItemComp)data;
            Assert.IsNotNull(itemComp);
            if (itemComp == null || itemComp.ParentDialogKey != gameObject.name)
                return;

            EventLockedSlotClicked.Invoke(itemComp.SlotIndex);
        }
    }
}
