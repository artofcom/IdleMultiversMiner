using App.GamePlay.Common;
using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AppPlayerModel : MultiGatewayWritablePlayerModel
{
    MetaCurrencyBundle metaCurrencyBundle;

    EventsGroup events = new EventsGroup();

    public AppPlayerModel(AContext ctx, List<IDataGatewayService> gatewayServiceList) : base(ctx, gatewayServiceList)  { }

    static string CurrencyDataKey = "Currencies";

    const int DefaultIAP = 50;
    const int DefaultStar = 10;

    public override void Init()
    {     
        base.Init();

        LoadAppData();

        RegisterRequestables();

        events.RegisterEvent(EventID.GAME_RESET_REFRESH, OnGameResetRefresh);

        IsInitialized = true;
    }

    public override void Dispose()
    {
        base.Dispose();

        UnregisterRequestables();

        events.UnRegisterEvent(EventID.GAME_RESET_REFRESH, OnGameResetRefresh);

        IsInitialized = false;
    }

    // App Data Warpper.
    void LoadAppData()
    {        
        string curSignedPlayerId = (string)context.GetData("PlayerId", string.Empty);
        //string lastSignedPlayerId = PlayerPrefs.GetString(DataKeys.PLAYER_ID, string.Empty);

        int idxGatewayService = (context as IdleMinerContext).ValidGatewayServiceIndex;
        // FetchData(idxGatewayService, EnvironmentDataKey, out environment, new EnvironmentInfo("1.0"));
        FetchData(idxGatewayService, CurrencyDataKey, out metaCurrencyBundle, null);

        if(metaCurrencyBundle==null || metaCurrencyBundle.GetCurrency("iap")==null)
        {
            metaCurrencyBundle = new MetaCurrencyBundle();
            metaCurrencyBundle.AddCurrency(new MetaCurrency("iap", DefaultIAP));
            metaCurrencyBundle.AddCurrency(new MetaCurrency("star", DefaultStar));
            SetDirty();
        }

        Debug.Log($"<color=green>User App Data : Star[{metaCurrencyBundle.GetCurrency("iap").Amount}], Volt:[{metaCurrencyBundle.GetCurrency("star").Amount}]</color>");
    }

    void OnGameResetRefresh(object data)
    {
    }


    
    #region Requestables.

    void RegisterRequestables()
    {
        context.AddRequestDelegate("AppPlayerModel", "UpdateStarCurrency", updateStarCurrency);
        context.AddRequestDelegate("AppPlayerModel", "GetStarCurrency", getStarCurrency);
        context.AddRequestDelegate("AppPlayerModel", "UpdateIAPCurrency", updateIAPCurrency);
        context.AddRequestDelegate("AppPlayerModel", "GetIAPCurrency", getIAPCurrency);   
    }

    void UnregisterRequestables()
    {
        context.RemoveRequestDelegate("AppPlayerModel", "UpdateStarCurrency");
        context.RemoveRequestDelegate("AppPlayerModel", "GetStarCurrency");
        context.RemoveRequestDelegate("AppPlayerModel", "UpdateIAPCurrency");
        context.RemoveRequestDelegate("AppPlayerModel", "GetIAPCurrency");
    }

    object updateStarCurrency(params object[] data) // int amount, bool isOffset)
    {
        if(data.Length < 2)
            return null;

        int amount = (int)data[0];
        bool isOffset = (bool)data[1];

        string type = "star";
        metaCurrencyBundle.SetCurrency(type, isOffset ? metaCurrencyBundle.GetCurrency(type).Amount + amount : amount);
        SetDirty();

        EventSystem.DispatchEvent(EventID.STAR_AMOUNT_CHANGED);
        return metaCurrencyBundle.GetCurrency(type).Amount;
    }
    object getStarCurrency(params object[] data)
    {
        return metaCurrencyBundle.GetCurrency(type:"star").Amount;
    }
    object updateIAPCurrency(params object[] data) // int amount, bool isOffset)
    {
        if(data.Length < 2)
            return null;

        int amount = (int)data[0];
        bool isOffset = (bool)data[1];
        
        string type = "iap";
        metaCurrencyBundle.SetCurrency(type, isOffset ? metaCurrencyBundle.GetCurrency(type).Amount + amount : amount);
        SetDirty();

        EventSystem.DispatchEvent(EventID.IAP_MONEY_CHANGED);
        return metaCurrencyBundle.GetCurrency(type).Amount;
    }
    object getIAPCurrency(params object[] data)
    {
        return metaCurrencyBundle.GetCurrency(type:"iap").Amount;
    }
    #endregion


    public override List<Tuple<string, string>> GetSaveDataWithKeys()
    {
        List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
        
        Assert.IsNotNull(metaCurrencyBundle);
        listDataSet.Add(new Tuple<string, string>(CurrencyDataKey, JsonUtility.ToJson(metaCurrencyBundle)));
        
        return listDataSet;
    }

#if UNITY_EDITOR
    public static void ClearAllData()
    {
      //  WriteFileInternal(AppDataKey, string.Empty, convertToJson:false);
    }
#endif
}
