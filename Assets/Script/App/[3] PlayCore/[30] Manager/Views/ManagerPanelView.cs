using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using UnityEngine.UI;
using Core.Events;
using FrameCore.UI;
using UnityEngine.Events;
using System;

namespace App.GamePlay.IdleMiner
{
    public class ManagerPanelView : IGCore.MVCS.AView// ARefreshable
    {
        public const string EVENT_ONENABLED = "ManagerPanelView_OnEnabled";


        [SerializeField] ManagerList3ItemComp ListItemCache;
        [SerializeField] GameObject ContentRoot;

     ///   [Header("Planet Section")]
      //  [SerializeField] PlanetSectorComp planetSector;

      //  [Header("Manager Section")]
      //  [SerializeField] PlanetManagerCardComp ManagerCard;

        [Header("Manager List Section")]
        [SerializeField] TabButtons MainMenuTabs;
        [SerializeField] TMP_Text TxtSlotInfo;

        [SerializeField] GameObject InfoPnlNotSelected;
        [SerializeField] GameObject InfoPnlSelected;

        [SerializeField] Button BtnPromote;
        [SerializeField] Button BtnHire;

        List<ManagerList3ItemComp> List3Items = new List<ManagerList3ItemComp>();

        //  Events ----------------------------------------
        //
        // EventsGroup Events = new EventsGroup();
        public static Action EventOnBtnBrowseToRight;
        public static Action EventOnBtnBrowseToLeft;
        public static Action EventOnBtnPlanetManagerClicked;
        public static Action EventOnBtnHireClicked;
        public static Action EventOnBtnPromoteClicked;
        public static Action EventOnBtnDiscardClicked;
        public static Action EventOnBtnRecruitClicked;
        public static Action EventOnBtnAddSlotClicked;
        public static Action EventOnBtnManagerImageClicked;
        

        public class PresentInfo : APresentor
        {
            /*
            public PresentInfo(
                 PlanetSectorComp.PresentInfo _planetInfo, PlanetManagerCardComp.PresentInfo _managerInfo,
                bool _anyManagerSelected, int _usingSlot, int _maxSlot,
                bool _canPromote, bool _canHire,
                List<ManagerList3ItemComp.PresentInfo> _list3ItemPresentInfo)
            {
                PlanetSection = _planetInfo;
                ManagerSection = _managerInfo;
                UsingSlotCount = _usingSlot;
                MaxSlotCount = _maxSlot;
                AnyManagerSelected = _anyManagerSelected;
                ManagerList3ItemPresentor = _list3ItemPresentInfo;
                CanPromote = _canPromote;
                CanHire = _canHire;
            }*/

            public bool AnyManagerSelected { get; private set; }
            public int UsingSlotCount { get; private set; }
            public int MaxSlotCount { get; private set; }
            //public PlanetSectorComp.PresentInfo PlanetSection { get; private set; } 
            //public PlanetManagerCardComp.PresentInfo ManagerSection { get; private set; }
            public List<ManagerList3ItemComp.PresentInfo> ManagerList3ItemPresentor { get; private set; }

            public bool CanPromote { get; private set; }
            public bool CanHire { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(ListItemCache);
            Assert.IsNotNull(ContentRoot);
       //     Assert.IsNotNull(planetSector);
       //     Assert.IsNotNull(ManagerCard);
            Assert.IsNotNull(InfoPnlNotSelected);
            Assert.IsNotNull(InfoPnlSelected);
            Assert.IsNotNull(TxtSlotInfo);

            Assert.IsNotNull(BtnPromote);
            Assert.IsNotNull(BtnHire);

            ListItemCache.gameObject.SetActive(false);
        }

        public override void Refresh(APresentor _presentor)
        {
            if (_presentor == null)
                return;

            var presentor = (PresentInfo)_presentor;

            for (int q = 0; q < List3Items.Count; ++q)
                Destroy(List3Items[q].gameObject);
            List3Items.Clear();

            // Planet Area.
            //TxtPlanetName.text = presentor.PlanetName;
            //TxtPlanetStat.text = $"{presentor.PlanetMiningRate}/sec, {presentor.PlanetSpeed} mkph, {presentor.PlanetPackage}";
            //ImgPlanet.sprite = presentor.PlanetSprite;
            // Planet Manager Area.
            //PlanetManager.Refresh(presentor.PlanetManagerPresentor);

         //   planetSector.Refresh(presentor.PlanetSection);

         //   ManagerCard.Refresh(presentor.ManagerSection);

            // Manager List.
            for (int q = 0; q < presentor.ManagerList3ItemPresentor.Count; ++q)
            {
                var newObj = Instantiate(ListItemCache, ContentRoot.transform);
                var itemComp = newObj.GetComponent<ManagerList3ItemComp>();
                Assert.IsNotNull(itemComp);

                itemComp.Refresh(presentor.ManagerList3ItemPresentor[q]);
                itemComp.gameObject.SetActive(true);
                List3Items.Add(itemComp);
            }

            // Slot Info.
            TxtSlotInfo.text = $"SLOT : {presentor.UsingSlotCount}/{presentor.MaxSlotCount}";
            InfoPnlNotSelected.SetActive(!presentor.AnyManagerSelected);
            InfoPnlSelected.SetActive(presentor.AnyManagerSelected);
            BtnPromote.interactable = presentor.CanPromote;
            BtnHire.interactable = presentor.CanHire;
        }

        private void OnEnable()
        {
            EventSystem.DispatchEvent(EVENT_ONENABLED, this);

            ManagerItemComp.EventOnBtnCardClicked += ManagerItemComp_OnBtnCardClicked;
        }

        private void OnDisable()
        {
            ManagerItemComp.EventOnBtnCardClicked -= ManagerItemComp_OnBtnCardClicked;
        }

        public void OnBtnClose()
        {
            MainMenuTabs.CloseAll();
            gameObject.SetActive(false);
        }

        public void OnBtnPlanetBrowseRight()
        {
            EventOnBtnBrowseToRight?.Invoke();
        }
        public void OnBtnPlanetBrowseLeft()
        {
            EventOnBtnBrowseToLeft?.Invoke();
        }

        public void OnBtnPlanetManagerClicked()
        {
            EventOnBtnPlanetManagerClicked?.Invoke();
        }

        public void OnBtnHireClicked()
        {
            EventOnBtnHireClicked?.Invoke();
        }

        public void OnBtnPromoteClicked()
        {
            EventOnBtnPromoteClicked?.Invoke();
        }

        public void OnBtnDiscardClicked()
        {
            EventOnBtnDiscardClicked?.Invoke();
        }

        public void OnBtnRecruitClicked()
        {
            EventOnBtnRecruitClicked?.Invoke();
        }

        public void OnBtnAddSlotClicked()
        {
            EventOnBtnAddSlotClicked?.Invoke();
        }

        public void OnBtnManagerImageClicked()
        {
            EventOnBtnManagerImageClicked?.Invoke();
        }

        void ManagerItemComp_OnBtnCardClicked(string mngId)
        {
            Debug.Log(mngId + " Clicked...!");
        }
    }
}