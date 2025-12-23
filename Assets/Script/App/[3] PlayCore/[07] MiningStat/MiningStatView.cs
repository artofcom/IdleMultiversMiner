using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Events;
using App.GamePlay.IdleMiner;
using UnityEngine.Assertions;
using UnityEngine.Events;
using System;
using FrameCore.UI;
using App.GamePlay.IdleMiner.Common.Types;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class MiningStatView : IGCore.MVCS.AView
    {
        #region ===> Instance Properties

        public static readonly string ID = "PlanetView";
        public enum SECTION
        {
            ALL, PLANET, MANAGER, RESOURCE, PERFORMANCE, MAX
        }

        [SerializeField] TabButtons MainMenuTabs;
        [SerializeField] TMP_Text txtTitle;
        
        [Header("Planet Section")]
        [SerializeField] PlanetSectorComp planetSector;

        [Header("Manager Section")]
        [SerializeField] PlanetManagerCardComp ManagerCard;

        [Header("Resource/Damage Section")]
        [SerializeField] PlanetResourceSectorComp resourceSector;
        [SerializeField] PlanetDamageSectorComp damageSector;

        [Header("Performance Section")]
        [SerializeField] PlanetPerformanceSectorComp performanceSector;

        public static Action EventOnBtnCloseClicked;
        public static Action<Tuple<int, int, eABILITY>> EventOnUpgradeClicked = null;
        public static Action EventOnUpgradeLevelModeClicked = null;
        public static Action<Tuple<int, int, eABILITY>> EventOnStatResetClicked = null;
        public Action EventOnBtnBrowseToRight;
        public Action EventOnBtnBrowseToLeft;
        public static Action EventOnBtnResourceShortCutClicked;
        public static Action EventOnBtnManagerShortCutClicked;


        public PlanetDamageSectorComp DamageSector => damageSector;
      //  public MiningPlayView MiningPlayView => playView as MiningPlayView;

        //
        // NOTE : Fileds have NOT been fully optimized due to readablility.
        //
        public class PresentInfo : APresentor
        {
            // Mining Mode. - Open
            public PresentInfo(string txtTitle, PlanetSectorComp.PresentInfo _planetInfo, PlanetManagerCardComp.PresentInfo _managerInfo,
                List<PlanetResourceItemComp.PresentInfo> _mineInfo,
                List<PlanetDamageItemComp.PresentInfo> _damagesInfo, 
                string _lvMode,
                PlanetStatComp.PresentInfo _shotInterval,
                PlanetStatComp.PresentInfo _shotAccuracy,
                PlanetStatComp.PresentInfo _shipSpeed,
                PlanetStatComp.PresentInfo _cargoSize)
            {
               // PlanetId = _planetId;
                PnlTitle = txtTitle;
                PlanetSection = _planetInfo;
                ManagerSection = _managerInfo;
                MiningStats = _mineInfo;
                DamagesInfo = _damagesInfo;

                LevelMode = _lvMode;
                ShotInterval = _shotInterval;
                ShotAccuracy = _shotAccuracy;

                ShipSpeed = _shipSpeed;
                CargoSize = _cargoSize;
                IsOpened = true;
            }

            // Battle Mode. - Open
            public PresentInfo(string txtTitle, PlanetSectorComp.PresentInfo _planetInfo, PlanetManagerCardComp.PresentInfo _managerInfo,
                List<PlanetResourceItemComp.PresentInfo> _mineInfo,
                List<PlanetDamageItemComp.PresentInfo> _damagesInfo,
                string _lvMode,
                PlanetStatComp.PresentInfo _miningRate,
                PlanetStatComp.PresentInfo _shotInterval,
                PlanetStatComp.PresentInfo _shotAccuracy)
            {
           //     PlanetId = _planetId;
                PnlTitle = txtTitle;
                PlanetSection = _planetInfo;
                ManagerSection = _managerInfo;
                MiningStats = _mineInfo;
                DamagesInfo = _damagesInfo;

                LevelMode = _lvMode;

                MiningRate = _miningRate;
                ShotInterval = _shotInterval;
                ShotAccuracy = _shotAccuracy;
                IsOpened = true;
            }
            // Battle Mode - Cleared.
            public PresentInfo(string txtTitle, PlanetSectorComp.PresentInfo _planetInfo, PlanetManagerCardComp.PresentInfo _managerInfo, bool dontcare_4BattleCleared)
            {
                IsOpened = true;
                IsBattleCleared = true;

             //   PlanetId = _planetId;
                PnlTitle = txtTitle;
                PlanetSection = _planetInfo;
                ManagerSection = _managerInfo;

                // Close Mode only for BattleMode. - Make some dummy object for the BattleMode.
                DamagesInfo = new List<PlanetDamageItemComp.PresentInfo> { };
                MiningRate = new PlanetStatComp.PresentInfo();
            }

            // Both - closed.
            public PresentInfo(string txtTitle, PlanetSectorComp.PresentInfo _planetInfo, PlanetManagerCardComp.PresentInfo _managerInfo)
            {
                IsOpened = false;
              //  PlanetId = _planetId;
                PnlTitle = txtTitle;
                PlanetSection = _planetInfo;
                ManagerSection = _managerInfo;

                // Close Mode only for BattleMode. - Make some dummy object for the BattleMode.
                DamagesInfo = new List<PlanetDamageItemComp.PresentInfo> { };
                MiningRate = new PlanetStatComp.PresentInfo();
            }

            // Data Section.
            //public int PlanetId { get; private set; }
            public bool IsOpened {  get; private set; }
            public bool IsBattleCleared { get; private set; } = false;
            public string PnlTitle {  get; private set; }
            public PlanetSectorComp.PresentInfo PlanetSection { get; private set; } 
            public PlanetManagerCardComp.PresentInfo ManagerSection { get; private set; }
            public List<PlanetResourceItemComp.PresentInfo> MiningStats { get; private set; } // should be 1 ~ 3.
            public List<PlanetDamageItemComp.PresentInfo> DamagesInfo { get; private set; } 

            public string LevelMode { get; private set; }
            public PlanetStatComp.PresentInfo MiningRate { get; private set; }
            public PlanetStatComp.PresentInfo ShotInterval { get; private set; }
            public PlanetStatComp.PresentInfo ShotAccuracy { get; private set; }
            public PlanetStatComp.PresentInfo ShipSpeed  { get; private set; }
            public PlanetStatComp.PresentInfo CargoSize  { get; private set; }
        }

        #endregion ===> Instance Properties


        #region ===> Unity Callbacks

        // Start is called before the first frame update
        private void Awake()
        {
         //   Assert.IsNotNull(MainMenuTabs);
            Assert.IsNotNull(txtTitle);
            Assert.IsNotNull(planetSector);
     //       Assert.IsNotNull(ManagerCard);
            Assert.IsNotNull(resourceSector);
       //     Assert.IsNotNull(damageSector);
            Assert.IsNotNull(performanceSector);
        }

        protected override void OnEnable()
        { 
            base.OnEnable();

            PlanetStatComp.EventOnUpgradeClicked += OnStatUpgradeClicked;
            PlanetStatComp.EventOnResetClicked += OnStatResetStatClicked;
        }

        protected override void OnDisable()
        { 
            base.OnDisable();

            PlanetStatComp.EventOnUpgradeClicked -= OnStatUpgradeClicked;
            PlanetStatComp.EventOnResetClicked -= OnStatResetStatClicked;
        }

        #endregion ===> Unity Callbacks

        
        #region ===> Interfaces

        public override void Refresh(APresentor presentInfo)
        {
            RefreshSction(presentInfo, SECTION.ALL);
        }

        internal void RefreshSction(APresentor presentInfo, SECTION eSection)
        {
            if (presentInfo == null)
                return;

            PresentInfo presentor = (PresentInfo)presentInfo;
            if(presentor == null) return;

            txtTitle.text = presentor.PnlTitle;

            // Planet Section.
            if (eSection == SECTION.ALL || eSection == SECTION.PLANET)
                planetSector.Refresh(presentor.PlanetSection);
            

            // Manager Section.
         //   if(eSection == SECTION.ALL || eSection == SECTION.MANAGER)
        //        ManagerCard.Refresh(presentor.ManagerSection);


            // Mining Status Section.
            if (eSection == SECTION.ALL || eSection == SECTION.RESOURCE)
            {
                resourceSector.gameObject.SetActive(presentor.MiningStats!=null);
            //    damageSector.gameObject.SetActive(presentor.DamagesInfo!=null);
                
                if (resourceSector.gameObject.activeSelf)
                    resourceSector.Refresh(new PlanetResourceSectorComp.PresentInfo(presentor.MiningStats));
            //    else if (damageSector.gameObject.activeSelf)
            //    {
                   /* if (presentor.IsBattleCleared)
                        damageSector.Refresh(dontcare_4clear:true);
                    else if (presentor.IsOpened) 
                        damageSector.Refresh(new PlanetDamageItemComp.PresentInfo(presentor.DamagesInfo));
                    else 
                        damageSector.Refresh();
                   */
            //    }
            }


            // Upgrade Section.
            if (eSection == SECTION.ALL || eSection == SECTION.PERFORMANCE)
            {
                if (presentor.MiningRate == null)   // Mining Mode.
                {
                    performanceSector.Refresh(
                        new PlanetPerformanceSectorComp.PresentInfo( presentor.LevelMode, presentor.ShotInterval, presentor.ShotAccuracy,
                                                                    presentor.ShipSpeed, presentor.CargoSize));
                }
                else                                // Boss BattleMode.
                {
                    if (presentor.IsBattleCleared)
                        performanceSector.Refresh(dontcare_4cleared: true);
                    else if (presentor.IsOpened)
                        performanceSector.Refresh(presentor.LevelMode, presentor.MiningRate, presentor.ShotInterval, presentor.ShotAccuracy);
                    else
                        performanceSector.Refresh();
                }
            }
            
        }

        #endregion ===> Interfaces
        

        #region ===> Event Handlers

        public void OnBtnClose()
        {
     //       MainMenuTabs.CloseAll();
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

        // planetId, MineAbility-Type.
        void OnStatUpgradeClicked(Tuple<int, int, eABILITY> tupData)
        {
            EventOnUpgradeClicked?.Invoke(tupData);
        }
        void OnStatResetStatClicked(Tuple<int, int, eABILITY> tupData)
        {
            EventOnStatResetClicked?.Invoke(tupData);
        }
        public void OnLevelUpgradeModeClicked()
        {
            EventOnUpgradeLevelModeClicked?.Invoke();
        }
        public void OnResourceShortCutClicked()
        {
            EventOnBtnResourceShortCutClicked?.Invoke();
        }
        public void OnManagerShortCutClicked()
        {
            EventOnBtnManagerShortCutClicked?.Invoke();
        }

        #endregion ===> Event Handlers
    }

}
