using App.GamePlay.Common;
using App.GamePlay.IdleMiner;
using Core.Utils;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using App.GamePlay.IdleMiner.Common.Types;

public class GameCardsController : AController
{
    public Action<string> EventOnBtnStart;


    bool bLoadingGame = false;

    public GameCardsController(AView view, AModel model, AContext context) : base(view, model, context)
    { }

    public override void Init() { }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  
    {
        bLoadingGame = false;

        RefreshView();

        view.StartCoroutine(coUpdate());

        (view as GameCardsView).EventGameCardClicked += OnBtnGameStart;
    }
    protected override void OnViewDisable() 
    { 
        (view as GameCardsView).EventGameCardClicked -= OnBtnGameStart;
    }

    void OnBtnGameStart(string gameKey)
    {
        if(!bLoadingGame)
            EventSystem.DispatchEvent(EventID.GAME_LEVEL_START, gameKey);

        bLoadingGame = true;
    }

    IEnumerator coUpdate()
    {
        yield return new WaitForSeconds(0.1f);
        RefreshView();

        var waitForASec = new WaitForSeconds(1.0f);
        while(true)
        {
            yield return waitForASec;
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

        view.Refresh( new GameCardsView.Presentor(listPresentor) );

        // Debug.Log("Refreshing Lobby view....");
    }
}
