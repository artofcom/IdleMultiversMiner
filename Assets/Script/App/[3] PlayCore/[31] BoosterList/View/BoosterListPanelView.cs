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
using System;

namespace App.GamePlay.IdleMiner
{
    public class BoosterListPanelView : ARefreshable
    {
        // Events from this view.
        public static Action<ARefreshable> EventOnEnable;
        public static Action EventOnDisable;

        // Serialize Field.
        [SerializeField] TabButtons MainMenuTabs;
        [SerializeField] GameObject ItemCache;
        [SerializeField] Transform Content;
        [SerializeField] Transform PoolerParent;

        public class PresentInfo : IPresentor
        {
            public PresentInfo(List<BoosterItemComp.PresentInfo> itemInfo)
            {
                ItemPresentInfo = itemInfo;
            }
            public List <BoosterItemComp.PresentInfo> ItemPresentInfo { get; private set; }            
        }

        // Members.
        GameObjectPooler ListItemPooler = new GameObjectPooler();
        List<BoosterItemComp> Items = new List<BoosterItemComp>();


        // Start is called before the first frame update
        void Awake()
        {
            Assert.IsNotNull(MainMenuTabs);
            Assert.IsNotNull(ItemCache);
            Assert.IsNotNull(Content);
            Assert.IsNotNull(PoolerParent);

            ItemCache.SetActive(false);
            ListItemPooler.Create(ItemCache, PoolerParent);
        }

        private void OnEnable()
        {
            EventOnEnable?.Invoke(this);
        }
        private void OnDisable()
        {
            for (int q = 0; q < Items.Count; ++q)
                GameObjectPooler.ReleasePoolItem(ListItemPooler, Items[q].gameObject);
            Items.Clear();
            EventOnDisable?.Invoke();
        }

        public override void Refresh(IPresentor presentor)
        {
            PresentInfo info = (PresentInfo)presentor;
            if (info == null) return;

            bool rebuildList = info.ItemPresentInfo.Count != Items.Count;
            if(rebuildList)
            {
                for (int q = 0; q < Items.Count; ++q)
                    GameObjectPooler.ReleasePoolItem(ListItemPooler, Items[q].gameObject);
                Items.Clear();
            }

            int idx = 0;
            for (int q = 0; q < info.ItemPresentInfo.Count; ++q)
            {
                var obj = rebuildList ? GameObjectPooler.GetPoolItem(ListItemPooler, Content, Vector3.zero) : Items[q].gameObject;
                obj.SetActive(true);
                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, .0f);
                var itemComp = obj.GetComponent<BoosterItemComp>();
                Assert.IsNotNull(itemComp);

                itemComp.Refresh(info.ItemPresentInfo[q]);
                if (rebuildList) Items.Add(itemComp);

                ++idx;
            }
        }

        


        //
        // Event Recvr.
        //
        public void OnBtnCloseClicked()
        {
            MainMenuTabs.CloseAll();
            gameObject.SetActive(false);
        }
    }
}
