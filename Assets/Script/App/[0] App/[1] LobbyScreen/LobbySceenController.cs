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
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class LobbyScreenController : AController
{
    const float WAIT_TIME_SEC = 1.5f;

    //public Action<Type> OnClose;    // Next AController Type.

    IdleMinerContext IMContext => (IdleMinerContext)context;

    TopUICompController topUICtrl;

    EventsGroup Events = new EventsGroup();

    bool isGameDataLoadingCompleted = false;
    bool IsGameDataLoadingCompleted()  { return isGameDataLoadingCompleted; }

    public LobbyScreenController(AUnit unit, AView view, AModel model, AContext ctx)
        : base(unit, view, model, ctx)
    { 
        topUICtrl = new TopUICompController(null, ((LobbyScreenView)view).TopHUDView, model, context);
    }

    protected override void OnViewEnable()
    {
        Events.RegisterEvent(EventID.GAME_LEVEL_START, EventOnBtnStart);
        LobbyScreenView.EventOnBtnOptionDialogClicked += EventOnBtnOptionDlgClicked;
        LobbyScreenView.EventOnBtnShopDialogClicked += EventOnBtnShopDlgClicked;
        LobbyScreenView.EventOnBtnDailyMissionClicked += EventOnBtnDailyMissionClicked;

        ShopPopupDialog.EventOnBuyClicked += ShopDlg_EventOnBtnBuyClicked;

        RefreshView();

        DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, WAIT_TIME_SEC, () =>
        {
            //OnClose?.Invoke(typeof(TitleScreenController));
        });
    }
    
    protected override void OnViewDisable() 
    {
        Events.UnRegisterAll();
        
        LobbyScreenView.EventOnBtnOptionDialogClicked -= EventOnBtnOptionDlgClicked;
        LobbyScreenView.EventOnBtnShopDialogClicked -= EventOnBtnShopDlgClicked;
        ShopPopupDialog.EventOnBuyClicked -= ShopDlg_EventOnBtnBuyClicked;
        LobbyScreenView.EventOnBtnDailyMissionClicked -= EventOnBtnDailyMissionClicked;

        ShopPopupDialog.EventOnBuyClicked -= ShopDlg_EventOnBtnBuyClicked;
    }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }

    
    void RefreshView()
    {
        if(context == null)     return;

        // view.Refresh()
    }

    void EventOnBtnStart(object data) 
    { 
        if(data == null)
            return;

        string gameKey = (string)data;

        this.context.AddData("gameKey" , gameKey.ToLower());

        ConductGameInitProcess().Forget();

        (unit as LobbyScreen).SwitchUnit("PlayScreen", (Func<bool>)IsGameDataLoadingCompleted);
    }

    async Task ConductGameInitProcess()
    {
        isGameDataLoadingCompleted = false;

        try
        {
            await context.InitGame();
        }
        finally
        { 
            isGameDataLoadingCompleted = true;
        }
    }

    void EventOnBtnOptionDlgClicked()
    {
        AUnit dialogUnit = null;
        context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayUnitPopupDialog", 
            (errMsg, ret) => 
            {
                dialogUnit = ret as AUnit;
                Assert.IsNotNull(dialogUnit);
            }, 
            "OptionDialog", 
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("Option dlg has been closed from X.");

            } ));

        dialogUnit.OnEventDetached += OnOptionDialogClosed;
    }

    void OnOptionDialogClosed(object dlgUnit)
    {
        var settingDlgUnit = dlgUnit as SettingUnit;
        settingDlgUnit.OnEventDetached -= OnOptionDialogClosed;
        Debug.Log("Option Dialog has been closed.");
        
        if((bool)context.GetData("IsTitleViewLoginRequired", false))
            (unit as LobbyScreen).SwitchUnit("TitleScreen");
    }

    void EventOnBtnShopDlgClicked()
    {
         context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", 
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
        context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayUnitPopupDialog", 
            (errMsg, ret) => {}, 
            "DailyMission",
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("Daily Mission Dialog has been closed.");

            } ));
    }

    void ShopDlg_EventOnBtnBuyClicked(int amount)
    {
        const bool offset = true;
        context.RequestQuery("AppPlayerModel", "UpdateIAPCurrency", amount, offset);
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
