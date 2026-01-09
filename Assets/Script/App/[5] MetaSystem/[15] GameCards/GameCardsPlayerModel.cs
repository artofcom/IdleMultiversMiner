using App.GamePlay.Common;
using Core.Events;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using UnityEngine;
using App.GamePlay.IdleMiner.Common.Types;
using IGCore.PlatformService;

public class GameCardsPlayerModel : MultiGatewayWritablePlayerModel
{
    GameCardBundle gameCardBundle;

    EventsGroup events = new EventsGroup();

    public GameCardsPlayerModel(AContext ctx, List<IDataGatewayService> gatewayServices) : base(ctx, gatewayServices) { }

    string DataKey => "GameCardsData";

    public override void Init()
    {
        base.Init();

        LoadGameCardData();

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

    void LoadGameCardData()
    {
        gameCardBundle = null;
        FetchData((context as IdleMinerContext).TargetMetaDataGatewayServiceIndex, DataKey, out gameCardBundle, new GameCardBundle());
    }

    public override List<Tuple<string, string>> GetSaveDataWithKeys()
    {
        UnityEngine.Assertions.Assert.IsNotNull(gameCardBundle);
        List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
        listDataSet.Add(new Tuple<string, string>(DataKey, JsonUtility.ToJson(gameCardBundle)));
        return listDataSet;
    }

    void OnGameResetRefresh(object data)
    {
        string gameKey = (string)data;
        GameCardInfo gameCardInfo = (GameCardInfo)getGameCardInfo(gameKey);
        if(gameCardInfo == null)
            gameCardInfo = new GameCardInfo(gameKey);

        gameCardInfo.ResetCount += 1;
        updateGameCardInfo(gameCardInfo);
        SetDirty();
    }

    void RegisterRequestables()
    {
        context.AddRequestDelegate("AppPlayerModel", "UpdateGameCardInfo", updateGameCardInfo);
        context.AddRequestDelegate("AppPlayerModel", "GetGameCardInfo", getGameCardInfo);
        context.AddRequestDelegate("AppPlayerModel", "GetGameCardInfoFromIndex", getGameCardInfoFromIndex);
        context.AddRequestDelegate("AppPlayerModel", "GetGameCardCount", getGameCardCount);
    }

    void UnregisterRequestables()
    {
        context.RemoveRequestDelegate("AppPlayerModel", "UpdateGameCardInfo");
        context.RemoveRequestDelegate("AppPlayerModel", "GetGameCardInfo");
        context.RemoveRequestDelegate("AppPlayerModel", "GetGameCardInfoFromIndex");
        context.RemoveRequestDelegate("AppPlayerModel", "GetGameCardCount");
    }


    object updateGameCardInfo(params object[] data)
    {
        if(data.Length < 1)
            return null;

        gameCardBundle.UpdateGameCardInfo(data[0] as GameCardInfo);    
        SetDirty();
        return null;
    }
    object getGameCardInfo(params object [] data) 
    {
        if(data.Length < 1)
            return null;

        return (GameCardInfo)gameCardBundle.GetGameCardInfo((string)data[0]);
    }
    object getGameCardInfoFromIndex(params object [] data) 
    {
        if(data.Length < 1)
            return null;

        return (GameCardInfo)gameCardBundle.GetGameCardInfo((int)data[0]);
    }
    object getGameCardCount(params object [] data) 
    {
        return gameCardBundle.GameCardCount();
    }
}
