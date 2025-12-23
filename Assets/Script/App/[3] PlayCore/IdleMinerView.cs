using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.MiningStat;
using Core.Events;
using Core.Tween;
//using App.GamePlay.IdleGame.SubSystem.UIComponent;
//using App.GamePlay.IdleGame.SubSystem.GamePlay;
using Core.Utils;
using FrameCore.UI;
using IGCore.Components;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using UI.Scroller;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    // Mainly Covers Top UX Group.
    //
    public class IdleMinerView : IGCore.MVCS.AView
    {
        //=============================================================================
        //
        #region ===> Instance Properties

        public Action EventOnEnabled;
        public Action EventOnDisabled;
        public Action<int> EventOnTabBtnChanged;        

   //     public Action EventOnBtnOptionClicked       { get => TopUIComp.EventOnBtnOptionClicked;         set => TopUIComp.EventOnBtnOptionClicked = value; }
   //     public Action EventOnBtnShopClicked         { get => TopUIComp.EventOnBtnShopClicked;           set => TopUIComp.EventOnBtnShopClicked = value; }
  //      public Action EventOnBtnTimedBonusClicked   { get => TopUIComp.EventOnBtnTimedBonusClicked;     set => TopUIComp.EventOnBtnTimedBonusClicked = value; }
  //      public Action EventOnBtnDailyAttendClicked  { get => TopUIComp.EventOnBtnDailyAttendClicked;    set => TopUIComp.EventOnBtnDailyAttendClicked = value; }
  //      public Action EventOnBtnTaskClicked         { get => TopUIComp.EventOnBtnOptionClicked;         set => TopUIComp.EventOnBtnOptionClicked = value; }
        

        public Action EventOnBtnBackClicked;
        public Action EventOnBtnSettingClicked;
        public Action EventOnBtnTimedBonusClicked;
        public Action EventOnBtnAdsBonusClicked;
        public Action EventOnGameCardsPortalClicked;

    //    [SerializeField] IMGameSetting gameSetting;
   //     [SerializeField] MonoBehaviour CoroutineRunner;
        [SerializeField] TopUIComp TopUIComp;
        //[SerializeField] PinchablePageScroller PinchableScroller;
        [SerializeField] TabButtons MainTabBtns;
        [SerializeField] MainTabComp featureTabBtns;

        //[ImplementsInterface(typeof(App.SubSystem.IBonus))] 
        //[SerializeField] GameObject btnTimedBonus;
        //[ImplementsInterface(typeof(App.SubSystem.IBonus))] 
        //[SerializeField] GameObject btnAdsBonus;

        [SerializeField] Core.Platform.AdmobHandler admobHandler;
        
        [SerializeField] Transform DebugObjectRoot;

        //public IMGameSetting GameSetting => gameSetting;
        public Core.Platform.AdmobHandler AdmonHandler => admobHandler;
        public TopUIComp TopHUDView => TopUIComp;

        [SerializeField] Transform BGMListRoot;
        [SerializeField] Transform SoundFXListRoot;

        public class PresentInfo : APresentor
        {
            public PresentInfo(string amountGold, string IAP_Amount, string amountStar, List<string> openedTabBtns)
            {
                AmountGold = amountGold;
                AmountIAP = IAP_Amount;
                AmountStar = amountStar;
                OpenedTabBtns = openedTabBtns;
            }

            // Data Section.
            public string AmountGold { get; private set; }
            public string AmountIAP { get; private set; }
            public string AmountStar {  get; private set; }
            public List<string> OpenedTabBtns { get; private set; }
        }
        
        #endregion ===> Instance Properties



        //=============================================================================
        //
        #region ===> Unity Callbacks

        // Start is called before the first frame update
        protected void Awake()
        {
          //  Assert.IsNotNull(CoroutineRunner);
        //    Assert.IsNotNull(TopUIComp);
            //Assert.IsNotNull(PinchableScroller);
            Assert.IsNotNull(MainTabBtns);
            //Assert.IsNotNull(btnAdsBonus);
            //Assert.IsNotNull(btnTimedBonus);

            if(DebugObjectRoot != null)
            {
#if UNITY_EDITOR
                DebugObjectRoot.gameObject.SetActive(true);
#else
                DebugObjectRoot.gameObject.SetActive(false);
#endif
            }

        }

        protected override void OnEnable()
        {
            base.OnEnable();

            EventOnEnabled?.Invoke();
            MainTabBtns.OnSelectionChanged.AddListener(OnMainTabButtonChanged);

            //btnAdsBonus.SetActive(false);
            //btnTimedBonus.SetActive(true);//false);

            // TownManager.Init(CoroutineRunner, MainCharObj, FlyMonObjCache, _controller.BuildPlanetBaseInfoDictionary());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            EventOnDisabled?.Invoke();
            MainTabBtns.OnSelectionChanged.RemoveListener(OnMainTabButtonChanged);
        }

#endregion ===> Unity Callbacks


        //=============================================================================
        //
        #region ===> Interfaces

        public override void Refresh(APresentor presentData)
        {
            PresentInfo info = presentData as PresentInfo;
            if(info == null)    return;

            if(TopUIComp != null) 
                TopUIComp.Refresh(new TopUIComp.PresentInfo(info.AmountGold, info.AmountIAP, info.AmountStar));

   //         featureTabBtns.Refresh(new MainTabComp.PresentInfo(info.OpenedTabBtns));
        }
        

        public void ClosePanelView(string panelViewKey="")
        {
            Assert.IsTrue(false);
        }

        public void EnableSoundFX(bool enable)
        {
            if(SoundFXListRoot == null) return;

            for(int q = 0; q < SoundFXListRoot.childCount; ++q)
            {
                var audio = SoundFXListRoot.GetChild(q).GetComponent<AudioSource>();
                if(audio == null) continue;

                audio.volume = enable ? 0.9f : .0f;
            }
        }

        public void EnableBGM(bool enable)
        {
            if(BGMListRoot == null) return;

            for(int q = 0; q < BGMListRoot.childCount; ++q)
            {
                var audio = BGMListRoot.GetChild(q).GetComponent<AudioSource>();
                if(audio == null) continue;

                audio.volume = enable ? 0.9f : .0f;
            }
        }

        #endregion ===> Interfaces



        //=============================================================================
        //
        #region ===> Event Handlers

        void OnMainTabButtonChanged(int idx)
        {
            //if (idx >= 0)
            //    ClosePanelView();
            EventOnTabBtnChanged?.Invoke(idx);
        }

        // For App on Device.
        private void OnApplicationFocus(bool focus)
        {
            Debug.Log("Application Focus. " + focus.ToString());
            EventSystem.DispatchEvent(EventID.APPLICATION_FOCUSED, focus);
        }

        // For Editor.
        private void OnApplicationPause(bool pause)
        {
            Debug.Log("Application Pause. " + pause.ToString());
            EventSystem.DispatchEvent(EventID.APPLICATION_PAUSED, pause);
        }


        public void OnBtnBackClicked()
        {
            EventOnBtnBackClicked?.Invoke();
        }
        public void OnBtnSettingClicked()
        {
            EventOnBtnSettingClicked?.Invoke();
        }

        public void OnBtnGameCardsPortalClicked()
        {
            EventOnGameCardsPortalClicked?.Invoke();
        }

        public void OnBtnTimedBonusClicked()
        {
            EventOnBtnTimedBonusClicked?.Invoke();
        }
        public void OnBtnAdsBonusClicked()
        {
            EventOnBtnAdsBonusClicked?.Invoke();
        }
        #endregion ===> Event Handlers



#if UNITY_EDITOR
        void OnGUI()
        {
            //GUI.skin.button.fontSize = 30;
            //if (GUI.Button(new Rect(10, 300, 150, 100), "Button"))
            //{
            //    print("You clicked the button!");
            //}
        }
#endif
    }
}