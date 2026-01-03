//using System.Collections;
using App.GamePlay.Common;
using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using Core.Events;
using Core.Util;
using Core.Utils;
using IGCore.Components;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class IdleMinerController : AController
    {
        //  IEnumerator miningPumpCoroutine = null;
        IEnumerator welcomebackDlgCoroutine = null;

        //  Events ----------------------------------------
        //
        EventsGroup Events = new EventsGroup();

        string mStrWelcomeMsg = string.Empty;
        float mLastWriteTime = .0f;
        bool isResumeProcessing = false;

        TopUICompController topUICompController = null;
        IdleMinerContext IMContext => (IdleMinerContext)context;

        internal IdleMinerModel Model => (IdleMinerModel)model;
        public IdleMinerView View => (IdleMinerView)view;

        #region AController

        public IdleMinerController(AUnit unit, AView view, AModel model, AContext context)
            : base(unit, view, model, context)
        { 
            topUICompController = new TopUICompController(null, ((IdleMinerView)view).TopHUDView, model, context);
        }

        // PlayScreen.Attach() -> IdleMinerControler.() -> AController.() -> InitController()
        public override void Init() 
        {
            // Alloc Events.
            Events.RegisterEvent(EventID.APPLICATION_FOCUSED, EventOnApplicationFocus);
            Events.RegisterEvent(EventID.APPLICATION_PAUSED, EventOnApplicationPause);
            Events.RegisterEvent(EventID.GAME_CURRENCY_UPDATED, EventOnMoneyUpdated);
            
            Events.RegisterEvent(EventID.MINING_STAT_UPGRADED, EventOnMiningStatusChanged);
            Events.RegisterEvent(EventID.PLANET_UNLOCKED, EventOnMiningStatusChanged);
            Events.RegisterEvent(EventID.PLANET_BOOSTER_TRIGGERED, EventOnMiningStatusChanged);

            Events.RegisterEvent(EventID.SKILL_LEARNED, EventOnSkillLearned);
         //   Events.RegisterEvent(EventID.GAME_RESET_REFRESH, EventOnResetRefresh);

            Events.RegisterEvent(EventID.CRAFT_RECIPE_ASSIGNED, EventOnCraftStatusChanged);
            Events.RegisterEvent(EventID.CRAFT_RECIPE_PURCHASED, EventOnCraftStatusChanged);
            Events.RegisterEvent(EventID.CRAFT_SLOT_EXTENDED, EventOnCraftStatusChanged);

            Events.RegisterEvent(EventID.DAILY_MISSION_GOAL_ACHIEVED, Event_DailyMissionGoalAchieved);
            Events.RegisterEvent(EventID.DAILY_MISSION_RESET, Event_DailyMissionReset);

      //      View.EventOnBtnOptionClicked += View_OnBtnOptionClicked;
     // //      View.EventOnBtnShopClicked += View_OnBtnShopClicked;
     //       View.EventOnBtnTimedBonusClicked += View_OnBtnTimedBonusClicked;
     //       View.EventOnBtnDailyAttendClicked += View_OnBtnDailyAttendClicked;
    //        View.EventOnBtnTaskClicked += View_OnBtnTaskClicked; 

            View.EventOnBtnBackClicked += View_OnBtnBackClicked;
            View.EventOnBtnSettingClicked += View_OnBtnSettingClicked;
            View.EventOnBtnAdsBonusClicked += View_OnBtnAdsBonusClicked;
            View.EventOnBtnTimedBonusClicked += View_OnBtnTimedBonusClicked;
            View.EventOnGameCardsPortalClicked += View_OnBtnGameCardsPortalClicked;
            View.EventOnBtnDailyMissionClicked += View_OnBtnDailyMissionClicked;

            SettingDialogView.EventOnBtnBGMClicked += EventOptionDlgOnBtnBGMClicked;
            SettingDialogView.EventOnBtnSoundFXClicked += EventOptionDlgOnBtnSoundFXClicked;
            

         //   OptionPopupDialog.EventOnClickedPlayerLocalData += OptionDialog_OnBtnClearPlayerDataClicked;
         //   OptionPopupDialog.EventOnClickedCollectMiningCoin += OptionDialog_OnBtnCollect_MiningCoinClicked;
         //   OptionPopupDialog.EventOnClickedCollectRuneCoin += OptionDialog_OnBtnCollect_RuneCoinClicked;

            ShopPopupDialog.EventOnBuyClicked += ShopDialog_OnBtnBuyClicked;

            //
            welcomebackDlgCoroutine = coFireWelcomeBackDialog();  
            
            if(!context.IsSimulationMode())
            {
                View.EnableBGM((bool)context.RequestQuery("AppPlayerModel", "IsBGMOn"));
                View.EnableSoundFX((bool)context.RequestQuery("AppPlayerModel", "IsSoundFXOn"));

                RefreshNotificator();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            // Alloc Events.
            Events.UnRegisterAll();

            View.EventOnBtnBackClicked -= View_OnBtnBackClicked;
            View.EventOnBtnSettingClicked -= View_OnBtnSettingClicked;
            View.EventOnBtnAdsBonusClicked -= View_OnBtnAdsBonusClicked;
            View.EventOnBtnTimedBonusClicked -= View_OnBtnTimedBonusClicked;
            View.EventOnGameCardsPortalClicked -= View_OnBtnGameCardsPortalClicked;
            View.EventOnBtnDailyMissionClicked -= View_OnBtnDailyMissionClicked;

            SettingDialogView.EventOnBtnBGMClicked -= EventOptionDlgOnBtnBGMClicked;
            SettingDialogView.EventOnBtnSoundFXClicked -= EventOptionDlgOnBtnSoundFXClicked;

  //          View.EventOnBtnOptionClicked -= View_OnBtnOptionClicked;
  //          View.EventOnBtnShopClicked -= View_OnBtnShopClicked;
  //          View.EventOnBtnTimedBonusClicked -= View_OnBtnTimedBonusClicked;
  //          View.EventOnBtnDailyAttendClicked -= View_OnBtnDailyAttendClicked;
 //           View.EventOnBtnTaskClicked -= View_OnBtnTaskClicked; 

       //     OptionPopupDialog.EventOnClickedPlayerLocalData -= OptionDialog_OnBtnClearPlayerDataClicked;
       //     OptionPopupDialog.EventOnClickedCollectMiningCoin -= OptionDialog_OnBtnCollect_MiningCoinClicked;
      //      OptionPopupDialog.EventOnClickedCollectRuneCoin -= OptionDialog_OnBtnCollect_RuneCoinClicked;

            ShopPopupDialog.EventOnBuyClicked -= ShopDialog_OnBtnBuyClicked;
        }

        public override void Resume(int awayTimeInSec) 
        {
#if UNITY_EDITOR
            if(context.IsSimulationMode()) 
                SIM_UpdateResourceReqStatus();
#endif

            // Logging player data ATM.
            Model.Resume();
        }
        
        public override void Pump() { }

        public override void WriteData()
        {
            if(IMContext.IsSimulationMode())
                return;

            //Model.SavePlayerData();
            //IMContext.RequestStoreGameData();

            mLastWriteTime = Time.time;
          //  Debug.Log("[DataRecorded]..Player Data has been written... " + mLastWriteTime.ToString());
        }

        protected override void OnViewEnable() 
        {
            if(welcomebackDlgCoroutine == null)
                welcomebackDlgCoroutine = coFireWelcomeBackDialog();  

            view.StartCoroutine(welcomebackDlgCoroutine);
            view.StartCoroutine( coFirstTimeRefreshView() );

            // Fire event once for updating UI.
           // DelayedAction.TriggerActionWithDelay(view, 0.1f, () =>
           // {
           //     EventSystem.DispatchEvent(EventID.GAME_CURRENCY_UPDATED, new CurrencyAmount(Model.PlayerData.GetMoney(eCurrencyType.MINING_COIN).ToString(), eCurrencyType.MINING_COIN));
           // });
        }
        protected override void OnViewDisable() { }

        #endregion




        #region Event Handlers.
        void EventOnApplicationFocus(object data)
        {
            bool bFocus = (bool)data;
            bool isBackgrounded = !bFocus;

            if(!isBackgrounded) StartResumePrcess();
            else                isResumeProcessing = false;
        }

        void EventOnApplicationPause(object data)
        {
            bool bPaused = (bool)data;
            bool isBackgrounded = bPaused;

            if(!isBackgrounded) StartResumePrcess();
            else                isResumeProcessing = false;
        }

        void EventOnMoneyUpdated(object data)
        {
            RefreshView();
        }

        void EventOnMiningStatusChanged(object data)
        {
            WriteData();
        }
        void EventOnSkillLearned(object data)
        {
            WriteData();
        }
        void EventOnCraftStatusChanged(object data)
        { 
            WriteData(); 
        }

        void Event_DailyMissionGoalAchieved(object data)
        {
            if(context.IsSimulationMode())
                return;

            DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, 0.1f, () =>
            {
                RefreshNotificator();
            });
        }

        void Event_DailyMissionReset(object data)
        {
            if(context.IsSimulationMode())
                return;

            DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, 0.1f, () =>
            {
                RefreshNotificator();
            });
        }

        void ShopDialog_OnBtnBuyClicked(int amount)
        {
            Model.PlayerData.AddMoney(new CurrencyAmount(amount.ToString(), eCurrencyType.IAP_COIN));
        }

        void View_OnBtnOptionClicked()
        {
            //_model.PlayerData.AddMoney(new CurrencyAmount("100000", eCurrencyType.MINING_COIN));

         //   OptionPopupDialog.PresentInfo info = new OptionPopupDialog.PresentInfo(true, true);
            //_context.PopUpScreen.DisplayPopupDialog(OptionPopupDialog.sID, info, (dlg) => { });
        }

        void View_OnBtnShopClicked()
        {
            //model.PlayerData.AddMoney(new CurrencyAmount("1000", eCurrencyType.IAP_COIN));

            //_context.PopUpScreen.DisplayPopupDialog(ShopPopupDialog.sID, null, (dlg) => { });

            var presentInfo = new ShopPopupDialog.PresentInfo(null, null, null);

            context.RequestQuery(unitName:(string)context.GetData(KeySets.CTX_KEYS.GAME_DLG_KEY), endPoint:"DisplayPopupDialog", 
                finishCallback : (errMsg, ret) => 
                {
                    if(string.IsNullOrEmpty(errMsg))
                        Debug.Log("Shop Dialog has been poped up successfully!");
                }, 
                // Params for the endPoint.
                "ShopDialog",  presentInfo,
                new System.Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("Shop Dialog has been closed.");
                } ) );  
        }

        void View_OnBtnAdsBonusClicked()
        {
            CurrencyAmount bonusAmount = null;
            context.RequestQuery(unitName:"GamePlay", endPoint:"CalculateBonusRewardCurrencyAmount",  (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                bonusAmount = (CurrencyAmount)ret;
            }, 100.0f);

            Debug.Log("TimedBonus Amount : " + bonusAmount.BIAmount);

          
            var presentInfo = new MessageDialog.PresentInfo( 
                message :  $"Watch Ads. \nEarn ${bonusAmount.BIAmount.ToAbbString()} coins", 
                title : "Timed Bonus", type : MessageDialog.Type.CONFIRM, 
                () => 
                {
                    View.AdmonHandler.DisplayAds( () =>
                    {                
                        Model.PlayerData.AddMoney( bonusAmount );
                    });

                });


            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                "MessageDialog",  
                presentInfo,
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("Message Dialog has been closed.");
                } ) );    
        }

        void View_OnBtnGameCardsPortalClicked()
        {
            var cardsCompPresentInfo = new GameCardsView.Presentor( BuildGameCardsPortalData() );

            var presentInfo = new GameCardsPortalDialog.PresentInfo(message:string.Empty, cardsCompPresentInfo, GameCardsPortalDlg_OnBtnGameCardClicked);

            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GAME_DLG_KEY), "DisplayPopupDialog", 
                finishCallback : (errMsg, ret) =>  { },
                "GameCardsPortalDialog",  
                presentInfo,
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("Game Portal Dialog has been closed.");
                } ) );   
        }

        void View_OnBtnDailyMissionClicked()
        {
            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayUnitPopupDialog", 
                (errMsg, ret) => {}, 
                "DailyMission",
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("Daily Mission Dialog has been closed.");

                } ));
            
            context.RequestQuery("DailyMission.PlayerData", "SeenAllNotificationInfo");
            RefreshNotificator();
        }


        List<Tuple < string, AView.APresentor >> BuildGameCardsPortalData()
        { 
            string curGameKey = (string)context.GetData("gameKey", string.Empty);
            Assert.IsTrue(!string.IsNullOrEmpty(curGameKey));

            GameCardComp.Presentor presentor = null;
            List<Tuple < string, AView.APresentor >> listPresentor = new List<Tuple < string, AView.APresentor >>();
            int cntGameCardsData = (int)context.RequestQuery("AppPlayerModel", "GetGameCardCount");
            for(int q = 0; q < cntGameCardsData; ++q)
            {
                var cardInfo = (GameCardInfo)context.RequestQuery("AppPlayerModel", "GetGameCardInfoFromIndex", q);
                if(cardInfo == null)        continue;

                long lastPlayedTick;
                string awayTime = "AwayTime : Unknown";
                if(long.TryParse(cardInfo.LastPlayedTimeStamp, out lastPlayedTick)) 
                {
                    long elapsedTicks = IdleMinerPlayerModel.UTCNowTick - lastPlayedTick;
                    double awayTimeInSec = (new TimeSpan(elapsedTicks)).TotalSeconds;
                    awayTime = $"AwayTime : {TimeExt.ToTimeString((long)awayTimeInSec, TimeExt.UnitOption.SHORT_NAME, TimeExt.TimeOption.HOUR, useUpperCase:true)}";
                }
                presentor = string.Compare(curGameKey, cardInfo.GameId, ignoreCase:true)==0 ? 
                                new GameCardComp.Presentor() :
                                new GameCardComp.Presentor(string.Empty, awayTime, $"Reset : {cardInfo.ResetCount}", false) ;
                listPresentor.Add( new Tuple<string, AView.APresentor>(cardInfo.GameId, presentor));
            }

            return listPresentor;
        }

        void GameCardsPortalDlg_OnBtnGameCardClicked(string gameKey)
        {
            Debug.Log("Game Start Clicked..." + gameKey);
            (context as IdleMinerContext).CoRunner.StartCoroutine( coTriggerActionWithDelay(0.1f, () =>
            {
                context.AddData("gameKey" , gameKey.ToLower());
                
                //OnEventClose?.Invoke("PlayScreen");
                context.RequestQuery("PlayScreen", "SwitchUnit", (errMsg, ret) => { }, "PlayScreen");

            }));
       
        }

        void View_OnBtnTimedBonusClicked()
        {
            var gameConfig = (GameConfig)context.GetData("GameConfig", null);
            Assert.IsNotNull(gameConfig);

            BigInteger bonusGold = BigInteger.Zero;
            context.RequestQuery(unitName:"GamePlay", endPoint:"CalculateBonusRewardCurrencyAmount",  (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                bonusGold = (BigInteger)ret;

            }, gameConfig.TimedBonusRewardRatio);

            Debug.Log("TimedBonus Gold Amount : " + bonusGold.ToAbbString());

          
            var presentInfo = new MessageDialog.PresentInfo( 
                message :  $"Earnning ${bonusGold.ToAbbString()} coins", 
                title : "Timed Bonus", type : MessageDialog.Type.CONFIRM, 
                () => 
                {
                    Model.PlayerData.AddMoney( new CurrencyAmount(bonusGold.ToString(), eCurrencyType.MINING_COIN) );
                });

            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                "MessageDialog",  
                presentInfo,
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("Message Dialog has been closed.");
                } ) );    
        
        }

        void View_OnBtnDailyAttendClicked() { }
        void View_OnBtnTaskClicked() { }

        void View_OnBtnBackClicked()
        {
            Debug.Log("IEM - OnBtnBackClicked..");
            (context as IdleMinerContext).CoRunner.StartCoroutine( coTriggerActionWithDelay(0.1f, () =>
            {
                // OnEventClose?.Invoke("LobbyScreen");
                context.RequestQuery("PlayScreen", "SwitchUnit", (errMsg, ret) => { }, "LobbyScreen");

            }));

        }

        void View_OnBtnSettingClicked()
        {
            //Model.PlayerData.AddMoney(new CurrencyAmount("50", eCurrencyType.IAP_COIN));
            //Model.PlayerData.AddMoney(new CurrencyAmount("500", eCurrencyType.MINING_COIN));
            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayUnitPopupDialog", 
                (errMsg, ret) => {}, 
                "OptionDialog", 
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("Option dlg has been closed from X.");

                }));  
        }

        
        
        void EventOptionDlgOnBtnBGMClicked(bool isOn)
        {
            Debug.Log("BGM has been clicked..." + isOn);
            context.RequestQuery("AppPlayerModel", "SetBGM", isOn);
            View.EnableBGM(isOn);
        }
        void EventOptionDlgOnBtnSoundFXClicked(bool isOn)
        {
            Debug.Log("SoundFX has been clicked..." + isOn);
            context.RequestQuery("AppPlayerModel", "SetSoundFX", isOn);
            View.EnableSoundFX(isOn);
        }
        

        void OptionDialog_OnBtnClearPlayerDataClicked()
        {
          //  PlayerData.ClearAllData();
            PlayerPrefs.DeleteAll();
            Debug.Log("Deleting All PlayerPrefab data...");

#if UNITY_EDITOR 
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OptionDialog_OnBtnCollect_MiningCoinClicked()
        {
            //_model.PlayerData.AddMoney(new CurrencyAmount("100000", eCurrencyType.MINING_COIN));
        }
        void OptionDialog_OnBtnCollect_RuneCoinClicked()
        {
            //_model.PlayerData.AddMoney(new CurrencyAmount("100000", eCurrencyType.IAP_COIN));   
        }   

        #endregion  // Event Handlers.



        void StartResumePrcess()
        {
            if (isResumeProcessing)
                return;

            isResumeProcessing = true;
            Debug.Log("Resume Processing....");

            Model.EventOnFocused();

            if (welcomebackDlgCoroutine != null)
                view.StopCoroutine(welcomebackDlgCoroutine);

            welcomebackDlgCoroutine = coFireWelcomeBackDialog();
            view.StartCoroutine(welcomebackDlgCoroutine);

        }


        #region ===> Helpers

        IEnumerator coTriggerActionWithDelay(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);

            action?.Invoke();
        }

        IEnumerator coFireWelcomeBackDialog()
        {
            yield return new WaitForSeconds(0.5f);

            bool isNewPlayer = false;
            context.RequestQuery("IdleMiner", "IsNewPlayer", (errMsg, ret) => isNewPlayer = (bool)ret); 

           if (Model.PlayerData.IdleAwayTime < 1 || isNewPlayer)
                yield break;

            int idlePumpTime = (int)Model.PlayerData.FlushAwayTime(); //Model.PlayerData.IdleAwayTime;

            yield return null;// new WaitUntil(() => string.IsNullOrEmpty(WelcomeBackPopupDialog.sID) == false);

            string awayTime = TimeExt.ToTimeString(idlePumpTime, TimeExt.UnitOption.NO_USE, TimeExt.TimeOption.HOUR);
            mStrWelcomeMsg = $"You've been away for \n {awayTime}.";
            WelcomeBackPopupDialog.PresentInfo info = new WelcomeBackPopupDialog.PresentInfo(mStrWelcomeMsg, string.Empty, isLoading:true);
            
            context.RequestQuery(unitName:(string)context.GetData(KeySets.CTX_KEYS.GAME_DLG_KEY), endPoint:"DisplayPopupDialog", 
                finishCallback : (errMsg, ret) => 
                {
                    if(string.IsNullOrEmpty(errMsg))
                        Debug.Log("WelcomeBack Dialog has been poped up successfully!");
                }, 
                // Params for the endPoint.
                "WelcomeBackDialog",  
                info, 
                new System.Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("WelcomeBack Dialog has been closed.");
                } ) );    
        }

        IEnumerator coFirstTimeRefreshView()
        {
            yield return new WaitUntil( () => Model.PlayerData.IsInitialized==true );

            RefreshView();
        }

        void RefreshView()
        {
#if UNITY_EDITOR
            if(context.IsSimulationMode())
                return;
#endif

            long iapCurrency = (long)context.RequestQuery("AppPlayerModel", "GetIAPCurrency");
            long starCurrency = (long)context.RequestQuery("AppPlayerModel", "GetStarCurrency");

            Debug.Log("<color=green>[MainView] Refreshing....</color>");
            IdleMinerView.PresentInfo info = new IdleMinerView.PresentInfo(
                Model.PlayerData.GetMoney(eCurrencyType.MINING_COIN).ToAbbString(), 
                iapCurrency.ToString(),
                starCurrency.ToString(), 
                Model.PlayerData.OpenedTabBtns);
            View.Refresh( info );
        }

        void RefreshNotificator()
        {
            // Daily Mission.
            NotificationInfo dailyMissionNotiInfo = null;

            context.RequestQuery("DailyMission.PlayerData", "GetNotificationInfo",  
                (errMsg, ret) => 
                {
                    dailyMissionNotiInfo = (NotificationInfo)ret;
                }); 
            Assert.IsNotNull( dailyMissionNotiInfo );

            View.DailyMissionNotificator.Reset();
            if(dailyMissionNotiInfo.SeenReasons != null)
            {
                for(int q = 0; q < dailyMissionNotiInfo.SeenReasons.Count; ++q)
                    View.DailyMissionNotificator.EnableNotification(dailyMissionNotiInfo.SeenReasons[q]);
            
                View.DailyMissionNotificator.DisableNotification();
            }
            if(dailyMissionNotiInfo.UnseenReasons != null)
            {
                for(int q = 0; q < dailyMissionNotiInfo.UnseenReasons.Count; ++q)
                    View.DailyMissionNotificator.EnableNotification(dailyMissionNotiInfo.UnseenReasons[q]);
            }
        }

        #endregion



        #region Simulator Related.

#if UNITY_EDITOR

        void SIM_UpdateResourceReqStatus()
        {
            context.RequestQuery(unitName:"Craft", endPoint:"SIM_UpdateResourceReqStatus",  (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
            });
        }

#endif

        #endregion

        // void InitLearnedSkills()
        ////{
        //     GetMinerModule<SkillTreeController>()?.InitLearnedSkills();
        // }
    }
}
