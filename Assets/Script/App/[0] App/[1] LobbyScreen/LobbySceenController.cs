using App.GamePlay.Common;
using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using Core.Events;
//using Game.Manager.Data;
using Core.Util;
using Core.Utils;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScreenController : AController
{
    const float WAIT_TIME_SEC = 1.5f;

    public Action<Type> OnClose;    // Next AController Type.

    IdleMinerContext IMContext => (IdleMinerContext)context;

    TopUICompController topUICtrl;

    bool bLoadingGame = false;

    public LobbyScreenController(AView view, AModel model, AContext ctx)
        : base(view, model, ctx)
    { 
        topUICtrl = new TopUICompController(((LobbyScreenView)view).TopHUDView, model, context);
    }

    public override void Init()
    {
        object queryRet = context.RequestQuery("AppPlayerModel", "IsBGMOn");
        bool bEnabled = queryRet != null ? (bool)queryRet : true;
        (view as LobbyScreenView).EnableBGM(bEnabled);

        queryRet = context.RequestQuery("AppPlayerModel", "IsSoundFXOn");
        bEnabled = queryRet != null ? (bool)queryRet : true;
        (view as LobbyScreenView).EnableSoundFX(bEnabled);
    }

    protected override void OnViewEnable()
    {
        LobbyScreenView.EventOnBtnStart += EventOnBtnStart;
        LobbyScreenView.EventOnBtnOptionDialogClicked += EventOnBtnOptionDlgClicked;
        LobbyScreenView.EventOnBtnShopDialogClicked += EventOnBtnShopDlgClicked;
        LobbyScreenView.EventOnBtnDailyMissionClicked += EventOnBtnDailyMissionClicked;
        
        OptionDialog.EventOnBtnBGMClicked += EventOptionDlgOnBtnBGMClicked;
        OptionDialog.EventOnBtnSoundFXClicked += EventOptionDlgOnBtnSoundFXClicked;

        ShopPopupDialog.EventOnBuyClicked += ShopDlg_EventOnBtnBuyClicked;

        bLoadingGame = false;

        RefreshView();

        view.StartCoroutine(coUpdate());

        DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, WAIT_TIME_SEC, () =>
        {
            OnClose?.Invoke(typeof(TitleScreenController));
        });
    }
    
    protected override void OnViewDisable() 
    {
        LobbyScreenView.EventOnBtnStart -= EventOnBtnStart;
        LobbyScreenView.EventOnBtnOptionDialogClicked -= EventOnBtnOptionDlgClicked;
        LobbyScreenView.EventOnBtnShopDialogClicked -= EventOnBtnShopDlgClicked;
        ShopPopupDialog.EventOnBuyClicked -= ShopDlg_EventOnBtnBuyClicked;
        LobbyScreenView.EventOnBtnDailyMissionClicked -= EventOnBtnDailyMissionClicked;

        OptionDialog.EventOnBtnBGMClicked -= EventOptionDlgOnBtnBGMClicked;
        OptionDialog.EventOnBtnSoundFXClicked -= EventOptionDlgOnBtnSoundFXClicked;

        ShopPopupDialog.EventOnBuyClicked -= ShopDlg_EventOnBtnBuyClicked;

        bLoadingGame = false;
    }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }

    
    IEnumerator coUpdate()
    {
        yield return new WaitForSeconds(0.1f);
        RefreshView();

        while(true)
        {
            yield return new WaitForSeconds(1.0f);

            RefreshView();
        }
    }

    void RefreshView()
    {
        if(context == null)     return;

        GameCardComp.Presentor presentor = null;
        List<Tuple < string, AView.APresentor >> listPresentor = new List<Tuple < string, AView.APresentor >>();
        object queryResult = context.RequestQuery("AppPlayerModel", "GetGameCardCount");
        if(queryResult == null) return;
        
        int cntGameCardsData = (int)queryResult;
        for(int q = 0; q < cntGameCardsData; ++q)
        {
            var cardInfo = (GameCardInfo)context.RequestQuery("AppPlayerModel", "GetGameCardInfoFromIndex", q);
            if(cardInfo == null)        continue;

            //AView cardView = (view as LobbyScreenView).GetGameCardView( cardInfo.GameId );
            //if(cardView == null)        continue;
            //var cardComp = (cardView as GameCardComp);
            //if(cardComp == null)        continue;

            long lastPlayedTick;
            string awayTime = "AwayTime : Unknown";
            if(long.TryParse(cardInfo.LastPlayedTimeStamp, out lastPlayedTick)) 
            {
                long elapsedTicks = IdleMinerPlayerModel.UTCNowTick - lastPlayedTick;
                double awayTimeInSec = (new TimeSpan(elapsedTicks)).TotalSeconds;
                awayTime = $"AwayTime : {TimeExt.ToTimeString((long)awayTimeInSec, TimeExt.UnitOption.SHORT_NAME, TimeExt.TimeOption.HOUR, useUpperCase:true)}";
            }
            presentor = new GameCardComp.Presentor(string.Empty, awayTime, $"Reset : {cardInfo.ResetCount}", false);
            listPresentor.Add( new Tuple<string, AView.APresentor>(cardInfo.GameId, presentor));
        }

        view.Refresh(new LobbyScreenView.Presentor( new GameCardsPortalComp.Presentor(listPresentor) ) );

        // Debug.Log("Refreshing Lobby view....");
    }


    void EventOnBtnStart(string gameKey) 
    { 
        if(bLoadingGame)        return;

        bLoadingGame = true;

        this.context.AddData("gameKey" , gameKey.ToLower());

        OnEventClose?.Invoke("PlayScreen");
    }

    void EventOnBtnOptionDlgClicked()
    {
         context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.LOBBY_DLG_KEY), "DisplayPopupDialog", 
            (errMsg, ret) => {}, 
            "OptionDialog",  
            new OptionDialog.PresentInfo((bool)context.RequestQuery("AppPlayerModel", "IsSoundFXOn"), (bool)context.RequestQuery("AppPlayerModel", "IsBGMOn")),
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("Option Dialog has been closed.");

            } ) );
    }

    void EventOnBtnShopDlgClicked()
    {
         context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.LOBBY_DLG_KEY), "DisplayPopupDialog", 
            (errMsg, ret) => {}, 
            "ShopDialog",  
            new ShopPopupDialog.PresentInfo(null, null, null),
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("Shop Dialog has been closed.");

            } ) );
    }

    void EventOnBtnDailyMissionClicked()
    {
        context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.LOBBY_DLG_KEY), "DisplayPopupDialog", 
            (errMsg, ret) => {}, 
            "DailyMissionDialog",  
            new DailyTaskView.Presentor(),
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("Shop Dialog has been closed.");

            } ) );
    }

    void ShopDlg_EventOnBtnBuyClicked(int amount)
    {
        const bool offset = true;
        context.RequestQuery("AppPlayerModel", "UpdateIAPCurrency", amount, offset);
    }

    void EventOptionDlgOnBtnBGMClicked(bool isOn)
    {
        Debug.Log("BGM has been clicked..." + isOn);
        context.RequestQuery("AppPlayerModel", "SetBGM", isOn);
        (view as LobbyScreenView).EnableBGM(isOn);
    }
    void EventOptionDlgOnBtnSoundFXClicked(bool isOn)
    {
        Debug.Log("SoundFX has been clicked..." + isOn);
        context.RequestQuery("AppPlayerModel", "SetSoundFX", isOn);
        (view as LobbyScreenView).EnableSoundFX(isOn);
    }


    /*
    LobbyScreenView _view;
    GameContext _context;

    //  Events ----------------------------------------
    //
    EventsGroup Events = new EventsGroup();


    string mGameMainPrefab;

    public LobbyScreenController(AView view, GameContext ctx)
    {
        _view = view as LobbyScreenView;
        _context = ctx;

        Events.RegisterEvent(LobbyScreenView.EVENT_BTN_PLAY_CLICKED, LobbyScreenView_OnBtnPlayClicked);
    }




    void LobbyScreenView_OnBtnPlayClicked(object data)
    {
        _view.StartCoroutine(coLoadGame());
    }

    IEnumerator coLoadGame()
    {
        // yield return _view.StartCoroutine(coLoadGameController());

        yield return _view.StartCoroutine(coLoadGameBundle());
    }*/


    IEnumerator coLoadGameBundle()
    {
        yield break;
        /*
        string bundleName = "G001_CobraHearts";
        string assetName = // "SlotMainPortrait.prefab";//
                           "SlotMain.prefab";
        string LOCAL_BUNDLE_PATH = "Assets/Bundles";
        string assetPathExt = $"{bundleName}/{assetName}";// + GetFileExtension(typeof(T));
        string externalizedAssetPath = $"{LOCAL_BUNDLE_PATH}/{assetPathExt}";
        Debug.Log("[Fetching] Loading locally..." + externalizedAssetPath);
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(externalizedAssetPath);
        */
        /*
        mGameMainPrefab = "G010_WordTileSearch/GameMain";

        GameObject prefab = Resources.Load($"Bundles/{mGameMainPrefab}") as GameObject;
        UnityEngine.Assertions.Assert.IsNotNull(prefab);

        _context.GamePrefab = prefab;
        yield return null;

        EventSystem.DispatchEvent("LobbyScreenView_OnGamePrefabLoaded");
        */
    }

    /*
     * IEnumerator coLoadGameController()
    {
        // Load Controller config.
        //string controllerName = "G001_CobraHearts";// SurgeInfo.ControllerAsset;
        string controllerName = "G030_TripleGolds";// SurgeInfo.ControllerAsset;
        //if (true)  // for now -  !_context.BootStrap.setting.UseRemoteBundle)
       //     controllerName += ".json";

        bool isFinishLoadingConfig = false;
        yield return _view.StartCoroutine(_context.GameCtrlManager.CoLoadController(controllerName,
            (loadedInfo) =>
            {
                _context.GameControlData = loadedInfo as ISlotControlData;

                UnityEngine.Assertions.Assert.IsNotNull(loadedInfo);

                if (loadedInfo != null) Debug.Log("Controller Loaded Successfully!");
                else Debug.Log("Controller Load Failed!!!");


                Debug.Log($"Slot - {loadedInfo.DisplayName}");
                Debug.Log($"Reel - {loadedInfo.Rule.RowCount}X{loadedInfo.Rule.ColCount}");
                Debug.Log($"Paylines - {loadedInfo.Rule.Paylines[0]}");
                Debug.Log($"Reel - {loadedInfo.Rule.Reels.PaidSpin[0]}");
                Debug.Log($"ClientData - {loadedInfo.ClientData.Bundle}/{loadedInfo.ClientData.Prefab}");

                mGameMainPrefab = $"{loadedInfo.ClientData.Bundle}/{loadedInfo.ClientData.Prefab}";

                isFinishLoadingConfig = true;
            }));

        yield return new WaitUntil(() => isFinishLoadingConfig == true);
    }

    IEnumerator coLoadGameBundle()
    {
        /*
        string bundleName = "G001_CobraHearts";
        string assetName = // "SlotMainPortrait.prefab";//
                           "SlotMain.prefab";
        string LOCAL_BUNDLE_PATH = "Assets/Bundles";
        string assetPathExt = $"{bundleName}/{assetName}";// + GetFileExtension(typeof(T));
        string externalizedAssetPath = $"{LOCAL_BUNDLE_PATH}/{assetPathExt}";
        Debug.Log("[Fetching] Loading locally..." + externalizedAssetPath);
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(externalizedAssetPath);
        

    GameObject prefab = Resources.Load($"Bundles/{mGameMainPrefab}") as GameObject;
    UnityEngine.Assertions.Assert.IsNotNull(prefab);

        _context.GamePrefab = prefab;
        yield return null;

        EventSystem.DispatchEvent("LobbyScreenView_OnGamePrefabLoaded");
    }*/
}
